using System;
using UnityEngine;

namespace LCG
{
    public class LauncherEngine : SingletonMonobehaviour<LauncherEngine>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Define.ELauncher e)
        {
            ABCheck.Instance.Initialize(() =>
            {
                CSharpCallLua.Instance.Initialize((int)e);
            });
        }

        /// <summary>
        /// ab更新
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="versionIdUrl"></param>
        /// <param name="resRemoteUrl"></param>
        /// <param name="handleState"></param>
        public void ABHotfix(string versionId, string versionIdUrl, string resRemoteUrl, Action<ABHelper.VersionArgs> handleState)
        {
            ABCheck.Instance.CheckHotter(versionId, versionIdUrl, resRemoteUrl, handleState);
        }

        /// <summary>
        /// ab自助修复
        /// </summary>
        /// <param name="isDeep"></param>
        /// <param name="handleState"></param>
        public void ABRepair(bool isDeep, Action<ABHelper.VersionArgs> handleState)
        {
            ABAutofix.Instance.Repair(isDeep, handleState);
        }

        /// <summary>
        /// 检测是否需要更新
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public bool ABCheckNeedHotfix(string versionId)
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

        private void Update()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomUpdate();
                LuaEnv.Instance.CustomUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomFixedUpdate();
                ABCheck.Instance.CustomFixedUpdate();
            }
        }

        private void OnDestroy()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomDestroy();
                ABAutofix.Instance.CustomDestroy();
                ABCheck.Instance.CustomDestroy();
                LuaEnv.Instance.CustomDestroy();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.ApplicationFocus(focus);
            }
        }
    }
}
