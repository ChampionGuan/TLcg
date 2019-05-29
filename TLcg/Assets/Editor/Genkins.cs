using UnityEngine;
using UnityEditor;

namespace LCG
{
    public class Genkins : EditorWindow
    {
        static BuildTarget Platform = BuildTarget.NoTarget;
        static bool IsDebug = false;

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
            }
        }
    }
}