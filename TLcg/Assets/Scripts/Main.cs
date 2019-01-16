using UnityEngine;

namespace LCG
{
    public class Main : MonoBehaviour
    {
        public static Main Instance = null;
        public Define.EMode Mode { get; private set; }

        void Awake()
        {
            if (null != Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            StartupLauncher(Define.ELauncher.Initialize);
        }

        /// <summary>
        /// 启动器
        /// </summary>
        public void StartupLauncher(Define.ELauncher e)
        {
            Main.Instance.Mode = Define.EMode.Launcher;
            LauncherEngine.Instance.Initialize(e);
        }
        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartupGame()
        {
            Main.Instance.Mode = Define.EMode.Game;
            GameEngine.Instance.Initialize();
        }
    }
}