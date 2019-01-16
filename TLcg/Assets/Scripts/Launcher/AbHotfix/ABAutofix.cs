using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace LCG
{
    public class ABAutofix : Singleton<ABAutofix>
    {
        // 处理进度
        private Action<ABHelper.VersionArgs> onHandleState;
        // 需要删除的文件夹
        private List<string> deleteDirectorys;
        // 需要删除的文件
        private List<string> deleteFiles;
        // 异常列表
        private Dictionary<string, List<string>> exceptionList;
        // 版本文件名称
        private List<string> versionFileName = null;
        // 下载文件大小
        private long downloadSize = 0;
        // 是否为深度修复
        private bool isDeepFix = false;

        public void CustomDestroy()
        {
            onHandleState = null;
            ABDownload.Instance.PauseDownload();
        }
        // severeFix：重度修复
        // handleState：处理句柄
        public void Repair(bool isDeep, Action<ABHelper.VersionArgs> handleState)
        {
            // 保存修复类型
            isDeepFix = isDeep;
            // 处理状态
            onHandleState = handleState;
            // 客户端版本
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.ClientVersionId, ABVersion.CurVersionId.Id));
            // 和本地发包时的版号一致，不许更新
            if (ABVersion.CurVersionId.Id3rd == ABVersion.OriginalVersionId.Id3rd)
            {
                // 检测完成
                FixResult(true);
            }
            else
            {
                // 网络不可达检测
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.Unreachable));
                    return;
                }
                LauncherEngine.Instance.StartCoroutine(DownloadVersion());
            }
        }
        private IEnumerator DownloadVersion()
        {
            string path = string.Format("/{0}/{1}", ABVersion.CurVersionId.Id3rd.ToString(), ABHelper.VersionFileName);
            WWW www = new WWW(ABVersion.RemoteUrlPlatform + path);
            yield return www;

            // 下载异常处理
            if (null != www.error)
            {
                // Debug.LogFormat("sizeInfo 文件下载失败：", www.error);
                FixResult(false, www.error);
                yield break;
            }

            // 更新version文件
            ABHelper.WriteFileByBytes(ABVersion.LocalStorgePath + path, www.bytes);
            ABVersion.CurVersionInfo.ParseVersionList();

            // 异步读取异常文件
            LauncherEngine.Instance.StartCoroutine(GetExceptionList());
        }
        private IEnumerator GetExceptionList()
        {
            downloadSize = 0;
            exceptionList = new Dictionary<string, List<string>>();
            versionFileName = new List<string>(ABVersion.CurVersionInfo.VersionInfoList.Keys);

            int index = 0;
            string key = "";
            string abFullPath = "";
            float count = versionFileName.Count;
            while (index < versionFileName.Count)
            {
                key = versionFileName[index];
                List<string> value = ABVersion.CurVersionInfo.VersionInfoList[key];
                if (int.Parse(value[1]) > ABVersion.OriginalVersionId.Id3rd)
                {
                    abFullPath = ABVersion.CurVersionInfo.GetABFullPath(key);
                    if (ABHelper.BuildMD5ByFile(abFullPath) != value[0])
                    {
                        exceptionList.Add(key, value);
                        downloadSize += long.Parse(value[2]);
                    }
                }

                CheckFileProcess(++index / count);
                yield return null;
            }

            //判断是否存在异常
            if (exceptionList.Count > 0)
            {
                DownloadExceptionFile();
            }
            else
            {
                FixResult(true);
            }
            yield return null;
        }
        private void DownloadExceptionFile()
        {
            // 下载弹框确认
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadConfirm, (float)downloadSize, (str) =>
            {
                Action download = () =>
                {
                    // 初始化
                    ABDownload.Instance.InitDownload();
                    foreach (var pair in exceptionList)
                    {
                        // 注册下载事件
                        ABDownload.Instance.downloadResult = FixResult;
                        ABDownload.Instance.downloadCountProcess = DownloadPorcess;

                        string relativePath = ABVersion.CurVersionInfo.GetAbRelativePath(pair.Key);
                        string fileSuffix = relativePath.Contains(".") ? relativePath.Substring(relativePath.LastIndexOf(".")) : "";
                        string fileName = ABHelper.GetFileNameWithoutSuffix(relativePath);
                        string remoteUrl = string.Format("{0}{1}", ABVersion.RemoteUrlPlatform, relativePath);
                        string localUrl = string.Format("{0}/{1}", ABVersion.LocalStorgePath, ABHelper.GetFileFolderPath(relativePath));

                        ABDownload.Instance.CreateDownloadTask(remoteUrl, localUrl, fileName, fileSuffix, int.Parse(pair.Value[2]), false);
                    }
                    ABDownload.Instance.BeginDownload();
                };
                // 检测是否为移动数据
                if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    // wifi状态检测
                    onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadWifi, (str2) =>
                    {
                        download();
                    }
                    ));
                }
                else
                {
                    download();
                }
            }));
        }
        // 收集冗余文件夹
        private void CollectRedundantDirectorys()
        {
            // 获取本地的所有版本
            DirectoryInfo dirInfo = new DirectoryInfo(ABHelper.AppTempCachePath);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
            foreach (DirectoryInfo info in dirInfos)
            {
                if (info.Name != ABVersion.OriginalVersionId.Id1A2)
                {
                    if (!deleteDirectorys.Contains(info.FullName))
                    {
                        deleteDirectorys.Add(info.FullName);
                    }
                }
            }
        }
        // 收集冗余文件
        private void CollectRedundantFiles()
        {
            List<string> allFilePath = new List<string>();

            List<string> versionFilePath = ABHelper.GetAllFilesPathInDir(ABVersion.LocalStorgePath);
            foreach (string path in versionFilePath)
            {
                allFilePath.Add(path.Replace("\\", "/"));
            }

            List<string> imageFilePath = ABHelper.GetAllFilesPathInDir(ABHttpImg.LocalStorgePath);
            foreach (string path in imageFilePath)
            {
                allFilePath.Add(path.Replace("\\", "/"));
            }

            List<string> needFilePath = new List<string>();
            if (null != ABVersion.CurVersionInfo)
            {
                needFilePath.Add(ABVersion.CurVersionInfo.VersionFilePath);
                foreach (var value in ABVersion.CurVersionInfo.VersionInfoList)
                {
                    if (int.Parse(value.Value[1]) <= ABVersion.OriginalVersionId.Id3rd)
                    {
                        continue;
                    }
                    string path = ABVersion.CurVersionInfo.GetABFullPath(value.Key);
                    if (!string.IsNullOrEmpty(path))
                    {
                        needFilePath.Add(path);
                    }
                }
            }

            foreach (var value in allFilePath)
            {
                if (!needFilePath.Contains(value))
                {
                    deleteFiles.Add(value);
                }
            }

            List<int> versionList = new List<int>(ABVersion.LocalVersionList.Keys);
            foreach (var value in versionList)
            {
                if (value != ABVersion.ServerVersionId.Id3rd)
                {
                    ABVersion.LocalVersionList.Remove(value);
                }
            }
        }
        // 删除冗余数据
        private IEnumerator DeleteRedundantInfo()
        {
            int deleteCount = 0;
            float allCount = deleteDirectorys.Count + deleteFiles.Count;

            while (deleteDirectorys.Count > 0)
            {
                Directory.Delete(deleteDirectorys[0], true);
                deleteDirectorys.RemoveAt(0);
                deleteCount++;
                DeletePorcess(deleteCount / allCount);
                yield return null;
            }

            while (deleteFiles.Count > 0)
            {
                File.Delete(deleteFiles[0]);
                deleteFiles.RemoveAt(0);
                deleteCount++;
                DeletePorcess(deleteCount / allCount);
                yield return null;
            }

            FixComplete();
            yield return null;
        }
        private void FixComplete()
        {
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.ABInit));

            // Debug.Log("修复完成，下面进行ab初始化！！");
            LauncherEngine.Instance.StartCoroutine(ABLoad.Instance.Init(() =>
            {
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.AutofixComplete));
                onHandleState = null;
            }));
        }
        private void FixResult(bool result, string error = null)
        {
            if (result)
            {
                deleteFiles = new List<string>();
                deleteDirectorys = new List<string>();
                if (!isDeepFix)
                {
                    CollectRedundantDirectorys();
                    LauncherEngine.Instance.StartCoroutine(DeleteRedundantInfo());
                }
                else
                {
                    CollectRedundantDirectorys();
                    CollectRedundantFiles();
                    LauncherEngine.Instance.StartCoroutine(DeleteRedundantInfo());
                }
            }
            else
            {
                // Debug.Log("资源更新异常！！" + error);
                // todo 异常，弹框再试一次，或者退出app
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.UnknowError, error));
                onHandleState = null;
            }
        }
        private ABHelper.VersionArgs checkFileProcess = new ABHelper.VersionArgs(ABHelper.EVersionState.CheckFileProgress);
        private void CheckFileProcess(float p)
        {
            //Debug.Log("检测进度：" + p);
            checkFileProcess.fValue = p;
            onHandleState(checkFileProcess);
        }
        private ABHelper.VersionArgs downloadPorcess = new ABHelper.VersionArgs(ABHelper.EVersionState.AutofixProgress);
        private void DownloadPorcess(float p)
        {
            //Debug.Log("修复进度：" + p);
            downloadPorcess.fValue = p;
            onHandleState(downloadPorcess);
        }
        private ABHelper.VersionArgs deletePorcess = new ABHelper.VersionArgs(ABHelper.EVersionState.AutofixDeleteFile);
        private void DeletePorcess(float p)
        {
            //Debug.Log("删除进度：" + p);
            deletePorcess.fValue = p;
            onHandleState(deletePorcess);
        }
    }
}
