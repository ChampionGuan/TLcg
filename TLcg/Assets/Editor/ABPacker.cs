using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Net;

namespace LCG
{
    public class ABPacker : EditorWindow
    {
        #region 入口
        private static EditorWindow TheWindow = null;
        private static string AssetsFolder = "Assets/";
        private static string ResFolder = "Assets/Resources/";
        private static bool Building = false;
        private static bool IgnoreScript = false;

        private static string ScriptDirStr = "Scripts;3rdLibs";
        private static string ScriptDirStr2;
        private static string BundleFolderDirStr = "UI";
        private static string BundleFolderDirStr2;
        private static string BundleLuaDirStr = "Lua";
        private static string BundleLuaDirStr2;
        private static string BundleFileCullingDirStr = "Video;Ignore";
        private static string BundleFileCullingDirStr2;
        private static string DingMsgUrl;
        private static string DingMsgUrl2;

        private static List<string> ScriptFolderPath = new List<string>(new string[] { });
        private static List<string> BundleFolderPath = new List<string>(new string[] { });
        private static List<string> BundleLuaPath = new List<string>(new string[] { });
        private static List<string> BundleFilePath = new List<string>();
        private static List<string> BundleFileCullingPath = new List<string>(new string[] { });

        private static string[] TheVersionNum = new string[4] { "0", "0", "0", "0" };
        private static string TheRootFolderName = "tlcg";
        private static string TheApkFolderName = "tlcg";

