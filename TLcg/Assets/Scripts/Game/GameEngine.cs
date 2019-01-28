﻿using UnityEngine;

namespace LCG
{
    public class GameEngine : SingletonMonobehaviour<GameEngine>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            CSharpCallLua.Instance.Initialize();
        }

        private void Update()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomUpdate();
                Gameobjects.Instance.CustomUpdate();
                Network.Instance.CustomUpdate();
                Http.Instance.CustomUpdate();
                LuaEnv.Instance.CustomUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomFixedUpdate();
                Gameobjects.Instance.CustomFixedUpdate();
                Network.Instance.CustomFixedUpdate();
                LuaEnv.Instance.CustomFixedUpdate();
            }
        }

        private void OnDestroy()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomDestroy();
                Gameobjects.Instance.CustomDestroy();
                Network.Instance.CustomDestroy();
                Http.Instance.CustomDestroy();
                SceneLoader.Instance.CustomDestroy();
                LuaEnv.Instance.CustomDestroy();
                ResourceLoader.UnloadAll();
                StopAllCoroutines();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomAppFocus(focus);
            }
        }
    }
}