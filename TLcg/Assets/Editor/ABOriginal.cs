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
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf("/"));
            path = "ab_" + path.Substring(path.LastIndexOf("/") + 1);
            return path;
        }
        private static string[] ParseTheVersion()
        {
            TextAsset resInfo = Resources.Load<TextAsset>("versionId");
            string[] versionNum = new string[4] { "0", "0", "0", "0" };
            if (null != resInfo)
            {
                versionNum = ABHelper.VersionNumSplit(resInfo.text.Replace("\r", ""));
            }
            return versionNum;
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
            int versionNum2nd = int.Parse(versionNum[2]);
            string path = string.Format("{0}/../{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, rootPath, platformName, versionNum[0], versionNum[1], (versionNum2nd - 1), versionNum2nd, versionNum2nd);
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
            StringBuilder zipTxt = new StringBuilder();
            int length = files.Count;
            for (int i = 0; i < length; i++)
            {
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

                // 50m
                if (bytesLen > 50 * 1024 * 1024 || i == length - 1)
                {
                    string name = "native_" + index + ".zip";
                    zipTxt.AppendLine(name + ":" + bytesLen);
                    ABHelper.ZipFile(zipList, Application.streamingAssetsPath + "/" + name, versionNum2nd.ToString() + "/");
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
            int versionNum2nd = int.Parse(versionNum[2]);
            string path = string.Format("{0}/../{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, rootPath, platformName, versionNum[0], versionNum[1], (versionNum2nd - 1), versionNum2nd, versionNum2nd);
            if (!Directory.Exists(path))
            {
                Debug.LogError("路径不存在：" + path);
                return;
            }
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            List<string> files = ABHelper.GetAllFilesPathInDir(path);
            StringBuilder txt = new StringBuilder();
            string name1 = "";
            string path1 = "";
            string path2 = "";
            foreach (var v in files)
            {
                path1 = v.Replace("\\", "/");
                name1 = ABHelper.GetFileName(path1);
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
    }
}