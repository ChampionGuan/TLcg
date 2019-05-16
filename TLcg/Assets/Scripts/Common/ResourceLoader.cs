using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using FairyGUI;

namespace LCG
{
    public class ResourceLoader
    {
        public static void UnloadAll()
        {
            foreach (var v in m_pkgDir.Values)
            {
                UnloadUI(v);
            }
            foreach (var v in m_objDir.Keys)
            {
                UnloadObject(v);
            }
            m_objDir.Clear();
            m_pkgDir.Clear();
            m_abDir.Clear();
            ABLoader.UnloadAll();
            UIPackage.RemoveAllPackages();
            Resources.UnloadUnusedAssets();
        }
        private static Dictionary<string, ReferenceCount> m_objDir = new Dictionary<string, ReferenceCount>();
        public static UnityEngine.Object LoadObject(string resourcePath, Type type = null)
        {
            ReferenceCount rc = null;
            if (m_objDir.ContainsKey(resourcePath))
            {
                rc = m_objDir[resourcePath];
            }
            else
            {
                rc = new ReferenceCount(resourcePath);
                m_objDir.Add(resourcePath, rc);
            }
            return rc.Load(type);
        }
        public static void AsyncLoadObject(string resourcePath, Type type, Action<UnityEngine.Object> complete, Action<float> progress = null)
        {
            ReferenceCount rc = null;
            if (m_objDir.ContainsKey(resourcePath))
            {
                rc = m_objDir[resourcePath];
            }
            else
            {
                rc = new ReferenceCount(resourcePath);
                m_objDir.Add(resourcePath, rc);
            }
            rc.AsyncLoad(type, complete, progress);
        }
        public static void UnloadObject(string resourcePath)
        {
            if (!m_objDir.ContainsKey(resourcePath))
            {
                return;
            }
            m_objDir[resourcePath].Unload();
        }
        private static Dictionary<string, string> m_pkgDir = new Dictionary<string, string>();
        private static Dictionary<string, AssetBundle> m_abDir = new Dictionary<string, AssetBundle>();
        public static void LoadUI(string uipath)
        {
            AssetBundle ab = null;
            if (m_abDir.ContainsKey(uipath))
            {
                ab = m_abDir[uipath];
            }
            if (null == ab)
            {
                ab = ABLoader.GetUIAssetBundle(uipath);
                m_abDir.Remove(uipath);
                if (null != ab)
                {
                    m_abDir.Add(uipath, ab);
                }
            }

            string id = uipath;
            if (m_pkgDir.ContainsKey(uipath))
            {
                id = m_pkgDir[uipath];
            }

            UIPackage pkg = UIPackage.GetById(id);
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
            if (!m_pkgDir.ContainsKey(uipath))
            {
                m_pkgDir.Add(uipath, pkg.id);
            }
        }
        public static void UnloadUI(string uipath)
        {
            string id = uipath;
            if (m_pkgDir.ContainsKey(uipath))
            {
                id = m_pkgDir[uipath];
            }
            if (m_abDir.ContainsKey(uipath))
            {
                m_abDir.Remove(uipath);
            }
            UIPackage pkg = UIPackage.GetById(id);
            if (null != pkg)
            {
                pkg.UnloadAssets();
            }
        }
        public static string ScenePathPrefix = "Scenes/";
        private static string lastSceneName = null;
        public static void LoadScene(string sname)
        {
            //  加载新场景
            bool r = ABLoader.LoadScene(ScenePathPrefix + sname, sname);

            // 卸载上一个场景
            if (!string.IsNullOrEmpty(lastSceneName))
            {
                ABLoader.UnloadScene(ScenePathPrefix + lastSceneName);
                lastSceneName = null;
            }
            if (r)
            {
                lastSceneName = sname;
            }
        }
        public static void UnloadScene(string sname)
        {
            ABLoader.UnloadScene(ScenePathPrefix + sname);
            // 和上一场景一致
            if (lastSceneName == sname)
            {
                lastSceneName = null;
            }
        }
        public static List<string> LoadedLua = new List<string>();
        public static byte[] LoadLua(ref string filePath)
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

                luaFullPath = ABLoader.GetLuaPath(filePath);
                if (!string.IsNullOrEmpty(luaFullPath))
                {
                    Debug.Log("加载lua:" + filePath);
                    luaBytes = TryLoadLuaFromFile(luaFullPath, true);
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
            // // 转换为utf8
            // using (Stream stream = new MemoryStream(bytes))
            // {
            //     using (StreamReader sr = new StreamReader(stream))
            //     {
            //         string code = sr.ReadToEnd();
            //         return System.Text.Encoding.UTF8.GetBytes(code);
            //     }
            // }
            return bytes;
        }
        public class ReferenceCount
        {
            private UnityEngine.Object theObj;
            private string thePath;
            private Type theType;
            private int referenceCount = 0;
            public ReferenceCount(string path)
            {
                thePath = path;
            }
            public UnityEngine.Object Load(Type type = null)
            {
                if (null != theObj)
                {
                    LoadComplete(theObj);
                    return theObj;
                }

                UnityEngine.Object obj = ABLoader.LoadObject(thePath, null, type);
                if (null != obj)
                {
                    LoadComplete(obj);
                    return obj;
                }

                if (null == type)
                {
                    obj = Resources.Load(thePath);
                }
                else
                {
                    obj = Resources.Load(thePath, type);
                }
                LoadComplete(obj);
                return obj;
            }
            public void AsyncLoad(Type type, Action<UnityEngine.Object> complete, Action<float> progress = null)
            {
                if (null == complete)
                {
                    return;
                }

                if (null != theObj)
                {
                    complete.Invoke(theObj);
                    complete = null;
                    progress = null;
                    LoadComplete(theObj);
                    return;
                }

                if (ABLoader.AsyncLoadObject(thePath, null, type, (s, o) =>
                {
                    complete.Invoke(o);
                    complete = null;
                    progress = null;
                    LoadComplete(o);
                }, progress))
                {
                    return;
                }

                UnityEngine.Object obj;
                if (null == type)
                {
                    obj = Resources.Load(thePath);
                }
                else
                {
                    obj = Resources.Load(thePath, type);
                }
                complete.Invoke(obj);
                complete = null;
                progress = null;
                LoadComplete(obj);
            }
            public void Unload()
            {
                if (null == theObj)
                {
                    return;
                }
                ReferenceDec();
            }
            private void LoadComplete(UnityEngine.Object obj)
            {
                if (null == obj)
                {
                    referenceCount = 0;
                    return;
                }
                if (null == theObj)
                {
                    theObj = obj;
                }
                if (null == theType)
                {
                    theType = theObj.GetType();
                }
                ReferenceAsc();
            }
            private void ReferenceAsc()
            {
                referenceCount++;
            }
            private void ReferenceDec()
            {
                referenceCount--;
                if (referenceCount > 0)
                {
                    return;
                }
                if (theType != typeof(UnityEngine.GameObject))
                {
                    Resources.UnloadAsset(theObj);
                }
                ABLoader.UnloadObject(thePath);
                theObj = null;
            }
        }
    }
}
