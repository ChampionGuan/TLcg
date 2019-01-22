using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace Rnet
{
    public class RnetStream : Stream
    {
        private const byte PROTOCOL_VERSION = 1;
        private ulong _ID;
        private string _Host;
        private int _Port;
        private RC4Cipher _ReadCipher;
        private RC4Cipher _WriteCipher;

        private readonly byte[] _token;
        private readonly byte[] _preKey;

        private readonly Mutex _ReadLock = new Mutex();
        private readonly Mutex _WriteLock = new Mutex();
        private readonly ReaderWriterLock _ReconnLock = new ReaderWriterLock();

        private NetworkStream _BaseStream;
        private NetworkStream _TempStream;

        private readonly Rewriter _Rewriter;
        private readonly Rereader _Rereader;

        private byte index;

        private ulong _ReadCount;
        private ulong _WriterCount;
        private bool isEncrypt;

        private bool _Closed;

        public RnetStream(int size, byte[] token, byte[] preKey)
        {
            _token = token;
            _preKey = preKey;
            _Rewriter = new Rewriter(size);
            _Rereader = new Rereader();

            ConnectTimeout = 5000;
            HandShakeTimeout = 10000;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        internal class AsyncResult : IAsyncResult
        {
            internal AsyncResult(AsyncCallback callback, object state, RnetStream socket)
            {
                this.Socket = socket;
                this.Callback = callback;
                this.AsyncState = state;
                this.IsCompleted = false;
                this.AsyncWaitHandle = new ManualResetEvent(false);
            }

            internal AsyncCallback Callback { get; set; }
            public RnetStream Socket { get; internal set; }
            public object AsyncState { get; internal set; }
            public WaitHandle AsyncWaitHandle { get; internal set; }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted { get; internal set; }
            internal int ReadCount { get; set; }
            internal Exception Error { get; set; }

            internal int Wait()
            {
                AsyncWaitHandle.WaitOne();
                if (Error != null)
                    throw Error;
                return ReadCount;
            }
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callback, object state)
        {
            if (_BaseStream != null)
                throw new InvalidOperationException();

            AsyncResult ar1 = new AsyncResult(callback, state, this);
            ThreadPool.QueueUserWorkItem((object ar2) =>
            {
                AsyncResult ar3 = (AsyncResult)ar2;
                try
                {
                    Connect(host, port);
                }
                catch (Exception ex)
                {
                    ar3.Error = ex;
                }
                ar3.IsCompleted = true;
                ((ManualResetEvent)ar3.AsyncWaitHandle).Set();
                if (ar3.Callback != null)
                    ar3.Callback(ar3);
            }, ar1);

            return ar1;
        }

        public void WaitConnect(IAsyncResult asyncResult)
        {
            ((AsyncResult)asyncResult).Wait();
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            ((AsyncResult)asyncResult).Wait();
        }

        public void Connect(string host, int port)
        {
            if (_BaseStream != null)
                throw new InvalidOperationException();

            _Host = host;
            _Port = port;
            handshake();
        }

        private void handshake()
        {
            DateTime endTime = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, HandShakeTimeout));
            for (; ; )
            {
                try
                {
                    negotiateKey(false);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogFormat("handshake catch exception: {0}: {1}", ex.GetType(), ex.StackTrace);

                    if (ex is MustReconnectException || ex is MustReloginException)
                    {
                        throw;
                    }
                    if (DateTime.Now.CompareTo(endTime) > 0) throw new MustReloginException();
                    Thread.Sleep(1000);
                    continue;
                }
            }
        }

        private void negotiateKey(bool isReconn)
        {
            byte[] request = new byte[80];
            byte[] response = request;
            request[0] = PROTOCOL_VERSION;

            var key = new byte[16];
            Buffer.BlockCopy(_preKey, 0, key, 0, 8);
            Buffer.BlockCopy(_token, 0, key, 8, 8);

            var encryptCipher = new RC4Cipher(key);

            var nonce = new byte[8];

            new Random().NextBytes(nonce);
            if (isReconn)
            {
                nonce[0] |= 1; // sets last bit
            }
            else
            {
                nonce[0] &= 0xfe; // clear last bit
            }
            Buffer.BlockCopy(_token, 0, request, 1, 64);
            encryptCipher.XORKeyStream(request, 65, nonce, 0, 8);

            TcpClient client = new TcpClient();
            var ar = client.BeginConnect(_Host, _Port, null, null);
            ar.AsyncWaitHandle.WaitOne(new TimeSpan(0, 0, 0, 0, ConnectTimeout));
            if (!ar.IsCompleted)
            {
                throw new TimeoutException();
            }
            client.EndConnect(ar);

            var stream = client.GetStream();
            _TempStream = stream;

            stream.Write(request, 0, 73);

            for (int n = 16; n > 0;)
            {
                int x = stream.Read(response, 16 - n, n);
                if (x == 0)
                    throw new EndOfStreamException();
                n -= x;
            }

            if (isAllZero(response, 0, 16))
            {
                throw new MustReloginException();
            }

            encryptCipher.XORKeyStream(response, 0, response, 0, 16);
            Buffer.BlockCopy(response, 0, key, 8, 8);

            var serverNonce = new byte[8];
            Buffer.BlockCopy(response, 8, serverNonce, 0, 8);
            if (!nonce.SequenceEqual(serverNonce))
            {
                throw new ArgumentException();
            }

            _WriteCipher = new RC4Cipher(key);
            _ReadCipher = new RC4Cipher(key);

            if (isReconn)
            {
                using (MemoryStream ms = new MemoryStream(request))
                {
                    using (BinaryWriter w = new BinaryWriter(ms))
                    {
                        w.Write(nonce);
                        w.Write(_ID);
                        w.Write(_ReadCount + _Rereader.Count);
                        w.Write(_WriterCount);
                    }
                }
                _WriteCipher.XORKeyStream(request, 0, request, 0, 32);
                stream.Write(request, 0, 32);

                for (int n = 80; n > 0;)
                {
                    int x = stream.Read(response, 80 - n, n);
                    if (x == 0)
                        throw new EndOfStreamException();
                    n -= x;
                }

                _ReadCipher.XORKeyStream(response, 0, response, 0, 80);

                if (isAllZero(response, 1, 63))
                {
                    switch (response[0])
                    {
                        case 0:
                            throw new MustReloginException();
                        case 1:
                            throw new MustReconnectException();
                        default: // wtf?
                            throw new MustReloginException();
                    }
                }
                Buffer.BlockCopy(response, 0, _token, 0, 64);

                ulong writeCount = 0;
                ulong readCount = 0;
                using (MemoryStream ms = new MemoryStream(response, 64, 16))
                {
                    using (BinaryReader r = new BinaryReader(ms))
                    {
                        readCount = r.ReadUInt64();
                        writeCount = r.ReadUInt64();
                    }
                }
                doReconn(stream, writeCount, readCount);
            }
            else
            {
                _WriteCipher.XORKeyStream(request, 0, nonce, 0, 8);
                stream.Write(request, 0, 8);

                for (int n = 73; n > 0;)
                {
                    int x = stream.Read(response, 73 - n, n);
                    if (x == 0)
                        throw new EndOfStreamException();
                    n -= x;
                }
                _ReadCipher.XORKeyStream(response, 0, response, 0, 73);
                if (isAllZero(response, 0, 64))
                {
                    throw new MustReloginException();
                }

                Buffer.BlockCopy(response, 0, _token, 0, 64);
                _ID = BitConverter.ToUInt64(response, 64);
                this.isEncrypt = response[72] == 1;

                setBaseStream(stream);
            }
        }

        public bool isAllZero(byte[] b, int offset, int length)
        {
            for (int i = offset; i < offset + length; i++)
            {
                if (b[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public String toString(byte[] bb)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bb.Length; i++)
            {
                var b = bb[i];
                sb.Append(b);
                if (i < bb.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            AsyncResult ar1 = new AsyncResult(callback, state, this);
            ThreadPool.QueueUserWorkItem((object ar2) =>
            {
                AsyncResult ar3 = (AsyncResult)ar2;
                try
                {
                    while (ar3.ReadCount != count)
                    {
                        ar3.ReadCount += Read(buffer, offset + ar3.ReadCount, count - ar3.ReadCount);
                    }
                }
                catch (Exception ex)
                {
                    ar3.Error = ex;
                }
                ar3.IsCompleted = true;
                ((ManualResetEvent)ar3.AsyncWaitHandle).Set();
                if (ar3.Callback != null)
                    ar3.Callback(ar3);
            }, ar1);
            return ar1;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((AsyncResult)asyncResult).Wait();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            AsyncResult ar1 = new AsyncResult(callback, state, this);
            ThreadPool.QueueUserWorkItem((object ar2) =>
            {
                AsyncResult ar3 = (AsyncResult)ar2;
                try
                {
                    Write(buffer, offset, count);
                }
                catch (Exception ex)
                {
                    ar3.Error = ex;
                }
                ar3.IsCompleted = true;
                ((ManualResetEvent)ar3.AsyncWaitHandle).Set();
                if (ar3.Callback != null)
                    ar3.Callback(ar3);
            }, ar1);
            return ar1;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ((AsyncResult)asyncResult).Wait();
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            _ReadLock.WaitOne();
            _ReconnLock.AcquireReaderLock(-1);
            int n = 0;
            try
            {
                for (; ; )
                {
                    n = _Rereader.Pull(buffer, offset, size);
                    if (n > 0)
                    {
                        return n;
                    }

                    try
                    {
                        n = _BaseStream.Read(buffer, offset + n, size);
                        if (n == 0)
                        {
                            if (!tryReconn())
                                throw new IOException();
                            continue;
                        }
                    }
                    catch
                    {
                        if (!tryReconn())
                            throw;
                        continue;
                    }
                    break;
                }
            }
            finally
            {
                if (n > 0 && isEncrypt)
                {
                    _ReadCipher.XORKeyStream(buffer, offset, buffer, offset, n);
                }
                _ReadCount += (ulong)n;
                _ReconnLock.ReleaseReaderLock();
                _ReadLock.ReleaseMutex();
            }
            return n;
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (size == 0)
                return;

            _WriteLock.WaitOne();
            _ReconnLock.AcquireReaderLock(-1);
            try
            {
                // modify index & sum
                var msgSize = size - 2;
                buffer[offset + 2] = index++;
                int msgOffset = offset + 3;

                if (msgSize <= 127)
                {
                    buffer[offset + 1] = (byte)msgSize;
                    offset = offset + 1;
                    size = msgSize + 1;
                }
                else
                {
                    buffer[offset] = (byte)(0x80 | (msgSize & 0x7f));
                    buffer[offset + 1] = (byte)(msgSize >> 7);
                    size = msgSize + 2;
                }

                int sum = 0;
                int end = offset + size;
                for (int i = msgOffset; i < end; i++)
                {
                    sum += buffer[i];
                }
                byte byteSum = (byte)(sum & 0xff);
                buffer[msgOffset - 1] ^= byteSum;

                if (isEncrypt)
                {
                    _WriteCipher.XORKeyStream(buffer, offset, buffer, offset, size);
                }

                _Rewriter.Push(buffer, offset, size);
                _WriterCount += (ulong)size;

                try
                {
                    _BaseStream.Write(buffer, offset, size);
                }
                catch
                {
                    if (!tryReconn())
                        throw;
                }
            }
            finally
            {
                _ReconnLock.ReleaseReaderLock();
                _WriteLock.ReleaseMutex();
            }
        }

        private bool tryReconn()
        {
            _BaseStream.Close();
            if (_Closed) return false;//client force close connection,no need to reconnect

            NetworkStream badStream = _BaseStream;

            _ReconnLock.ReleaseReaderLock();
            _ReconnLock.AcquireWriterLock(-1);

            try
            {
                if (badStream != _BaseStream)
                    return true;
                DateTime endTime = DateTime.Now.Add(new TimeSpan(0, 0, 0, 0, HandShakeTimeout));
                for (; ; )
                {
                    try
                    {
                        negotiateKey(true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogFormat("handshake catch exception: {0}", ex.GetType());
                        if (ex is MustReconnectException || ex is MustReloginException)
                        {
                            throw;
                        }
                        if (DateTime.Now.CompareTo(endTime) > 0) throw new MustReloginException();
                        Thread.Sleep(1000);
                        continue;
                    }
                }
            }
            finally
            {
                _ReconnLock.ReleaseWriterLock();
                _ReconnLock.AcquireReaderLock(-1);
            }
            return false;
        }

        private void doReconn(NetworkStream stream, ulong writeCount, ulong readCount)
        {
            if (writeCount < _ReadCount)
                throw new MustReconnectException();

            if (_WriterCount < readCount)
                throw new MustReconnectException();

            Thread thread = null;
            bool rereadSucceed = false;

            if (writeCount != _ReadCount)
            {
                thread = new Thread(() =>
                {
                    int n = (int)writeCount - (int)_ReadCount;
                    rereadSucceed = _Rereader.Reread(stream, n);
                });
                thread.Start();
            }

            if (_WriterCount != readCount)
            {
                if (!_Rewriter.Rewrite(stream, _WriterCount, readCount))
                    throw new MustReconnectException();
            }

            if (thread != null)
            {
                thread.Join();
                if (!rereadSucceed)
                    throw new MustReconnectException();
            }

            setBaseStream(stream);
        }

        private void setBaseStream(NetworkStream stream)
        {
            _BaseStream = stream;
            _TempStream = null;

            if (_ReadTimeout > 0)
                _BaseStream.ReadTimeout = this.ReadTimeout;

            if (_WriteTimeout > 0)
                _BaseStream.WriteTimeout = this.WriteTimeout;
        }

        public override void Flush()
        {
            _WriteLock.WaitOne();
            _ReconnLock.AcquireReaderLock(-1);
            try
            {
                _BaseStream.Flush();
            }
            catch
            {
                if (!tryReconn())
                    throw;
            }
            finally
            {
                _ReconnLock.ReleaseReaderLock();
                _WriteLock.ReleaseMutex();
            }
        }

        public override void Close()
        {
            _Closed = true;
            var stream = _BaseStream;
            if (stream != null)
            {
                stream.Close();
                _BaseStream = null;
            }

            stream = _TempStream;
            if (stream != null)
            {
                stream.Close();
                _TempStream = null;
            }
        }

        public int ConnectTimeout { get; set; }

        private int HandShakeTimeout { get; set; }

        private int _ReadTimeout;

        public override int ReadTimeout
        {
            get { return _ReadTimeout; }
            set
            {
                _ReadTimeout = value;
                if (_BaseStream != null)
                    _BaseStream.ReadTimeout = value;
            }
        }

        private int _WriteTimeout;

        public override int WriteTimeout
        {
            get { return _WriteTimeout; }
            set
            {
                _WriteTimeout = value;
                if (_BaseStream != null)
                    _BaseStream.WriteTimeout = value;
            }
        }
    }
}

