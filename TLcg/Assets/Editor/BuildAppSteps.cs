using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace LCG
{
    public class BuildAppSteps : EditorWindow
    {
        static System.Diagnostics.Process m_process = null;
        static EditorWindow m_editorWindow;

        static string m_productName = "tlcg";
        static string m_companyName = "champion";
        static string m_appIdentifier = "com.champion.tlcg";
        static string m_scriptingDefineSymbols = "HOTFIX_ENABLE;";
        static string m_dataPathPrefix = Application.dataPath + "/";
        static string m_luaResourceFolderPath = m_dataPathPrefix + "Lua/";
        static string m_luaTempFolderPath = m_dataPathPrefix + "Resources/Lua/";
        static VersionNum m_versionNum;
        static string[] m_buildScenes = null;
        static string m_buildPath = string.Empty;
        static string m_apkFolderPath = string.Empty;
        static BuildTarget m_buildTarget = BuildTarget.Android;
        static BuildTargetGroup m_buildTargetGroup = BuildTargetGroup.Android;
        static BuildOptions m_buildOptions = BuildOptions.None;
        static string m_luajitWorkingPath = "/Luajit/luajit-2.1.0b2/src/";
        static string m_luajitExePath = "luajit.exe";

        [MenuItem("Tools/版本打包工具")]
        private static void Build()
        {
            m_process = new System.Diagnostics.Process();
            m_process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            m_process.StartInfo.CreateNoWindow = true;
            m_process.StartInfo.UseShellExecute = false;
            m_process.StartInfo.RedirectStandardOutput = true;
            m_process.StartInfo.RedirectStandardError = true;

            // combine luajit working path
            DirectoryInfo parentInfo = Directory.GetParent(Application.dataPath);
            m_luajitWorkingPath = parentInfo.FullName + m_luajitWorkingPath;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            m_luajitExePath = m_luajitWorkingPath + "luajit.exe";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            m_luajitExePath = m_luajitWorkingPath + "luajit";
#endif
            m_versionNum = new VersionNum(ABHelper.ReadVersionIdFile()[0]);
            m_apkFolderPath = ABHelper.ReadVersionIdFile()[2];

            m_editorWindow = EditorWindow.GetWindow(typeof(BuildAppSteps), true, "打包");
        }

        private static string mode = "debug";
        private static string platform = ABHelper.AndroidPlatform;
        private void OnGUI()
        {
            GUILayout.Label("自动打包工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            string t = "debug";
            if (EditorGUILayout.Toggle(t, mode == "debug"))
            {
                mode = t;
            }
            t = "release";
            if (EditorGUILayout.Toggle(t, mode == "release"))
            {
                mode = t;
            }
            EditorGUILayout.Space();

            t = ABHelper.AndroidPlatform;
            if (EditorGUILayout.Toggle(t, platform == ABHelper.AndroidPlatform))
            {
                platform = t;
            }
            t = ABHelper.IosPlatform;
            if (EditorGUILayout.Toggle(t, platform == ABHelper.IosPlatform))
            {
                platform = t;
            }
            t = ABHelper.WinPlatform;
            if (EditorGUILayout.Toggle(t, platform == ABHelper.WinPlatform))
            {
                platform = t;
            }
            EditorGUILayout.Space();

            GUILayout.Label("资源版号", EditorStyles.boldLabel);
            GUILayout.TextArea(m_versionNum.Id);
            EditorGUILayout.Space();

            if (GUILayout.Button("Build正式版本"))
            {
                if (platform == ABHelper.AndroidPlatform)
                {
                    m_buildTarget = BuildTarget.Android;
                }
                else if (platform == ABHelper.IosPlatform)
                {
                    m_buildTarget = BuildTarget.iOS;
                }
                else if (platform == ABHelper.WinPlatform)
                {
                    m_buildTarget = BuildTarget.StandaloneWindows;
                }
                else
                {
                    m_buildTarget = BuildTarget.NoTarget;
                }

                if (m_buildTarget != BuildTarget.Android)
                {
                    BulidTarget(mode == "debug");
                }
                else
                {
                    // 移入ab
                    ABStreaming.BuildABZip(ABHelper.AndroidPlatform);
                    // 移出资源
                    ABStreaming.AssetMoveout();
                    // 将文件移至指定位置！！！
                    if (BulidTarget(mode == "debug"))
                    {
                        string apkpath1;
                        string apkpath2;
                        string apkpathS;

                        apkpathS = string.Format("{0}/../../../release/{1}", Application.dataPath, m_apkFolderPath);
                        if (!Directory.Exists(apkpathS))
                        {
                            Directory.CreateDirectory(apkpathS);
                        }

                        apkpath1 = string.Format("{0}/male7_v{1}.apk", apkpathS, m_versionNum.Id);
                        File.Copy(m_buildPath, apkpath1, true);
                        FileInfo file = new FileInfo(apkpath1);

                        apkpath2 = string.Format("{0}/male7_v{1}.ini", apkpathS, m_versionNum.Id);
                        ABHelper.WriteFile(apkpath2, file.Length.ToString().TrimEnd());

                        Debug.Log("成功生成版本！！！！" + apkpath1);
                    }
                    // 移入资源
                    ABStreaming.AssetMovein();
                }
            }
            if (GUILayout.Button("Build内网版本"))
            {
                if (platform == ABHelper.AndroidPlatform)
                {
                    m_buildTarget = BuildTarget.Android;
                }
                else if (platform == ABHelper.IosPlatform)
                {
                    m_buildTarget = BuildTarget.iOS;
                }
                else if (platform == ABHelper.WinPlatform)
                {
                    m_buildTarget = BuildTarget.StandaloneWindows;
                }
                else
                {
                    m_buildTarget = BuildTarget.NoTarget;
                }

                BulidTarget(mode == "debug");
            }
        }

        //目标平台
        private static bool BulidTarget(bool isDebug = false)
        {
            bool result = true;
            try
            {
                Debug.Log("开始预处理过程...");
                AssetDatabase.Refresh();

                // 场景
                EditorScenes();
                // 打包环境
                BuildEnv(isDebug);

                // 处理XLua脚本
                CSObjectWrapEditor.Generator.ClearAll();
                CSObjectWrapEditor.Generator.GenAll();

                // 收集lua文件
                BuildLuaConfigs.Collect();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // 拷贝所有lua并加上txt后缀
                DirRemove(m_luaTempFolderPath);
                DirCopy(m_luaResourceFolderPath, m_luaTempFolderPath, ".txt");
#else
                // 拷贝所有lua并编译为bytecode
                DirRemove(m_luaTempFolderPath);
                GenLuaByteCode(m_luaResourceFolderPath, m_luaTempFolderPath);
#endif
                AssetDatabase.Refresh();

                Debug.Log("预处理过程结束...");

                // 开始Build,等待吧～
                if (m_buildTarget == BuildTarget.NoTarget)
                {
                    throw new Exception("未选择打包平台！！！");
                }
                EditorUserBuildSettings.SwitchActiveBuildTarget(m_buildTargetGroup, m_buildTarget);
                string res = BuildPipeline.BuildPlayer(m_buildScenes, m_buildPath, m_buildTarget, m_buildOptions);
                if (!String.IsNullOrEmpty(res))
                {
                    throw new Exception("BuildPlayer failure: " + res);
                }
                else
                {
                    Debug.Log("生成版本成功！");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[BuildingStepError]: " + e);
                result = false;
                throw;
            }
            finally
            {
                Debug.Log("开始后处理过程...");

                // 删除临时文件夹
                DirRemove(m_luaTempFolderPath);
                // ScriptingBackend
                PlayerSettings.SetScriptingBackend(m_buildTargetGroup, ScriptingImplementation.IL2CPP);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                Debug.Log("后处理过程结束...");
                Debug.Log("版本发布结束！");

                m_editorWindow.Close();
            }
            return result;
        }
        private static void BuildEnv(bool isDebug = false)
        {
            string target_dir = "";
            string target_name = "";
            string buildFolderPath = Application.dataPath.Replace("/Assets", "");
            m_buildOptions = isDebug ? (BuildOptions.CompressWithLz4 | BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler) : (BuildOptions.CompressWithLz4HC | BuildOptions.SymlinkLibraries);

            if (m_buildTarget == BuildTarget.Android)
            {
                target_dir = buildFolderPath + "/Builds/Android";
                target_name = m_productName + ".apk";
                m_buildTargetGroup = BuildTargetGroup.Android;
                PlayerSettings.Android.keystorePass = "champion";
                PlayerSettings.Android.keyaliasName = "com.champion.tlcg";
                PlayerSettings.Android.keyaliasPass = "champion";
            }
            else if (m_buildTarget == BuildTarget.iOS)
            {
                target_dir = buildFolderPath + "/Builds/iOS";
                target_name = m_productName;
                m_buildTargetGroup = BuildTargetGroup.iOS;
                EditorUserBuildSettings.iOSBuildConfigType = isDebug ? iOSBuildType.Debug : iOSBuildType.Release;
                PlayerSettings.iOS.appleDeveloperTeamID = "YG2T7K2S55";
            }
            else if (m_buildTarget == BuildTarget.StandaloneWindows)
            {
                target_dir = buildFolderPath + "/Builds/Windows";
                target_name = m_productName + ".exe";
                m_buildTargetGroup = BuildTargetGroup.Standalone;
            }
            else if (m_buildTarget == BuildTarget.StandaloneOSX)
            {
                target_dir = buildFolderPath + "/Builds/OSX";
                target_name = m_productName + ".app";
                m_buildTargetGroup = BuildTargetGroup.Standalone;
                PlayerSettings.iOS.appleDeveloperTeamID = "YG2T7K2S55";
            }
            else if (m_buildTarget == BuildTarget.WebGL)
            {
                target_dir = buildFolderPath + "/Builds/WebGL";
                target_name = m_productName;
                m_buildTargetGroup = BuildTargetGroup.WebGL;
            }
            else
            {
                return;
            }

            PlayerSettings.companyName = m_companyName;
            PlayerSettings.productName = m_productName;
            PlayerSettings.applicationIdentifier = m_appIdentifier;
            PlayerSettings.bundleVersion = m_versionNum.Id;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(m_buildTargetGroup, m_scriptingDefineSymbols);
            PlayerSettings.SetArchitecture(m_buildTargetGroup, 2);

            if (isDebug)
            {
                PlayerSettings.SetScriptingBackend(m_buildTargetGroup, ScriptingImplementation.Mono2x);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(m_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }

            //每次build删除之前的残留
            if (Directory.Exists(target_dir))
            {
                if (File.Exists(target_name))
                {
                    File.Delete(target_name);
                }
            }
            else
            {
                Directory.CreateDirectory(target_dir);
            }

            m_buildPath = target_dir + "/" + target_name;
        }
        // 生成lua的bytecode
        private static void GenLuaByteCode(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // 遍历所有文件
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // 剔除meta文件, .DS打头的文件
                if (file.Name.EndsWith(".meta") || file.Name.Contains(".DS"))
                    continue;

                string sourcePath = Path.Combine(sourceDirName, file.Name);
                string destinationPath = Path.Combine(destDirName, file.Name);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                // 路径转义字符替换
                sourcePath = sourcePath.Replace("/", "\\");
                destinationPath = destinationPath.Replace("/", "\\");
#endif
                // 目标路径，加个txt后缀
                destinationPath = destinationPath + ".txt";

                // 编译bytecode
                m_process.StartInfo.FileName = m_luajitExePath;
                m_process.StartInfo.Arguments = string.Format(" -b {0} {1}", sourcePath, destinationPath);
                m_process.StartInfo.WorkingDirectory = m_luajitWorkingPath;

                try
                {
                    m_process.Start();
                    m_process.WaitForExit();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                // Debug.Log("~~~~~执行输出\n" + m_process.StandardOutput.ReadToEnd());
                // Debug.Log("~~~~~执行错误\n" + m_process.StandardError.ReadToEnd());
                // Debug.Log("~~~~~执行返回码：" + m_process.ExitCode);

                // 处理成功后
                if (m_process.ExitCode == 0)
                {
                    byte[] content = ABHelper.ReadFileToBytes(sourcePath);
                    ABHelper.Encrypt(ref content);
                    ABHelper.WriteFileByBytes(destinationPath, content);
                }
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    GenLuaByteCode(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        private static void EditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }
                if (!File.Exists(scene.path))
                {
                    continue;
                }
                EditorScenes.Add(scene.path);
            }

            m_buildScenes = EditorScenes.ToArray();
        }
        // 删除目录
        public static void DirRemove(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
        // 重命名
        private static void DirRename(string dir, string newName)
        {
            if (Directory.Exists(dir))
            {
                Directory.Move(dir, newName);
            }
        }

        // 拷贝目录，包括
        private static void DirCopy(string srcPath, string tarPath, string fileExtension = null)
        {
            try
            {
                bool needAddExtension = !string.IsNullOrEmpty(fileExtension);

                // 检查目标目录是否以目录分割字符结束如果不是则添加
                if (tarPath[tarPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    tarPath += System.IO.Path.DirectorySeparatorChar;
                }
                // 判断目标目录是否存在如果不存在则新建
                if (!System.IO.Directory.Exists(tarPath))
                {
                    System.IO.Directory.CreateDirectory(tarPath);
                }
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles（srcPath）；
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
                // 遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    string targetFileName = tarPath + System.IO.Path.GetFileName(file);

                    // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                    if (System.IO.Directory.Exists(file))
                    {
                        DirCopy(file, targetFileName, fileExtension);
                    }
                    // 否则直接Copy文件
                    else
                    {
                        // 剔除meta文件, .DS打头的文件
                        if (targetFileName.EndsWith(".meta") || targetFileName.Contains(".DS"))
                        {
                            continue;
                        }
                        // 添加后缀
                        if (needAddExtension)
                        {
                            targetFileName = targetFileName + fileExtension;
                        }
                        byte[] content = ABHelper.ReadFileToBytes(file); ;
                        if (null == content || content.Length <= 0)
                        {
                            continue;
                        }
                        ABHelper.Encrypt(ref content);
                        ABHelper.WriteFileByBytes(targetFileName, content);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [PostProcessBuild(999)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            // 仅在iOS运行
            if (buildTarget == BuildTarget.iOS)
            {
                string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject proj = new PBXProject();
                proj.ReadFromFile(projectPath);

                string target = proj.TargetGuidByName("Unity-iPhone");
                var capManager = new ProjectCapabilityManager(projectPath, m_productName + ".entitlements", PBXProject.GetUnityTargetName());

                // 设置签名
                proj.SetTeamId(target, "YG2T7K2S55"); //自动签名 自动选择证书 TeamID
                proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
                // Set a custom link flag
                proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
                // proj.AddBuildProperty(target, "SystemCapabilities", "{com.apple.Push = {enabled = 1;};}");

                proj.WriteToFile(projectPath);
            }
        }
    }
}
