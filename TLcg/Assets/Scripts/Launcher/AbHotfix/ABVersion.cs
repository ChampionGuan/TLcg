using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System;

namespace LCG
{
    public class VersionNum
    {
        // 版号
        // 第一位表示大版本号，比如0号一般代表研发中，1代表正式上线，2海外版本，等等…
        // 第二位表示cs、dll脚本更改后的增量id
        // 第三位表示生成的热更增量id
        // 第四位一般是svn、git、perforce等资源管理软件上的提交号，用来查错的
        public string Id { get; private set; }
        /// <summary>
        /// 版号列表
        /// </summary>
        public string[] IdArray { get; private set; }
        /// <summary>
        /// 前两位版号
        /// </summary>
        public string Id1A2 { get; private set; }
        /// <summary>
        /// 前三位版号
        /// </summary>
        public string Id1A2A3 { get; private set; }
        /// <summary>
        /// 第一位版号
        /// </summary>
        public string Id1st { get; private set; }
        /// <summary>
        /// 第二位版号
        /// </summary>
        public int Id2nd { get; private set; }
        /// <summary>
        /// 第三位版号
        /// </summary>
        public int Id3rd { get; private set; }
        /// <summary>
        /// 第四位版号
        /// </summary>
        public int Id4th { get; private set; }