        [MenuItem("Tools/资源打包优化版")]
        static void Build()
        {
            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            ParseAllDir();
            TheWindow = EditorWindow.GetWindow(typeof(ABPacker), true, "资源打包优化版");
        }
        public static void BuildAndroidPacker()
        {
            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            ParseAllDir();
            BuildPacker(BuildTarget.Android);
        }
        public static void BuildIosPacker()
        {
            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            ParseAllDir();
            BuildPacker(BuildTarget.iOS);
        }
        public static void BuildWinPacker()
        {
            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            ParseAllDir();
            BuildPacker(BuildTarget.StandaloneWindows);
        }
        public static bool CommandBuild(bool ignoreScript, BuildTarget target)
        {
            IgnoreScript = ignoreScript;

            ReadFile();
            RootFolderNmae();
            ParseTheVersion();
            ParseAllDir();
            return BuildPacker(target);
        }
        private static void RootFolderNmae()
        {
            // TheRootFolderName = Application.dataPath;
            // TheRootFolderName = TheRootFolderName.Substring(0, TheRootFolderName.LastIndexOf("/"));
            // TheRootFolderName = TheRootFolderName.Substring(TheRootFolderName.LastIndexOf("/") + 1);
        }
        private static void ReadFile()
        {
            string s = ABHelper.ReadFile(Application.dataPath + "/Editor/ABPacker.txt");
            string[] sp = s.TrimEnd().Replace("\r", "").Split('\n');
            string[] sp1;
            foreach (var v in sp)
            {
                if (string.IsNullOrEmpty(v))
                {
                    continue;
                }
                sp1 = v.Split('|');

                switch (sp1[0])
                {
                    case "ScriptDir": ScriptDirStr = sp1[1]; break;
                    case "BundleFolderDir": BundleFolderDirStr = sp1[1]; break;
                    case "BundleLuaDir": BundleLuaDirStr = sp1[1]; break;
                    case "BundleFileCullingDir": BundleFileCullingDirStr = sp1[1]; break;
                    case "DingMsgUrl": DingMsgUrl = sp1[1]; break;
                    default: break;
                }
            }
        }
        private static void WriteFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ScriptDir|" + ScriptDirStr + "\n");
            sb.Append("BundleFolderDir|" + BundleFolderDirStr + "\n");
            sb.Append("BundleLuaDir|" + BundleLuaDirStr + "\n");
            sb.Append("BundleFileCullingDir|" + BundleFileCullingDirStr + "\n");
            sb.Append("DingMsgUrl|" + DingMsgUrl + "\n");
            ABHelper.WriteFile(Application.dataPath + "/Editor/ABPacker.txt", sb.ToString().TrimEnd());
        }
        private static void ParseAllDir()
        {
            ParseDir(ScriptDirStr, ScriptFolderPath);
            ParseDir(BundleFolderDirStr, BundleFolderPath);
            ParseDir(BundleLuaDirStr, BundleLuaPath);
            ParseDir(BundleFileCullingDirStr, BundleFileCullingPath);
            FileBundleList();
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
        private static void ParseTheVersion()
        {
            List<string> version = ABHelper.ReadVersionIdFile();
            TheVersionNum = ABHelper.VersionNumSplit(version[0]);
            TheRootFolderName = version[1];
            TheApkFolderName = version[2];
        }
        private static void SaveTheVersion()
        {
            int num;
            if (!int.TryParse(TheVersionNum[3], out num))
            {
                Debug.LogError("资源单号错误！！！！！");
            }
            CurVersionId = ABHelper.VersionNumCombine(TheVersionNum[0], TheVersionNum[1], CurVersionNum.ToString(), TheVersionNum[3]);
            ABHelper.WriteVersionIdFile(CurVersionId, TheRootFolderName, TheApkFolderName);
        }
        private static void BuildResult(bool r, string e = "")
        {
            if (string.IsNullOrEmpty(DingMsgUrl))
            {
                return;
            }

            string js = r ? "成功" : string.IsNullOrEmpty(e) ? "失败" : "失败。" + e;
            js = string.Format("状态：{0}\n平台：{1}\n版本标识：{2}", js, CurBuildTarget.ToString(), TheRootFolderName);
            js = r ? string.Format("{0}\n版本id：{1}", js, CurVersionId) : js;
            js = "{ \"msgtype\": \"text\", \"text\": {\"content\": \"" + js + "\"}}";

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };
            WebRequest request = WebRequest.Create(DingMsgUrl);
            request.Method = "POST";
            request.ContentType = "application/json";

            Debug.Log(js);

            if (string.IsNullOrEmpty(js))
            {
                request.ContentLength = 0;
            }
            else
            {
                byte[] bs = Encoding.UTF8.GetBytes(js);
                request.ContentLength = bs.Length;
                Stream newStream = request.GetRequestStream();
                newStream.Write(bs, 0, bs.Length);
                newStream.Close();
            }
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            Encoding encode = Encoding.UTF8;
            StreamReader reader = new StreamReader(stream, encode);
            Debug.Log(reader.ReadToEnd());
        }
        private static void UploadAB()
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string path = new DirectoryInfo(string.Format("{0}/../../../", Application.dataPath)).FullName;
                string batTool = "scps3upload.sh";
                if (!File.Exists(string.Format("{0}/../../../{1}", Application.dataPath, "scps3")))
                {
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("#!/bin/sh");
                sb.AppendLine(string.Format("./scps3 {0} {1}", PlatformABExportRelativePath + "/" + CurVersionNum, PlatformABExportRelativePath + "/"));
                ABHelper.WriteFile(path + batTool, sb.ToString());

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = path;
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = batTool;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.Start();
                proc.WaitForExit();
                Debug.Log(string.Format("ab上传:{0}", proc.StandardOutput.ReadToEnd()));
                proc.Close();

                StringBuilder sb2 = new StringBuilder();
                sb2.AppendLine("#!/bin/sh");
                for (int i = 0; i < CurVersionNum; i++)
                {
                    string p = string.Format("{0}/HotterZip/{1}-{2}", PlatformABExportRelativePath, i, CurVersionNum);
                    sb2.AppendLine(string.Format("./scps3 {0}.zip {1}.zip", p, p));
                    sb2.AppendLine(string.Format("./scps3 {0}.ini {1}.ini", p, p));
                }
                ABHelper.WriteFile(path + batTool, sb2.ToString());

                System.Diagnostics.Process proc2 = new System.Diagnostics.Process();
                proc2.StartInfo.WorkingDirectory = path;
                proc2.StartInfo.FileName = "/bin/bash";
                proc2.StartInfo.Arguments = batTool;
                proc2.StartInfo.UseShellExecute = false;
                proc2.StartInfo.RedirectStandardOutput = true;
                proc2.StartInfo.RedirectStandardInput = true;
                proc2.Start();
                proc2.WaitForExit();
                Debug.Log(string.Format("ab上传:{0}", proc2.StandardOutput.ReadToEnd()));
                proc2.Close();

                Debug.Log("上传完成！！！！");
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string path = new DirectoryInfo(string.Format("{0}/../../../", Application.dataPath)).FullName;
                string batTool = "scps3upload.bat";
                if (!File.Exists(string.Format("{0}/../../../{1}", Application.dataPath, "scps3.exe")))
                {
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("@echo off");
                sb.AppendLine(string.Format("scps3.exe {0} {1}", PlatformABExportRelativePath + "/" + CurVersionNum, PlatformABExportRelativePath) + "/");
                ABHelper.WriteFile(path + batTool, sb.ToString());

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = path;
                proc.StartInfo.FileName = batTool;
                proc.Start();
                proc.WaitForExit();
                proc.Close();

                StringBuilder sb2 = new StringBuilder();
                sb2.AppendLine("@echo off");
                for (int i = 0; i < CurVersionNum; i++)
                {
                    string p = string.Format("{0}/HotterZip/{1}-{2}", PlatformABExportRelativePath, i, CurVersionNum);
                    sb2.AppendLine(string.Format("scps3.exe {0}.zip {1}.zip", p, p));
                    sb2.AppendLine(string.Format("scps3.exe {0}.ini {1}.ini", p, p));
                }
                ABHelper.WriteFile(path + batTool, sb2.ToString());

                System.Diagnostics.Process proc2 = new System.Diagnostics.Process();
                proc2.StartInfo.WorkingDirectory = path;
                proc2.StartInfo.FileName = batTool;
                proc2.Start();
                proc2.WaitForExit();
                proc2.Close();

                Debug.Log("上传完成！！！！");
            }
        }

