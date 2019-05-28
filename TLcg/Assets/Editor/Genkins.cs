using UnityEngine;
using UnityEditor;

namespace LCG
{
    public class Genkins : EditorWindow
    {
        static BuildTarget Platform = BuildTarget.NoTarget;
        static bool IgnoreScript;
        static bool IsDebug = false;

        static void BuildApk()
        {
            parseCommand();
            if (!BuildAppSteps.CommandBuild(IsDebug, Platform))
            {
                throw new System.Exception("生成apk失败！！");
            }
        }
        static void BuildAB()
        {
            parseCommand();
            if (!ABPacker.CommandBuild(IsDebug, Platform))
            {
                throw new System.Exception("生成ab失败！！");
            }
        }
        static void BuildAB2Apk()
        {
            bool result;
            parseCommand();

            result = ABPacker.CommandBuild(IsDebug, Platform);
            if (!result)
            {
                throw new System.Exception("生成ab失败！！");
            }

            result = BuildAppSteps.CommandBuild(IsDebug, Platform);
            if (!result)
            {
                throw new System.Exception("生成apk失败！！");
            }
        }
        static void parseCommand()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; ++i)
            {
                if ("-ignoreScript" == args[i])
                {
                    IgnoreScript = bool.Parse(args[i + 1]);
                }
                else if ("-mode" == args[i])
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
            }
        }
    }
}