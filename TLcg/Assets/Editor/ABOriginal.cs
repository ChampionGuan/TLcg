using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace LCG
{
    public class ABOriginal : EditorWindow
    {
        private static EditorWindow TheWindow = null;
        private static string AssetFolder = "Assets/";
        private static string ResFolder = "Assets/Resources/";
        private static List<string> DoNotRemoveDir = new List<string>(new string[] { "Lua", "UI", "Audio", "Video" });
        private static List<string> DoNotRemoveFile = new List<string>(new string[] { "Resources/Prefabs/Misc/VideoPlayer.prefab", "Scenes/Bootup.unity" });
        private static List<string> AssetFilePath = new List<string>(new string[] { "Scenes", "Lua" });
        private static List<string> ResourcesFilePath = new List<string>();

        private static string[] TheVersionNum = new string[4] { "0", "0", "0", "0" };
        private static string TheRootFolderName = "tlcg";
        private static string TheApkFolderName = "tlcg";

        [MenuItem("Tools/资源打包前处理")]
        public static void Build()
        {
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();
            TheWindow = EditorWindow.GetWindow(typeof(ABOriginal), true, "打包前的资源处理");
        }
        void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("打包前，生成ab 资源包 至streamingAssets", EditorStyles.boldLabel);
            if (GUILayout.Button("android"))
            {
                BuildAB(ABHelper.AndroidPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("ios"))
            {
                BuildAB(ABHelper.IosPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("win"))
            {
                BuildAB(ABHelper.WinPlatform);
                TheWindow.Close();
            }

            EditorGUILayout.Space();
            GUILayout.Label("打包前，生成ab zip资源包 至streamingAssets", EditorStyles.boldLabel);
            if (GUILayout.Button("android-zip"))
            {
                BuildABZip(ABHelper.AndroidPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("ios-zip"))
            {
                BuildABZip(ABHelper.IosPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("win-zip"))
            {
                BuildABZip(ABHelper.WinPlatform);
                TheWindow.Close();
            }

            EditorGUILayout.Space();
            GUILayout.Label("打包前，移除非必要资源，减小包体", EditorStyles.boldLabel);
            for (int i = 0; i < ResourcesFilePath.Count; i++)
            {
                EditorGUILayout.LabelField(ResFolder + ResourcesFilePath[i]);
            }
            for (int i = 0; i < AssetFilePath.Count; i++)
            {
                EditorGUILayout.LabelField(AssetFolder + AssetFilePath[i]);
            }
            if (GUILayout.Button("移出"))
            {
                AssetMoveout();
                TheWindow.Close();
            }
            if (GUILayout.Button("移入"))
            {
                AssetMovein();
                TheWindow.Close();
            }
        }
        private static void RootFolderNmae()
        {
            // TheRootFolderName = Application.dataPath;
            // TheRootFolderName = TheRootFolderName.Substring(0, TheRootFolderName.LastIndexOf("/"));
            // TheRootFolderName = TheRootFolderName.Substring(TheRootFolderName.LastIndexOf("/") + 1);
        }
        private static void ParseTheVersion()
        {
            List<string> version = ABHelper.ReadVersionIdFile();
            TheVersionNum = ABHelper.VersionNumSplit(version[0]);
            TheRootFolderName = version[1];
            TheApkFolderName = version[2];
        }
        private static void FilesFilter()
        {
            ResourcesFilePath = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(ResFolder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string fname = d.FullName.Replace(@"/", @"\");
                string folderName = fname.Substring(fname.LastIndexOf(@"\") + 1);

                ResourcesFilePath.Add(folderName);
            }

            foreach (var v in DoNotRemoveDir)
            {
                if (ResourcesFilePath.Contains(v))
                {
                    ResourcesFilePath.Remove(v);
                }
                if (AssetFilePath.Contains(v))
                {
                    AssetFilePath.Remove(v);
                }
            }
        }
        private static Dictionary<string, string> ReadVersionFile(string filePath)
        {
            Dictionary<string, string> versionInfo = new Dictionary<string, string>();

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
        private static bool CheckRemoveDir(string name)
        {
            foreach (var v1 in DoNotRemoveDir)
            {
                if (name.StartsWith(v1.ToLower() + "/"))
                {
                    return false;
                }
            }
            return true;
        }
        private static void BuildABZip(string platformName)
        {
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

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
            string path = string.Format("{0}/../../../assetBundle/{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, TheRootFolderName, platformName, TheVersionNum[0], TheVersionNum[1], -1, TheVersionNum[2], TheVersionNum[2]);
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
                    if (!CheckRemoveDir(versionInfo[name1]))
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
                    ABHelper.ZipFile(zipList, Application.streamingAssetsPath + "/" + zip, TheVersionNum[2] + "/");
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
        private static void BuildAB(string platformName)
        {
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

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
            string path = string.Format("{0}/../../../assetBundle/{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, TheRootFolderName, platformName, TheVersionNum[0], TheVersionNum[1], -1, TheVersionNum[2], TheVersionNum[2]);
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
                    if (!CheckRemoveDir(versionInfo[name1]))
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
        private void AssetMoveout()
        {
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

            string path = string.Format("{0}/../../../temp/", Application.dataPath);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);

            string path1;
            foreach (var v in ResourcesFilePath)
            {
                path1 = ResFolder.Replace("Assets", "") + v;
                ABHelper.DirectoryMove(Application.dataPath + path1, path + path1);
            }
            foreach (var v in AssetFilePath)
            {
                path1 = AssetFolder.Replace("Assets", "") + v;
                ABHelper.DirectoryMove(Application.dataPath + path1, path + path1);
            }

            string path2;
            foreach (var v in DoNotRemoveFile)
            {
                path1 = string.Format("{0}/../../temp/{1}", Application.dataPath, v).Replace("Assets", "");
                if (!File.Exists(path1))
                {
                    continue;
                }
                path2 = Application.dataPath + "/" + v;
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Move(path1, path2);
            }

            AssetDatabase.Refresh();
            Debug.Log("资源移出成功！！");
        }
        public void AssetMovein()
        {
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

            string path = string.Format("{0}/../../../temp/", Application.dataPath);
            if (!Directory.Exists(path))
            {
                return;
            }

            ABHelper.DirectoryMove(path, Application.dataPath + "/");

            AssetDatabase.Refresh();
            Debug.Log("资源移入成功！！");
        }
    }
}