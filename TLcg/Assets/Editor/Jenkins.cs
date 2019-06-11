using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LCG
{
    public class Jenkins : EditorWindow
    {
        static BuildTarget Platform = BuildTarget.NoTarget;
        static bool IsDebug = false;
        static string ABRoot = null;
        static string DeployId = null;

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
            if (string.IsNullOrEmpty(DeployId)) return;
        }
        static void ChangeABRoot()
        {
            parseCommand();
            if (string.IsNullOrEmpty(ABRoot)) return;
            List<string> version = ABHelper.ReadVersionIdFile();
            ABHelper.WriteVersionIdFile(version[0], ABRoot, version[2]);
        }
        static void parseCommand()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; ++i)
            {
                if ("-mode" == args[i])
                {
                    IsDebug = bool.Parse(args[i + 1]);
                }
                else if ("-platform" == args[i])
                {
                    if (args[i + 1] == ABHelper.AndroidPlatform)
                    {
                        Platform = BuildTarget.Android;
                    }
                    else if (args[i + 1] == ABHelper.IosPlatform)
                    {
                        Platform = BuildTarget.iOS;
                    }
                    else if (args[i + 1] == ABHelper.WinPlatform)
                    {
                        Platform = BuildTarget.StandaloneWindows;
                    }
                }
                else if ("-abRoot" == args[i])
                {
                    ABRoot = args[i + 1];
                }
                else if ("-deployId" == args[i])
                {
                    DeployId = args[i + 1];
                }
            }
        }
    }
}