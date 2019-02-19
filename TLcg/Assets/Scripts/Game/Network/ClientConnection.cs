using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;

namespace LCG
{
    public class ClientConnection
    {
        // 192.168.1.110
        private string m_ip;
        // 60000
        private int m_port;

        private Socket m_socket;

        private Queue<byte[]> m_recevQueue;
        private EConnectionState m_connectionstate;
        private Action m_connectedCallBack;

        public ClientConnection()
        {
            m_connectionstate = EConnectionState.NONE;
            m_recevQueue = new Queue<byte[]>();
        }

        public EConnectionState State
        {
            get
            {
                return m_connectionstate;
            }
            private set
            {
                m_connectionstate = value;
                lock (m_recevQueue)
                {
                    m_recevQueue.Enqueue(new byte[1] { (byte)value });
                }
                UnityEngine.Debug.Log("socket状态：" + value);
            }
        }
        public byte[] GetRecvMsg()
        {
            lock (m_recevQueue)
            {
                if (m_recevQueue.Count > 0)
                {
                    return m_recevQueue.Dequeue();
                }
                return null;
            }
        }
        public bool Connect(string address, int port, Action onConnnected)
        {
            if (State == EConnectionState.CONNECTING)
            {
                return true;
            }

            ShutDown();
            IPEndPoint ipEndPoint;
            State = EConnectionState.CONNECTING;

            try
            {
                IPAddress ip;
                if (!IPAddress.TryParse(address, out ip))
                {
                    throw new Exception("address failure.");
                }
                m_ip = ip.ToString();
                m_port = port;
                ipEndPoint = new IPEndPoint(ip, port);

                if (ipEndPoint.AddressFamily.ToString() == ProtocolFamily.InterNetworkV6.ToString())
                {
                    throw new Exception("nonsupport ipv6.");
                }

                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (null == m_socket)
                {
                    throw new Exception("socket null.");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("build socket failure :" + e.Message);
                State = EConnectionState.CONNECTING_ERROR;
                return false;
            }

            m_connectedCallBack = onConnnected;
            m_socket.BeginConnect(ipEndPoint, ConnectCallBack, new ConnectState(m_socket));
            return true;
        }
        public bool Send(byte[] msg)
        {
            if (null == m_socket || State != EConnectionState.CONNECTED)
            {
                return false;
            }

            byte[] head;
            int msgLength = msg.Length;
            if (msgLength <= 127) //2,7 -1
            {
                head = new byte[1];
                head[0] = (byte)msgLength;
            }
            else if (msgLength <= 16383) //2,14 -1
            {
                head = new byte[2];
                head[0] = (byte)(0x80 | (msgLength & 0x7f));
                head[1] = (byte)(msgLength >> 7);
            }
            else
            {
                head = new byte[3];
                head[0] = (byte)((0x80 | (msgLength & 0x7f)));
                head[1] = (byte)(0x80 | ((msgLength >> 7) & 0x7f));
                head[2] = (byte)(msgLength >> 14);
            }

            int fullLength = head.Length + msgLength;
            byte[] fullBytes = new byte[fullLength];
            Array.Copy(head, 0, fullBytes, 0, head.Length);
            Array.Copy(msg, 0, fullBytes, head.Length, msg.Length);
            m_socket.BeginSend(fullBytes, 0, fullLength, SocketFlags.None, SendCallBack, new SendState(m_socket, fullLength));
            return true;
        }
        public void Disconnect()
        {
            ShutDown();
            State = EConnectionState.CLOSE;
        }
        private void ConnectCallBack(IAsyncResult result)
        {
            ConnectState state = result.AsyncState as ConnectState;
            if (m_socket != state.WorkSocket)
            {
                return;
            }
            try
            {
                state.WorkSocket.EndConnect(result);
                if (!state.WorkSocket.Connected)
                {
                    throw new Exception("connect failure.");
                }

                State = EConnectionState.CONNECTED;
                if (null != m_connectedCallBack)
                {
                    m_connectedCallBack.Invoke();
                    m_connectedCallBack = null;
                }
                RecvState recvState = new RecvState(state.WorkSocket, Define.BODY_HEAD_LENGTH);
                state.WorkSocket.BeginReceive(recvState.Bytes, 0, recvState.RecvLength, SocketFlags.None, ReadHeadCallBack, recvState);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("connect failure :" + e.Message);
                State = EConnectionState.CONNECTING_ERROR;
                Disconnect();
            }
        }
        private void ReadHeadCallBack(IAsyncResult result)
        {
            RecvState state = result.AsyncState as RecvState;
            if (m_socket != state.WorkSocket)
            {
                return;
            }
            try
            {
                if (state.WorkSocket.EndReceive(result) != state.RecvLength)
                {
                    throw new Exception("recv failure.");
                }
                int offset = 0;
                int bodyLength = 0;
                byte[] bodyBytes = null;
                if (state.Bytes[0] <= 127)
                {
                    bodyLength = state.Bytes[0];
                    bodyBytes = new byte[bodyLength];
                    bodyBytes[0] = state.Bytes[1];
                    bodyBytes[1] = state.Bytes[2];
                    offset = 2;
                }
                else if (state.Bytes[1] <= 127)
                {
                    bodyLength = ((int)(state.Bytes[1]) << 7) | ((int)(state.Bytes[0]) & 0x7f);
                    bodyBytes = new byte[bodyLength];
                    bodyBytes[0] = state.Bytes[2];
                    offset = 1;
                }
                else
                {
                    bodyLength = ((int)(state.Bytes[2]) << 14) | (((int)(state.Bytes[1]) & 0x7f) << 7) | ((int)(state.Bytes[0]) & 0x7f);
                    bodyBytes = new byte[bodyLength];
                    offset = 0;
                }
                if (bodyLength > Define.BODY_MAX_LENGTH)
                {
                    throw new Exception("recv too large.");
                }
                RecvState recvState = new RecvState(state.WorkSocket, bodyLength - offset, bodyBytes);
                state.WorkSocket.BeginReceive(recvState.Bytes, offset, bodyLength - offset, SocketFlags.None, RecvCallBack, recvState);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("recv failure：" + e.Message);
                State = EConnectionState.RECV_ERROR;
                Disconnect();
            }
        }
        private void RecvCallBack(IAsyncResult result)
        {
            RecvState state = result.AsyncState as RecvState;
            if (m_socket != state.WorkSocket)
            {
                return;
            }
            try
            {
                if (state.WorkSocket.EndReceive(result) != state.RecvLength)
                {
                    throw new Exception("recv failure.");
                }
                lock (m_recevQueue)
                {
                    m_recevQueue.Enqueue(state.Bytes);
                }

                RecvState recvState = new RecvState(state.WorkSocket, Define.BODY_HEAD_LENGTH);
                state.WorkSocket.BeginReceive(recvState.Bytes, 0, recvState.RecvLength, SocketFlags.None, ReadHeadCallBack, recvState);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("recv failure：" + e.Message);
                State = EConnectionState.RECV_ERROR;
                Disconnect();
            }
        }
        private void SendCallBack(IAsyncResult result)
        {
            SendState state = result.AsyncState as SendState;
            if (m_socket != state.WorkSocket)
            {
                return;
            }
            try
            {
                if (state.WorkSocket.EndSend(result) != state.SendLength)
                {
                    throw new Exception("send failure.");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("send failure：" + e.Message);
                State = EConnectionState.SEND_ERROR;
                Disconnect();
            }
        }
        private void ShutDown()
        {
            if (null != m_socket)
            {
                if (m_socket.Connected)
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                }
                m_socket.Close();
                m_socket = null;
            }
            lock (m_recevQueue)
            {
                m_recevQueue.Clear();
            }
            m_connectedCallBack = null;
        }


        public class Define
        {
            public const int BODY_MAX_LENGTH = 2097151; // 2^21- 1
            public const int BODY_HEAD_LENGTH = 3;
        }
        public enum EConnectionState
        {
            NONE = -1,
            CONNECTING = 0,
            CONNECTING_ERROR,
            CONNECTED,
            RECV_ERROR,
            SEND_ERROR,
            CLOSE,
            Error,
        }
        public class ConnectState
        {
            public Socket WorkSocket { get; private set; }
            public ConnectState(Socket socket)
            {
                WorkSocket = socket;
            }
        }
        public class RecvState
        {
            public Socket WorkSocket { get; private set; }
            public int RecvLength { get; private set; }
            public byte[] Bytes { get; private set; }
            public RecvState(Socket socket, int length)
            {
                WorkSocket = socket;
                RecvLength = length;
                Bytes = new byte[length];
            }
            public RecvState(Socket socket, int length, byte[] bytes)
            {
                WorkSocket = socket;
                RecvLength = length;
                Bytes = bytes;
            }
        }
        public class SendState
        {
            public Socket WorkSocket { get; private set; }
            public int SendLength { get; private set; }
            public SendState(Socket socket, int length)
            {
                WorkSocket = socket;
                SendLength = length;
            }
        }
    }
}
