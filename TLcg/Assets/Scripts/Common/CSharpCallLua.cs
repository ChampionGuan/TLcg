using System;

namespace LCG
{
    public class CSharpCallLua : Singleton<CSharpCallLua>
    {
        private Action<int> m_actionInitialize;
        private Action m_actionUpdate;
        private Action m_actionFixedUpdate;
        private Action m_actionDestroy;
        private Action<bool> m_actionAppliationFocus;
        private Action<byte[]> m_actionReceiveMsg;

        public void Initialize(int? i = 1)
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                LuaEnv.Instance.DoString("require 'Launcher.Main'");
            }
            else
            {
                LuaEnv.Instance.DoString("require 'Game.Main'");
            }
            m_actionInitialize = LuaEnv.Instance.BindToLua<Action<int>>("Initialize");
            m_actionUpdate = LuaEnv.Instance.BindToLua<Action>("Update");
            m_actionFixedUpdate = LuaEnv.Instance.BindToLua<Action>("FixedUpdate");
            m_actionDestroy = LuaEnv.Instance.BindToLua<Action>("OnDestroy");
            m_actionAppliationFocus = LuaEnv.Instance.BindToLua<Action<bool>>("OnAppFocus");
            m_actionReceiveMsg = LuaEnv.Instance.BindToLua<Action<byte[]>>("OnReceiveMsg");

            if (null != m_actionInitialize)
            {
                m_actionInitialize(i.Value);
            }
        }

        public void CustomUpdate()
        {
            if (null != m_actionUpdate)
            {
                m_actionUpdate();
            }
        }

        public void CustomFixedUpdate()
        {
            if (null != m_actionFixedUpdate)
            {
                m_actionFixedUpdate();
            }
        }

        public void CustomDestroy()
        {
            if (null != m_actionDestroy)
            {
                m_actionDestroy();
            }
            m_actionInitialize = null;
            m_actionUpdate = null;
            m_actionFixedUpdate = null;
            m_actionDestroy = null;
            m_actionAppliationFocus = null;
            m_actionReceiveMsg = null;
        }

        public void ApplicationFocus(bool hasFocus)
        {
            if (null != m_actionAppliationFocus)
            {
                m_actionAppliationFocus(hasFocus);
            }
        }

        public void ReceiveServerMsg(byte[] msg)
        {
            if (null != m_actionReceiveMsg)
            {
                m_actionReceiveMsg(msg);
            }
        }
    }
}