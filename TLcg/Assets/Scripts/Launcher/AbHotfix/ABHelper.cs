using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Text;

namespace LCG
{
    public class ABHelper
    {
        public enum EVersionState
        {
            None = -1,
            UnknowError,
            PrepareAssets,
            MovefileProcess,
            PrepareAssetsComplete,
            APKDownloadComplete,
            ServerVersionId,
            AckServerVersionId,
            OriginalVersionId,
            ClientVersionId,
            CheckLocalVersion,
            CheckLocalVersionOver,
            DownloadAPKConfirm,
            DownloadConfirm,
            DownloadApkName,
            DownloadWifi,
            DownloadSpeed,
            DownloadProgress,
            UnzipProgress,
            HotfixComplete,
            NeedGoStore,
            NeedRestart,
            Unreachable,
            DownloadingWifi,
            AutofixDeleteFile,
            AutofixComplete,
            AutofixProgress,
            CheckFileProgress,
            ABInit,
        }
        public class VersionArgs
        {
            public EVersionState state = EVersionState.None;
            public string sValue = "";
            public float fValue = 0;
            public Action<string> callBack = null;
            public VersionArgs(EVersionState e)
            {
                state = e;
            }
            public VersionArgs(EVersionState e, string s)
            {
                state = e;
                sValue = s;
            }

            public VersionArgs(EVersionState e, float f)
            {
                state = e;
                fValue = f;
            }
            public VersionArgs(EVersionState e, Action<string> c)
            {
                state = e;
                callBack = c;
            }
            public VersionArgs(EVersionState e, float f, Action<string> c)
            {
                state = e;
                fValue = f;
                callBack = c;
            }
            public VersionArgs(EVersionState e, string s, Action<string> c)
            {
                state = e;
                sValue = s;
                callBack = c;
            }
        }
        public static bool IgnoreHotfix
        {
            get; set;
        }
        public static string ABFolderRoot
        {
            get; set;
        }
        public static string ApkFolderRoot
        {
            get; set;
        }
        public static string AndroidPlatform
        {
            get
            {
                return "Android";
            }
        }
        public static string IosPlatform
        {
            get
            {
                return "Ios";
            }
        }
        public static string WinPlatform
        {
            get
            {
                return "Win";
            }
        }
        public static string VersionNumFileName
        {
            get
            {
                return "versionnum.ini";
            }
        }
        public static string VersionFileName
        {
            get
            {
                return "version.ini";
            }
        }
        public static string DependFileName
        {
            get
            {
                return "depend.ini";
            }
        }
        public static string ManifestFileName
        {
            get
            {
                return "manifest.ini";
            }
        }
        public static string Md5FileName
        {
            get
            {
                return "md5.ini";
            }

        }
        public static string NativeFileName
        {
            get
            {
                return "native.ini";
            }
        }
        public static string AppVersionPath
        {
            get
            {
                // return Application.temporaryCachePath;
                return Application.persistentDataPath + "/version";
            }
        }
        public static string AppNativeVersionPath
        {
            get
            {
                return Application.streamingAssetsPath + "/version";
            }
        }

