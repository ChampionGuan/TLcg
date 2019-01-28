﻿using System;
using UnityEngine;

namespace LCG
{
    public class LauncherEngine : SingletonMonobehaviour<LauncherEngine>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Define.EBootup e)
        {
            ABCheck.Instance.Initialize(() =>
            {
                CSharpCallLua.Instance.Initialize(e);
            });
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            CSharpCallLua.Instance.CustomDestroy();
            Gameobjects.Instance.CustomDestroy();
            ABAutofix.Instance.CustomDestroy();
            ABCheck.Instance.CustomDestroy();
            ResourceLoader.UnloadAll();
            StopAllCoroutines();
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

        private void Update()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomUpdate();
                Gameobjects.Instance.CustomUpdate();
                LuaEnv.Instance.CustomUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomFixedUpdate();
                Gameobjects.Instance.CustomFixedUpdate();
                ABCheck.Instance.CustomFixedUpdate();
                LuaEnv.Instance.CustomFixedUpdate();
            }
        }

        private void OnDestroy()
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomDestroy();
                Gameobjects.Instance.CustomDestroy();
                ABAutofix.Instance.CustomDestroy();
                ABCheck.Instance.CustomDestroy();
                LuaEnv.Instance.CustomDestroy();
                ResourceLoader.UnloadAll();
                StopAllCoroutines();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Main.Instance.Mode == Define.EMode.Launcher)
            {
                CSharpCallLua.Instance.CustomAppFocus(focus);
            }
        }
    }
}