        public VersionNum(string v)
        {
            Update(v);
        }
        public VersionNum(string v1, int v2, int v3, int? v4 = null)
        {
            Update(v1, v2, v3, null == v4 ? 0 : (int)v4);
        }
        public void Update(string v)
        {
            Id = v;
            IdArray = ABHelper.VersionNumSplit(Id);
            Id1A2 = string.Format("{0}.{1}", IdArray[0], IdArray[1]);
            Id1A2A3 = string.Format("{0}.{1}", Id1A2, IdArray[2]);
            Id1st = IdArray[0];
            Id2nd = int.Parse(IdArray[1]);
            Id3rd = int.Parse(IdArray[2]);
            Id4th = int.Parse(IdArray[3]);
        }
        public void Update(string[] v)
        {
            Id = string.Format("{0}.{1}.{2}.{3}", v[0], v[1], v[2], v[3]);
            IdArray = v;
            Id1A2 = string.Format("{0}.{1}", IdArray[0], IdArray[1]);
            Id1A2A3 = string.Format("{0}.{1}", Id1A2, IdArray[2]);
            Id1st = v[0];
            Id2nd = int.Parse(v[1]);
            Id3rd = int.Parse(v[2]);
            Id4th = int.Parse(v[3]);
        }
        public void Update(string v1, int v2, int v3, int v4)
        {
            Id = ABHelper.VersionNumCombine(v1, v2, v3, v4);
            IdArray = ABHelper.VersionNumSplit(Id);
            Id1A2 = string.Format("{0}.{1}", IdArray[0], IdArray[1]);
            Id1A2A3 = string.Format("{0}.{1}", Id1A2, IdArray[2]);
            Id1st = v1;
            Id2nd = v2;
            Id3rd = v3;
            Id4th = v4;
        }
    }
    public class VersionInfo
    {
        // 版本号
        public VersionNum VersionId
        {
            get; private set;
        }
        // 版本信息
        public Dictionary<string, List<string>> VersionInfoList
        {
            get; private set;
        }
        public List<string> NativeInfoList
        {
            get; private set;
        }
        public string VersionFolderPath
        {
            get; private set;
        }
        public string VersionFilePath
        {
            get; private set;
        }
        public string NativeFilePath
        {
            get; private set;
        }
        public bool IsNativeVersion
        {
            get; private set;
        }
        public bool IsManifestFileExist
        {
            get; private set;
        }
        public bool IsVersionFileExist
        {
            get; private set;
        }
        public bool IsValid
        {
            get
            {
                return IsManifestFileExist && IsVersionFileExist;
            }
        }
        public VersionInfo(VersionNum versionId, bool isNativeVersion = false)
        {
            VersionId = versionId;
            IsNativeVersion = isNativeVersion;

            if (isNativeVersion)
            {
                VersionFolderPath = ABHelper.AppNativeVersionPath;
                VersionFilePath = string.Format("{0}/{1}", VersionFolderPath, ABHelper.VersionFileName);
                NativeFilePath = string.Format("{0}/{1}", VersionFolderPath, ABHelper.NativeFileName);
                IsVersionFileExist = true;
                IsManifestFileExist = true;
            }
            else
            {
                bool fromNativePath = true;
                VersionFolderPath = string.Format("{0}/{1}/{2}", ABHelper.AppVersionPath, VersionId.Id1A2, VersionId.Id3rd.ToString());
                VersionFilePath = string.Format("{0}/{1}", VersionFolderPath, ABHelper.VersionFileName);
                IsVersionFileExist = File.Exists(VersionFilePath);
                IsManifestFileExist = !string.IsNullOrEmpty(GetABFullPath(ABHelper.ManifestFileName, ref fromNativePath));

                if (IsValid)
                {
                    string versionNumPath = GetABFullPath(ABHelper.VersionNumFileName, ref fromNativePath);
                    if (!string.IsNullOrEmpty(versionNumPath))
                    {
                        VersionId.Update(ABHelper.ReadVersionNumFileByPath(versionNumPath));
                    }
                }
            }
        }
        public void ParseVersionList()
        {
            if (!IsVersionFileExist)
            {
                Debug.LogFormat("<color=#FFFF33> 检测不到version文件:{0} ,需要从服务器更新</color>", VersionFilePath);
                return;
            }
            VersionInfoList = ABHelper.ReadVersionFileByPath(VersionFilePath);
        }
        public IEnumerator ParseVersionListByWWW()
        {
            if (!IsVersionFileExist)
            {
                Debug.LogFormat("<color=#FFFF33> 检测不到version文件:{0} ,需要从服务器更新</color>", VersionFilePath);
                yield break;
            }

            WWW www = new WWW(VersionFilePath);
            yield return www;
            VersionInfoList = ABHelper.ReadVersionFileByBytes(www.bytes);
        }
        public IEnumerator ParseNatvieListByWWW()
        {
            if (string.IsNullOrEmpty(NativeFilePath))
            {
                Debug.LogFormat("<color=#FFFF33> 检测不到Native文件路径:{0} </color>", NativeFilePath);
                yield break;
            }

            WWW www = new WWW(NativeFilePath);
            yield return www;
            NativeInfoList = ABHelper.ReadNativeFileByString(www.text);
        }
        public string GetAbRelativePath(string abPath, ref bool fromNativePath)
        {
            if (null == VersionInfoList)
            {
                ParseVersionList();
            }
            if (null == VersionInfoList)
            {
                return null;
            }

            string abRelativePath = "";
            abPath = abPath.ToLower();

            List<string> abVerInfo = null;
            VersionInfoList.TryGetValue(abPath, out abVerInfo);
            if (null != abVerInfo)
            {
                int id = int.Parse(abVerInfo[1]);
                fromNativePath = id <= ABVersion.OriginalVersionId.Id3rd;
                if (fromNativePath)
                {
                    abRelativePath = ABVersion.OriginalVersionInfo.NativeInfoList.Contains(abVerInfo[3]) ? abVerInfo[3] : null;
                }
                else
                {
                    abRelativePath = string.Format("{0}/{1}", abVerInfo[1], abVerInfo[3]);
                }
            }
            return abRelativePath;
        }
        public string GetABFullPath(string abPath, ref bool fromNativePath)
        {
            string abFullPath = null;
            string abRelativePath = GetAbRelativePath(abPath, ref fromNativePath);

            if (string.IsNullOrEmpty(abRelativePath))
            {
                return abFullPath;
            }

            if (fromNativePath)
            {
                abFullPath = string.Format("{0}/{1}", ABHelper.AppNativeVersionPath, abRelativePath);
            }
            else
            {
                abFullPath = string.Format("{0}/{1}/{2}", ABHelper.AppVersionPath, VersionId.Id1A2, abRelativePath);
                if (!File.Exists(abFullPath))
                {
                    abFullPath = null;
                }
            }

            return abFullPath;
        }
    }
    public class ABVersion
    {
        public static string RemoteUrlPrefix
        {
            get; set;
        }
        public static string RemoteUrlPlatform
        {
            get
            {
                if (RuntimePlatform.Android == Application.platform)
                    return string.Format("{0}{1}/{2}/{3}/", RemoteUrlPrefix, ABHelper.ABFolderRoot, ABHelper.AndroidPlatform, ServerVersionId.Id1A2);
                else if (RuntimePlatform.IPhonePlayer == Application.platform)
                    return string.Format("{0}{1}/{2}/{3}/", RemoteUrlPrefix, ABHelper.ABFolderRoot, ABHelper.IosPlatform, ServerVersionId.Id1A2);
                else
                    return string.Format("{0}{1}/{2}/{3}/", RemoteUrlPrefix, ABHelper.ABFolderRoot, ABHelper.WinPlatform, ServerVersionId.Id1A2);
            }
        }
        public static string RemoteUrlHotter
        {
            get
            {
                return string.Format("{0}{1}/", RemoteUrlPlatform, "HotterZip");
            }
        }
        // 保存地址
        public static string LocalStorgePath
        {
            get; private set;
        }
        // 初始的版本号
        public static VersionNum OriginalVersionId
        {
            get; private set;
        }
        // 服务器所需版本号
        public static VersionNum ServerVersionId
        {
            get; set;
        }
        // 当前使用的版本号
        public static VersionNum CurVersionId
        {
            get; private set;
        }
        // 不同版本号下的资源信息
        public static Dictionary<int, VersionInfo> LocalVersionList
        {
            get; private set;
        }
        // 初始版号资源
        public static VersionInfo OriginalVersionInfo
        {
            get; private set;
        }
        // 当前使用的版本资源信息
        public static VersionInfo CurVersionInfo
        {
            get
            {
                if (null == LocalVersionList)
                {
                    return null;
                }
                if (null == CurVersionId)
                {
                    return null;
                }
                if (!LocalVersionList.ContainsKey(CurVersionId.Id3rd))
                {
                    return null;
                }
                return LocalVersionList[CurVersionId.Id3rd];
            }
        }

