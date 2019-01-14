using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using FairyGUI;

namespace LCG
{
    public class ABManager
    {
        #region 卸载资源
        public static void UnloadAll()
        {
            UnloadAllAb();
            UnLoadAllUI();
        }
        #endregion

        #region 加载资源
        // 加载资源
        public static UnityEngine.Object Load(string resourcePath, string assetName, Type type)
        {
            UnityEngine.Object obj = null;

            // 检测是否存在ab
            if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsValid)
            {
                // 检测是否从ab加载
                string abPath = resourcePath + ".ab";
                TrySyncLoadFromAB(abPath, assetName, type, out obj);
                if (null == obj)
                {
                    abPath = ABHelper.GetFileFolderPath(resourcePath) + ".ab";
                    TrySyncLoadFromAB(abPath, assetName, type, out obj);
                }
            }

            if (null == obj)
            {
                obj = Resources.Load(resourcePath, type);
            }

            return obj;
        }
        public static void AsyncLoad(string resourcePath, string assetName, Type type, Action<UnityEngine.Object> complete, Action<float> progress = null)
        {
            bool loadFromAb = false;

            // 检测是否存在ab
            if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsValid)
            {
                // 检测是否从ab加载
                string abPath = resourcePath + ".ab";
                loadFromAb = TryAsyncLoadFromAB(abPath, assetName, type, complete, progress);
                if (!loadFromAb)
                {
                    abPath = ABHelper.GetFileFolderPath(resourcePath) + ".ab";
                    loadFromAb = TryAsyncLoadFromAB(abPath, assetName, type, complete, progress);
                }
            }

