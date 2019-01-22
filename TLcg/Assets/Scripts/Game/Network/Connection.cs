/*
* ==============================================================================
* 
* Created: 2017-3-3
* Author: Panda,Champion
* Company: LightPaw
* 
* ==============================================================================
*/

using System;
using System.IO;
using System.Threading;
using Rnet;
using UnityEngine;

namespace LCG
{
    public class ProtoPackageLength
    {
        // 最大写入长度
        public const Int32 MAX_WRITE_LENGTH = 32768;
        // 最大读取长度
        public const Int32 MAX_READ_LENGTH = 1024 * 1024;
        // 客户端消息的最大长度
        public const Int32 MAX_CLIENT_PACKET_LENGTH = 2048 + 2; // 加2个byte[]预留的2个byte
    }

    public enum EConnectionState
    {
        NONE = 0,
        //连接中
        CONNECTING = 1,
        //已连接
        CONNECTED = 2,
        // 必须重新走登录流程
        MUST_RELOGIN = 3,
        // 必须重新发起连接
        MUST_RECONNECT = 4,
        //客户端强制退出连接
        FROCE_CLOSED = 5,
    }

    public class Connection
    {
        private readonly byte[] header;
        private RnetStream _workSocket;

        private EConnectionState _state;
        public EConnectionState State
        {
            get { return _state; }
            set
            {
                _state = value;
                if (value == EConnectionState.NONE || value == EConnectionState.FROCE_CLOSED)
                {
                    return;
                }

                Debug.Log("切换连接状态到:" + value.ToString());
                Network.Instance.ServerMsgEnqueue(new byte[] { (byte)value });
            }
        }

        public Connection()
        {
            this.State = EConnectionState.NONE;
            this.header = new byte[3];
        }

        /// 建立连接
        /// loginToken = new byte[] { 97, 219, 141, 187, 52, 110, 222, 178, 181, 66, 41, 20, 228, 214, 172, 70, 60, 87, 145, 95, 92, 227, 129, 222, 121, 144, 87, 18, 58, 151, 186, 27, 184, 79, 70, 203, 91, 215, 55, 78, 98, 220, 132, 86, 175, 100, 224, 30, 249, 220, 165, 52, 100, 209, 2, 242, 59, 131, 194, 139, 13, 117, 177, 29 };
        /// key = new byte[] {100, 186, 220, 224, 99, 0, 0, 0};
        public void Connect(string remoteAddress, int remotePort, byte[] loginToken, byte[] key)
        {
            if (null != this._workSocket)
            {
                Close();
            }

            this.State = EConnectionState.CONNECTING;

            var socket = new RnetStream(ProtoPackageLength.MAX_WRITE_LENGTH, loginToken, key);
            if (Interlocked.CompareExchange(ref _workSocket, socket, null) == null)
            {
                socket.BeginConnect(remoteAddress, remotePort, ConnectCallBack, null);
            }
        }
        private void ConnectCallBack(IAsyncResult result)
        {
            var socket = (result as RnetStream.AsyncResult).Socket;
            if (socket != this._workSocket)
            {
                return;
            }

            try
            {
                socket.EndConnect(result);
            }
            catch (Exception ex)
            {
                OnError(ex, socket);
                return;
            }

            this.State = EConnectionState.CONNECTED;
            ReadHeader(socket);
        }

        /// 接收消息
        private void ReadHeader(RnetStream socket)
        {
            socket.BeginRead(header, 0, 3, HeaderCallback, null);
        }
        private void HeaderCallback(IAsyncResult result)
        {
            var socket = (result as RnetStream.AsyncResult).Socket;
            try
            {
                socket.EndRead(result);
            }
            catch (Exception ex)
            {
                OnError(ex, socket);
                return;
            }

            if (header[0] <= 127)
            {
                ReadContent(header[0], 2, socket);
            }
            else if (header[1] <= 127)
            {
                int size = ((int)(header[1]) << 7) | ((int)(header[0]) & 0x7f);
                ReadContent(size, 1, socket);
            }
            else
            {
                int size = ((int)(header[2]) << 14) | (((int)(header[1]) & 0x7f) << 7) | ((int)(header[0]) & 0x7f);
                ReadContent(size, 0, socket);
            }
        }
        private void ReadContent(int contentLength, int dataSize, RnetStream socket)
        {
            if (contentLength > ProtoPackageLength.MAX_READ_LENGTH)
            {
                Debug.Log("消息包太大:" + contentLength.ToString());
                Close();
                return;
            }

            var buf = new byte[contentLength];
            switch (dataSize)
            {
                case 1:
                    buf[0] = header[2];
                    break;
                case 2:
                    buf[0] = header[1];
                    buf[1] = header[2];
                    break;
            }

            socket.BeginRead(buf, dataSize, contentLength - dataSize, RecvCallback, buf);
        }
        private void RecvCallback(IAsyncResult result)
        {
            var socket = (result as RnetStream.AsyncResult).Socket;
            if (socket != this._workSocket)
            {
                return;
            }

            try
            {
                socket.EndRead(result);
            }
            catch (Exception ex)
            {
                OnError(ex, socket);
                return;
            }

            Network.Instance.ServerMsgEnqueue(result.AsyncState as byte[]);
            ReadHeader(socket);
        }

        /// 发送消息
        public void SendMessage(byte[] msg)
        {
            if (null == this._workSocket)
            {
                return;
            }

            if (msg.Length >= ProtoPackageLength.MAX_CLIENT_PACKET_LENGTH)
            {
                Debug.Log(string.Format("要发送的消息包太大, 最大长度: {0}, 现在要发送的长度: {1}", ProtoPackageLength.MAX_CLIENT_PACKET_LENGTH, msg.Length));
                Close();
                return;
            }

            this._workSocket.BeginWrite(msg, 0, msg.Length, SendCallBack, null);
        }

        private void SendCallBack(IAsyncResult result)
        {
            var socket = (result as RnetStream.AsyncResult).Socket;
            if (socket != this._workSocket)
            {
                return;
            }

            try
            {
                socket.EndWrite(result);
            }
            catch (Exception ex)
            {
                OnError(ex, socket);
                return;
            }
        }

        /// 关闭连接
        public void Close()
        {
            if (null == this._workSocket)
            {
                return;
            }
            var socket = this._workSocket;
            this.State = EConnectionState.FROCE_CLOSED;
            this._workSocket = null;
            socket.Close();
            socket = null;
        }

        /// 异常检测
        private void OnError(Exception ex, RnetStream socket)
        {
            socket.Close();
            if (Interlocked.CompareExchange(ref _workSocket, null, socket) != socket)
            {
                return;
            }

            if (ex is MustReconnectException)
            {
                this.State = EConnectionState.MUST_RECONNECT;
            }
            else if (ex is MustReloginException)
            {
                this.State = EConnectionState.MUST_RELOGIN;
            }
            else
            {
                this.State = EConnectionState.MUST_RELOGIN;
            }
        }
    }
}

