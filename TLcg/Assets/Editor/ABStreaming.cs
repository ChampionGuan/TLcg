using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace LCG
{
    public class ABStreaming : EditorWindow
    {
        private static EditorWindow TheWindow = null;

        private static string AssetsFolder = "Assets/";
        private static string ResFolder = "Assets/Resources/";

        private static string UseAssetDirStr = "Lua;Scenes";
        private static string UseAssetDirStr2;
        private static string NotRemoveDirStr = "Lua;UI;Audio;Video;Ignore";
        private static string NotRemoveDirStr2;
        private static string NotRemoveFileStr = "Resources/Prefabs/Misc/VideoPlayer.prefab;Scenes/Bootup.unity";
        private static string NotRemoveFileStr2;

        private static List<string> NotRemoveDirList = new List<string>(new string[] { });
        private static List<string> NotRemoveFileList = new List<string>(new string[] { });
        private static List<string> UseAssetsDir = new List<string>(new string[] { });
        private static List<string> RemoveResourcesDir = new List<string>();
        private static List<string> RemoveAssetsDir = new List<string>();

        private static string[] TheVersionNum = new string[4] { "0", "0", "0", "0" };
        private static string TheRootFolderName = "tlcg";
        private static string TheApkFolderName = "tlcg";

        [MenuItem("Tools/资源打包前处理")]
        public static void Build()
        {
            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

            TheWindow = EditorWindow.GetWindow(typeof(ABStreaming), true, "打包前的资源处理");
        }
        void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("打包前，生成ab 资源包 至streamingAssets", EditorStyles.boldLabel);
            if (GUILayout.Button("android"))
            {
                BuildNativeAB(ABHelper.AndroidPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("ios"))
            {
                BuildNativeAB(ABHelper.IosPlatform);
                TheWindow.Close();
            }
            if (GUILayout.Button("win"))
            {
                BuildNativeAB(ABHelper.WinPlatform);
                TheWindow.Close();
            }

            EditorGUILayout.Space();
            GUILayout.Label("有用的Assets下文件夹（以半角分号隔开）");
            UseAssetDirStr = GUILayout.TextField(UseAssetDirStr);
            if (UseAssetDirStr2 != UseAssetDirStr)
            {
                UseAssetDirStr2 = UseAssetDirStr;
                WriteFile();
                ParseAllDir();
            }

            GUILayout.Label("不移出的文件夹（以半角分号隔开）");
            NotRemoveDirStr = GUILayout.TextField(NotRemoveDirStr);
            if (NotRemoveDirStr2 != NotRemoveDirStr)
            {
                NotRemoveDirStr2 = NotRemoveDirStr;
                WriteFile();
                ParseAllDir();
            }

            GUILayout.Label("不移出的文件（以半角分号隔开）");
            NotRemoveFileStr = GUILayout.TextField(NotRemoveFileStr);
            if (NotRemoveFileStr2 != NotRemoveFileStr)
            {
                NotRemoveFileStr2 = NotRemoveFileStr;
                WriteFile();
                ParseAllDir();
            }

            EditorGUILayout.Space();
            GUILayout.Label("需要移出的文件夹路径，减小包体", EditorStyles.boldLabel);
            for (int i = 0; i < RemoveResourcesDir.Count; i++)
            {
                EditorGUILayout.LabelField(ResFolder + RemoveResourcesDir[i]);
            }
            for (int i = 0; i < RemoveAssetsDir.Count; i++)
            {
                EditorGUILayout.LabelField(AssetsFolder + RemoveAssetsDir[i]);
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
        private static void ReadFile()
        {
            string s = ABHelper.ReadFile(Application.dataPath + "/Editor/ABStreaming.txt");
            string[] sp = s.TrimEnd().Replace("\r", "").Split('\n');
            string[] sp1;
            foreach (var v in sp)
            {
                if (string.IsNullOrEmpty(v))
                {
                    continue;
                }
                sp1 = v.Split('=');

                switch (sp1[0])
                {
                    case "UseAssetsDir": UseAssetDirStr = sp1[1]; break;
                    case "NotRemoveDir": NotRemoveDirStr = sp1[1]; break;
                    case "NotRemoveFile": NotRemoveFileStr = sp1[1]; break;
                    default: break;
                }
            }
        }
        private static void WriteFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UseAssetsDir=" + UseAssetDirStr + "\n");
            sb.Append("NotRemoveDir=" + NotRemoveDirStr + "\n");
            sb.Append("NotRemoveFile=" + NotRemoveFileStr + "\n");
            ABHelper.WriteFile(Application.dataPath + "/Editor/ABStreaming.txt", sb.ToString().TrimEnd());
        }
        private static void ParseAllDir()
        {
            ParseDir(UseAssetDirStr, UseAssetsDir);
            ParseDir(NotRemoveDirStr, NotRemoveDirList);
            ParseDir(NotRemoveFileStr, NotRemoveFileList);
            FilesFilter();
        }
        private static void ParseDir(string s, List<string> l)
        {
            string[] sp = s.Split(';');

            l.Clear();
            foreach (var v in sp)
            {
                if (string.IsNullOrEmpty(v))
                {
                    continue;
                }
                l.Add(v);
            }
        }
        private static void FilesFilter()
        {
            if (null == RemoveResourcesDir)
            {
                RemoveResourcesDir = new List<string>();
            }
            if (null == RemoveAssetsDir)
            {
                RemoveAssetsDir = new List<string>();
            }
            RemoveResourcesDir.Clear();
            RemoveAssetsDir.Clear();

            foreach (var v in UseAssetsDir)
            {
                RemoveAssetsDir.Add(v);
            }

            DirectoryInfo dir = new DirectoryInfo(ResFolder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string fname = d.FullName.Replace(@"/", @"\");
                string folderName = fname.Substring(fname.LastIndexOf(@"\") + 1);

                RemoveResourcesDir.Add(folderName);
            }
            foreach (var v in NotRemoveDirList)
            {
                if (RemoveResourcesDir.Contains(v))
                {
                    RemoveResourcesDir.Remove(v);
                }
                if (RemoveAssetsDir.Contains(v))
                {
                    RemoveAssetsDir.Remove(v);
                }
            }
        }
        private static Dictionary<string, string> ReadVersionFileByPath(string filePath)
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
            foreach (var v1 in NotRemoveDirList)
            {
                if (name.StartsWith(v1.ToLower() + "/"))
                {
                    return false;
                }
            }
            return true;
        }
        public static void BuildNativeAB(string platformName)
        {
            System.GC.Collect();
            System.GC.Collect();

            ReadFile();
            ParseAllDir();
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

            string nativePath = string.Format("{0}/{1}", ABHelper.AppNativeVersionPath, ABHelper.NativeFileName);

            // 删
            List<string> nativeList = ABHelper.ReadNativeFileByPath(nativePath);
            foreach (var v in nativeList)
            {
                string p = string.Format("{0}/{1}", ABHelper.AppNativeVersionPath, v);
                if (File.Exists(p))
                {
                    File.Delete(p);
                }
            }
            if (File.Exists(nativePath))
            {
                File.Delete(nativePath);
            }

            // 增
            string path = string.Format("{0}/../../../assetBundle/{1}/{2}/{3}.{4}/HotterZip/{5}-{6}/{7}", Application.dataPath, TheRootFolderName, platformName, TheVersionNum[0], TheVersionNum[1], -1, TheVersionNum[2], TheVersionNum[2]);
            if (!Directory.Exists(path))
            {
                Debug.LogError("路径不存在：" + path);
                return;
            }
            if (!Directory.Exists(ABHelper.AppNativeVersionPath))
            {
                Directory.CreateDirectory(ABHelper.AppNativeVersionPath);
            }
            Dictionary<string, string> versionInfo = ReadVersionFileByPath(path + "/version.ini");
            List<string> files = ABHelper.GetAllFilesPathInDir(path);
            nativeList.Clear();
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

                path2 = ABHelper.AppNativeVersionPath + "/" + name1;
                File.Copy(path1, path2, true);

                if (!v.EndsWith("version.ini"))
                {
                    nativeList.Add(name1);
                }
            }
            // version.ini 放置尾包！！
            nativeList.Add("version.ini");

            // 生成列表文件
            ABHelper.WriteNativeFile(nativePath, nativeList);
            AssetDatabase.Refresh();

            System.GC.Collect();
            Debug.Log("streamingAssets文件生成成功！！文件源路径：" + path);
        }
        public static void AssetMoveout()
        {
            System.GC.Collect();
            System.GC.Collect();

            ReadFile();
            ParseAllDir();
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
            // ProjectSettings
            ABHelper.DirectoryCopy(string.Format("{0}/../{1}", Application.dataPath, "ProjectSettings"), path + "ProjectSettings");
            foreach (var v in RemoveResourcesDir)
            {
                path1 = ResFolder.Replace("Assets", "") + v;
                ABHelper.DirectoryMove(Application.dataPath + path1, path + path1);
            }
            foreach (var v in RemoveAssetsDir)
            {
                path1 = AssetsFolder.Replace("Assets", "") + v;
                ABHelper.DirectoryMove(Application.dataPath + path1, path + path1);
            }

            string path2;
            foreach (var v in NotRemoveFileList)
            {
                path1 = string.Format("{0}/../../../temp/{1}", Application.dataPath, v);
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
            AssetDatabase.SaveAssets();

            System.GC.Collect();
            Debug.Log("资源移出成功！！");
        }
        public static void AssetMovein()
        {
            System.GC.Collect();
            System.GC.Collect();

            ReadFile();
            ParseAllDir();
            RootFolderNmae();
            ParseTheVersion();
            FilesFilter();

            string path = string.Format("{0}/../../../temp/", Application.dataPath);
            if (!Directory.Exists(path))
            {
                return;
            }
            ABHelper.DirectoryMove(path, Application.dataPath + "/");
            ABHelper.DirectoryMove(string.Format("{0}/{1}", Application.dataPath, "ProjectSettings"), string.Format("{0}/../{1}", Application.dataPath, "ProjectSettings"));
            Directory.Delete(string.Format("{0}/{1}", Application.dataPath, "ProjectSettings"), true);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            System.GC.Collect();
            Debug.Log("资源移入成功！！");
        }
    }
}