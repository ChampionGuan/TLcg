using UnityEngine;

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
                LuaEnv.Instance.CustomUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomFixedUpdate();
            }
        }

        private void OnDestroy()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomDestroy();
                LuaEnv.Instance.CustomDestroy();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.ApplicationFocus(focus);
            }
        }
    }
}