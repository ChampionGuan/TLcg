using System;
using UnityEngine;

namespace LCG
{
    public class LuaEnv : Singleton<LCG.LuaEnv>, Define.IMonoBase
    {
        private XLua.LuaEnv m_luaVM;
        private float m_lastGCTime = 0;
        private float m_GCInterval = 1;

        public override void OnInstance()
        {
            m_luaVM = new XLua.LuaEnv();
            m_luaVM.AddLoader(ResourceLoader.LoadLua);

            m_luaVM.GcPause = 100;
            m_luaVM.GcStepmul = 5000;
        }

        public object[] DoString(string str)
        {
            if (null == m_luaVM)
            {
                return null;
            }
            return m_luaVM.DoString(str);
        }

        public object[] DoFile(string filename, string chunkName = "chunk", XLua.LuaTable env = null)
        {
            if (null == m_luaVM)
            {
                return null;
            }
            return m_luaVM.DoString("require '" + filename + "'", chunkName, env);
        }

        public T BindToLua<T>(string key)
        {
            if (null == m_luaVM)
            {
                return default(T);
            }
            T f = m_luaVM.Global.Get<T>(key);
            return f;
        }

        public void RestartGc()
        {
            if (null == m_luaVM)
            {
                return;
            }
            m_luaVM.RestartGc();
        }

        public void CollectGC()
        {
            if (null == m_luaVM)
            {
                return;
            }
            m_luaVM.FullGc();
        }

        public void CustomUpdate()
        {
            if (null == m_luaVM)
            {
                return;
            }
            if (Time.time - m_lastGCTime > m_GCInterval)
            {
                m_luaVM.Tick();
                m_lastGCTime = Time.time;
            }
        }

        public void CustomFixedUpdate()
        {
        }

        public void CustomAppFocus(bool focus)
        {
        }

        public void CustomDestroy()
        {
            if (null == m_luaVM)
            {
                return;
            }

            m_luaVM.Dispose();
            m_luaVM = null;

            Debug.Log("~LuaManager was destroyed!");
        }

    }
}