using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace LCG
{
    public class SceneLoader : Singleton<SceneLoader>, Define.IMonoBase
    {
        public Action LoadStart
        {
            private get; set;
        }
        public Action<int> LoadUpdate
        {
            private get; set;
        }
        public Action LoadComplete
        {
            private get; set;
        }
        public Action SwtichingScene
        {
            private get; set;
        }
        public void CustomUpdate()
        {
        }
        public void CustomFixedUpdate()
        {
        }
        public void CustomDestroy()
        {
            LoadStart = null;
            LoadUpdate = null;
            LoadComplete = null;
        }
        public void CustomAppFocus(bool focus)
        {
        }
        public void LoadScene(string sceneName)
        {
            if (null != LoadStart)
            {
                LoadStart.Invoke();
            }
            GameEngine.Instance.StartCoroutine(LoadAsync(sceneName));
        }
        private IEnumerator LoadAsync(string sceneName)
        {
            ResourceLoader.LoadScene(sceneName);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            int toPercent = 0;
            int curPercent = 0;

            while (operation.progress < 0.9f)
            {
                toPercent = (int)operation.progress * 100 - curPercent;
                yield return IncreaseInstruction(curPercent, toPercent);
            }

            toPercent = 100;
            yield return IncreaseInstruction(curPercent, toPercent);
            operation.allowSceneActivation = true;
            yield return operation;

            if (null != SwtichingScene)
            {
                SwtichingScene.Invoke();
            }
            if (null != LoadComplete)
            {
                LoadComplete.Invoke();
            }
        }
        private YieldInstruction IncreaseInstruction(int curPercent, int toPercent)
        {
            return GameEngine.Instance.StartCoroutine(LoadPercentSlerp(curPercent, toPercent));
        }
        private IEnumerator LoadPercentSlerp(int curPercent, int toPercent)
        {
            while (curPercent < toPercent)
            {
                curPercent += 2;
                if (null != LoadUpdate)
                {
                    LoadUpdate.Invoke(curPercent);
                }
                yield return null;
            }
        }
    }
}
