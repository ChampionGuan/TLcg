using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System.Text;
using System.Text.RegularExpressions;

namespace LCG
{
    public class AssetsCreate
    {
        [MenuItem("Assets/Create/Custom Lua Script", false, 80)]
        private static void CreateNewLuaScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<DoCreateScriptAsset>(),
                EndNameEditAction() + "/New Lua.lua",
                null,
                "Assets/Editor/Luatemplate.txt");
        }

        [MenuItem("Assets/Create/Custom C# Script", false, 80)]
        private static void CreateNewCSharpScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<DoCreateScriptAsset>(),
                EndNameEditAction() + "/New CSharp.cs",
                null,
                "Assets/Editor/CSharptemplate.txt");
        }
        private static string EndNameEditAction()
        {
            string path = "Assets";
            foreach (Object obj in UnityEditor.Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }
    class DoCreateScriptAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string templateFile)
        {
            ProjectWindowUtil.ShowCreatedAsset(CreateScriptAssetFromTemplate(pathName, templateFile));
        }

        private static Object CreateScriptAssetFromTemplate(string pathName, string templateFile)
        {
            if (pathName.EndsWith(".cs") && pathName.EndsWith(".lua"))
            {
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object)); ;
            }

            StreamReader sr = new StreamReader(templateFile);
            string text = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();

            text = Regex.Replace(text, "#FILE#", Path.GetFileNameWithoutExtension(pathName));
            text = Regex.Replace(text, "#YEAR#", System.DateTime.Now.Year.ToString());
            text = Regex.Replace(text, "#DATE#", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

            StreamWriter sw = new StreamWriter(Path.GetFullPath(pathName), false, new UTF8Encoding(true, false));
            sw.Write(text);
            sw.Close();
            sw.Dispose();

            AssetDatabase.ImportAsset(pathName);

            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }
}