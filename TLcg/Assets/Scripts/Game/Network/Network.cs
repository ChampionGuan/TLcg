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
        private Connection m_connection;
        /// <summary>
        /// 消息队列
        /// </summary>
        private DisruptorUnity3d.RingBuffer<byte[]> m_msgQueue;
        /// <summary>
        /// 消息缓存
        /// </summary>
        private static byte[] m_msgBuffer = new byte[ProtoPackageLength.MAX_READ_LENGTH];
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
            m_connection = new Connection();
            m_msgQueue = new DisruptorUnity3d.RingBuffer<byte[]>(64);
        }
        public void CustomAppFocus(bool focus)
        {
        }

        public void CustomDestroy()
        {
            Disconnect();
            m_msgQueue = null;
            m_connection = null;
            m_msgBuffer = null;
        }

        public void CustomFixedUpdate()
        {
        }

        public void CustomUpdate()
        {
            // 网络消息
            if (null != m_msgQueue && m_msgQueue.Count > 0)
            {
                //byte[] bytes;
                if (!m_msgQueue.TryDequeue(out m_msgBuffer))
                {
                    return;
                }
                try
                {
                    CSharpCallLua.Instance.ReceiveServerMsg(m_msgBuffer);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogErrorFormat("处理消息出错 {0}", ex.ToString());
#endif
                }
            }
        }

        /// <summary>
        /// 请求认证登陆
        /// </summary>
        /// <param name="remoteAddress">IP</param>
        /// <param name="remotePort">Port</param>
        /// <param name="token">token</param>
        /// <param name="key">key</param>
        public void Connect(string remoteAddress, int remotePort, string token, string key)
        {
            //转byte[]
            byte[] bToken = ConvertHexStringToByteArray(token);
            byte[] bKey = ConvertHexStringToByteArray(key);

            if (m_connection != null)
            {
                m_connection.Connect(remoteAddress, remotePort, bToken, bKey);
            }
        }

        /// <summary>
        /// 请求断线
        /// </summary>
        public void Disconnect()
        {
            if (m_connection != null)
            {
                m_connection.Close();
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(byte[] msg)
        {
            if (m_connection != null)
            {
                m_connection.SendMessage(msg);
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
        /// rc4编码
        /// </summary>
        /// <returns></returns>
        byte pfAppendByte = 156;
        byte[] pfRc4KeyV1 = Encoding.Default.GetBytes("f8607fdc9860b02fcc85f8692780d043");
        char[] padding = { '=' };
        public string Rc4StrEncoding(string bytes)
        {
            return Rc4BytesEncoding(Encoding.UTF8.GetBytes(bytes));
        }

        public string Rc4BytesEncoding(byte[] bytes)
        {
            Rnet.RC4Cipher rc4 = new Rnet.RC4Cipher(pfRc4KeyV1);

            byte[] bytes1 = new byte[bytes.Length + 1];
            Array.Copy(bytes, 0, bytes1, 0, bytes.Length);
            bytes1[bytes.Length] = pfAppendByte;

            byte[] bytes2 = new byte[bytes1.Length];
            rc4.XORKeyStream(bytes2, 0, bytes1, 0, bytes1.Length);

            return Convert.ToBase64String(bytes2).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// 消息进队列
        /// </summary>
        /// <param name="bytes"></param>
        public void ServerMsgEnqueue(byte[] bytes)
        {
            // 第一个字节是0，表示需要解压
            if (bytes.Length >= 2 && bytes[0] == 0)
            {
                // snappy解压
                if (bytes[1] == 0)
                {
                    try
                    {
                        m_msgQueue.Enqueue(m_snappyUncompress.Decompress(bytes, 2, bytes.Length - 2));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                    return;
                }
                // gzip解压
                else if (bytes[1] == 1)
                {
                    try
                    {
                        GZipInputStream gs = new GZipInputStream(new MemoryStream(bytes, 2, bytes.Length - 2));
                        using (MemoryStream re = new MemoryStream())
                        {
                            int count = 0;
                            while ((count = gs.Read(m_gzipReadBytes, 0, m_gzipReadBytes.Length)) != 0)
                            {
                                re.Write(m_gzipReadBytes, 0, count);
                            }

                            m_msgQueue.Enqueue(re.ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                return;
            }
            m_msgQueue.Enqueue(bytes);
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
                return UnityEngine.Network.player.ipAddress.ToString();
            }
        }
    }
}