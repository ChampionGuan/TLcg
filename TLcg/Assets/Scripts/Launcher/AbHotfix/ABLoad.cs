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
        public string AbRealPath
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
        public Action<string, UnityEngine.Object> Complete
        {
            get; private set;
        }
        public Action<float> Progress
        {
            get; private set;
        }

        public void Reset(string _abPath, string _abName, string _abRealPath, Type _type, Action<string, UnityEngine.Object> _complete = null, Action<float> _progress = null, bool _isScene = false)
        {
            AbPath = _abPath;
            AbName = _abName;
            AbRealPath = _abRealPath;
            AbType = _type;
            IsScene = _isScene;

            SetAction(_complete, _progress);
        }
        public void SetAction(Action<string, UnityEngine.Object> _complete = null, Action<float> _progress = null)
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
        private List<string> abDependList;

        public AssetBundle TheAB
        {
            get; private set;
        }

        public ABReference(AssetBundle _ab, string _abPath)
        {
            referenceCount = 0;
            abPath = _abPath;
            TheAB = _ab;
            ABLoad.Instance.ABManifest.TryGetValue(_abPath, out abDependList);
        }
        // 引用增加
        public void ReferenceAsc()
        {
            if (null != abDependList && abDependList.Count > 0)
            {
                ABReference abRef;
                int length = abDependList.Count;
                for (int i = 0; i < length; i++)
                {
                    abRef = null;
                    ABLoad.Instance.ABReferenceMap.TryGetValue(abDependList[i], out abRef);
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
            if (null != abDependList && abDependList.Count > 0)
            {
                ABReference abRef;
                int length = abDependList.Count;
                for (int i = 0; i < length; i++)
                {
                    abRef = null;
                    ABLoad.Instance.ABReferenceMap.TryGetValue(abDependList[i], out abRef);
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
                if (null != TheAB)
                {
                    TheAB.Unload(true);
                    TheAB = null;
                    Debug.Log("卸载ab:" + abPath);
                }
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
        private List<ABLoadTask> m_loadTaskRecycle = new List<ABLoadTask>();

        public IEnumerator Init(Action finish = null)
        {
            m_isInitialized = false;
            m_nowLoadTaskCount = 0;
            m_maxLoadTaskCount = 10;

            if (null == ABReferenceMap)
            {
                ABReferenceMap = new Dictionary<string, ABReference>();
            }

            if (null != ABVersion.CurVersionInfo && ABVersion.CurVersionInfo.IsValid)
            {
                // 首包
                yield return LauncherEngine.Instance.StartCoroutine(ABVersion.OriginalVersionInfo.ParseVersionListByWWW());
                yield return LauncherEngine.Instance.StartCoroutine(ABVersion.OriginalVersionInfo.ParseNatvieListByWWW());

                // 清单文件
                bool fromNativePath = true;
                string manifestFilePath = ABVersion.CurVersionInfo.GetABFullPath(ABHelper.ManifestFileName, ref fromNativePath);
                if (!string.IsNullOrEmpty(manifestFilePath))
                {
                    if (fromNativePath)
                    {
                        WWW www = Application.platform == RuntimePlatform.Android ? new WWW(manifestFilePath) : new WWW("file://" + manifestFilePath);
                        yield return www;
                        ABManifest = ABHelper.ReadManifestFileByBytes(www.bytes);
                    }
                    else
                    {
                        ABManifest = ABHelper.ReadManifestFileByPath(manifestFilePath);
                    }
                }
            }
            if (null != ABManifest)
            {
                m_isInitialized = true;
            }
            // 初始化完成，可进行后续操作
            if (null != finish)
            {
                Debug.Log("<color=#20F856>AB初始化完毕</color>");
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
            bool b = LoadAllDependAb(task);
            if (!task.IsScene)
            {
                if (LoadAssetBundle(b, task.AbPath, task.AbRealPath))
                {
                    ABReference abRef = ABReferenceMap[task.AbPath];
                    return SyncLoadAsset(abRef, task);
                }
                else
                {
                    return SyncLoadAsset(task);
                }
            }
            else
            {
                // 场景只加载ab
                if (LoadAssetBundle(b, task.AbPath, task.AbRealPath))
                {
                    ABReference abRef = ABReferenceMap[task.AbPath];
                    abRef.ReferenceAsc();
                }
            }
            return null;
        }
        private UnityEngine.Object SyncLoadAsset(ABReference abRef, ABLoadTask task)
        {
            if (null == abRef.TheAB)
            {
                abRef.ReferenceAsc();
                LoadComplete(null, task);
                return null;
            }

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
        private UnityEngine.Object SyncLoadAsset(ABLoadTask task)
        {
            UnityEngine.Object spawner;
            if (null == task.AbType)
            {
                spawner = Resources.Load(ABHelper.GetFileFullPathWithoutFtype(task.AbPath));
            }
            else
            {
                spawner = Resources.Load(ABHelper.GetFileFullPathWithoutFtype(task.AbPath), task.AbType);
            }
            LoadComplete(spawner, task);
            return spawner;
        }

        #endregion

        #region asyncLoad
        public bool AsyncLoad(string abPath, string abName, Type type, Action<string, UnityEngine.Object> complete, Action<float> progress = null, bool isScene = false)
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
            bool b = LoadAllDependAb(task);
            if (!task.IsScene)
            {
                if (LoadAssetBundle(b, task.AbPath, task.AbRealPath))
                {
                    ABReference abRef = ABReferenceMap[task.AbPath];
                    LauncherEngine.Instance.StartCoroutine(AsyncLoadAsset(abRef, task));
                }
                else
                {
                    LauncherEngine.Instance.StartCoroutine(AsyncLoadAsset(task));
                }
            }
            else
            {
                // 场景只加载ab
                if (LoadAssetBundle(b, task.AbPath, task.AbRealPath))
                {
                    ABReference abRef = ABReferenceMap[task.AbPath];
                    abRef.ReferenceAsc();
                }
            }
        }
        private IEnumerator AsyncLoadAsset(ABReference abRef, ABLoadTask task)
        {
            if (null == abRef.TheAB)
            {
                abRef.ReferenceAsc();
                LoadComplete(null, task);
                yield break;
            }

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
                abRef.ReferenceAsc();
                LoadComplete(request.asset, task);
            }
        }
        private IEnumerator AsyncLoadAsset(ABLoadTask task)
        {
            ResourceRequest request;
            if (null == task.AbType)
            {
                request = Resources.LoadAsync(ABHelper.GetFileFullPathWithoutFtype(task.AbPath));
            }
            else
            {
                request = Resources.LoadAsync(ABHelper.GetFileFullPathWithoutFtype(task.AbPath), task.AbType);
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
                LoadComplete(request.asset, task);
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
        private bool LoadAllDependAb(ABLoadTask task)
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
                LoadAssetBundle(false, path);
            }

            return length > 0;
        }
        private bool LoadAssetBundle(bool beDepend, string abPath, string abRealPath = null)
        {
            bool result = true;
            bool fromNativePath = true;
            abRealPath = null == abRealPath ? ABVersion.CurVersionInfo.GetABFullPath(abPath, ref fromNativePath) : abRealPath;
            if (string.IsNullOrEmpty(abRealPath))
            {
                result = false;
            }

            AssetBundle ab = null;
            if (result)
            {
                Debug.Log("加载ab:" + abPath);
                ab = AssetBundle.LoadFromFile(abRealPath);
            }
            if (null == ab)
            {
                result = false;
            }

            if (beDepend || null != ab)
            {
                ABReferenceMap.Add(abPath, new ABReference(ab, abPath));
            }
            return result;
        }
        private void LoadComplete(UnityEngine.Object obj, ABLoadTask task)
        {
            if (null != task.Complete)
            {
                task.Complete(task.AbPath, obj);
            }
            task.ClearAction();

            m_nowLoadTaskCount--;
            NextLoadTask();

            // 回收
            if (!m_loadTaskRecycle.Contains(task))
            {
                m_loadTaskRecycle.Add(task);
            }
        }
        private ABLoadTask NewLoadTask(string abPath, string abName, Type type, Action<string, UnityEngine.Object> complete = null, Action<float> progress = null, bool isScene = false)
        {
            bool fromNativePath = true;
            string abRealPath = ABVersion.CurVersionInfo.GetABFullPath(abPath, ref fromNativePath);
            abPath = abPath.ToLower();
            abName = (null == abName) ? ABHelper.GetFileNameWithoutSuffix(abPath) : abName.ToLower();

            // 新建任务
            ABLoadTask task = null;
            if (m_loadTaskRecycle.Count > 0)
            {
                task = m_loadTaskRecycle[0];
                m_loadTaskRecycle.RemoveAt(0);
            }
            else
            {
                task = new ABLoadTask();
            }
            task.Reset(abPath, abName, abRealPath, type, complete, progress, isScene);
            return task;
        }
        #endregion
    }

    public class ABLoader
    {
        public static void UnloadAll()
        {
            ABLoad.Instance.UnloadAll();
        }
        public static void UnloadObject(string resourcePath)
        {
            ABLoad.Instance.Unload(resourcePath + ".ab");
        }
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static UnityEngine.Object LoadObject(string resourcePath, string assetName, Type type)
        {
            if (ABHelper.IgnoreHotfix || null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsValid)
            {
                return null;
            }
            UnityEngine.Object obj = null;

            // 检测是否从ab加载
            string abPath = resourcePath + ".ab";
            TrySyncLoadFromAB(abPath, assetName, type, out obj);
            if (null == obj)
            {
                abPath = ABHelper.GetFileFolderPath(resourcePath) + ".ab";
                TrySyncLoadFromAB(abPath, assetName, type, out obj);
            }

            return obj;
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <param name="complete"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static bool AsyncLoadObject(string resourcePath, string assetName, Type type, Action<string, UnityEngine.Object> complete, Action<float> progress = null)
        {
            if (ABHelper.IgnoreHotfix || null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsValid)
            {
                return false;
            }
            bool loadFromAb = false;

            // 检测是否从ab加载
            string abPath = resourcePath + ".ab";
            loadFromAb = TryAsyncLoadFromAB(abPath, assetName, type, complete, progress);
            if (!loadFromAb)
            {
                abPath = ABHelper.GetFileFolderPath(resourcePath) + ".ab";
                loadFromAb = TryAsyncLoadFromAB(abPath, assetName, type, complete, progress);
            }

            return loadFromAb;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sname"></param>
        /// <returns></returns>
        public static bool LoadScene(string path, string sname)
        {
            if (ABHelper.IgnoreHotfix || null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsValid)
            {
                return false;
            }
            UnityEngine.Object obj = null;

            // 检测是否从ab加载
            TrySyncLoadFromAB(path + ".ab", sname, null, out obj, true);

            return null != obj;
        }
        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="path"></param>
        public static void UnloadScene(string path)
        {
            ABLoad.Instance.Unload(path + ".ab");
        }
        /// <summary>
        /// 获取assetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AssetBundle GetUIAssetBundle(string path)
        {
            if (ABHelper.IgnoreHotfix || null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsValid)
            {
                return null;
            }
            AssetBundle ab = null;

            // 检测是否存在ab
            bool fromNativePath = true;
            string abname = ABHelper.GetFileFolderPath(path).ToLower() + ".ab";
            string uiFullPath = ABVersion.CurVersionInfo.GetABFullPath(abname, ref fromNativePath);

            if (!string.IsNullOrEmpty(uiFullPath))
            {
                Debug.Log("加载ab:" + abname);
                ab = AssetBundle.LoadFromFile(uiFullPath);
            }

            return ab;
        }
        /// <summary>
        /// 获取lua路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetLuaPath(string filePath)
        {
            if (ABHelper.IgnoreHotfix || null == ABVersion.CurVersionInfo || !ABVersion.CurVersionInfo.IsValid)
            {
                return null;
            }

            bool fromNativePath = true;
            filePath = filePath + ".lua.txt";
            filePath = ABVersion.CurVersionInfo.GetABFullPath(filePath, ref fromNativePath);
            return fromNativePath ? null : filePath;
        }
        private static bool TrySyncLoadFromAB(string abPath, string assetName, Type type, out UnityEngine.Object obj, bool isScene = false)
        {
            obj = null;
            if (!string.IsNullOrEmpty(abPath))
            {
                obj = ABLoad.Instance.SyncLoad(abPath, assetName, type, isScene);
                return null != obj;
            }
            return false;
        }
        private static bool TryAsyncLoadFromAB(string abPath, string assetName, Type type, Action<string, UnityEngine.Object> complete, Action<float> progress = null, bool isScene = false)
        {
            if (!string.IsNullOrEmpty(abPath))
            {
                return ABLoad.Instance.AsyncLoad(abPath, assetName, type, complete, progress, isScene);
            }
            return false;
        }
    }
}
