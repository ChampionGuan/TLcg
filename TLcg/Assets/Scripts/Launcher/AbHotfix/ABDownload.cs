using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace LCG
{
    partial class DownloadTask
    {
        private string remoteUrl;
        private string localFolder;
        private string fileTempPath;
        private string fileLocalPath;
        private long downloadSize;
        private long fileSize;
        private Action<float> downloadProcess;
        private Action<string> downloadSpeed;
        private Action<string> handleError;
        private Action downloadComplete;
        private Thread downloadThread;
        private HttpWebRequest httpWebRequest;
        private UnzipTask unzipTask;

        public float DownloadProcess
        {
            get
            {
                return (float)downloadSize / fileSize;
            }
        }
        public bool IsNeedUnzip
        {
            get
            {
                if (null != unzipTask && unzipTask.UnzipProcess < 1)
                {
                    return true;
                }
                return false;
            }
        }
        public DownloadTask(string _remoteUrl, string _localFolder, string _fileName, string _fileSuffix, long _fileSize, bool _isZipFile = true, long? _unZipSize = null, bool _isDeleteZip = true)
        {
            remoteUrl = _remoteUrl;
            localFolder = _localFolder;
            downloadSize = 0;
            fileSize = _fileSize;
            fileTempPath = string.Format("{0}/{1}{2}", _localFolder, _fileName, ".temp");
            fileLocalPath = string.Format("{0}/{1}{2}", _localFolder, _fileName, _fileSuffix);

            if (_isZipFile)
            {
                unzipTask = new UnzipTask(localFolder, fileLocalPath, _fileName, _fileSize, _unZipSize, _isDeleteZip);
            }
        }
        public void SetAction(Action<string> _error, Action<float> _downloadProcess, Action<string> _downloadSpeed, Action _downloadComplete, Action<float> _unzipProcess, Action _unzipComplete)
        {
            handleError = _error;
            downloadProcess = _downloadProcess;
            downloadSpeed = _downloadSpeed;
            downloadComplete = _downloadComplete;
            SetUnzipAction(_error, _unzipProcess, _unzipComplete);
        }
        public void SetDownloadAction(Action<string> _error, Action<float> _downloadProcess, Action<string> _downloadSpeed, Action _downloadComplete)
        {
            handleError = _error;
            downloadProcess = _downloadProcess;
            downloadSpeed = _downloadSpeed;
            downloadComplete = _downloadComplete;
        }
        public void SetUnzipAction(Action<string> _error, Action<float> _unzipProcess, Action _unzipComplete)
        {
            if (null != unzipTask)
            {
                unzipTask.SetAction(_error, _unzipProcess, _unzipComplete);
            }
        }
        public void Start()
        {
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }
            downloadThread = new Thread(new ThreadStart(DoDownload));
            downloadThread.IsBackground = true;
            downloadThread.Start();
        }
        public void UnZip()
        {
            if (null != unzipTask)
            {
                unzipTask.Start();
            }
        }
        public void Abort()
        {
            if (null != httpWebRequest)
            {
                httpWebRequest.Abort();
            }
            if (null != httpWebRequest)
            {
                httpWebRequest.Abort();
            }
            downloadThread = null;
            httpWebRequest = null;
            unzipTask.Abort();
            FormatDownloadSpeed(0);
        }
        private void DoDownload()
        {
            FileStream fs;
            // 检测是否已经下载一部分
            if (File.Exists(fileTempPath))
            {
                fs = File.OpenWrite(fileTempPath);
                downloadSize = (int)fs.Length;
                if (downloadSize == fileSize)
                {
                    fs.Close();
                    fs.Dispose();
                    DownloadSucceed();
                    return;
                }
                fs.Seek(downloadSize, SeekOrigin.Current);
            }
            else
            {
                fs = new FileStream(fileTempPath, FileMode.Create);
                downloadSize = 0;
            }

            Debug.LogFormat("<color=#20F856>[{0}] 请求下载，服务器地址:{1}, 已下载:{2}</color>", DateTime.Now, remoteUrl, downloadSize);
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(remoteUrl);
            WebResponse response;
            Stream respStream = null;
            bool downloadSuc = true;
            byte[] buffer = null;
            DateTime begin = DateTime.Now;
            TimeSpan ts = new TimeSpan();
            try
            {
                if (downloadSize > 0)
                {
                    if (null != downloadProcess)
                    {
                        downloadProcess(DownloadProcess);
                    }
                    // 从制定位置获取，文件不大于2g（需服务器支持断点续传）
                    httpWebRequest.AddRange((int)downloadSize);
                }

                response = httpWebRequest.GetResponse();
                int bufferLength = (int)response.ContentLength;
                if (bufferLength <= 0)
                {
                    throw new Exception("response zero");
                }

                buffer = new byte[bufferLength];

                int pointOffset = 0;
                int receivedBytesCount = 0;
                int receivedBytesCountPerSec = 0;
                respStream = response.GetResponseStream();
                do
                {
                    // 读取长度
                    receivedBytesCount = respStream.Read(buffer, pointOffset, bufferLength - pointOffset);
                    if (receivedBytesCount > 0)
                    {
                        // 写入文件
                        fs.Write(buffer, pointOffset, receivedBytesCount);
                        pointOffset += receivedBytesCount;
                        downloadSize += receivedBytesCount;
                        receivedBytesCountPerSec += receivedBytesCount;
                        if (null != downloadProcess)
                        {
                            downloadProcess(DownloadProcess);
                        }
                    }

                    // 每隔1秒检测下载速度
                    ts = DateTime.Now - begin;
                    if (ts.TotalSeconds > 1)
                    {
                        FormatDownloadSpeed(receivedBytesCountPerSec);
                        receivedBytesCountPerSec = 0;
                        begin = DateTime.Now;
                    }
                }
                while (receivedBytesCount != 0);

                // 更新过程中网络断开，不会抛出异常，需要手动抛出异常
                if (downloadSize != fileSize)
                {
                    throw new Exception("response length error");
                }
            }
            catch (System.Exception e)
            {
                downloadSuc = false;
                if (null != handleError)
                {
                    handleError("download error :" + e.Message);
                }
            }
            finally
            {
                if (null != fs)
                {
                    fs.Close();
                    fs.Dispose();
                }
                if (null != respStream)
                {
                    respStream.Flush();
                    respStream.Close();
                }
                FormatDownloadSpeed(0);
                // 下载成功
                if (downloadSuc)
                {
                    DownloadSucceed();
                }
            }
        }
        private void DownloadSucceed()
        {
            if (File.Exists(fileTempPath))
            {
                try
                {
                    if (File.Exists(fileLocalPath))
                    {
                        File.Delete(fileLocalPath);
                    }
                    File.Move(fileTempPath, fileLocalPath);
                }
                catch (System.Exception e)
                {
                    Debug.Log("文件移动异常：" + e.Message);
                }
            }
            if (null != downloadComplete)
            {
                downloadComplete();
            }
        }
        private void FormatDownloadSpeed(long bytes)
        {
            string speed = "";
            long g = bytes / (1024 * 1024 * 1024);
            if (g > 1)
            {
                speed = string.Format("{0}G/s", g);
            }
            else
            {
                g = bytes / (1024 * 1024);
                if (g > 1)
                {
                    speed = string.Format("{0}M/s", g);
                }
                else
                {
                    g = bytes / (1024);
                    if (g > 1)
                    {
                        speed = string.Format("{0}K/s", g);
                    }
                    else
                    {
                        speed = string.Format("{0}B/s", bytes);
                    }

                }
            }
            if (null != downloadSpeed)
            {
                downloadSpeed(speed);
            }
        }
        private bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
    }
    public class ABDownload
    {
        // 单例
        private static ABDownload _instance;
        public static ABDownload Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ABDownload();
                }
                return _instance;
            }
        }

        // 下载速度
        public Action<string> downloadSpeed = null;
        // 下载单个进度
        public Action<float> downloadProcess = null;
        // 下载个数进度
        public Action<float> downloadCountProcess = null;
        // 解压进度
        public Action<float> unzipProcess = null;
        // 处理结果
        public Action<bool, string> downloadResult = null;

        // 下载列表
        private Dictionary<string, DownloadTask> downloadTaskList = new Dictionary<string, DownloadTask>();
        // 当前下载任务
        private DownloadTask curDownloadTask = null;
        // 当前解压文件
        private DownloadTask curUnzipTask = null;

        // 已下载个数
        private int downloadedCount = 0;

        // 是否正在下载中
        public bool IsDownloading
        {
            get
            {
                return null != curDownloadTask;
            }
        }
        // 是否正在解压中
        public bool IsUnziping
        {
            get
            {
                return null != curUnzipTask;
            }
        }
        public bool IsPause
        {
            get; private set;
        }
        public ABDownload()
        {
            // http并发数，其实不用设置，因为这里进行单个顺序下载
            ServicePointManager.DefaultConnectionLimit = 512;
        }
        public void InitDownload()
        {
            // 停止正在进行的下载
            PauseDownload();
            // 置空当前处理对象
            downloadTaskList.Clear();
            downloadSpeed = null;
            downloadProcess = null;
            downloadCountProcess = null;
            unzipProcess = null;
            downloadResult = null;
            curDownloadTask = null;
            curUnzipTask = null;
            downloadedCount = 0;
            IsPause = false;
        }
        public bool CreateDownloadTask(string _reomteUrl, string _localFolder, string _fileName, string _fileSuffix, long _fileSize, bool _isZipFile = true, long? _unZipSize = null, bool IsDeleteZip = true)
        {
            if (downloadTaskList.ContainsKey(_reomteUrl))
            {
                return false;
            }

            DownloadTask task = new DownloadTask(_reomteUrl, _localFolder, _fileName, _fileSuffix, _fileSize, _isZipFile, _unZipSize, IsDeleteZip);
            task.SetAction(DownloadError, DownloadProcess, DownloadSpeed, BeginDownload, UnzipProcess, BeginUnzip);

            downloadTaskList.Add(_reomteUrl, task);
            return true;
        }
        public void BeginDownload()
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                if (null != downloadCountProcess)
                {
                    downloadCountProcess(downloadedCount / (float)downloadTaskList.Count);
                }

                downloadedCount++;
                curDownloadTask = null;
                foreach (var task in downloadTaskList.Values)
                {
                    if (task.DownloadProcess < 1)
                    {
                        curDownloadTask = task;
                        curDownloadTask.Start();
                        break;
                    }
                }
                if (null == curDownloadTask)
                {
                    BeginUnzip();
                }
            });
        }
        public void PauseDownload()
        {
            if (null != curDownloadTask)
            {
                IsPause = true;
                curDownloadTask.Abort();
            }
        }
        public void ResumeDownload()
        {
            if (null != curDownloadTask)
            {
                IsPause = false;
                curDownloadTask.Start();
            }
        }
        private void BeginUnzip()
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                curUnzipTask = null;
                foreach (DownloadTask task in downloadTaskList.Values)
                {
                    if (task.IsNeedUnzip)
                    {
                        curUnzipTask = task;
                        curUnzipTask.UnZip();
                        break;
                    }
                }
                // 解压完成后，整个下载就结束了
                if (null == curUnzipTask)
                {
                    if (null != downloadResult)
                    {
                        downloadResult(true, null);
                    }
                }
            });
        }
        private void DownloadSpeed(string netSpeed)
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                if (null != downloadSpeed)
                {
                    downloadSpeed(netSpeed);
                }
            });
        }
        private void DownloadProcess(float process)
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                if (null != downloadProcess)
                {
                    downloadProcess(process);
                }
            });
        }
        private void UnzipProcess(float process)
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                if (null != unzipProcess)
                {
                    unzipProcess(process);
                }
            });
        }
        private void DownloadError(string error)
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                Debug.Log("更新异常！！" + error);
                if (null != downloadResult)
                {
                    downloadResult(false, error);
                }
            });
        }
    }
}
