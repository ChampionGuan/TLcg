using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace LCG
{
    public class ABLoadTask
    {
        public string AbPath
        {
            get; private set;
        }
        public string AbName
        {
            get; private set;
        }
        public Type AbType
        {
            get; private set;
        }
        public bool IsScene
        {
            get; private set;
        }
        public Action<UnityEngine.Object> Complete
        {
            get; private set;
        }
        public Action<float> Progress
        {
            get; private set;
        }

        public void Reset(string _abPath, string _abName, Type _type, Action<UnityEngine.Object> _complete = null, Action<float> _progress = null, bool _isScene = false)
        {
            AbPath = _abPath;
            AbName = _abName;
            AbType = _type;
            IsScene = _isScene;

            SetAction(_complete, _progress);
        }
        public void SetAction(Action<UnityEngine.Object> _complete = null, Action<float> _progress = null)
        {
            if (null != _complete)
            {
                Complete += _complete;
            }
            if (null != _progress)
            {
                Progress += _progress;
            }
        }
        public void ClearAction()
        {
            Complete = null;
            Progress = null;
        }
    }
    public class ABReference
    {
        private string abPath;
        private int referenceCount;

        public List<string> AbDependList
        {
            get; private set;
        }
        public AssetBundle TheAB
        {
            get; private set;
        }

        public ABReference(AssetBundle _ab, string _abPath)
        {
            referenceCount = 0;
            abPath = _abPath;
            TheAB = _ab;

            List<string> dependList = null;
            ABLoad.Instance.ABManifest.TryGetValue(_abPath, out dependList);
            AbDependList = dependList;
        }
        // 引用增加
        public void ReferenceAsc()
        {
            if (null == TheAB)
            {
                return;
            }
            if (null != AbDependList && AbDependList.Count > 0)
            {
                ABReference abRef;
                int length = AbDependList.Count;
                for (int i = 0; i < length; i++)
                {
                    abRef = null;
                    ABLoad.Instance.ABReferenceMap.TryGetValue(AbDependList[i], out abRef);
                    if (null != abRef)
                    {
                        abRef.AddRefCount();
                    }
                }
            }
            AddRefCount();
        }
        // 引用减少
        public void ReferenceDec()
        {
            if (null == TheAB)
            {
                return;
            }
            if (null != AbDependList && AbDependList.Count > 0)
            {
                ABReference abRef;
                int length = AbDependList.Count;
                for (int i = 0; i < length; i++)
                {
                    abRef = null;
                    ABLoad.Instance.ABReferenceMap.TryGetValue(AbDependList[i], out abRef);
                    if (null != abRef)
                    {
                        abRef.SubRefCount();
                    }
                }
            }
            SubRefCount();
        }
        public void AddRefCount()
        {
            referenceCount++;
        }
        public void SubRefCount()
        {
            referenceCount--;
            if (referenceCount <= 0)
            {
                ABLoad.Instance.ABReferenceMap.Remove(abPath);
                TheAB.Unload(true);
                TheAB = null;
            }
        }
    }
    public class ABLoad : Singleton<ABLoad>
    {
        public Dictionary<string, List<string>> ABManifest
        {
            get; private set;
        }
        public Dictionary<string, ABReference> ABReferenceMap
        {
            get; private set;
        }

        private int m_nowLoadTaskCount = 0;
        private int m_maxLoadTaskCount = 0;
        private bool m_isInitialized = false;
        private Queue<ABLoadTask> m_loadTaskList = new Queue<ABLoadTask>();
        private Dictionary<string, List<ABLoadTask>> m_loadTaskRecycle = new Dictionary<string, List<ABLoadTask>>();

        public IEnumerator Init(Action finish = null)
        {
            m_nowLoadTaskCount = 0;
            m_maxLoadTaskCount = 10;
            if (null == ABReferenceMap)
            {
                ABReferenceMap = new Dictionary<string, ABReference>();
            }
            // 加载清单文件
            yield return LauncherEngine.Instance.StartCoroutine(LoadManifest());
            // 初始化完成，可进行后续操作
            if (null != finish)
            {
                Debug.Log("<color=#20F856>AB初始化完毕</color>");
                m_isInitialized = true;
                finish();
            }
        }
        #region syncLoad
        public UnityEngine.Object SyncLoad(string abPath, string abName, Type type, bool isScene = false)
        {
            if (!m_isInitialized)
            {
                return null;
            }
            // 新建任务
            ABLoadTask task = NewLoadTask(abPath, abName, type, null, null, isScene);
            // 任务累加
            m_nowLoadTaskCount++;
            // 进行加载
            return SyncLoadAB(task);
        }
        private UnityEngine.Object SyncLoadAB(ABLoadTask task)
        {
            ABReference outAbRef = null;
            ABReferenceMap.TryGetValue(task.AbPath, out outAbRef);
            if (null == outAbRef)
            {
                return FirstSyncLoadAB(task);
            }
            else if (null == outAbRef.TheAB)
            {
                ABReferenceMap.Remove(task.AbPath);
                return FirstSyncLoadAB(task);
            }
            else
            {
                return NotFirstSyncLoadAb(task);
            }
        }
        private UnityEngine.Object NotFirstSyncLoadAb(ABLoadTask task)
        {
            ABReference abRef = ABReferenceMap[task.AbPath];
            if (!task.IsScene)
            {
                return SyncLoadAsset(abRef, task);
            }
            else
            {
                return null;
            }
        }
        private UnityEngine.Object FirstSyncLoadAB(ABLoadTask task)
        {
            LoadAllDependAb(task);
            if (!task.IsScene)
            {
                LoadAssetBundle(task.AbPath);

                ABReference abRef = ABReferenceMap[task.AbPath];
                return SyncLoadAsset(abRef, task);
            }
            else
            {
                // 场景只加载ab
                LoadAssetBundle(task.AbPath);
            }
            return null;
        }
        private UnityEngine.Object SyncLoadAsset(ABReference abRef, ABLoadTask task)
        {
            UnityEngine.Object spawner = null;

            if (null != task.AbType)
            {
                spawner = abRef.TheAB.LoadAsset(task.AbName, task.AbType);
            }
            else
            {
                spawner = abRef.TheAB.LoadAsset(task.AbName);
            }

            abRef.ReferenceAsc();
            LoadComplete(spawner, task);

            return spawner;
        }
        #endregion

        #region asyncLoad
        public bool AsyncLoad(string abPath, string abName, Type type, Action<UnityEngine.Object> complete, Action<float> progress = null, bool isScene = false)
        {
            if (!m_isInitialized)
            {
                return false;
            }
            // 新建任务
            ABLoadTask task = NewLoadTask(abPath, abName, type, complete, progress, isScene);
            // 进队列
            m_loadTaskList.Enqueue(task);
            // 开始load
            NextLoadTask();

            return true;
        }
        private void AsyncLoadAB(ABLoadTask task)
        {
            ABReference outAbRef = null;
            ABReferenceMap.TryGetValue(task.AbPath, out outAbRef);
            if (null == outAbRef)
            {
                FirstAsyncLoadAB(task);
            }
            else if (null == outAbRef.TheAB)
            {
                ABReferenceMap.Remove(task.AbPath);
                FirstAsyncLoadAB(task);
            }
            else
            {
                NotFirstAsyncLoadAb(task);
            }
        }
        private void NotFirstAsyncLoadAb(ABLoadTask task)
        {
            ABReference abRef = ABReferenceMap[task.AbPath];
            if (!task.IsScene)
            {
                LauncherEngine.Instance.StartCoroutine(AsyncLoadAsset(abRef, task));
            }
            else { }
        }
        private void FirstAsyncLoadAB(ABLoadTask task)
        {
            LoadAllDependAb(task);
            if (!task.IsScene)
            {
                LoadAssetBundle(task.AbPath);

                ABReference abRef = ABReferenceMap[task.AbPath];
                LauncherEngine.Instance.StartCoroutine(AsyncLoadAsset(abRef, task));
            }
            else
            {
                LoadAssetBundle(task.AbPath);
            }
        }
        private IEnumerator AsyncLoadAsset(ABReference abRef, ABLoadTask task)
        {
            AssetBundleRequest request;
            if (null != task.AbType)
            {
                request = abRef.TheAB.LoadAssetAsync(task.AbName, task.AbType);
            }
            else
            {
                request = abRef.TheAB.LoadAssetAsync(task.AbName);
            }
            while (!request.isDone)
            {
                if (null != task.Progress)
                {
                    task.Progress(request.progress);
                }
                yield return null;
            }
            yield return request;

            // 加载完成
            if (request.isDone)
            {
                UnityEngine.Object spawner = request.asset;

                abRef.ReferenceAsc();
                LoadComplete(spawner, task);
            }
        }
        private void NextLoadTask()
        {
            while (m_nowLoadTaskCount < m_maxLoadTaskCount)
            {
                if (m_loadTaskList.Count > 0)
                {
                    m_nowLoadTaskCount++;
                    AsyncLoadAB(m_loadTaskList.Dequeue());
                }
                else
                {
                    break;
                }
            }
        }
        #endregion

        #region common
        public void Unload(string abPath)
        {
            if (null == ABReferenceMap)
            {
                return;
            }
            abPath = abPath.ToLower();
            ABReference abRef = null;
            ABReferenceMap.TryGetValue(abPath, out abRef);
            if (null != abRef)
            {
                abRef.ReferenceDec();
            }
        }
        public void UnloadAll()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            if (null != ABReferenceMap)
            {
                ABReferenceMap.Clear();
            }
        }
        private void LoadAllDependAb(ABLoadTask task)
        {
            List<string> dependAbs = null;
            ABManifest.TryGetValue(task.AbPath, out dependAbs);

            int length = 0;
            if (null != dependAbs)
            {
                length = dependAbs.Count;
            }

            List<string> needABRef = new List<string>();
            ABReference outAbRef = null;
            for (int i = 0; i < length; i++)
            {
                outAbRef = null;
                ABReferenceMap.TryGetValue(dependAbs[i], out outAbRef);
                if (null == outAbRef)
                {
                    needABRef.Add(dependAbs[i]);
                }
                else if (null == outAbRef.TheAB)
                {
                    needABRef.Add(dependAbs[i]);
                    ABReferenceMap.Remove(dependAbs[i]);
                }
            }
            // 加载ab
            foreach (string path in needABRef)
            {
                LoadAssetBundle(path);
            }
        }
        private void LoadAssetBundle(string abPath)
        {
            string fullPath = ABVersion.CurVersionInfo.GetABFullPath(abPath);
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.Log("ab路径：" + abPath);
                return;
            }
            Debug.Log("加载热更ab:" + abPath);
            AssetBundle ab = AssetBundle.LoadFromFile(fullPath);
            if (null == ab)
            {
                Debug.LogError("ab load fail : path is :" + fullPath);
            }

            ABReference abRef = new ABReference(ab, abPath);
            ABReferenceMap.Add(abPath, abRef);
        }
        private void LoadComplete(UnityEngine.Object obj, ABLoadTask task)
        {
            if (null != task.Complete)
            {
                task.Complete(obj);
            }
            task.ClearAction();

            m_nowLoadTaskCount--;
            NextLoadTask();

            // 回收
            if (!m_loadTaskRecycle.ContainsKey(task.AbPath))
            {
                m_loadTaskRecycle.Add(task.AbPath, new List<ABLoadTask>());
            }
            m_loadTaskRecycle[task.AbPath].Add(task);
        }
        private IEnumerator LoadManifest()
        {
            if (null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsManifestFileExist)
            {
                yield break;
            }

            ABManifest = ABHelper.ReadManifestFile(ABVersion.CurVersionInfo.ManifestFilePath);
        }
        private ABLoadTask NewLoadTask(string abPath, string abName, Type type, Action<UnityEngine.Object> complete = null, Action<float> progress = null, bool isScene = false)
        {
            abPath = abPath.ToLower();
            abName = (null == abName) ? ABHelper.GetFileNameWithoutSuffix(abPath) : abName.ToLower();

            // 新建任务
            ABLoadTask task = null;
            if (m_loadTaskRecycle.ContainsKey(abPath) && m_loadTaskRecycle[abPath].Count > 0)
            {
                task = m_loadTaskRecycle[abPath][0];
                m_loadTaskRecycle[abPath].RemoveAt(0);
            }
            else
            {
                task = new ABLoadTask();
            }
            task.Reset(abPath, abName, type, complete, progress, isScene);
            return task;
        }
        #endregion
    }
}
