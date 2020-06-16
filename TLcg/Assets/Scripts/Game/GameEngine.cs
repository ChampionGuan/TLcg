using UnityEngine;

namespace LCG
{
    public class GameEngine : SingletonMonobehaviour<GameEngine>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Define.EBootup e)
        {
            HotfixTest();
            CSharpCallLua.Instance.Initialize(e);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomDestroy();
                Network.Instance.CustomDestroy();
                Http.Instance.CustomDestroy();
                SceneLoader.Instance.CustomDestroy();
                TouchHandle.Instance.CustomDestroy();
                GameObjectPool.Reset();
                ResourceLoader.UnloadAll();
                StopAllCoroutines();
            }
        }

        private void Update()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomUpdate();
                Network.Instance.CustomUpdate();
                Http.Instance.CustomUpdate();
                LuaEnv.Instance.CustomUpdate();
                TouchHandle.Instance.CustomUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomFixedUpdate();
                Network.Instance.CustomFixedUpdate();
                LuaEnv.Instance.CustomFixedUpdate();
            }
        }

        private void OnDestroy()
        {
            if (Main.Instance.Mode == Define.EMode.Game)
            {
                CSharpCallLua.Instance.CustomDestroy();
                Network.Instance.CustomDestroy();
                Http.Instance.CustomDestroy();
                SceneLoader.Instance.CustomDestroy();
                LuaEnv.Instance.CustomDestroy();
                TouchHandle.Instance.CustomDestroy();
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

        public void HotfixTest()
        {
            Debug.Log("cs hotfix log!");
            Debug.Log("test!!");
        }
    }
}