        public static string ApkLocalPath
        {
            get
            {
                // return Application.persistentDataPath;
                return Application.persistentDataPath + "/";
            }
        }
        public static string EncryptKey
        {
            get
            {
                return "jdiwajdlsakjpo132j29jdnuh021djij8jnbuhyb13bfbkabwhio123j21k2lkdklajaijie1j3j12ij2kknsad";
            }
        }
        public static bool VersionNumMatch(string value)
        {
            // v1.1.1
            bool result = System.Text.RegularExpressions.Regex.IsMatch(value, @"\S+\.\d+\.\d+\.\d{1,}$");
            if (!result)
            {
                // v1.1.1.1
                result = System.Text.RegularExpressions.Regex.IsMatch(value, @"\S+\.\d+\.\d{1,}$");
            }
            return result;
        }
        public static string[] VersionNumSplit(string value)
        {
            string[] v = new string[4] { "0", "0", "0", "0" };
            if (string.IsNullOrEmpty(value))
            {
                return v;
            }

            // 1.1.0.0
            string[] split = value.Split('.');
            if (split.Length > 0)
            {
                v[0] = split[0];
            }
            if (split.Length > 1)
            {
                v[1] = split[1];
            }
            if (split.Length > 2)
            {
                v[2] = split[2];
            }
            if (split.Length > 3)
            {
                v[3] = split[3];
            }
            return v;
        }
        public static string VersionNumCombine(string v1, string v2, string v3 = null, string v4 = null)
        {
            if (null == v3)
            {
                v3 = "0";
            }
            if (null == v4)
            {
                v4 = "0";
            }

            return string.Format("{0}.{1}.{2}.{3}", v1, v2, v3, v4);
        }
        public static string VersionNumCombine(string v1, int v2, int? v3 = null, int? v4 = null)
        {
            if (null == v3)
            {
                v3 = 0;
            }
            if (null == v4)
            {
                v4 = 0;
            }

            return string.Format("{0}.{1}.{2}.{3}", v1, v2.ToString(), v3.ToString(), v4.ToString());
        }
        public static string StreamingAssetsPath()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = "file:///" + Application.dataPath + "/StreamingAssets/";
                    break;
            }
            return path;
        }
        public static bool InstallApk(string path)
        {
#if UNITY_ANDROID
            AndroidJavaObject m_javaObject = new AndroidJavaObject("com.client.ClientClass");
            m_javaObject.Call("InstallApk", path);
            return true;
#else
            return false;
#endif
        }
        public static string BuildMD5ByFile(string filePath)
        {
            //string value = ABHelper.ReadFile(filePath);
            //return ABHelper.BuildMD5ByString(value);

            if (!File.Exists(filePath))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            FileStream fs = new FileStream(filePath, FileMode.Open);
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(fs);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            fs.Close();
            fs.Dispose();

            return sb.ToString();
        }
        public static string BuildMD5ByBytes(byte[] value)
        {
            if (null == value)
            {
                return "";
            }

            string destString = "";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] md5Data = md5.ComputeHash(value, 0, value.Length);
            md5.Clear();

            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }
        public static string BuildMD5ByString(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return "";
            }

            byte[] data = Encoding.UTF8.GetBytes(value);

            return BuildMD5ByBytes(data);
        }
        public static Dictionary<string, string> ReadVersionIdFile()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            TextAsset resInfo = Resources.Load<TextAsset>("versionId");
            string[] sb = resInfo.text.Replace("\r", "").Split('\n');
            string[] sb1;
            for (int i = 0; i < sb.Length; i++)
            {
                sb1 = sb[i].Split('=');
                if (sb1.Length > 1)
                {
                    list.Add(sb1[0], sb1[1]);
                }
            }
            if (list.Count < 3)
            {
                Debug.LogError("versionId.bytes error!!");
            }
            return list;
        }
        public static void WriteVersionIdFile(Dictionary<string, string> list)
        {
            if (null == list)
            {
                return;
            }
            string path = Application.dataPath + "/Resources/versionId.bytes";
            StringBuilder sb = new StringBuilder();
            foreach (var v in list)
            {
                sb.Append(v.Key + "=" + v.Value + "\n");
            }
            ABHelper.WriteFile(path, sb.ToString().TrimEnd());
        }
        public static string[] ReadVersionNumFileByBytes(byte[] numbytes)
        {
            string[] num = null;
            if (null == numbytes)
            {
                return num;
            }

            Encrypt(ref numbytes); //RC4 加密文件
            string numTxt = Encoding.UTF8.GetString(numbytes);

            if (!string.IsNullOrEmpty(numTxt))
            {
                num = ABHelper.VersionNumSplit(numTxt);
            }

            return num;
        }
        public static string[] ReadVersionNumFileByPath(string filePath)
        {
            return ReadVersionNumFileByBytes(ABHelper.ReadFileToBytes(filePath));
        }
        public static void WriteVersionNumFile(string filePath, string num)
        {
            byte[] numbytes = Encoding.UTF8.GetBytes(num.ToString().TrimEnd().ToLower());
            Encrypt(ref numbytes); //RC4 加密文件
            ABHelper.WriteFileByBytes(filePath, numbytes);
        }
        public static Dictionary<string, List<string>> ReadVersionFileByBytes(byte[] versionbytes)
        {
            Dictionary<string, List<string>> versionInfo = new Dictionary<string, List<string>>();
            if (null == versionbytes)
            {
                return versionInfo;
            }

            Encrypt(ref versionbytes); //RC4 加密文件
            string versionTxt = Encoding.UTF8.GetString(versionbytes);

            if (!string.IsNullOrEmpty(versionTxt))
            {
                //ui/tips.ab:ba2de82e3fc42e750d317b133096dfea:0:22455:fa7917d4974436b1214dc313fccda2a5
                //路径：内容md5：版号：size：路径md5
                string[] split = versionTxt.Split('\r');
                foreach (string k in split)
                {
                    string[] split2 = k.Split(':');
                    versionInfo.Add(split2[0], new List<string>() { split2[1], split2[2], split2[3], split2[4] });
                }
            }

            return versionInfo;
        }
        public static Dictionary<string, List<string>> ReadVersionFileByPath(string filePath)
        {
            return ReadVersionFileByBytes(ABHelper.ReadFileToBytes(filePath));
        }
        public static void WriteVersionFile(string filePath, Dictionary<string, List<string>> versionInfo)
        {
            StringBuilder versionTxt = new StringBuilder();
            foreach (KeyValuePair<string, List<string>> pair in versionInfo)
            {
                versionTxt.Append(pair.Key + ":" + pair.Value[0] + ":" + pair.Value[1] + ":" + pair.Value[2] + ":" + pair.Value[3] + "\r");
            }

            byte[] versionbytes = Encoding.UTF8.GetBytes(versionTxt.ToString().TrimEnd().ToLower());
            ABHelper.WriteFileByBytes(filePath + "-VersionFile.txt", versionbytes);
            Encrypt(ref versionbytes); //RC4 加密文件
            ABHelper.WriteFileByBytes(filePath, versionbytes);
        }
        public static Dictionary<string, List<string>> ReadManifestFileByBytes(byte[] manifestbytes)
        {
            Dictionary<string, List<string>> manifestInfo = new Dictionary<string, List<string>>();
            if (null == manifestbytes)
            {
                return manifestInfo;
            }

            Encrypt(ref manifestbytes); //RC4 加密文件
            string manifestTxt = Encoding.UTF8.GetString(manifestbytes);

            if (!string.IsNullOrEmpty(manifestTxt))
            {
                // scenesfinal/hottertest.ab:shaders/lpcpbrsoldier.ab,prefabs/monster/cha_archer_l1.ab
                string[] split = manifestTxt.Split('\r');
                foreach (string k in split)
                {
                    string[] split2 = k.Split(':');
                    if (!manifestInfo.ContainsKey(split2[0]))
                    {
                        manifestInfo.Add(split2[0], new List<string>());
                    }

                    string[] split3 = split2[1].Split(',');
                    foreach (string k3 in split3)
                    {
                        if (!manifestInfo[split2[0]].Contains(k3))
                        {
                            manifestInfo[split2[0]].Add(k3);
                        }
                    }
                }
            }

            return manifestInfo;
        }
        public static Dictionary<string, List<string>> ReadManifestFileByPath(string filePath)
        {
            return ReadManifestFileByBytes(ABHelper.ReadFileToBytes(filePath));
        }
        public static void WriteManifestFile(string filePath, Dictionary<string, List<string>> manifestInfo)
        {
            StringBuilder manifestTxt = new StringBuilder();
            foreach (KeyValuePair<string, List<string>> pair in manifestInfo)
            {
                string fileName = ABHelper.GetFileFullPathWithoutFtype(pair.Key) + ".ab";
                manifestTxt.Append(fileName + ":");

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    fileName = ABHelper.GetFileFullPathWithoutFtype(pair.Value[i]) + ".ab";
                    if (i == pair.Value.Count - 1)
                    {
                        manifestTxt.Append(fileName + "\r");
                    }
                    else
                    {
                        manifestTxt.Append(fileName + ",");
                    }
                }
            }

            byte[] manifestbytes = Encoding.UTF8.GetBytes(manifestTxt.ToString().TrimEnd().ToLower());
            ABHelper.WriteFileByBytes(filePath + "-Manifest.txt", manifestbytes);
            Encrypt(ref manifestbytes); //RC4 加密文件
            ABHelper.WriteFileByBytes(filePath, manifestbytes);
        }
        public static Dictionary<string, List<string>> ReadDependFileByString(string dependTxt)
        {
            Dictionary<string, List<string>> depenInfo = new Dictionary<string, List<string>>();
            if (!string.IsNullOrEmpty(dependTxt))
            {
                // assets/resources/ui/tips:assets/resources/ui/tips/tips.bytes,assets/resources/ui/tips/tips@atlas2!a.png
                string[] split = dependTxt.Split('\r');
                foreach (string k in split)
                {
                    string[] split2 = k.Split(':');
                    if (!depenInfo.ContainsKey(split2[0]))
                    {
                        depenInfo.Add(split2[0], new List<string>());
                    }

                    string[] split3 = split2[1].Split(',');
                    foreach (string k3 in split3)
                    {
                        depenInfo[split2[0]].Add(k3);
                    }
                }
            }

            return depenInfo;
        }
        public static Dictionary<string, List<string>> ReadDependFileByPath(string filePath)
        {
            return ReadDependFileByString(ABHelper.ReadFile(filePath));
        }
        public static void WriteDependFile(string filePath, Dictionary<string, List<string>> dependInfo)
        {
            StringBuilder dependTxt = new StringBuilder();
            foreach (KeyValuePair<string, List<string>> pair in dependInfo)
            {
                dependTxt.Append(pair.Key + ":");
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    if (i == pair.Value.Count - 1)
                    {
                        dependTxt.Append(pair.Value[i] + "\r");
                    }
                    else
                    {
                        dependTxt.Append(pair.Value[i] + ",");
                    }
                }
            }
            ABHelper.WriteFile(filePath, dependTxt.ToString().TrimEnd().ToLower());
        }
        public static Dictionary<string, List<string>> ReadMd5FileByString(string md5Txt)
        {
            Dictionary<string, List<string>> md5Info = new Dictionary<string, List<string>>();
            if (!string.IsNullOrEmpty(md5Txt))
            {
                // assets/resources/ui/tips/tips.bytes:bd51b54f940b530425662e4529ea5673
                string[] split = md5Txt.Split('\r');
                foreach (string k in split)
                {
                    string[] split2 = k.Split(':');
                    md5Info.Add(split2[0], new List<string>() { split2[1], split2[2] });
                }
            }

            return md5Info;
        }
        public static Dictionary<string, List<string>> ReadMd5FileByPath(string filePath)
        {
            return ReadMd5FileByString(ABHelper.ReadFile(filePath));
        }
        public static void WriteMd5File(string filePath, Dictionary<string, string> md5Info, Dictionary<string, int> crInfo)
        {
            StringBuilder md5Txt = new StringBuilder();
            int rc = 1;
            foreach (KeyValuePair<string, string> pair in md5Info)
            {
                rc = crInfo.ContainsKey(pair.Key) ? crInfo[pair.Key] : 1;
                md5Txt.Append(pair.Key + ":" + pair.Value + ":" + rc + "\r");
            }
            ABHelper.WriteFile(filePath, md5Txt.ToString().TrimEnd().ToLower());
        }
        public static List<string> ReadNativeFileByString(string nativeTxt)
        {
            List<string> nativeInfo = new List<string>();
            if (!string.IsNullOrEmpty(nativeTxt))
            {
                string[] split = nativeTxt.Split('\r');
                foreach (string k in split)
                {
                    nativeInfo.Add(k);
                }
            }

            return nativeInfo;
        }
        public static List<string> ReadNativeFileByPath(string filePath)
        {
            return ReadNativeFileByString(ABHelper.ReadFile(filePath));
        }
        public static void WriteNativeFile(string filePath, List<string> nativeInfo)
        {
            StringBuilder nativeTxt = new StringBuilder();
            foreach (var pair in nativeInfo)
            {
                nativeTxt.Append(pair + "\r");
            }
            ABHelper.WriteFile(filePath, nativeTxt.ToString().TrimEnd().ToLower());
        }
        /// <summary>
        /// 获取文件名(有后缀无路径)或者文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileName(string path, char separator = '/')
        {
            if (!path.Contains("/")) return path;
            return path.Substring(path.LastIndexOf(separator) + 1);
        }
        /// <summary>
        /// 获取文件名(无后缀无路径)或者文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutSuffix(string path, char separator = '/')
        {
            string name = path;
            if (name.Contains("/"))
            {
                name = name.Substring(name.LastIndexOf(separator) + 1);
            }
            if (name.Contains("."))
            {
                return name.Substring(0, name.LastIndexOf("."));
            }
            else
            {
                return name;
            }
        }
        /// <summary>
        /// 获取文件文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileFolderPath(string path)
        {
            if (!path.Contains("/")) return path;
            return path.Substring(0, path.LastIndexOf('/'));
        }
        /// <summary>
        /// 获取文件完整路径不包含文件类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileFullPathWithoutFtype(string fileName)
        {
            if (!fileName.Contains(".")) return fileName;
            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }
        /// <summary>
        /// 获取路径下的文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetAllFilesPathInDir(string path)
        {
            List<string> filePathList = new List<string>();
            if (!Directory.Exists(path))
            {
                return filePathList;
            }
            filePathList.AddRange(Directory.GetFiles(path));

            string[] subDirs = Directory.GetDirectories(path);
            foreach (string subPath in subDirs)
            {
                filePathList.AddRange(GetAllFilesPathInDir(subPath));
            }

            return filePathList;
        }
        /// <summary>
        /// 以覆盖的形式写文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static bool WriteFile(string path, string content)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false))
                {
                    sw.Write(content);
                }
            }
            catch (Exception e)
            {
                //路径与名称未找到文件则直接返回空
                Debug.LogWarning(path + "[error] 写入文件错误: " + e);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 以覆盖的形式写文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static bool WriteFileByBytes(string path, byte[] content)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(content, 0, content.Length);
                }
            }
            catch (Exception e)
            {
                //路径与名称未找到文件则直接返回空
                Debug.LogWarning(path + "[error] 写入文件错误: " + e);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 读取文件，以string格式返回
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("[warning] 文件不存在: " + path);
                return string.Empty;
            }

            string text = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(path + "[error] 读取文件错误: " + e);
                return string.Empty;
            }

            return text;
        }
        /// <summary>
        /// 读取文件，以bytes格式返回
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadFileToBytes(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("[warning] 文件不存在: " + path);
                return null;
            }

            byte[] bytes = null;
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(path + "[error] 读取文件错误: " + e);
                return null;
            }

            return bytes;
        }
        /// <summary>
        /// 文件大小
        /// </summary>
        /// <returns></returns>
        public static long FileSize(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("[warning] 文件不存在: " + path);
                return 0;
            }
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }
        /// <summary>
        /// 文件夹复制
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public static void DirectoryCopy(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }
            string sfilePath;
            string tfilePath;
            string tdirPath;
            List<string> filesPath = GetAllFilesPathInDir(sourcePath);
            foreach (string path in filesPath)
            {
                sfilePath = path.Replace("\\", "/");
                tfilePath = sfilePath.Replace(sourcePath, targetPath);
                tdirPath = GetFileFolderPath(tfilePath);

                if (!Directory.Exists(tdirPath))
                {
                    Directory.CreateDirectory(tdirPath);
                }
                File.Copy(sfilePath, tfilePath, true);
            }
        }
        /// <summary>
        /// 文件夹移动
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public static void DirectoryMove(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }
            string sfilePath;
            string tfilePath;
            string tdirPath;
            List<string> filesPath = GetAllFilesPathInDir(sourcePath);
            foreach (string path in filesPath)
            {
                sfilePath = path.Replace("\\", "/");
                tfilePath = sfilePath.Replace(sourcePath, targetPath);
                tdirPath = GetFileFolderPath(tfilePath);

                if (!Directory.Exists(tdirPath))
                {
                    Directory.CreateDirectory(tdirPath);
                }
                if (File.Exists(tfilePath))
                {
                    File.Delete(tfilePath);
                }
                File.Move(sfilePath, tfilePath);
            }
        }
        /// <summary>
        /// 文件夹删除
        /// </summary>
        public static void DirectoryRemove(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
        /// <summary>
        /// 文件夹重命名
        /// </summary>
        private static void DirectoryRename(string dir, string newDir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Move(dir, newDir);
            }
        }
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sourcedirectory"></param>
        /// <param name="zipevent"></param>
        public static void ZipFile(string filename, string sourcedirectory, FastZipEvents zipevent = null)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                FastZip fz;
                if (zipevent != null)
                {
                    fz = new FastZip(zipevent);
                }
                else
                {
                    fz = new FastZip();
                }
                fz.CreateEmptyDirectories = true;
                fz.CreateZip(filename, sourcedirectory, true, "");
                fz = null;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "  " + e.StackTrace);
            }
        }
        /// <summary>
        /// 压缩多个文件
        /// </summary>
        /// <param name="filespath"></param>
        /// <param name="sourcedpath"></param>
        public static void ZipFile(List<string> filespath, string sourcedpath, string entrnameprefix = "")
        {
            using (ZipFile zip = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(sourcedpath))
            {
                zip.BeginUpdate();
                foreach (var v in filespath)
                {
                    string p = v.Replace("\\", "/");
                    string p1 = ABHelper.GetFileName(p);
                    ZipEntry e = new ZipEntry(p1);
                    zip.Add(p, entrnameprefix + p1);
                }
                zip.CommitUpdate();
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input"></param>
        public static void Encrypt(ref byte[] input)
        {
            byte[] tmp = new byte[input.LongLength];
            System.Array.Copy(input, 0, tmp, 0, input.LongLength);
            byte[] de = RC4(tmp, EncryptKey);
            System.Array.Copy(de, 0, input, 0, input.LongLength);
            tmp = null;
            de = null;
        }
        /// <summary>
        /// rc4加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static string RC4(string str, String pass)
        {
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
            Byte[] bt = RC4(data, pass);
            return System.Text.Encoding.UTF8.GetString(bt);
        }
        /// <summary>
        /// rc4加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static Byte[] RC4(Byte[] data, String pass)
        {
            if (data == null || pass == null) return null;
            Byte[] output = new Byte[data.Length];
            Int64 i = 0;
            Int64 j = 0;
            Byte[] mBox = GetRC4Key(System.Text.Encoding.UTF8.GetBytes(pass), 256);

            // 加密
            for (Int64 offset = 0; offset < data.Length; offset++)
            {
                i = (i + 1) % mBox.Length;
                j = (j + mBox[i]) % mBox.Length;
                Byte temp = mBox[i];
                mBox[i] = mBox[j];
                mBox[j] = temp;
                Byte a = data[offset];
                //Byte b = mBox[(mBox[i] + mBox[j] % mBox.Length) % mBox.Length];
                // mBox[j] 一定比 mBox.Length 小，不需要在取模
                Byte b = mBox[(mBox[i] + mBox[j]) % mBox.Length];
                output[offset] = (Byte)((Int32)a ^ (Int32)b);
            }

            data = output;

            return output;
        }
        /// <summary>
        /// rc4 Key
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="kLen"></param>
        /// <returns></returns>
        private static Byte[] GetRC4Key(Byte[] pass, Int32 kLen)
        {
            Byte[] mBox = new Byte[kLen];

            for (Int64 i = 0; i < kLen; i++)
            {
                mBox[i] = (Byte)i;
            }
            Int64 j = 0;
            for (Int64 i = 0; i < kLen; i++)
            {
                j = (j + mBox[i] + pass[i % pass.Length]) % kLen;
                Byte temp = mBox[i];
                mBox[i] = mBox[j];
                mBox[j] = temp;
            }
            return mBox;
        }
        /// <summary>
        /// 清除热更的资源
        /// </summary>
        public static void ClearVersionAb()
        {
            if (Directory.Exists(ABHelper.AppVersionPath))
            {
                Directory.Delete(ABHelper.AppVersionPath, true);
            }
        }
    }
}