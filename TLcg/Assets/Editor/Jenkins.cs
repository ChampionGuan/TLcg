using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LCG
{
    public class Jenkins : EditorWindow
    {
        static BuildTarget Platform = BuildTarget.NoTarget;
        static bool IsDebug = false;
        static string TypeV = "AB";
        static string ABRoot = null;
        static string ApkRoot = null;
        static string DeployId = null;
        static string VersionId = null;

        static void Build()
        {
            parseCommand();

            Dictionary<string, string> versionInfo = ABHelper.ReadVersionIdFile();
            if (!string.IsNullOrEmpty(ABRoot) && "nil" != ABRoot.ToLower())
            {
                versionInfo["ABFolderRoot"] = ABRoot;
            }
            if (!string.IsNullOrEmpty(ApkRoot) && "nil" != ApkRoot.ToLower())
            {
                versionInfo["ApkFolderRoot"] = ApkRoot;
            }
            if (!string.IsNullOrEmpty(VersionId) && "nil" != VersionId.ToLower())
            {
                versionInfo["ResVersion"] = VersionId;
            }
            ABHelper.WriteVersionIdFile(versionInfo);
            AssetDatabase.Refresh();

            switch (TypeV.ToLower())
            {
                case "ab":
                    ABPacker.CommandBuild(true, Platform); break;
                case "apk":
                    BuildAppSteps.CommandBuild(IsDebug, Platform); break;
                case "ab2apk":
                    ABPacker.CommandBuild(IsDebug, Platform);
                    BuildAppSteps.CommandBuild(IsDebug, Platform); break;
                default: break;
            }
        }
        static void BuildApk()
        {
            parseCommand();
            BuildAppSteps.CommandBuild(IsDebug, Platform);
        }
        static void BuildAB()
        {
            parseCommand();
            ABPacker.CommandBuild(true, Platform);
        }
        static void BuildAB2Apk()
        {
            parseCommand();
            ABPacker.CommandBuild(IsDebug, Platform);
            BuildAppSteps.CommandBuild(IsDebug, Platform);
        }
        static void ChangeDeploy()
        {
            parseCommand();
            if (string.IsNullOrEmpty(DeployId) || "null" == DeployId.ToLower()) return;
        }
        static void ChangeABRoot()
        {
            parseCommand();
            if (string.IsNullOrEmpty(ABRoot) || "null" == ABRoot.ToLower()) return;
            Dictionary<string, string> versionInfo = ABHelper.ReadVersionIdFile();
            versionInfo["ABFolderRoot"] = ABRoot;
            ABHelper.WriteVersionIdFile(versionInfo);
        }
        static void ChangeVersionId()
        {
            parseCommand();
            if (string.IsNullOrEmpty(VersionId) || "null" == VersionId.ToLower()) return;
            Dictionary<string, string> versionInfo = ABHelper.ReadVersionIdFile();
            versionInfo["ResVersion"] = VersionId;
            ABHelper.WriteVersionIdFile(versionInfo);
        }
        static void ChangeApkRoot()
        {
            parseCommand();
            if (string.IsNullOrEmpty(ApkRoot) || "null" == ApkRoot.ToLower()) return;
            Dictionary<string, string> versionInfo = ABHelper.ReadVersionIdFile();
            versionInfo["ApkFolderRoot"] = ApkRoot;
            ABHelper.WriteVersionIdFile(versionInfo);
        }
        static void parseCommand()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; ++i)
            {
                if ("-platform" == args[i])
                {
                    if (args[i + 1].ToLower() == ABHelper.AndroidPlatform.ToLower())
                    {
                        Platform = BuildTarget.Android;
                    }
                    else if (args[i + 1].ToLower() == ABHelper.IosPlatform.ToLower())
                    {
                        Platform = BuildTarget.iOS;
                    }
                    else if (args[i + 1].ToLower() == ABHelper.WinPlatform.ToLower())
                    {
                        Platform = BuildTarget.StandaloneWindows;
                    }
                }
                else if ("-debug" == args[i])
                {
                    IsDebug = bool.Parse(args[i + 1]);
                }
                else if ("-abRoot" == args[i])
                {
                    ABRoot = args[i + 1];
                }
                else if ("-apkRoot" == args[i])
                {
                    ApkRoot = args[i + 1];
                }
                else if ("-version" == args[i])
                {
                    VersionId = args[i + 1];
                }
                else if ("-deployId" == args[i])
                {
                    DeployId = args[i + 1];
                }
                else if ("-type" == args[i])
                {
                    TypeV = args[i + 1];
                }
            }
        }
    }
}