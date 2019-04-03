using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace LCG
{
    public class ABServerPacker : EditorWindow
    {
        #region 入口
        private static EditorWindow TheWindow = null;
        private static string AssetFolder = "Assets/";
        private static string ResFolder = "Assets/Resources/";

        private static List<string> BundleFolderPath = new List<string>(new string[] { "UI" });
        private static List<string> BundleScenePath = new List<string>(new string[] { "ScenesFinal" });
        private static List<string> BundleLuaPath = new List<string>(new string[] { "Lua" });
        private static List<string> BundleFilePath = new List<string>();

        private static string TheRootFolderName = "ab_tlcg_server";

        [MenuItem("Tools/资源打包服务器")]
        public static void UnityPacker()
        {
            RootFolderName();
            FileBundleList();
            TheWindow = EditorWindow.GetWindow(typeof(ABServerPacker), true, "资源打包服务器");
        }
        public static bool AndroidPacker()
        {
            RootFolderName();
            FileBundleList();
            return StartBuild(BuildTarget.Android);
        }
        public static bool IOSPacker()
        {
            RootFolderName();
            FileBundleList();
            return StartBuild(BuildTarget.iOS);
        }
        public static bool BuildPacker(BuildTarget platform)
        {
            RootFolderName();
            FileBundleList();
            return StartBuild(platform);
        }
        private static void RootFolderName()
        {
            TheRootFolderName = Application.dataPath;
            TheRootFolderName = TheRootFolderName.Substring(0, TheRootFolderName.LastIndexOf("/"));
            TheRootFolderName = "ab_" + TheRootFolderName.Substring(TheRootFolderName.LastIndexOf("/") + 1) + "_server";
        }

        private static void FileBundleList()
        {
            BundleFilePath = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(ResFolder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string fname = d.FullName.Replace(@"/", @"\");
                string folderName = fname.Substring(fname.LastIndexOf(@"\") + 1);
                if (BundleFolderPath.Contains(folderName) || BundleScenePath.Contains(folderName) || BundleLuaPath.Contains(folderName))
                {
                    continue;
                }

                BundleFilePath.Add(folderName);
            }
        }

        void OnGUI()
        {
            GUILayout.Label("AssetBundle自动打包工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("*打包的根目录文件夹:");
            GUILayout.TextArea(TheRootFolderName);

            EditorGUILayout.Space();

            GUILayout.Label("需要文件夹打包的路径");
            for (int i = 0; i < BundleFolderPath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + ResFolder + BundleFolderPath[i]);
            }
            EditorGUILayout.Space();

            GUILayout.Label("需要场景打包的路径");
            for (int i = 0; i < BundleScenePath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + ResFolder + BundleScenePath[i]);
            }
            EditorGUILayout.Space();

            GUILayout.Label("需要lua打包的路径");
            for (int i = 0; i < BundleLuaPath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + AssetFolder + BundleLuaPath[i]);
            }
            EditorGUILayout.Space();

            GUILayout.Label("需要文件打包的路径");
            for (int i = 0; i < BundleFilePath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + ResFolder + BundleFilePath[i]);
            }
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("-----------------------------------华丽分割线----------------------------------------");
            EditorGUILayout.Space();

            if (GUILayout.Button("生成 win 资源包"))
            {
                StartBuild(BuildTarget.StandaloneWindows);
            }
            if (GUILayout.Button("生成 ios 资源包"))
            {
                StartBuild(BuildTarget.iOS);
            }
            if (GUILayout.Button("生成 android 资源包"))
            {
                StartBuild(BuildTarget.Android);
            }
        }
        #endregion

        #region 生成操作

        // 平台导出路径
        private static string PlatformABExportPath = "";
        private static string PlatformName = "";

        private static Dictionary<string, string> CurVersionFileUrlMd5 = null;
        private static Dictionary<string, List<string>> CurVersionManifestList = null;

        private static bool IsNeedFileRes(string fileName)
        {
            if (fileName.EndsWith(".meta") || fileName.EndsWith(".gitkeep") || fileName.EndsWith(".DS_Store"))
            {
                return false;
            }
            if (File.Exists(fileName))
            {
                return true;
            }
            return false;
        }
        private static bool IsScriptFileRes(string fileName)
        {
            if (fileName.EndsWith(".cs") || fileName.EndsWith(".dll"))
            {
                return true;
            }
            return false;
        }
        private static void ClearAssetBundlesName()
        {
            string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < oldABNames.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            }
            Debug.Log("所有assetBundle名称已移除！！！");
        }
        private static void CeateVersionDir(BuildTarget platform)
        {
            // 平台路径
            switch (platform)
            {
                case BuildTarget.StandaloneWindows:
                    PlatformName = ABHelper.WinPlatform;
                    break;
                case BuildTarget.iOS:
                    PlatformName = ABHelper.IosPlatform;
                    break;
                case BuildTarget.Android:
                    PlatformName = ABHelper.AndroidPlatform;
                    break;
                default:
                    Debug.LogError("暂不支持此平台！" + platform.ToString());
                    break;
            }

            // 创建平台文件夹
            PlatformABExportPath = string.Format("{0}/../{1}/{2}", Application.dataPath, TheRootFolderName, PlatformName);
            if (Directory.Exists(PlatformABExportPath))
            {
                Directory.Delete(PlatformABExportPath, true);
            }
            Directory.CreateDirectory(PlatformABExportPath);
        }
        private static void CreatPacker(BuildTarget platform)
        {
            ClearAssetBundlesName();
            AssetDatabase.Refresh();

            CurVersionManifestList = new Dictionary<string, List<string>>();
            List<string> folderBundlePathList = new List<string>();
            List<string> fileBundlePathList = new List<string>();
            List<string> sceneBundlePathList = new List<string>();
            List<string> luaPathList = new List<string>();
            foreach (string path in BundleFolderPath)
            {
                string fullPath = ResFolder + path;
                folderBundlePathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }
            foreach (string path in BundleFilePath)
            {
                string fullPath = ResFolder + path;
                fileBundlePathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }
            foreach (string path in BundleScenePath)
            {
                string fullPath = ResFolder + path;
                sceneBundlePathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }
            foreach (string path in BundleLuaPath)
            {
                string fullPath = AssetFolder + path;
                luaPathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }

            foreach (string path in folderBundlePathList)
            {
                if (!IsNeedFileRes(path))
                {
                    continue;
                }
                // "Assets/Resources/UI/Tips/Tips.bytes"
                string assetPath = path.Replace("\\", "/");
                string abName = ABHelper.GetFileFolderPath(assetPath.Replace(ResFolder, "")) + ".ab";
                AssetImporter.GetAtPath(assetPath).assetBundleName = CreatFileUrlMd5(abName);
            }
            foreach (string path in fileBundlePathList)
            {
                if (!IsNeedFileRes(path))
                {
                    continue;
                }
                string assetPath = path.Replace("\\", "/");
                string abName = ABHelper.GetFileFullPathWithoutFtype(assetPath.Replace(ResFolder, "")) + ".ab";
                AssetImporter.GetAtPath(assetPath).assetBundleName = CreatFileUrlMd5(abName);

                // 依赖文件
                string[] dependPaths = AssetDatabase.GetDependencies(assetPath);
                if (dependPaths.Length > 0)
                {
                    foreach (string path1 in dependPaths)
                    {
                        if (path1.Contains(ResFolder) && assetPath != path1)
                        {
                            if (!CurVersionManifestList.ContainsKey(assetPath))
                            {
                                CurVersionManifestList.Add(assetPath, new List<string>());
                            }
                            CurVersionManifestList[assetPath].Add(path1);
                        }
                    }
                }
            }
            foreach (string path in sceneBundlePathList)
            {
                if (!path.EndsWith(".unity"))
                {
                    continue;
                }
                string assetPath = path.Replace("\\", "/");
                string abName = ABHelper.GetFileFullPathWithoutFtype(assetPath.Replace(ResFolder, "") + ".ab");
                AssetImporter.GetAtPath(assetPath).assetBundleName = CreatFileUrlMd5(abName);

                // 依赖文件
                string[] dependPaths = AssetDatabase.GetDependencies(assetPath);
                if (dependPaths.Length > 0)
                {
                    foreach (string path1 in dependPaths)
                    {
                        if (path1.Contains(ResFolder) && assetPath != path1)
                        {
                            if (!CurVersionManifestList.ContainsKey(assetPath))
                            {
                                CurVersionManifestList.Add(assetPath, new List<string>());
                            }
                            CurVersionManifestList[assetPath].Add(path1);
                        }
                    }
                }
            }
            foreach (string path in luaPathList)
            {
                if (!path.EndsWith(".lua"))
                {
                    continue;
                }
                string filePath = path.Replace("\\", "/");
                CopyLuaFiles(filePath);
            }

            BuildPipeline.BuildAssetBundles(PlatformABExportPath, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, platform);

            ClearAssetBundlesName();
            AssetDatabase.Refresh();

            // ab的依赖文件
            string fileUrl = CreatFileUrlMd5(ABHelper.ManifestFileName);
            ABHelper.WriteManifestFile(PlatformABExportPath + "/" + fileUrl, ResFolder, CurVersionManifestList);

            // 删除不用的manifest
            List<string> allFiles = ABHelper.GetAllFilesPathInDir(PlatformABExportPath);
            foreach (string path in allFiles)
            {
                if (path.EndsWith(".manifest"))
                {
                    File.Delete(path);
                }
                if (path.EndsWith(PlatformName))
                {
                    File.Delete(path);
                }
            }

            // 写入文件
            StringBuilder txt = new StringBuilder();
            foreach (var pair in CurVersionFileUrlMd5)
            {
                txt.Append(pair.Value + ":" + pair.Key + "\r");
            }
            ABHelper.WriteFileByBytes(PlatformABExportPath + "/" + ABHelper.VersionFileName, Encoding.UTF8.GetBytes(txt.ToString().TrimEnd().ToLower()));
        }
        private static string CreatFileUrlMd5(string fileName)
        {
            if (null == CurVersionFileUrlMd5)
            {
                CurVersionFileUrlMd5 = new Dictionary<string, string>();
            }

            fileName = fileName.ToLower();
            string fileUrl = ABHelper.BuildMD5ByString(fileName);
            if (!CurVersionFileUrlMd5.ContainsKey(fileUrl))
            {
                CurVersionFileUrlMd5.Add(fileUrl, fileName);
            }

            return fileUrl;
        }
        private static void CopyLuaFiles(string path)
        {
            string filePath = CreatFileUrlMd5(path.Replace("Assets/", "") + ".txt");
            filePath = (PlatformABExportPath + "/" + filePath).ToLower();
            string fileFolder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // 加密
            byte[] bytes = ABHelper.ReadFileToBytes(path);
            ABHelper.Encrypt(ref bytes);
            ABHelper.WriteFileByBytes(filePath, bytes);
        }

        private static bool StartBuild(BuildTarget platform)
        {
            long start = System.DateTime.Now.Second;
            Debug.Log("开始处理：" + System.DateTime.Now.ToString());

            // 跟目录文件夹名称
            if (string.IsNullOrEmpty(TheRootFolderName))
            {
                RootFolderName();
            }
            // 本版本信息处理
            CeateVersionDir(platform);

            // 打包
            CreatPacker(platform);

            // 关闭面板
            if (null != TheWindow)
            {
                TheWindow.Close();
            }
            // 结束
            Debug.Log("处理结束：" + System.DateTime.Now.ToString());

            return true;
        }

        #endregion
    }
}