        // 初始化AB
        public static void InitABVersion()
        {
            Debug.Log(ABHelper.AppVersionPath);
            Debug.Log(ABHelper.AppNativeVersionPath);

            // 实例化
            LocalVersionList = new Dictionary<int, VersionInfo>();
            // 需要的文件夹
            if (!Directory.Exists(ABHelper.AppVersionPath))
            {
                Directory.CreateDirectory(ABHelper.AppVersionPath);
            }

            // 版号信息
            Dictionary<string, string> versionInfo = ABHelper.ReadVersionIdFile();
            OriginalVersionId = new VersionNum(versionInfo["ResVersion"]);
            ABHelper.ABFolderRoot = versionInfo["ABFolderRoot"];
            ABHelper.ApkFolderRoot = versionInfo["ApkFolderRoot"];

            // 当前版号
            CurVersionId = new VersionNum(OriginalVersionId.Id1st, OriginalVersionId.Id2nd, OriginalVersionId.Id3rd, OriginalVersionId.Id4th);

            // 本地更新地址
            LocalStorgePath = string.Format("{0}/{1}", ABHelper.AppVersionPath, OriginalVersionId.Id1A2);
            if (!Directory.Exists(LocalStorgePath))
            {
                Directory.CreateDirectory(LocalStorgePath);
            }

            // 获取本地的所有版本
            LoadAllVersionInfo();
            // 现在在用的版本号
            SetCurVersionNum(CurVersionId);
        }
        // 设置当前版号资源
        public static void SetCurVersionNum(VersionNum curVersion)
        {
            // 设置为当前使用版号
            CurVersionId = curVersion;
            // 将更新下的版本加载进来
            LoadVersionInfo(curVersion);
            Debug.Log("当前使用版本：" + curVersion.Id);
        }
        // 加载本地所有版本
        public static void LoadAllVersionInfo()
        {
            LocalVersionList.Clear();

            // 首包
            OriginalVersionInfo = new VersionInfo(OriginalVersionId, true);
            LocalVersionList.Add(OriginalVersionId.Id3rd, OriginalVersionInfo);

            // 获取本地的所有版本
            DirectoryInfo dirInfo = new DirectoryInfo(LocalStorgePath);
            foreach (DirectoryInfo info in dirInfo.GetDirectories())
            {
                int id3rd = 0;
                if (!int.TryParse(info.Name, out id3rd))
                {
                    continue;
                }
                if (id3rd <= OriginalVersionId.Id3rd)
                {
                    continue;
                }
                VersionNum versionId = new VersionNum(OriginalVersionId.Id1st, OriginalVersionId.Id2nd, id3rd);
                if (!LoadVersionInfo(versionId))
                {
                    continue;
                }
                if (null == CurVersionId || versionId.Id3rd > CurVersionId.Id3rd)
                {
                    CurVersionId = LocalVersionList[versionId.Id3rd].VersionId;
                }
            }
        }
        // 加载指定版本信息
        public static bool LoadVersionInfo(VersionNum version)
        {
            VersionInfo versionInfo = null;
            LocalVersionList.TryGetValue(version.Id3rd, out versionInfo);

            if (null == versionInfo)
            {
                versionInfo = new VersionInfo(version);
                if (versionInfo.IsValid)
                {
                    LocalVersionList.Add(version.Id3rd, versionInfo);
                    return true;
                }
            }
            else if (versionInfo.IsValid)
            {
                return true;
            }
            else
            {
                LocalVersionList.Remove(version.Id3rd);
                return false;
            }
            return false;
        }
    }
}