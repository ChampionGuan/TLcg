using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace LCG
{
    partial class UnzipTask
    {
        private string localFolder;
        private Thread unzipThread;
        private bool isDeleteZip;
        private string zipFileName;
        private string zipFilePath;
        private long? zipFileSize;
        private long? unzipFileSize;
        private FileStream fileStream;
        private Action<float> unzipProcess;
        private Action<string> handleError;
        private Action unzipComplete;

        public float UnzipProcess
        {
            get; private set;
        }
        public UnzipTask(string _localFolder, string _zipFilePath, string _zipFileName, long? _zipFileSize = null, long? _unZipFileSize = null, bool _isDeleteZip = true)
        {
            localFolder = _localFolder;
            isDeleteZip = _isDeleteZip;
            zipFileName = _zipFileName;
            zipFileSize = _zipFileSize;
            unzipFileSize = _unZipFileSize;
            zipFilePath = _zipFilePath;
        }
        public void SetAction(Action<string> _error, Action<float> _unzipProcess, Action _unzipComplete)
        {
            handleError = _error;
            unzipProcess = _unzipProcess;
            unzipComplete = _unzipComplete;
        }
        public void Start()
        {
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }
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
            if (null != fileStream)
            {
                fileStream.Flush();
                fileStream.Close();
            }
            unzipThread = null;
        }
        private void DoUnZip()
        {
            if (!File.Exists(zipFilePath))
            {
                if (null != unzipComplete)
                {
                    unzipComplete();
                }
                return;
            }
            fileStream = File.OpenRead(zipFilePath);
            ZipInputStream _zipInputStream = new ZipInputStream(fileStream);
            ZipEntry theEntry = null;
            zipFileSize = null == zipFileSize ? fileStream.Length : zipFileSize;

            bool unzipsuc = true;
            long unzipSize = 0;
            float zipfilesize = null == unzipFileSize ? (float)zipFileSize : (float)unzipFileSize; // fileStream.Length;
            string releaseFolder = localFolder + "/";
            string errorMsg = "";
            List<string> decompressfiles = new List<string>();

            Debug.Log("<color=#20F856>" + "解压文件: " + zipFilePath + " ----- fileName:" + zipFileName + " ---- zipfie size:" + zipfilesize + "</color>");

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
                string fileName = releaseFolder + theEntry.Name;
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
                        if (null != unzipProcess)
                        {
                            unzipProcess(UnzipProcess);
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

            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            // 删除源文件
            if (isDeleteZip && File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            // 解压成功
            if (unzipsuc)
            {
                UnzipProcess = 1;
                if (null != unzipComplete)
                {
                    unzipComplete();
                }
            }
            else
            {
                if (null != handleError)
                {
                    handleError("unzip error :" + errorMsg);
                }
            }
        }
    }
    public class ABUnzip
    {
        // 单例
        private static ABUnzip _instance;
        public static ABUnzip Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ABUnzip();
                }
                return _instance;
            }
        }

        // 解压个数进度
        public Action<float, int> unzipCountProcess = null;
        // 解压进度
        public Action<float> unzipProcess = null;
        // 处理结果
        public Action<bool, string> unzipResult = null;

        // 解压列表
        private Dictionary<string, UnzipTask> unzipTaskList = new Dictionary<string, UnzipTask>();
        // 当前解压文件
        private UnzipTask curUnzipTask = null;

        // 已解压个数
        private int unzipCount = 0;
        // 压缩文件个数
        private int zipCount = 0;
        // 压缩区间
        private float zipZone = 0;


        // 是否正在解压中
        public bool IsUnziping
        {
            get
            {
                return null != curUnzipTask;
            }
        }
        public void InitUnzip()
        {
            // 置空当前处理对象
            unzipTaskList.Clear();
            unzipCountProcess = null;
            unzipProcess = null;
            unzipResult = null;
            curUnzipTask = null;
            unzipCount = 0;
            zipCount = 0;
        }
        public bool CreateUnzipTask(string _localFolder, string _zipFilePath, string _zipFileName, long? _zipFileSize = null, long? _unZipFileSize = null, bool _isDeleteZip = true)
        {
            if (unzipTaskList.ContainsKey(_zipFilePath))
            {
                return false;
            }

            UnzipTask task = new UnzipTask(_localFolder, _zipFilePath, _zipFileName, _zipFileSize, _unZipFileSize, _isDeleteZip);
            task.SetAction(UnzipError, UnzipProcess, BeginUnzip);
            unzipTaskList.Add(_zipFilePath, task);
            zipCount = unzipTaskList.Count;
            zipZone = (int)(1 / (float)zipCount * 10000) * 0.0001f;

            return true;
        }
        public void BeginUnzip()
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                unzipCount++;
                curUnzipTask = null;
                unzipprocess = -1;
                UnzipCountProcess(unzipCount);
                foreach (var task in unzipTaskList.Values)
                {
                    if (task.UnzipProcess < 1)
                    {
                        curUnzipTask = task;
                        curUnzipTask.Start();
                        break;
                    }
                }
                // 解压完成
                if (null == curUnzipTask)
                {
                    if (null != unzipResult)
                    {
                        unzipResult(true, null);
                    }
                }
            });
        }
        public void PauseUnzip()
        {
            if (null != curUnzipTask)
            {
                curUnzipTask.Abort();
            }
        }
        public void ResumeUnzip()
        {
            if (null != curUnzipTask)
            {
                curUnzipTask.Start();
            }
        }
        private void UnzipCountProcess(int process)
        {
            if (null != unzipCountProcess)
            {
                unzipCountProcess(process * zipZone, zipCount);
            }
        }
        private float unzipprocess = 0;
        private void UnzipProcess(float process)
        {
            process = (int)(process * 100) * 0.01f;
            if (unzipprocess == process)
            {
                return;
            }
            unzipprocess = process;
            ABCheck.Instance.EnterInvoke(() =>
            {
                if (null != unzipProcess)
                {
                    unzipProcess((unzipCount + unzipprocess - 1) * zipZone);
                }
            });
        }
        private void UnzipError(string error)
        {
            ABCheck.Instance.EnterInvoke(() =>
            {
                Debug.Log("更新异常！！" + error);
                if (null != unzipResult)
                {
                    unzipResult(false, error);
                }
            });
        }
    }
}