            if (!loadFromAb)
            {
                UnityEngine.Object obj = Resources.Load(resourcePath, type);
                complete(obj);
            }
        }
        public static void UnloadAb(string resourcePath)
        {
            ABLoad.Instance.Unload(resourcePath + ".ab");
        }
        public static void UnloadAllAb()
        {
            ABLoad.Instance.UnloadAll();
        }
        private static bool TrySyncLoadFromAB(string abPath, string assetName, Type type, out UnityEngine.Object obj, bool isScene = false)
        {
            obj = null;
            if (!string.IsNullOrEmpty(abPath))
            {
                string luaFullPath = ABVersion.CurVersionInfo.GetABFullPath(abPath);
                if (string.IsNullOrEmpty(luaFullPath))
                {
                    return false;
                }
                obj = ABLoad.Instance.SyncLoad(abPath, assetName, type, isScene);
                return null != obj;
            }
            return false;
        }
        private static bool TryAsyncLoadFromAB(string abPath, string assetName, Type type, Action<UnityEngine.Object> complete, Action<float> progress = null, bool isScene = false)
        {
            if (!string.IsNullOrEmpty(abPath))
            {
                string luaFullPath = ABVersion.CurVersionInfo.GetABFullPath(abPath);
                if (string.IsNullOrEmpty(luaFullPath))
                {
                    return false;
                }
                return ABLoad.Instance.AsyncLoad(abPath, assetName, type, complete, progress, isScene);
            }
            return false;
        }
        #endregion

        #region 加载场景
        private static string lastLoadSceneName = null;
        /// <summary>
        /// 场景加载结束后，对上一场景执行卸载
        /// </summary>
        /// <param name="sname"></param>
        public static void LoadScene(string sname)
        {
            string scenePath = "ScenesFinal/" + sname;
            UnityEngine.Object obj = null;

            // 检测是否存在ab
            if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsValid)
            {
                // 检测是否从ab加载
                TrySyncLoadFromAB(scenePath + ".ab", sname, null, out obj, true);
            }

            // 卸载上一个场景
            if (!string.IsNullOrEmpty(lastLoadSceneName))
            {
                ABLoad.Instance.Unload("ScenesFinal/" + lastLoadSceneName + ".ab");
                lastLoadSceneName = null;
            }

            if (null == obj)
            {
                UnityEngine.Debug.LogFormat("<color=#33FFFF>Load Scene{0} From Resources</color>", sname);
            }
            else
            {
                lastLoadSceneName = sname;
            }
        }
        #endregion

        #region 加载UI
        /// <summary>
        /// fgui独立管理ab,接口单独拎出来！
        /// </summary>
        public static void LoadUI(string uipath)
        {
            AssetBundle ab = null;
            // 检测是否存在ab
            if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsValid)
            {
                string abname = ABHelper.GetFileFolderPath(uipath) + ".ab";
                string uiFullPath = ABVersion.CurVersionInfo.GetABFullPath(abname);

                if (!string.IsNullOrEmpty(uiFullPath))
                {
                    ab = AssetBundle.LoadFromFile(uiFullPath);
                }
            }

            UIPackage pkg = UIPackage.GetByName(uipath);
            if (null == pkg)
            {
                if (null == ab)
                {
                    pkg = UIPackage.AddPackage(uipath);
                }
                else
                {
                    pkg = UIPackage.AddPackage(ab);
                }
            }
            else
            {
                if (null == ab)
                {
                    pkg.ReloadAssets();
                }
                else
                {
                    pkg.ReloadAssets(ab);
                }
            }
        }
        public static void UnLoadUI(string uipath, bool destroyRes)
        {
            UIPackage pkg = UIPackage.GetByName(uipath);
            if (null != pkg)
            {
                pkg.UnloadAssets();
            }
        }
        public static void UnLoadAllUI()
        {
            List<UIPackage> pkgs = UIPackage.GetPackages();
            if (null != pkgs)
            {
                foreach (UIPackage pkg in pkgs)
                {
                    pkg.UnloadAssets();
                }
            }
            UIPackage.RemoveAllPackages();
        }
        #endregion

        #region 加载lua
        public static List<string> LoadedLua = new List<string>();
        public static byte[] LoadLua(string filePath)
        {
            try
            {
                if (!LoadedLua.Contains(filePath))
                {
                    LoadedLua.Add(filePath);
                }

                string luaPath = "";
                string luaFullPath = "";
                byte[] luaBytes = null;

                // 转换为小写路径
                filePath = ("Lua/" + filePath).ToLower();
                // 例：Lua文件中类似require "aaa.bbb"时，自动转换为路径aaa/bbb
                filePath = filePath.Replace(".", "/");

                if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsVersionFileExist)
                {
                    luaPath = filePath + ".lua.txt";
                    luaFullPath = ABVersion.CurVersionInfo.GetABFullPath(luaPath);
                    if (!string.IsNullOrEmpty(luaFullPath))
                    {
                        Debug.Log("加载热更lua:" + luaFullPath);
                        luaBytes = TryLoadLuaFromFile(luaFullPath, true);
                    }
                }

                if (null == luaBytes)
                {
                    luaPath = filePath + ".lua";
                    luaFullPath = string.Format("{0}/{1}", Application.dataPath, luaPath);
                    if (File.Exists(luaFullPath))
                    {
                        luaBytes = TryLoadLuaFromFile(luaFullPath, false);
                    }
                }

                if (null == luaBytes)
                {
                    luaPath = filePath + ".lua";
                    luaBytes = TryLoadLuaFromResource(luaPath, true);

                    if (null == luaBytes)
                    {
                        UnityEngine.Debug.LogError("[error] Cant find " + luaFullPath);
                    }
                }

                return luaBytes;

            }
            catch
            {
                return null;
            }
        }
        private static byte[] TryLoadLuaFromResource(string fullpath, bool encrypt = true)
        {
            if (string.IsNullOrEmpty(fullpath))
            {
                return null;
            }
            TextAsset tAsset = Resources.Load<TextAsset>(fullpath);
            if (null == tAsset)
            {
                return null;
            }

            byte[] bytes = tAsset.bytes;
            if (encrypt)
            {
                ABHelper.Encrypt(ref bytes);
            }
            return bytes;
        }
        private static byte[] TryLoadLuaFromFile(string fullpath, bool encrypt = true)
        {
            byte[] bytes = ABHelper.ReadFileToBytes(fullpath);
            if (null == bytes)
            {
                return null;
            }

            if (encrypt)
            {
                ABHelper.Encrypt(ref bytes);
            }
            // 转换为utf8
            using (Stream stream = new MemoryStream(bytes))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string code = sr.ReadToEnd();
                    return System.Text.Encoding.UTF8.GetBytes(code);
                }
            }
        }
        #endregion
    }
}