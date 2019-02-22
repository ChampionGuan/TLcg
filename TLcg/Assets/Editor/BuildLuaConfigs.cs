using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LCG
{
    public class BuildLuaConfigs : EditorWindow
    {
        // 导出路径
        private static string m_exportPath = string.Format("{0}/{1}/", Application.dataPath, "Lua/Game/Config");
        // lua文件夹路径
        private static List<string> m_folderLua = new List<string>() { "Protobuf", "Common", "Game/Common", "Game/Config", "Game/Data", "Game/LevelLogic", "Game/Manager", "Game/Network", "Game/Timer" };
        // 插在前排的lua
        private static List<string> m_forwardLua = new List<string>() { "bit", "protobuf", "Common.CSUtils", "Game.Common.Define", "Game.Common.Utils", "Game.Common.UIUtils", "Game.Common.Event", "Game.UI.Common.UIConfig" };
        // 插在后排的lua
        private static List<string> m_backLua = new List<string>() { "Game.Manager.DataManager" };
        // 剔除的lua
        private static List<string> m_exclusiveLua = new List<string>() { "Common.Common", "Common.LuaHandle", "Game.Network.NetConnect", "Game.Config.LuaPreloadConfig" };
        // 配置名称
        private static string m_configName = "LuaPreloadConfig.lua";

        [MenuItem("Tools/收集lua文件")]
        private static void BuildLuaWindow()
        {
            Collect();
        }
        public static void Collect()
        {
            if (!Directory.Exists(m_exportPath))
            {
                Directory.CreateDirectory(m_exportPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("return {\n");

            foreach (string v in m_forwardLua)
            {
                sb.Append("\t\"" + v + "\",\n");
            }

            List<string> list = new List<string>();
            foreach (string v in m_folderLua)
            {
                list.AddRange(ABHelper.GetAllFilesPathInDir("Assets/Lua/" + v));
            }
            foreach (string v in list)
            {
                if (!v.EndsWith(".lua"))
                {
                    continue;
                }

                string nv = v.Replace("Assets/Lua/", "");
                nv = nv.Replace(@"/", ".");
                nv = nv.Replace(@"\", ".");
                nv = nv.Replace(".lua", "");

                if (m_exclusiveLua.Contains(nv) || m_forwardLua.Contains(nv) || m_backLua.Contains(nv))
                {
                    continue;
                }

                sb.Append("\t\"" + nv + "\",\n");
            }

            foreach (string v in m_backLua)
            {
                sb.Append("\t\"" + v + "\",\n");
            }

            sb.Append("}");

            string filePath = m_exportPath + m_configName;
            ABHelper.WriteFile(filePath, sb.ToString());

            Debug.Log(string.Format("已存储在路径{0}", filePath));
        }
    }
}