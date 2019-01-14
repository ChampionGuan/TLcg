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
        private string fileSuffix;
        private string fileTempPath;
        private int downloadSize;
        private Thread downloadThread;
        private Thread unzipThread;
        private HttpWebRequest httpWebRequest;
        public string FileName
        {
            get; private set;
        }
        public string FilePath
        {
            get; private set;
        }
        public long FileWholeSize
        {
            get; private set;
        }
        public long? UnZipWholeSize
        {
            get; private set;
        }
        public float DownloadProcess
        {
            get
            {
                return (float)downloadSize / FileWholeSize;
            }
        }
        public float UnzipProcess
        {
            get; private set;
        }
        public Action<float> OnUnzipProcess
        {
            private get; set;
        }
        public Action<float> OnDownloadProcess
        {
            private get; set;
        }
        public Action<string> OnDownloadSpeed
        {
            private get; set;
        }
        public Action<string> OnHandleError
        {
            private get; set;
        }
        public Action OnDownloadComplete
        {
            private get; set;
        }
        public Action OnUnzipComplete
        {
            private get; set;
        }
        public bool IsZipFile
        {
            get; private set;
        }
        public DownloadTask(string _remoteUrl, string _localFolder, string _fileName, string _fileSuffix, long _fileSize, bool _isZipFile = true, long? _unZipSize = null)
        {
            downloadSize = 0;
            IsZipFile = _isZipFile;
            remoteUrl = _remoteUrl;
            localFolder = _localFolder;
            fileSuffix = _fileSuffix;
            FileName = _fileName;
            FileWholeSize = _fileSize;
            UnZipWholeSize = _unZipSize;
            fileTempPath = _localFolder + "/" + _fileName + ".temp";
            FilePath = _localFolder + "/" + _fileName + _fileSuffix;
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
            unzipThread = new Thread(new ThreadStart(DoUnZip));
            unzipThread.IsBackground = true;
            unzipThread.Start();
        }
        public void Abort()
        {
            if (null != unzipThread)
            {
                unzipThread.Abort();
            }
            if (null != httpWebRequest)
            {
                httpWebRequest.Abort();
            }
            if (null != httpWebRequest)
            {
                httpWebRequest.Abort();
            }
            unzipThread = null;
            downloadThread = null;
            httpWebRequest = null;
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
                if (downloadSize == FileWholeSize)
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
                    if (null != OnDownloadProcess)
                    {
                        OnDownloadProcess(DownloadProcess);
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
                        if (null != OnDownloadProcess)
                        {
                            OnDownloadProcess(DownloadProcess);
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
                if (downloadSize != FileWholeSize)
                {
                    throw new Exception("response length error");
                }
            }
            catch (System.Exception e)
            {
                downloadSuc = false;
                if (null != OnHandleError)
                {
                    OnHandleError("download error :" + e.Message);
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
                    if (File.Exists(FilePath))
                    {
                        File.Delete(FilePath);
                    }
                    File.Move(fileTempPath, FilePath);
                }
                catch (System.Exception e)
                {
                    Debug.Log("文件移动异常：" + e.Message);
                }
            }
            if (null != OnDownloadComplete)
            {
                OnDownloadComplete();
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
            if (null != OnDownloadSpeed)
            {
                OnDownloadSpeed(speed);
            }
        }
        private void DoUnZip()
        {
            if (fileSuffix != ".zip")
            {
                Debug.Log("该文件不是zip文件，当前仅支持下载zip文件解压！！");
                return;
            }
            if (!File.Exists(FilePath))
            {
                if (null != OnUnzipComplete)
                {
                    OnUnzipComplete();
                }
                return;
            }
            FileStream fs = File.OpenRead(FilePath);
            ZipInputStream _zipInputStream = new ZipInputStream(fs);
            ZipEntry theEntry = null;

            bool unzipsuc = true;
            long unzipSize = 0;
            float zipfilesize = null == UnZipWholeSize ? (float)FileWholeSize : (float)UnZipWholeSize; // fs.Length;
            string tempFolder = localFolder + "/Temp/";
            string releaseFolder = localFolder + "/";
            string errorMsg = "";
            List<string> decompressfiles = new List<string>();

            Debug.Log("<color=#20F856>" + "解压文件: " + FilePath + " ----- fileName:" + FileName + " ---- zipfie size:" + zipfilesize + "</color>");

            while (true)
            {
                try
                {
                    theEntry = _zipInputStream.GetNextEntry();
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                    unzipsuc = false;
                    break;
                }
                if (theEntry == null)
                {
                    break;
                }
                if (theEntry.Name.EndsWith("/"))
                {
                    continue;
                }

                // 文件存放位置
                string fileName = tempFolder + theEntry.Name;
                string directoryName = ABHelper.GetFileFolderPath(fileName);
                decompressfiles.Add(fileName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // 写入文件
                FileStream sw = File.Create(fileName);
                int readSize = 0;
                byte[] data = new byte[2048];
                while (true)
                {
                    if (theEntry.Size == 0)
                    {
                        break;
                    }
                    readSize = _zipInputStream.Read(data, 0, data.Length);
                    unzipSize += readSize;
                    if (readSize > 0)
                    {
                        UnzipProcess = unzipSize / zipfilesize;
                        if (null != OnUnzipProcess)
                        {
                            OnUnzipProcess(UnzipProcess);
                        }
                        sw.Write(data, 0, readSize);
                    }
                    else
                    {
                        break;
                    }
                }
                sw.Close();
                sw.Dispose();
            }

            fs.Close();
            fs.Dispose();
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            // 解压成功
            if (unzipsuc)
            {
                UnzipProcess = 1;
                foreach (var upfile in decompressfiles)
                {
                    string newfile = upfile.Replace(tempFolder, releaseFolder);
                    if (File.Exists(newfile))
                    {
                        File.Delete(newfile);
                    }
                    string dir = Path.GetDirectoryName(newfile);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.Move(upfile, newfile);
                }
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
                if (null != OnUnzipComplete)
                {
                    OnUnzipComplete();
                }
            }
            else
            {
                if (null != OnHandleError)
                {
                    OnHandleError("unzip error :" + errorMsg);
                }
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
        public bool CreateDownloadTask(string _reomteUrl, string _localFolder, string _fileName, string _fileSuffix, long _fileSize, bool _isZipFile = true, long? _unZipSize = null)
        {
            if (downloadTaskList.ContainsKey(_reomteUrl))
            {
                return false;
            }

            DownloadTask task = new DownloadTask(_reomteUrl, _localFolder, _fileName, _fileSuffix, _fileSize, _isZipFile, _unZipSize);
            task.OnHandleError = HotterError;
            task.OnDownloadComplete = BeginDownload;
            task.OnUnzipComplete = BeginUnzip;
            task.OnDownloadSpeed = DownloadSpeed;
            task.OnDownloadProcess = DownloadProcess;
            task.OnUnzipProcess = UnzipProcess;

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
                foreach (DownloadTask task in downloadTaskList.Values)
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
                    if (task.IsZipFile && task.UnzipProcess < 1)
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
        private void HotterError(string error)
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
