using UnityEngine;
using System.IO;
using System;

namespace LCG
{
    public class Main : MonoBehaviour
    {
        public static Main Instance = null;
        public Define.EMode Mode { get; private set; }
        public Transform Target { get; private set; }
        private FileStream m_fsLog = null;
        void Awake()
        {
            if (null != Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Target = transform;
            Debug.unityLogger.logEnabled = Debug.isDebugBuild;
            Application.logMessageReceivedThreaded += SaveLogMessage;

            OpenReporter(true);
            DontDestroyOnLoad(gameObject);
            StartupLauncher(Define.EBootup.Launcher);
        }
        void Destroy()
        {
            if (null != m_fsLog)
            {
                m_fsLog.Close();
            }
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

        /// <summary>
        /// 打开日志
        /// </summary>
        public void OpenReporter(bool b)
        {
            if (null == Reporter.Instance)
            {
                UnityEngine.Object obj = Resources.Load(Define.ReporterPath);
                if (null != obj)
                {
                    GameObject.Instantiate(obj);
                }
            }
            if (null != Reporter.Instance)
            {
                Reporter.Instance.show = b;
            }
        }

        /// <summary>
        /// 保存log信息
        /// </summary>
        private void SaveLogMessage(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Log: condition = string.Format("【log】{0}", condition); break;
                case LogType.Warning: condition = string.Format("【Warning】{0}", condition); break;
                case LogType.Error: condition = string.Format("【Error】{0}", condition); break;
                default: break;
            }
            if (null == m_fsLog)
            {
                m_fsLog = new FileStream(Define.OutputLogPath, FileMode.Create);
            }

            byte[] bytes = System.Text.Encoding.Default.GetBytes(condition);
            m_fsLog.Write(bytes, 0, bytes.Length);
            bytes = System.Text.Encoding.Default.GetBytes(stackTrace + "\n");
            m_fsLog.Write(bytes, 0, bytes.Length);

            m_fsLog.Flush();
        }
    }
}