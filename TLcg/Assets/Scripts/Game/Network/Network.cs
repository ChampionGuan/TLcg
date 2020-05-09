using UnityEngine;
using System;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace LCG
{
    public class Network : Singleton<Network>, Define.IMonoBase
    {
        /// <summary>
        /// 网络句柄
        /// </summary>
        private ClientConnection m_connection;
        /// <summary>
        /// snappy解压
        /// </summary>
        private Snappy.Sharp.SnappyDecompressor m_snappyUncompress = new Snappy.Sharp.SnappyDecompressor();
        /// <summary>
        /// gzip解压
        /// </summary>
        private byte[] m_gzipReadBytes = new byte[4096];

        public override void OnInstance()
        {
            m_connection = new ClientConnection();
        }
        public void CustomAppFocus(bool focus)
        {
        }

        public void CustomDestroy()
        {
            Disconnect();
        }

        public void CustomFixedUpdate()
        {
        }

        public void CustomUpdate()
        {
            CSharpCallLua.Instance.ReceiveServerMsg(m_connection.GetRecvMsg());
        }

        /// <summary>
        /// 请求连接
        /// </summary>
        /// <param name="remoteAddress">IP</param>
        /// <param name="remotePort">Port</param>
        /// <param name="onConnected">Action</param>
        public void Connect(string remoteAddress, int remotePort, Action onConnected)
        {
            m_connection.Connect(remoteAddress, remotePort, onConnected);
        }

        /// <summary>
        /// 请求断线
        /// </summary>
        public void Disconnect()
        {
            m_connection.Disconnect();
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(byte[] msg)
        {
            m_connection.Send(msg);
        }

        /// <summary>
        /// 网络可用
        /// </summary>
        public bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }
        ///// <summary>
        ///// 本机ip地址
        ///// </summary>

        public string IpAddress
        {
            get
            {
                // return UnityEngine.Network.player.ipAddress.ToString();
                return "";
            }
        }

        /// <summary>
        /// sha1安全签名
        /// </summary>
        /// <returns></returns>
        public string GetSha1(string value)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();

            byte[] dataToHash = enc.GetBytes(value);
            byte[] dataHashed = sha.ComputeHash(dataToHash);

            return BitConverter.ToString(dataHashed).Replace("-", "");
        }

        /// <summary>
        /// 转byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }
}