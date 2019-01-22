using UnityEngine;

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

        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public bool CheckNeedHotfix(string versionId)
        {
            if (ABCheck.Instance.IsNeedGoStore(versionId))
            {
                return true;
            }
            if (ABCheck.Instance.IsNeedHotter(versionId))
            {
                return true;
            }
            return false;
        }
    }
}