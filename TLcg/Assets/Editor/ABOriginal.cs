using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace LCG
{
    public class ABOriginal : EditorWindow
    {
        [MenuItem("Tools/生成StreamingAssets路径下资源包/android")]
        public static void BuildAndroid()
        {
            Build(ABHelper.AndroidPlatform);
        }
        [MenuItem("Tools/生成StreamingAssets路径下资源包/ios")]
        public static void BuildIos()
        {
            Build(ABHelper.IosPlatform);
        }
        [MenuItem("Tools/生成StreamingAssets路径下资源包/win")]
        public static void BuildWin()
        {
            Build(ABHelper.WinPlatform);
        }
        [MenuItem("Tools/生成StreamingAssets路径下资源包/android-zip")]
        public static void BuildZipAndroid()
        {
            BuildZip(ABHelper.AndroidPlatform);
        }
        [MenuItem("Tools/生成StreamingAssets路径下资源包/ios-zip")]
        public static void BuildZipIos()
        {
            BuildZip(ABHelper.IosPlatform);
        }
        [MenuItem("Tools/生成StreamingAssets路径下资源包/win-zip")]
        public static void BuildZipWin()
        {
            BuildZip(ABHelper.WinPlatform);
        }
        private static string RootFolderNmae()
        {
            List<string> version = ABHelper.ReadVersionIdFile();
            return version[1];
        }
        private static string[] ParseTheVersion()
        {
            List<string> version = ABHelper.ReadVersionIdFile();
            return ABHelper.VersionNumSplit(version[0]);
        }
        private static void BuildZip(string platformName)
        {
            // 删
            string native = ABHelper.ReadFile(Application.streamingAssetsPath + "/native.txt");
            if (!string.IsNullOrEmpty(native))
            {
                string[] names = native.Replace("\n", "").Split('\r');
                foreach (var v in names)
                {
                    string[] namess = v.Split(':');
                    string p = Application.streamingAssetsPath + "/" + namess[0];
                    if (File.Exists(p))
                    {
                        File.Delete(p);
                    }
                }
                File.Delete(Application.streamingAssetsPath + "/native.txt");
            }

            // 增
            string rootPath = RootFolderNmae();
            string[] versionNum = ParseTheVersion();
            string path = string.Format("{0}/../../../assetBundle/{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, rootPath, platformName, versionNum[0], versionNum[1], -1, versionNum[2], versionNum[2]);
            if (!Directory.Exists(path))
            {
                Debug.LogError("路径不存在：" + path);
                return;
            }
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            int index = 0;
            long bytesLen = 0;
            string versionpath = null;
            List<string> zipList = new List<string>();
            List<string> files = ABHelper.GetAllFilesPathInDir(path);
            Dictionary<string, string> versionInfo = ReadVersionFile(path + "/version.ini");
            StringBuilder zipTxt = new StringBuilder();
            int length = files.Count;
            string name1 = "";
            string path1 = "";
            for (int i = 0; i < length; i++)
            {
                // 过滤 ui，lua和audio文件
                path1 = files[i].Replace("\\", "/");
                name1 = ABHelper.GetFileName(path1);
                if (versionInfo.ContainsKey(name1))
                {
                    if (versionInfo[name1].StartsWith("lua/"))
                    {
                        continue;
                    }
                    if (versionInfo[name1].StartsWith("ui/"))
                    {
                        continue;
                    }
                    if (versionInfo[name1].StartsWith("audio/"))
                    {
                        continue;
                    }
                }

                // version.ini 放置尾包！！
                if (i != length - 1 && files[i].EndsWith("version.ini"))
                {
                    versionpath = files[i];
                    continue;
                }

                FileInfo f = new FileInfo(files[i]);
                bytesLen += f.Length;
                zipList.Add(files[i]);

                // version.ini 放置尾包！！
                if (!string.IsNullOrEmpty(versionpath))
                {
                    FileInfo v = new FileInfo(versionpath);
                    bytesLen += v.Length;
                    zipList.Add(versionpath);
                }

                // 50m一个压缩包
                if (bytesLen > 50 * 1024 * 1024 || i == length - 1)
                {
                    string zip = "native_" + index + ".zip";
                    zipTxt.AppendLine(zip + ":" + bytesLen);
                    ABHelper.ZipFile(zipList, Application.streamingAssetsPath + "/" + zip, versionNum[2] + "/");
                    index++;
                    bytesLen = 0;
                    zipList.Clear();
                }
            }

            // 生成列表文件
            ABHelper.WriteFile(Application.streamingAssetsPath + "/native.txt", zipTxt.ToString().TrimEnd());
            AssetDatabase.Refresh();

            Debug.Log("streamingAssets压缩文件生成成功！！压缩文件源路径：" + path);
        }
        private static void Build(string platformName)
        {
            // 删
            string native = ABHelper.ReadFile(Application.streamingAssetsPath + "/native.txt");
            if (!string.IsNullOrEmpty(native))
            {
                string[] names = native.Replace("\n", "").Split('\r');
                foreach (var v in names)
                {
                    string p = Application.streamingAssetsPath + "/" + v;
                    if (File.Exists(p))
                    {
                        File.Delete(p);
                    }
                }
                File.Delete(Application.streamingAssetsPath + "/native.txt");
            }

            // 增
            string rootPath = RootFolderNmae();
            string[] versionNum = ParseTheVersion();
            string path = string.Format("{0}/../../../assetBundle/{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, rootPath, platformName, versionNum[0], versionNum[1], -1, versionNum[2], versionNum[2]);
            if (!Directory.Exists(path))
            {
                Debug.LogError("路径不存在：" + path);
                return;
            }
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            Dictionary<string, string> versionInfo = ReadVersionFile(path + "/version.ini");
            List<string> files = ABHelper.GetAllFilesPathInDir(path);
            StringBuilder txt = new StringBuilder();
            string name1 = "";
            string path1 = "";
            string path2 = "";
            foreach (var v in files)
            {
                // 过滤 ui，lua和audio文件
                path1 = v.Replace("\\", "/");
                name1 = ABHelper.GetFileName(path1);
                if (versionInfo.ContainsKey(name1))
                {
                    if (versionInfo[name1].StartsWith("lua/"))
                    {
                        continue;
                    }
                    if (versionInfo[name1].StartsWith("ui/"))
                    {
                        continue;
                    }
                    if (versionInfo[name1].StartsWith("audio/"))
                    {
                        continue;
                    }
                }

                path2 = Application.streamingAssetsPath + "/" + name1;
                File.Copy(path1, path2, true);

                if (!v.EndsWith("version.ini"))
                {
                    txt.AppendLine(name1);
                }
            }
            // version.ini 放置尾包！！
            txt.AppendLine("version.ini");

            // 生成列表文件
            ABHelper.WriteFile(Application.streamingAssetsPath + "/native.txt", txt.ToString().TrimEnd());
            AssetDatabase.Refresh();

            Debug.Log("streamingAssets文件生成成功！！文件源路径：" + path);
        }
        private static Dictionary<string, string> ReadVersionFile(string filePath)
        {
            Dictionary<string, string> versionInfo = new Dictionary<string, string>();

            // 暂时不过滤文件！！
            return versionInfo;

            byte[] versionbytes = ABHelper.ReadFileToBytes(filePath);
            if (null == versionbytes)
            {
                return versionInfo;
            }

            ABHelper.Encrypt(ref versionbytes); //RC4 加密文件
            string versionTxt = Encoding.UTF8.GetString(versionbytes);

            if (!string.IsNullOrEmpty(versionTxt))
            {
                //ui/tips.ab:ba2de82e3fc42e750d317b133096dfea:0:22455:fa7917d4974436b1214dc313fccda2a5
                //路径：内容md5：版号：size：路径md5
                string[] split = versionTxt.Split('\r');
                foreach (string k in split)
                {
                    string[] split2 = k.Split(':');
                    versionInfo.Add(split2[4], split2[0]);
                }
            }

            return versionInfo;
        }
    }
}