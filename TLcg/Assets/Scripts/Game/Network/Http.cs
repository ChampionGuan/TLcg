using System.Net;
using System.Text;
using System.IO;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LCG
{
    public class Http : Singleton<Http>, Define.IMonoBase
    {
        /// <summary>
        /// http错误码
        /// </summary>
        public static class ErrorCode
        {
            public static string Unknow = "Unknow";
            public static string Unreachable = "Unreachable";
            public static string ReceiveZero = "ReceiveZero";
            public static string ReceiveUndone = "ReceiveUndone";
            public static string RequstTimeOut = "RequstTimeOut";
        }

        //http方法
        private List<Action> m_httpActions = new List<Action>();

        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        public void Get(string url, Action<string, byte[]> succeedCallBack, Action<string, string> netErrorCallBack)
        {
            string identification = url;
            // 网络不可达
            if (!Network.Instance.NetAvailable)
            {
                netErrorCallBack(identification, ErrorCode.Unreachable);
                return;
            }
            // 开启线程
            Thread thread = new Thread(delegate ()
            {
                // 凭证
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

                HttpWebRequest request = WebRequest.Create(identification) as HttpWebRequest;
                request.Method = "GET";
                request.Timeout = 3000;

                ReadBytesFromResponse(identification, request, succeedCallBack, netErrorCallBack);
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        public void Post(string url, string parameters, Action<string, byte[]> succeedCallBack, Action<string, string> netErrorCallBack)
        {
            string identification = url + "?" + parameters;
            // 网络不可达
            if (!Network.Instance.NetAvailable)
            {
                netErrorCallBack(identification, ErrorCode.Unreachable);
                return;
            }
            // 开启线程
            Thread thread = new Thread(delegate ()
            {
                // 凭证
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                byte[] data = Encoding.ASCII.GetBytes(parameters);
                request.Method = "POST";
                request.Timeout = 3000;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                //发送POST数据  
                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception e)
                {
                    succeedCallBack = null;
                    // 连接超时
                    if (e.Message == "The request timed out")
                    {
                        m_httpActions.Add(() => { netErrorCallBack(identification, ErrorCode.RequstTimeOut); });
                    }
                    else
                    {
                        m_httpActions.Add(() => { netErrorCallBack(identification, e.Message); });
                    }
                    return;
                }
                // 无异常，则进行接收
                if (null != succeedCallBack)
                {
                    ReadBytesFromResponse(identification, request, succeedCallBack, netErrorCallBack);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public void ReadBytesFromResponse(string identification, HttpWebRequest request, Action<string, byte[]> succeedCallBack, Action<string, string> netErrorCallBack)
        {
            WebResponse response = null;
            Stream respStream = null;
            byte[] buffer = null;
            try
            {
                response = request.GetResponse();
                int bufferLength = (int)response.ContentLength;
                if (bufferLength <= 0)
                {
                    throw new Exception(ErrorCode.ReceiveZero);
                }

                long totalSize = bufferLength;
                buffer = new byte[totalSize];

                long readPointer = 0;
                int offset = 0;
                int receivedBytesCount = 0;
                respStream = response.GetResponseStream();
                do
                {
                    //写入
                    receivedBytesCount = respStream.Read(buffer, offset, bufferLength - offset);
                    offset += receivedBytesCount;
                    if (receivedBytesCount > 0)
                    {
                        readPointer += receivedBytesCount;
                    }
                }
                while (receivedBytesCount != 0);

                // 更新过程中网络断开，不会抛出异常，需要手动抛出异常
                if (readPointer != totalSize)
                {
                    throw new Exception(ErrorCode.ReceiveUndone);
                }
            }
            catch (Exception e)
            {
                succeedCallBack = null;
                // 连接超时
                if (e.Message == "The request timed out")
                {
                    m_httpActions.Add(() => { netErrorCallBack(identification, ErrorCode.RequstTimeOut); });
                }
                else if (e.Message == "receiveZero" || e.Message == "receiveUndone")
                {
                    m_httpActions.Add(() => { netErrorCallBack(identification, e.Message); });
                }
                else
                {
                    m_httpActions.Add(() => { netErrorCallBack(identification, e.Message); });
                }
            }
            finally
            {
                if (respStream != null)
                {
                    respStream.Flush();
                    respStream.Close();
                    respStream = null;
                }
            }
            // 接收成功回调
            if (null != succeedCallBack)
            {
                m_httpActions.Add(() =>
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.Log("http接收：" + System.Text.Encoding.Default.GetString(buffer));
#endif
                    succeedCallBack(identification, buffer);
                });
            }
        }

        /// <summary>
        /// 凭证确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }

        /// <summary>
        /// httpGet
        /// </summary>
        public void WWWGet(string url, Action<byte[]> succeedCallBack, Action<string> netErrorCallBack, Action<string> otherErrorCallBack)
        {
            if (!Network.Instance.NetAvailable)
            {
                otherErrorCallBack(ErrorCode.Unreachable);
                return;
            }
            GameEngine.Instance.StartCoroutine(HttpReceiver(url, succeedCallBack, netErrorCallBack, otherErrorCallBack));
        }

        /// <summary>
        /// httpPost
        /// </summary>
        public void WWWPost(string url, string from, Action<byte[]> succeedCallBack, Action<string> netErrorCallBack, Action<string> otherErrorCallBack)
        {
            if (!Network.Instance.NetAvailable)
            {
                otherErrorCallBack(ErrorCode.Unreachable);
                return;
            }
            byte[] form = System.Text.Encoding.ASCII.GetBytes(from);
            GameEngine.Instance.StartCoroutine(HttpReceiver(url, succeedCallBack, netErrorCallBack, otherErrorCallBack, form));
        }

        /// <summary>
        /// HttpReceiver
        /// </summary>
        /// <param name="_url"></param>
        /// <param name="_wForm"></param>
        /// <returns></returns>
        private IEnumerator HttpReceiver(string _url, Action<byte[]> succeedCallBack, Action<string> netErrorCallBack, Action<string> otherErrorCallBack, byte[] _wForm = null)
        {
            WWW receiver;
            if (null != _wForm)
            {
                receiver = new WWW(_url, _wForm);
            }
            else
            {
                receiver = new WWW(_url);
            }

            int time = 0;
            while (true)
            {
                if (receiver.error != null)
                {
#if UNITY_EDITOR
                    Debug.Log("http错误：" + receiver.url + " error:" + receiver.error);
#endif
                    netErrorCallBack(receiver.error);
                    break;
                }
                if (receiver.isDone)
                {
#if UNITY_EDITOR
                    Debug.Log("http接收：" + receiver.text);
#endif
                    if (receiver.text == "")
                    {
                        otherErrorCallBack(ErrorCode.ReceiveZero);
                    }
                    else
                    {
                        succeedCallBack(receiver.bytes);
                    }
                    break;
                }
                // 500帧延时
                if (time >= 500)
                {
                    otherErrorCallBack(ErrorCode.RequstTimeOut);
                    break;
                }

                yield return new WaitForEndOfFrame();
                time++;
            }
        }

        public void CustomUpdate()
        {
            for (int i = 0; i < m_httpActions.Count; i++)
            {
                if (null != m_httpActions[i])
                {
                    m_httpActions[i]();
                }
            }
            m_httpActions.Clear();
        }

        public void CustomFixedUpdate()
        {
        }

        public void CustomDestroy()
        {
            m_httpActions.Clear();
        }

        public void CustomAppFocus(bool focus)
        {
        }
    }
}
