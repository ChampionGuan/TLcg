using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace LCG
{
    public class ABCheck : Singleton<ABCheck>, Define.IMonoBase
    {
        // id3rd
        private int id3rd;
        // 等待调用
        private List<Action> waitInvoke = new List<Action>();
        // 处理进度
        private Action<ABHelper.VersionArgs> onHandleState;

        public void CustomUpdate()
        {
        }
        public void CustomFixedUpdate()
        {
            for (int i = 0; i < waitInvoke.Count; i++)
            {
                if (null != waitInvoke[i])
                {
                    waitInvoke[i]();
                }
            }
            waitInvoke.Clear();
        }
        public void CustomAppFocus(bool focus)
        {
        }
        public void CustomDestroy()
        {
            onHandleState = null;
            ABUnzip.Instance.PauseUnzip();
            ABDownload.Instance.PauseDownload();
        }
        public void EnterInvoke(Action action)
        {
            lock (waitInvoke)
            {
                waitInvoke.Add(action);
            }
        }
        public void Initialize(System.Action callback)
        {
            ABVersion.InitABVersion();
            LauncherEngine.Instance.StartCoroutine(ABLoad.Instance.Init(callback));
        }
        /// <summary>
        /// 首次进入游戏
        /// </summary>
        public bool PrepareAssets(Action<ABHelper.VersionArgs> handleState)
        {
            if (ABVersion.CurVersionId.Id3rd >= ABVersion.OriginalVersionId.Id3rd)
            {
                return false;
            }
            if (ABHelper.IgnoreHotfix)
            {
                return false;
            }

            return false;
        }
        /// <summary>
        /// 检测更新
        /// </summary>
        /// <param name="versionId">资源版号</param>
        /// <param name="versionIdUrl">如果上值为空，则远端获取资源版号</param>
        /// <param name="resRemoteUrl">资源的远端获取地址</param>
        /// <param name="handleState">处理句柄</param>
        public void CheckHotter(string versionId, string versionIdUrl, string resRemoteUrl, Action<ABHelper.VersionArgs> handleState)
        {
            // 处理状态
            onHandleState = handleState;
            // 资源远端地址
            ABVersion.RemoteUrlPrefix = resRemoteUrl;
            // 客户端初始版号
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.OriginalVersionId, ABVersion.OriginalVersionId.Id));
            // 检测本地资源
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.CheckLocalVersion));
            // 客户端版本
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.ClientVersionId, ABVersion.CurVersionId.Id));

            if (!string.IsNullOrEmpty(versionId))
            {
                LauncherEngine.Instance.StartCoroutine(CheckHotter(versionId));
            }
            else
            {
                // 网络不可达检测
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.Unreachable));
                    return;
                }

                // 请求服务器版号
                LauncherEngine.Instance.StartCoroutine(HttpReceiver(versionIdUrl, (str) =>
                {
                    onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.AckServerVersionId, str, (versionId2) =>
                    {
                        // 1.1.1.1 https://male7client-1256076575.cos.ap-guangzhou.myqcloud.com/male7ab
                        string[] res = versionId2.Split(' ');
                        // 资源远端地址
                        ABVersion.RemoteUrlPrefix = res[1];
                        // 检测热更
                        LauncherEngine.Instance.StartCoroutine(CheckHotter(res[0]));
                    }));
                }, (str) =>
                {
                    HotterResult(false, str);
                }));
            }
        }
        /// <summary>
        /// 检测apk
        /// </summary>
        /// <param name="remoteUrl"></param>
        public void CheckApk(string remoteUrl, string apkName)
        {
            // 网络不可达检测
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.Unreachable));
                return;
            }
            LauncherEngine.Instance.StartCoroutine(ShowApkDownloadInfo(remoteUrl, apkName));
        }
        /// <summary>
        /// 是否需要去商店更新
        /// </summary>
        /// <param name="resNum"></param>
        /// <returns></returns>
        public bool IsNeedGoStore(string versionNum = null)
        {
            if (ABHelper.IgnoreHotfix)
            {
                return false;
            }
            VersionNum versionId;
            if (null == versionNum)
            {
                versionId = ABVersion.ServerVersionId;
            }
            else
            {
                versionId = new VersionNum(versionNum);
            }
            // 前两位版本号不等于本地发布的版本号，那么无需更新，直接去store重新下载包吧！
            if (versionId.Id1A2 != ABVersion.OriginalVersionId.Id1A2)
            {
                return true;
            }
            // 所需版本号小于本地最小版号
            if (versionId.Id3rd < ABVersion.OriginalVersionId.Id3rd)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 是否需要热更新
        /// </summary>
        /// <param name="resNum"></param>
        /// <returns></returns>
        public bool IsNeedHotter(string versionNum = null)
        {
            if (ABHelper.IgnoreHotfix)
            {
                return false;
            }
            VersionNum versionId;
            if (null == versionNum)
            {
                versionId = ABVersion.ServerVersionId;
            }
            else
            {
                versionId = new VersionNum(versionNum);
            }
            // 本地已更新到服务器给到的版本号资源
            if (ABVersion.LocalVersionList.ContainsKey(versionId.Id3rd))
            {
                Debug.Log("无需更新!!服务器资源版本为:" + ABVersion.ServerVersionId.Id3rd + "  本地资源版本为:" + ABVersion.CurVersionId.Id3rd);
                return false;
            }
            return true;
        }
        private void HotterVersionNum()
        {
            id3rd = ABVersion.ServerVersionId.Id3rd - 1;

            while (true)
            {
                if (id3rd <= ABVersion.OriginalVersionId.Id3rd)
                {
                    break;
                }
                if (id3rd == ABVersion.CurVersionId.Id3rd)
                {
                    break;
                }
                else
                {
                    // 检测到客户端已更新到的版本
                    if (ABVersion.LocalVersionList.ContainsKey(id3rd))
                    {
                        break;
                    }
                }
                id3rd--;
            }
        }
        /// <summary>
        /// HttpReceiver
        /// </summary>
        /// <param name="_url"></param>
        /// <param name="_wForm"></param>
        /// <returns></returns>
        private IEnumerator HttpReceiver(string _url, Action<string> onSucceed, Action<string> onError, byte[] _wForm = null)
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
                    onError(receiver.error);
                    break;
                }
                if (receiver.isDone)
                {
                    if (receiver.text == "")
                    {
                        onError("receiveZero");
                    }
                    else
                    {
                        onSucceed(receiver.text);
                    }
                    break;
                }
                // 500帧延时
                if (time >= 500)
                {
                    onError("timeout");
                    break;
                }

                yield return new WaitForEndOfFrame();
                time++;
            }
        }
        private IEnumerator CheckHotter(string serverVersionId)
        {
            // 设置服务器版号
            ABVersion.ServerVersionId = new VersionNum(serverVersionId);
            // 检测本地结束
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.CheckLocalVersionOver));

            // 重新下载大版本
            if (IsNeedGoStore())
            {
                // Debug.Log("去商店吧！！更新大包！！");
                // 弹出一个面板
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.NeedGoStore));
                yield break;
            }

            // 检测是否需要热更
            if (IsNeedHotter())
            {
                // 网络不可达检测
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.Unreachable));
                    yield break;
                }
                // 客户端最新版本
                HotterVersionNum();
                // 开始热更
                LauncherEngine.Instance.StartCoroutine(ShowHotterDownloadInfo());
                yield break;
            }

            HotfixComplete();
            yield return null;
        }
        private IEnumerator ShowApkDownloadInfo(string remoteUrl, string apkName)
        {
            Debug.Log("路径：" + remoteUrl + apkName + ".ini");
            WWW www = new WWW(remoteUrl + apkName + ".ini");
            yield return www;

            // 下载异常处理
            if (!string.IsNullOrEmpty(www.error))
            {
                APKDownloadResult(false, www.error);
                yield break;
            }

            long fileSize = long.Parse(www.text);
            // Debug.LogFormat("需要下载文件的大小:{0}", fileSize);

            // 文件已下载
            if (File.Exists(ABHelper.ApkLocalPath + apkName + ".apk"))
            {
                FileInfo file = new FileInfo(ABHelper.ApkLocalPath + apkName + ".apk");
                if (file.Length == fileSize)
                {
                    APKDownloadResult(true);
                    yield break;
                }
            }

            // 下载弹框确认
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadAPKConfirm, fileSize, (str) =>
            {
                Action download = () =>
                {
                    // 初始进度
                    DownloadProcess(0);

                    // 初始化
                    ABDownload.Instance.InitDownload();
                    // 注册下载事件
                    ABDownload.Instance.downloadProcess = DownloadProcess;
                    ABDownload.Instance.downloadSpeed = DownloadSpeed;
                    ABDownload.Instance.downloadResult = APKDownloadResult;
                    ABDownload.Instance.CreateDownloadTask(remoteUrl + apkName + ".apk", ABHelper.ApkLocalPath, apkName, ".apk", fileSize, false);
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
        private IEnumerator ShowHotterDownloadInfo()
        {
            // 需要下载文件的大小
            string httpFileName = string.Format("{0}-{1}", id3rd, ABVersion.ServerVersionId.Id3rd);
            string httpFileNameWithSuffix = string.Format("{0}.ini", httpFileName);
            Debug.Log("路径：" + ABVersion.RemoteUrlHotter + httpFileNameWithSuffix);
            WWW www = new WWW(ABVersion.RemoteUrlHotter + httpFileNameWithSuffix);
            yield return www;

            // 下载异常处理
            if (!string.IsNullOrEmpty(www.error))
            {
                // Debug.LogFormat("sizeInfo 文件下载失败：", www.error);
                HotterResult(false, www.error);
                yield break;
            }

            // zipsize:10670:11727
            string[] split = www.text.Split(':');
            long zipFileSize = long.Parse(split[1]);
            long unzipFileSize = long.Parse(split[2]);
            // Debug.LogFormat("需要下载文件的大小:{0}，原文件大小:{1}", zipFileSize, unzipFileSize);

            // 下载弹框确认
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadConfirm, zipFileSize, (str) =>
            {
                Action download = () =>
                {
                    // 初始进度
                    DownloadProcess(0);

                    // 初始化
                    ABDownload.Instance.InitDownload();
                    // 注册下载事件
                    ABDownload.Instance.downloadProcess = DownloadProcess;
                    ABDownload.Instance.downloadSpeed = DownloadSpeed;
                    ABDownload.Instance.downloadResult = HotterResult;
                    ABDownload.Instance.unzipProcess = UnzipProcess;

                    // 下载zip
                    string zipFileSuffix = ".zip";
                    // 和上面文件同名，后缀不同
                    string zipFileName = httpFileName;
                    string zipFileRemoteUrl = string.Format("{0}{1}{2}", ABVersion.RemoteUrlHotter, zipFileName, zipFileSuffix);

                    //"http://7.lightpaw.com:18080/ab_test/Win/v0.0//HotterZip/0-1.zip"
                    //"C:/Users/guangzhua001/AppData/LocalLow/DefaultCompany/hotter/version/v0.0"
                    //"0-1"
                    //".zip"
                    //"0"
                    ABDownload.Instance.CreateDownloadTask(zipFileRemoteUrl, ABVersion.LocalStorgePath, zipFileName, zipFileSuffix, zipFileSize, true, unzipFileSize);
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
        private void HotfixComplete()
        {
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.ABInit));
            // 设置当前使用的版本
            ABVersion.SetCursVersionNum(ABVersion.ServerVersionId);
            // 当前版本
            onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.ServerVersionId, ABVersion.CurVersionId.Id));

            // Debug.Log("热更完成，下面进行ab初始化！！");
            LauncherEngine.Instance.StartCoroutine(ABLoad.Instance.Init(() =>
            {
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.HotfixComplete));
                onHandleState = null;
            }));
        }
        private void HotterResult(bool result, string error = null)
        {
            if (result)
            {
                // 热更结束
                HotfixComplete();
            }
            else
            {
                // Debug.Log("资源更新异常！！" + error);
                // todo 异常，弹框再试一次，或者退出app
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.UnknowError, error));
                onHandleState = null;
            }
        }
        private void APKDownloadResult(bool result, string error = null)
        {
            if (result)
            {
                // apk下载成功
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.APKDownloadComplete));
            }
            else
            {
                // Debug.Log("资源更新异常！！" + error);
                // todo 异常，弹框再试一次，或者退出app
                onHandleState(new ABHelper.VersionArgs(ABHelper.EVersionState.UnknowError, error));
                onHandleState = null;
            }
        }
        private ABHelper.VersionArgs netSpeed = new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadSpeed);
        private void DownloadSpeed(string s)
        {
            // Debug.Log("下载速度：" + s);
            netSpeed.sValue = s;
            onHandleState(netSpeed);
        }
        private ABHelper.VersionArgs downloadProgress = new ABHelper.VersionArgs(ABHelper.EVersionState.DownloadProgress);
        private void DownloadProcess(float p)
        {
            // Debug.Log("下载进度：" + p);
            downloadProgress.fValue = p;
            onHandleState(downloadProgress);
        }
        private ABHelper.VersionArgs unzipProgress = new ABHelper.VersionArgs(ABHelper.EVersionState.UnzipProgress);
        private void UnzipProcess(float p)
        {
            // Debug.Log("解压进度：" + p);
            unzipProgress.fValue = p;
            onHandleState(unzipProgress);
        }
        private ABHelper.VersionArgs movefileProgress = new ABHelper.VersionArgs(ABHelper.EVersionState.MovefileProcess);
        private void MovefileProcess(float p)
        {
            // Debug.Log("准备进度：" + p);
            movefileProgress.fValue = p;
            onHandleState(movefileProgress);
        }
    }
}