        private static void FileBundleList()
        {
            if (null == BundleFilePath)
            {
                BundleFilePath = new List<string>();
            }
            BundleFilePath.Clear();

            DirectoryInfo dir = new DirectoryInfo(ResFolder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string fname = d.FullName.Replace(@"/", @"\");
                string folderName = fname.Substring(fname.LastIndexOf(@"\") + 1);
                if (BundleFolderPath.Contains(folderName) || BundleLuaPath.Contains(folderName))
                {
                    continue;
                }

                BundleFilePath.Add(folderName);
            }

            foreach (var v in BundleFileCullingPath)
            {
                if (BundleFilePath.Contains(v))
                {
                    BundleFilePath.Remove(v);
                }
            }
        }
        void OnGUI()
        {
            GUILayout.Label("AssetBundle自动打包工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            GUILayout.Label("钉钉机器人");
            DingMsgUrl = GUILayout.TextField(DingMsgUrl);
            if (DingMsgUrl2 != DingMsgUrl)
            {
                DingMsgUrl2 = DingMsgUrl;
                WriteFile();
                ParseAllDir();
            }
            EditorGUILayout.Space();
            GUILayout.Label("检测cs、dll的文件夹（以半角分号隔开）");
            ScriptDirStr = GUILayout.TextField(ScriptDirStr);
            if (ScriptDirStr2 != ScriptDirStr)
            {
                ScriptDirStr2 = ScriptDirStr;
                WriteFile();
                ParseAllDir();
            }
            GUILayout.Label("需要文件夹打包的文件夹（以半角分号隔开）");
            BundleFolderDirStr = GUILayout.TextField(BundleFolderDirStr);
            if (BundleFolderDirStr2 != BundleFolderDirStr)
            {
                BundleFolderDirStr2 = BundleFolderDirStr;
                WriteFile();
                ParseAllDir();
            }
            GUILayout.Label("需要lua打包的文件夹（以半角分号隔开）");
            BundleLuaDirStr = GUILayout.TextField(BundleLuaDirStr);
            if (BundleLuaDirStr2 != BundleLuaDirStr)
            {
                BundleLuaDirStr2 = BundleLuaDirStr;
                WriteFile();
                ParseAllDir();
            }
            GUILayout.Label("不需要打ab包的文件夹（以半角分号隔开）");
            BundleFileCullingDirStr = GUILayout.TextField(BundleFileCullingDirStr);
            if (BundleFileCullingDirStr2 != BundleFileCullingDirStr)
            {
                BundleFileCullingDirStr2 = BundleFileCullingDirStr;
                WriteFile();
                ParseAllDir();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            GUILayout.Label("~~~~~~~~~~~~~~~~~~~~~~~~~分割线~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            GUILayout.Label("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label("*打包的根目录文件夹:");
            GUILayout.TextArea(TheRootFolderName);

            GUILayout.Label("*版号第一位:");
            TheVersionNum[0] = GUILayout.TextField(TheVersionNum[0]);

            GUILayout.Label("*版号第二位:");
            TheVersionNum[1] = GUILayout.TextField(TheVersionNum[1]);

            GUILayout.Label("*版号第四位:");
            TheVersionNum[3] = GUILayout.TextField(TheVersionNum[3]);

            EditorGUILayout.Space();

            GUILayout.Label("检测cs、dll的路径");
            for (int i = 0; i < ScriptFolderPath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + AssetsFolder + ScriptFolderPath[i]);
            }
            EditorGUILayout.Space();

            GUILayout.Label("需要文件夹打包的路径");
            for (int i = 0; i < BundleFolderPath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + ResFolder + BundleFolderPath[i]);
            }
            EditorGUILayout.Space();

            GUILayout.Label("需要lua打包的路径");
            for (int i = 0; i < BundleLuaPath.Count; i++)
            {
                EditorGUILayout.LabelField((i + 1).ToString() + ":" + AssetsFolder + BundleLuaPath[i]);
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

            if (GUILayout.Button("生成 Win 资源包"))
            {
                BuildPacker(BuildTarget.StandaloneWindows);
            }
            if (GUILayout.Button("生成 ios 资源包"))
            {
                BuildPacker(BuildTarget.iOS);
            }
            if (GUILayout.Button("生成 android 资源包"))
            {
                BuildPacker(BuildTarget.Android);
            }
        }
        #endregion

        #region 压缩
        private static long filePreSize = 0;
        private static long fileZipSize = 0;
        private static bool ZipSuccess = true;

        private static void DirectoryFailureHandler(object sender, ICSharpCode.SharpZipLib.Core.ScanFailureEventArgs e)
        {
            ZipSuccess = false;
            Debug.LogError("zip directoty fail :" + e.Name);
        }
        private static void FileFailureHandler(object sender, ICSharpCode.SharpZipLib.Core.ScanFailureEventArgs e)
        {
            ZipSuccess = false;
            Debug.LogError("zip file fail :" + e.Name);
        }
        private static void ProgressHandler(object sender, ICSharpCode.SharpZipLib.Core.ProgressEventArgs e)
        {
            fileZipSize += e.Processed;
            float percent = (float)fileZipSize / filePreSize;
            EditorUtility.DisplayProgressBar("提示", "版本压缩中..", percent);
        }
        #endregion

        #region 生成操作

        // 平台导出路径
        private static string PlatformABExportPath = "";
        // 平台导出的相对路径
        private static string PlatformABExportRelativePath = "";
        // 本次版本需更新资源路径
        private static string ABExportHotterZipPath = "";

        // 本次版本号
        private static int CurVersionNum = 0;
        // 本次版号
        private static string CurVersionId;
        // 本次版本导出路径
        private static string CurVersionABExportPath = "";
        // 版本平台
        private static BuildTarget CurBuildTarget;

        // 本次版本的一些信息
        private static Dictionary<string, List<string>> CurVersionDependenciesList = null;
        private static Dictionary<string, List<string>> CurVersionManifestList = null;
        private static Dictionary<string, List<string>> CurVersionList = null;
        private static Dictionary<string, string> CurVersionMd5List = null;
        private static Dictionary<string, int> CurVersionRCList = null;
        private static Dictionary<string, ResType> CurVersionFileType = null;
        private static Dictionary<string, string> CurVersionFileUrlMd5 = null;
        private static List<string> CurVersionRmFileList = null;

        private enum ResType { None, Folder, Lua, File, Depend, Script }
        private static bool IsNeedFileRes(string fileName)
        {
            if (fileName.EndsWith(".cs") || fileName.EndsWith(".meta") || fileName.EndsWith(".gitkeep") || fileName.EndsWith(".DS_Store"))
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
        private static System.Threading.Thread GCCollect()
        {
            // 开启线程
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                while (true)
                {
                    System.GC.Collect();
                    System.Threading.Thread.Sleep(5000);
                }
            });
            thread.IsBackground = true;

            return thread;
        }
        private static void BuildFailure()
        {
            if (Directory.Exists(CurVersionABExportPath))
            {
                Directory.Delete(CurVersionABExportPath, true);
            }
        }
        private static void HandleScriptAssets(string path)
        {
            string filePath = path.Replace("\\", "/");
            // 类型
            if (!CurVersionFileType.ContainsKey(filePath))
            {
                CurVersionFileType.Add(filePath, ResType.None);
            }
            CurVersionFileType[filePath] = ResType.Script;

            // 依赖
            if (!CurVersionDependenciesList.ContainsKey(filePath))
            {
                CurVersionDependenciesList.Add(filePath, new List<string>() { filePath });
            }

            // md5
            if (!CurVersionMd5List.ContainsKey(filePath))
            {
                CurVersionMd5List.Add(filePath, ABHelper.BuildMD5ByFile(filePath));
            }

            // 引用次数
            if (!CurVersionRCList.ContainsKey(filePath))
            {
                CurVersionRCList.Add(filePath, 0);
            }
            CurVersionRCList[filePath]++;

            System.GC.Collect();
        }
        private static void HandleFolderAssets(string path)
        {
            string filePath = path.Replace("\\", "/");
            string filePath2 = ABHelper.GetFileFolderPath(filePath);

            // 文件类型
            if (!CurVersionFileType.ContainsKey(filePath))
            {
                CurVersionFileType.Add(filePath, ResType.None);
            }
            CurVersionFileType[filePath] = ResType.Folder;
            // 文件夹类型
            if (!CurVersionFileType.ContainsKey(filePath2))
            {
                CurVersionFileType.Add(filePath2, ResType.None);
            }
            CurVersionFileType[filePath2] = ResType.Folder;

            // 依赖
            if (!CurVersionDependenciesList.ContainsKey(filePath2))
            {
                CurVersionDependenciesList.Add(filePath2, new List<string>());
            }
            CurVersionDependenciesList[filePath2].Add(filePath);

            // md5
            if (!CurVersionMd5List.ContainsKey(filePath))
            {
                CurVersionMd5List.Add(filePath, ABHelper.BuildMD5ByFile(filePath));
            }

            // 引用次数
            if (!CurVersionRCList.ContainsKey(filePath))
            {
                CurVersionRCList.Add(filePath, 0);
            }
            CurVersionRCList[filePath]++;

            System.GC.Collect();
        }
        private static void HandleLuaAssets(string path)
        {
            string filePath = path.Replace("\\", "/");
            // 类型
            if (!CurVersionFileType.ContainsKey(filePath))
            {
                CurVersionFileType.Add(filePath, ResType.None);
            }
            CurVersionFileType[filePath] = ResType.Lua;

            // 依赖
            if (!CurVersionDependenciesList.ContainsKey(filePath))
            {
                CurVersionDependenciesList.Add(filePath, new List<string>() { filePath });
            }

            // md5
            if (!CurVersionMd5List.ContainsKey(filePath))
            {
                CurVersionMd5List.Add(filePath, ABHelper.BuildMD5ByFile(filePath));
            }

            // 引用次数
            if (!CurVersionRCList.ContainsKey(filePath))
            {
                CurVersionRCList.Add(filePath, 0);
            }
            CurVersionRCList[filePath]++;

            System.GC.Collect();
        }
        private static void HandleFileAssets(string path)
        {
            string filePath = path.Replace("\\", "/");

            // 类型
            if (!CurVersionFileType.ContainsKey(filePath))
            {
                CurVersionFileType.Add(filePath, ResType.None);
            }
            CurVersionFileType[filePath] = ResType.File;

            // 依赖
            string[] dependPaths = AssetDatabase.GetDependencies(filePath);
            if (!CurVersionDependenciesList.ContainsKey(filePath))
            {
                CurVersionDependenciesList.Add(filePath, new List<string>());
            }
            CurVersionDependenciesList[filePath].AddRange(dependPaths);

            // 依赖文件
            if (dependPaths.Length > 0)
            {
                foreach (string path1 in dependPaths)
                {
                    // md5
                    if (!CurVersionMd5List.ContainsKey(path1))
                    {
                        CurVersionMd5List.Add(path1, ABHelper.BuildMD5ByFile(path1));
                    }

                    // 引用次数
                    if (!CurVersionRCList.ContainsKey(path1))
                    {
                        CurVersionRCList.Add(path1, 0);
                    }
                    CurVersionRCList[path1]++;

                    // 依赖文件的类型
                    if (!CurVersionFileType.ContainsKey(path1))
                    {
                        CurVersionFileType.Add(path1, ResType.Depend);
                    }
                }
            }

            System.GC.Collect();
        }
        private static void OutputMainfestFile()
        {
            foreach (var v in CurVersionDependenciesList)
            {
                // 文件夹打包不存在依赖关系！！
                if (CurVersionFileType[v.Key] == ResType.Folder)
                {
                    continue;
                }

                // 只有resource下文件存在依赖关系！！
                string filePath2 = v.Key.Substring(ResFolder.Length);
                if (v.Key.StartsWith(ResFolder))
                {
                    foreach (var m in v.Value)
                    {
                        if (m == v.Key)
                        {
                            continue;
                        }
                        if (IsScriptFileRes(m))
                        {
                            continue;
                        }
                        if (m.StartsWith(ResFolder))
                        {
                            // 文件夹打包不存在依赖关系！！

                            if (!CurVersionManifestList.ContainsKey(filePath2))
                            {
                                CurVersionManifestList.Add(filePath2, new List<string>());
                            }
                            if (CurVersionFileType[m] == ResType.Folder)
                            {
                                CurVersionManifestList[filePath2].Add(ABHelper.GetFileFolderPath(m).Substring(ResFolder.Length));
                            }
                            else
                            {
                                CurVersionManifestList[filePath2].Add(m.Substring(ResFolder.Length));
                            }
                        }
                        else if (CurVersionRCList.ContainsKey(m) && CurVersionRCList[m] > 1)
                        {
                            if (!CurVersionManifestList.ContainsKey(filePath2))
                            {
                                CurVersionManifestList.Add(filePath2, new List<string>());
                            }
                            CurVersionManifestList[filePath2].Add(m.Substring(AssetsFolder.Length));
                        }
                    }
                }
            }
        }
        private static void CreatFileDependencies()
        {
            List<string> scriptPathList = new List<string>();
            List<string> folderBundlePathList = new List<string>();
            List<string> fileBundlePathList = new List<string>();
            List<string> luaPathList = new List<string>();
            foreach (string path in ScriptFolderPath)
            {
                string fullPath = AssetsFolder + path;
                scriptPathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }
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
            foreach (string path in BundleLuaPath)
            {
                string fullPath = AssetsFolder + path;
                luaPathList.AddRange(ABHelper.GetAllFilesPathInDir(fullPath));
            }
            CurVersionFileType = new Dictionary<string, ResType>();
            CurVersionDependenciesList = new Dictionary<string, List<string>>();
            CurVersionManifestList = new Dictionary<string, List<string>>();
            CurVersionMd5List = new Dictionary<string, string>();
            CurVersionRCList = new Dictionary<string, int>();
            CurVersionRmFileList = new List<string>();
            foreach (string path in scriptPathList)
            {
                if (!IsScriptFileRes(path))
                {
                    continue;
                }
                HandleScriptAssets(path);
            }
            foreach (string path in folderBundlePathList)
            {
                if (!IsNeedFileRes(path))
                {
                    continue;
                }
                HandleFolderAssets(path);
            }
            foreach (string path in luaPathList)
            {
                if (!path.EndsWith(".lua"))
                {
                    continue;
                }
                HandleLuaAssets(path);
            }
            foreach (string path in fileBundlePathList)
            {
                if (!IsNeedFileRes(path))
                {
                    continue;
                }
                HandleFileAssets(path);
            }

            OutputMainfestFile();
            // 版本所需资源的md5码保存
            ABHelper.WriteMd5File(CurVersionABExportPath + ABHelper.Md5FileName, CurVersionMd5List, CurVersionRCList);
            // 版本所需资源的依赖关系
            ABHelper.WriteDependFile(CurVersionABExportPath + ABHelper.DependFileName, CurVersionDependenciesList);

            System.GC.Collect();
        }
        private static void CeateVersionDir(BuildTarget platform)
        {
            // 平台路径
            string platformName = "";
            switch (platform)
            {
                case BuildTarget.StandaloneWindows:
                    platformName = ABHelper.WinPlatform;
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                    break;
                case BuildTarget.iOS:
                    platformName = ABHelper.IosPlatform;
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;
                case BuildTarget.Android:
                    platformName = ABHelper.AndroidPlatform;
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
                default:
                    Debug.LogError("暂不支持此平台！" + platform.ToString());
                    break;
            }

            // 创建平台文件夹
            PlatformABExportRelativePath = string.Format("assetBundle/{0}/{1}/{2}.{3}", TheRootFolderName, platformName, TheVersionNum[0], TheVersionNum[1]);
            PlatformABExportPath = string.Format("{0}/../../../{1}", Application.dataPath, PlatformABExportRelativePath);
            if (!Directory.Exists(PlatformABExportPath))
            {
                Directory.CreateDirectory(PlatformABExportPath);
            }

            // 创建压缩文件夹
            ABExportHotterZipPath = PlatformABExportPath + "/HotterZip/";
            if (!Directory.Exists(ABExportHotterZipPath))
            {
                Directory.CreateDirectory(ABExportHotterZipPath);
            }

            // 创建版本文件夹
            CurVersionNum = 0;
            while (true)
            {
                string versionFilePath = PlatformABExportPath + "/" + CurVersionNum.ToString() + "/" + ABHelper.VersionFileName;
                if (File.Exists(versionFilePath))
                {
                    CurVersionNum++;
                }
                else
                {
                    break;
                }
            }

            CurVersionABExportPath = PlatformABExportPath + "/" + CurVersionNum.ToString() + "/";
            if (Directory.Exists(CurVersionABExportPath))
            {
                Directory.Delete(CurVersionABExportPath, true);
            }
            Directory.CreateDirectory(CurVersionABExportPath);

            System.GC.Collect();
            System.GC.Collect();
        }
        private static string CreatFileUrlMd5(string fileName)
        {
            if (null == CurVersionFileUrlMd5)
            {
                CurVersionFileUrlMd5 = new Dictionary<string, string>();
            }

            string fileUrl = fileName.Replace("/", "-").ToLower();
            //string fileUrl = ABHelper.BuildMD5ByString(fileName);
            if (!CurVersionFileUrlMd5.ContainsKey(fileUrl))
            {
                CurVersionFileUrlMd5.Add(fileUrl, fileName);
            }

            return fileUrl;
        }
        private static void CopyLuaFiles(string path)
        {
            string filePath = CreatFileUrlMd5(path.Substring(AssetsFolder.Length) + ".txt");
            filePath = (CurVersionABExportPath + filePath).ToLower();
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
            ABHelper.Encrypt(ref bytes); //RC4 加密lua文件
            ABHelper.WriteFileByBytes(filePath, bytes);
        }
        private static void AddUpdateFile(List<string> list, string pathName)
        {
            if (list.Contains(pathName))
            {
                return;
            }
            list.Add(pathName);
        }
        private static bool CreatABPacker(BuildTarget platform)
        {
            List<string> updateFileList = new List<string>();
            List<string> updateScriptList = new List<string>();

            Dictionary<string, List<string>> lastVersionMd5List = ABHelper.ReadMd5FileByPath(PlatformABExportPath + "/" + (CurVersionNum - 1) + "/" + ABHelper.Md5FileName);
            Dictionary<string, List<string>> lastVersionDependenciesList = ABHelper.ReadDependFileByPath(PlatformABExportPath + "/" + (CurVersionNum - 1) + "/" + ABHelper.DependFileName);

            foreach (KeyValuePair<string, List<string>> pair in CurVersionDependenciesList)
            {
                string pathName = pair.Key;

                // 冗余资源处理
                foreach (string depend in pair.Value)
                {
                    // 非resource下资源！！
                    if (depend.StartsWith(ResFolder))
                    {
                        continue;
                    }
                    if (!CurVersionRCList.ContainsKey(depend))
                    {
                        continue;
                    }
                    string dependLower = depend.ToLower();

                    // 无上一版本,本次版本引用数>1
                    if (!lastVersionMd5List.ContainsKey(dependLower) && CurVersionRCList[depend] > 1)
                    {
                        if (IsScriptFileRes(depend))
                            AddUpdateFile(updateScriptList, depend);
                        else
                            AddUpdateFile(updateFileList, depend);
                        continue;
                    }

                    // 此版本引用数<=1,且上次版本引用数>1
                    if (CurVersionRCList[depend] <= 1 && lastVersionMd5List.ContainsKey(dependLower) && int.Parse(lastVersionMd5List[dependLower][1]) > 1)
                    {
                        if (IsScriptFileRes(pathName))
                            AddUpdateFile(updateScriptList, pathName);
                        else
                            AddUpdateFile(updateFileList, pathName);
                    }

                    // 此版本引用数>1
                    if (CurVersionRCList[depend] > 1)
                    {
                        if (!lastVersionMd5List.ContainsKey(dependLower) || int.Parse(lastVersionMd5List[dependLower][1]) <= 1)
                        {
                            if (IsScriptFileRes(pathName))
                                AddUpdateFile(updateScriptList, pathName);
                            else
                                AddUpdateFile(updateFileList, pathName);

                            if (IsScriptFileRes(depend))
                                AddUpdateFile(updateScriptList, depend);
                            else
                                AddUpdateFile(updateFileList, depend);
                        }
                        else if (lastVersionMd5List[dependLower][0] != CurVersionMd5List[depend])
                        {
                            if (IsScriptFileRes(depend))
                                AddUpdateFile(updateScriptList, depend);
                            else
                                AddUpdateFile(updateFileList, depend);
                        }
                    }
                    // <=1，但是资源更新了！！
                    else if (!lastVersionMd5List.ContainsKey(dependLower) || lastVersionMd5List[dependLower][0] != CurVersionMd5List[depend])
                    {
                        if (IsScriptFileRes(pathName))
                            AddUpdateFile(updateScriptList, pathName);
                        else
                            AddUpdateFile(updateFileList, pathName);
                    }
                }

                // 新资源时
                if (!lastVersionDependenciesList.ContainsKey(pathName.ToLower()))
                {
                    if (IsScriptFileRes(pathName))
                        AddUpdateFile(updateScriptList, pathName);
                    else
                        AddUpdateFile(updateFileList, pathName);
                    continue;
                }
                // 依赖文件
                foreach (string depend in pair.Value)
                {
                    string dependLower = depend.ToLower();
                    // 新增了依赖文件时
                    if (!lastVersionDependenciesList[pathName.ToLower()].Contains(dependLower))
                    {
                        if (IsScriptFileRes(pathName))
                            AddUpdateFile(updateScriptList, pathName);
                        else
                            AddUpdateFile(updateFileList, pathName);
                        break;
                    }
                    // 老文件有更新时
                    else if (CurVersionMd5List[depend].ToLower() != lastVersionMd5List[dependLower][0])
                    {
                        bool need = false;
                        // 自身更改
                        if (!need && depend == pathName)
                        {
                            need = true;
                        }
                        // 场景依赖的预设更改，场景需要重新打包！！坑点
                        if (!need && depend.StartsWith(ResFolder) && depend.EndsWith(".prefab"))
                        {
                            need = true;
                        }
                        // 文件夹打包的资源更改，文件重新打包！！！
                        if (!need && depend.StartsWith(ResFolder) && CurVersionFileType[depend] == ResType.Folder)
                        {
                            need = true;
                        }

                        if (need)
                        {
                            if (IsScriptFileRes(pathName))
                                AddUpdateFile(updateScriptList, pathName);
                            else
                                AddUpdateFile(updateFileList, pathName);
                            break;
                        }
                    }
                }
            }
            System.GC.Collect();

            // 检测cs和dll脚本
            if (updateScriptList.Count > 0 && !IgnoreScript)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var pair in updateScriptList)
                {
                    sb.AppendLine(pair);
                }
                string sbb = "";
                if (sb.Length > 1500)
                {
                    sbb = sb.ToString().Substring(0, 1500);
                    sbb = sbb + "......";
                }
                else
                {
                    sbb = sb.ToString();
                }
                if (EditorUtility.DisplayDialog("提示", "检测到有cs等脚本的更新！！\n “停止”则中断打包，“忽略”则继续。如下：\n" + sbb, "停止", "忽略"))
                {
                    throw new Exception("打包中断!!!!!!!!!!");
                }
            }
            System.GC.Collect();

            if (updateFileList.Count > 0)
            {
                ClearAssetBundlesName();
                AssetDatabase.Refresh();

                string[] depends;
                string fileName = "";
                foreach (var path in updateFileList)
                {
                    if (CurVersionFileType[path] == ResType.Lua)
                    {
                        CopyLuaFiles(path);
                    }
                    else if (CurVersionFileType[path] == ResType.Folder)
                    {
                        foreach (var path2 in CurVersionDependenciesList[path])
                        {
                            fileName = (ABHelper.GetFileFolderPath(path2).Substring(ResFolder.Length)) + ".ab";
                            fileName = CreatFileUrlMd5(fileName);
                            AssetImporter.GetAtPath(path2).assetBundleName = fileName;
                        }
                    }
                    else
                    {
                        depends = AssetDatabase.GetDependencies(path);
                        foreach (var path2 in depends)
                        {
                            if (path2 == path || (CurVersionRCList.ContainsKey(path2) && CurVersionRCList[path2] > 1))
                            {
                                if (CurVersionFileType[path2] == ResType.File)
                                {
                                    fileName = (ABHelper.GetFileFullPathWithoutFtype(path2).Substring(ResFolder.Length)) + ".ab";
                                    fileName = CreatFileUrlMd5(fileName);
                                    AssetImporter.GetAtPath(path2).assetBundleName = fileName;
                                }
                                else if (CurVersionFileType[path2] == ResType.Depend)
                                {
                                    fileName = (ABHelper.GetFileFullPathWithoutFtype(path2).Substring(AssetsFolder.Length)) + ".ab";
                                    fileName = CreatFileUrlMd5(fileName);
                                    AssetImporter.GetAtPath(path2).assetBundleName = fileName;
                                }
                                else
                                {
                                    fileName = "";
                                }
                                // 待移除的文件
                                if (!string.IsNullOrEmpty(fileName) && !updateFileList.Contains(path2) && !CurVersionRmFileList.Contains(fileName))
                                {
                                    CurVersionRmFileList.Add(fileName);
                                }
                            }
                        }
                    }
                }
                try
                {
                    // 开启线程
                    System.Threading.Thread thread = GCCollect();
                    thread.Start();

                    // 生成ab文件
                    BuildPipeline.BuildAssetBundles(CurVersionABExportPath, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, platform);

                    // 移除不需要更新文件
                    foreach (var path in CurVersionRmFileList)
                    {
                        string path2 = CurVersionABExportPath + path;
                        if (File.Exists(path2))
                        {
                            File.Delete(path2);
                        }
                    }

                    thread.Abort();
                    System.GC.Collect();
                    System.GC.Collect();
                }
                catch (Exception e)
                {
                    BuildFailure();
                    Debug.LogError("打包异常！！" + e.Message);
                }
                System.Threading.Thread.Sleep(1000);

                ClearAssetBundlesName();
                AssetDatabase.Refresh();

                System.GC.Collect();
            }
            else
            {
                BuildFailure();
            }

            bool result = updateFileList.Count > 0;
            if (result)
            {
                // 删除不用的manifest
                List<string> allFiles = ABHelper.GetAllFilesPathInDir(PlatformABExportPath);
                foreach (string path in allFiles)
                {
                    if (path.EndsWith(".manifest"))
                    {
                        File.Delete(path);
                    }
                    if (ABHelper.GetFileNameWithoutSuffix(path.Replace("\\", "/")) == CurVersionNum.ToString())
                    {
                        File.Delete(path);
                    }
                }
            }
            System.GC.Collect();
            System.GC.Collect();

            return result;
        }
        private static void CreatVersionTxt()
        {
            // 版号文件
            string fileUrl = CreatFileUrlMd5(ABHelper.VersionNumFileName);
            ABHelper.WriteVersionNumFile(CurVersionABExportPath + fileUrl, ABHelper.VersionNumCombine(TheVersionNum[0], TheVersionNum[1], CurVersionNum.ToString(), TheVersionNum[3]));
            // ab的依赖文件
            fileUrl = CreatFileUrlMd5(ABHelper.ManifestFileName);
            ABHelper.WriteManifestFile(CurVersionABExportPath + fileUrl, CurVersionManifestList);
            // 创建版本文件
            CurVersionList = ABHelper.ReadVersionFileByPath(PlatformABExportPath + "/" + (CurVersionNum - 1) + "/" + ABHelper.VersionFileName);
            List<string> filePaths = ABHelper.GetAllFilesPathInDir(CurVersionABExportPath);
            foreach (string path in filePaths)
            {
                if (path.EndsWith(".manifest"))
                {
                    continue;
                }
                string path2 = path.Replace("\\", "/").Replace(CurVersionABExportPath, "").ToLower();
                if (path2.Equals(CurVersionNum.ToString()) || path2.Equals(ABHelper.DependFileName) || path2.Equals(ABHelper.Md5FileName))
                {
                    continue;
                }
                if (!CurVersionFileUrlMd5.ContainsKey(path2))
                {
                    continue;
                }
                string value = CurVersionFileUrlMd5[path2].ToLower();
                if (CurVersionList.ContainsKey(value))
                {
                    CurVersionList.Remove(value);
                }
                CurVersionList.Add(value, new List<string>() { ABHelper.BuildMD5ByFile(path), CurVersionNum.ToString(), ABHelper.FileSize(path).ToString(), path2 });
            }
            ABHelper.WriteVersionFile(CurVersionABExportPath + ABHelper.VersionFileName, CurVersionList);

            System.GC.Collect();
        }
        private static void CreatHotterZip(int preVersionNum)
        {
            if (preVersionNum == CurVersionNum)
            {
                return;
            }
            Debug.Log("Update Version From :" + preVersionNum + " to " + CurVersionNum);

            Dictionary<string, List<string>> preVersionMd5 = ABHelper.ReadVersionFileByPath(PlatformABExportPath + "/" + preVersionNum.ToString() + "/" + ABHelper.VersionFileName);

            // 需要更新的文件
            Dictionary<string, string> updateFiles = new Dictionary<string, string>();
            foreach (KeyValuePair<string, List<string>> pair in CurVersionList)
            {
                string path = pair.Key;
                string md5 = pair.Value[0];
                string verId = pair.Value[1];
                string name = pair.Value[3];
                List<string> oldMd5Info = null;
                if (preVersionMd5.TryGetValue(path, out oldMd5Info))
                {
                    if (oldMd5Info[0] != md5 || oldMd5Info[1] != verId)
                    {
                        updateFiles.Add(name, verId);
                    }
                }
                else
                {
                    updateFiles.Add(name, verId);
                }
            }
            // version添加进来
            if (File.Exists(CurVersionABExportPath + ABHelper.VersionFileName) && !updateFiles.ContainsKey(ABHelper.VersionFileName))
            {
                updateFiles.Add(ABHelper.VersionFileName, CurVersionNum.ToString());
            }

            // 生成更新文件夹
            string zipFolder = ABExportHotterZipPath + preVersionNum + "-" + CurVersionNum;
            if (Directory.Exists(zipFolder))
            {
                Directory.Delete(zipFolder, true);
            }
            Directory.CreateDirectory(zipFolder);

            // 复制到更新文件夹下
            foreach (var fileInfo in updateFiles)
            {
                string sourceFile = PlatformABExportPath + "/" + fileInfo.Value + "/" + fileInfo.Key;
                string copyFile = zipFolder + "/" + fileInfo.Value + "/" + fileInfo.Key;
                if (File.Exists(copyFile))
                {
                    File.Delete(copyFile);
                }
                string copyPath = Path.GetDirectoryName(copyFile);
                if (!Directory.Exists(copyPath))
                {
                    Directory.CreateDirectory(copyPath);
                }
                // 资源拷贝
                File.Copy(sourceFile, copyFile, true);
            }

            // 压缩
            string zipFullName = zipFolder + ".zip";
            zipFolder = zipFolder.Replace("\\", "/");
            DirectoryInfo sourzeZipFolder = new DirectoryInfo(zipFolder);

            FastZipEvents zipEvents = new FastZipEvents();
            zipEvents.DirectoryFailure = DirectoryFailureHandler;
            zipEvents.FileFailure = FileFailureHandler;
            zipEvents.Progress = ProgressHandler;

            // 压缩前的文件大小
            filePreSize = 0;
            fileZipSize = 0;
            foreach (var path in ABHelper.GetAllFilesPathInDir(sourzeZipFolder.FullName))
            {
                FileInfo fileInfo = new FileInfo(path);
                filePreSize += fileInfo.Length;
            }

            // 开启线程
            System.Threading.Thread thread = GCCollect();
            thread.Start();

            // 进行压缩
            ZipSuccess = true;
            ABHelper.ZipFile(zipFullName, sourzeZipFolder.FullName, zipEvents);
            while (!ZipSuccess)
            {
                ZipSuccess = true;
                ABHelper.ZipFile(zipFullName, sourzeZipFolder.FullName, zipEvents);
            }
            EditorUtility.ClearProgressBar();

            // 压缩后的文件大小
            // 1-2.ini
            FileInfo zipfile = new FileInfo(zipFullName);
            ABHelper.WriteFile(zipFolder + ".ini", ("zipsize:" + zipfile.Length + ":" + filePreSize));

            thread.Abort();
            System.GC.Collect();
        }
        private static bool BuildPacker(BuildTarget platform, bool onlyLua = false)
        {
            Building = true;
            CurBuildTarget = platform;
            long start = System.DateTime.Now.Second;
            Debug.Log("开始处理：" + System.DateTime.Now.ToString());

            // 跟目录文件夹名称
            if (string.IsNullOrEmpty(TheRootFolderName))
            {
                RootFolderNmae();
            }

            // 本版本信息处理
            CeateVersionDir(platform);

            // 创建依赖关系文件
            CreatFileDependencies();

            // 关闭面板
            if (null != TheWindow)
            {
                TheWindow.Close();
            }
            // 生成ab资源
            try
            {
                if (!CreatABPacker(platform))
                {
                    EditorUtility.DisplayDialog("提示", "本次版本与上次版本没有变化！！", "ok");
                    Building = false;
                    BuildResult(false, "本次版本与上次版本没有变化！！");
                    return false;
                }
            }
            catch (Exception e)
            {
                if (Directory.Exists(CurVersionABExportPath))
                {
                    Directory.Delete(CurVersionABExportPath, true);
                }
                Debug.Log("打包失败！！！！！" + e.Message);
                Building = false;
                BuildResult(false, e.Message);
                return false;
            }

            // 生成版本文件
            CreatVersionTxt();

            // 生成版本增量包
            for (int i = -1; i < CurVersionNum; i++)
            {
                CreatHotterZip(i);
            }

            // 保存版号
            SaveTheVersion();
            // 上传ab
            UploadAB();
            // 结束
            Building = false;
            Debug.Log("处理结束：" + System.DateTime.Now.ToString());
            EditorUtility.DisplayDialog("提示", "打包已完成！！", "ok");
            BuildResult(true);
            return true;
        }

        #endregion

        [UnityEditor.Callbacks.PostProcessScene]
        public static void PackerScene()
        {
            if (!Building)
            {
                return;
            }
            System.GC.Collect();
            System.GC.Collect();
            System.GC.Collect();
        }
    }
}