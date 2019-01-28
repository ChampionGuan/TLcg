using UnityEngine;
using System;

namespace LCG
{
    public class Main : MonoBehaviour
    {
        public static Main Instance = null;
        public Define.EMode Mode { get; private set; }
        public Transform Target { get; private set; }
        void Awake()
        {
            if (null != Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Target = transform;
            DontDestroyOnLoad(gameObject);
            StartupLauncher(Define.EBootup.Launcher);
        }

        /// <summary>
        /// 启动器
        /// </summary>
        public void StartupLauncher(Define.EBootup e)
        {
            GameEngine.Instance.Destroy();
            Main.Instance.Mode = Define.EMode.Launcher;
            LauncherEngine.Instance.Initialize(e);
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartupGame(Define.EBootup e)
        {
            LauncherEngine.Instance.Destroy();
            Main.Instance.Mode = Define.EMode.Game;
            GameEngine.Instance.Initialize(e);
        }

        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public bool CheckNeedHotfix(string versionId)
        {
            return LauncherEngine.Instance.CheckNeedHotfix(versionId);
        }
    }
}