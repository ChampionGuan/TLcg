using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class GameObjectPool
    {
        private static Transform m_root;

        // idle队列采用栈逻辑处理，先进后出
        private static Dictionary<string, List<GameObjectCeil>> m_idle = new Dictionary<string, List<GameObjectCeil>>();
        private static Dictionary<string, List<GameObjectCeil>> m_using = new Dictionary<string, List<GameObjectCeil>>();

        static GameObjectPool()
        {
            GameObject root = new GameObject("GameObjectRoot");
            GameObject.DontDestroyOnLoad(root);
            m_root = root.transform;
        }

        /// <summary>
        /// 重置池
        /// </summary>
        public static void Reset()
        {
            List<GameObjectCeil> temp = new List<GameObjectCeil>();
            foreach (var v in m_idle.Values)
            {
                foreach (var v2 in v)
                {
                    temp.Add(v2);
                }
            }
            foreach (var v in m_using.Values)
            {
                foreach (var v2 in v)
                {
                    temp.Add(v2);
                }
            }
            m_idle.Clear();
            m_using.Clear();
            foreach (var v in temp)
            {
                GameObject.Destroy(v.TheGameobject);
            }
        }
        /// <summary>
        /// 依据空闲池大小，缩小空闲资产
        /// </summary>
        /// <param name="ratio"></param>
        public static void CleanupBySizeDecrease(float ratio)
        {
            if (ratio < 0 || ratio > 1)
            {
                return;
            }
            VerifyIsValid();

            List<GameObjectCeil> temp = new List<GameObjectCeil>();
            foreach (var v in m_idle.Values)
            {
                int count = Mathf.CeilToInt(v.Count * ratio);
                for (int i = 0; i < count; i++)
                {
                    temp.Add(v[i]);
                }
            }
            foreach (var v in temp)
            {
                GameObject.Destroy(v.TheGameobject);
            }
        }
        /// <summary>
        /// 依据空闲时间，清理空闲资产
        /// </summary>
        /// <param name="duration"></param>
        public static void CleanupByIdleTime(float duration)
        {
            if (duration < 0)
            {
                return;
            }
            VerifyIsValid();

            List<GameObjectCeil> temp = new List<GameObjectCeil>();
            foreach (var v in m_idle.Values)
            {
                foreach (var v2 in v)
                {
                    if (v2.IdleDuration > duration)
                    {
                        temp.Add(v2);
                    }
                }
            }
            foreach (var v in temp)
            {
                GameObject.Destroy(v.TheGameobject);
            }
        }
        public static GameObject Get(string path)
        {
            GameObjectCeil ceil = GetFromPool(path);
            if (null == ceil)
            {
                ceil = GetFromLoad(path);
            }
            if (null == ceil)
            {
                return null;
            }
            CreatDict(path);
            ceil.SetBeingUse(true);
            m_using[path].Add(ceil);

            return ceil.TheGameobject;
        }
        public static void AsyncGet(string path, Action<GameObject> cb)
        {
            GameObjectCeil ceil = GetFromPool(path);
            Action<GameObjectCeil> go = (obj) =>
            {
                if (obj = null)
                {
                    return;
                }

                CreatDict(path);
                ceil.SetBeingUse(true);
                m_using[path].Add(ceil);

                cb?.Invoke(obj.TheGameobject);
            };
            if (null != ceil)
            {
                go.Invoke(ceil);
            }
            else
            {
                GetFromAysncLoad(path, go);
            }
        }
        public static void Back(GameObject obj)
        {
            if (null == obj)
            {
                return;
            }
            GameObjectCeil ceil = obj.GetComponent<GameObjectCeil>();
            if (null == ceil)
            {
                GameObject.Destroy(obj);
            }
            else
            {
                BackToPool(ceil);
            }
        }
        private static void VerifyIsValid()
        {
            List<GameObjectCeil> temp = new List<GameObjectCeil>();
            foreach (var v in m_using)
            {
                temp.Clear();
                foreach (var v2 in v.Value)
                {
                    if (!v2.IsBeingUse)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][正在使用的资产处于未使用状态！！][The Asset Name:]" + v2.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                        temp.Add(v2);
                    }
                    else if (v2.IsDestroyed)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][正在使用的资产处于销毁状态！！][The Asset Name:]" + v2.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                        temp.Add(v2);
                    }

                }
                foreach (var v2 in temp)
                {
                    m_using[v2.ThePath].Remove(v2);
                    if (!v2.IsDestroyed)
                    {
                        m_idle[v2.ThePath].Add(v2);
                    }
                }
            }

            foreach (var v in m_idle)
            {
                temp.Clear();
                foreach (var v2 in v.Value)
                {
                    if (v2.IsBeingUse)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][空闲的资产处于使用状态！！][The Asset Name:]" + v2.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                        temp.Add(v2);
                    }
                    else if (v2.IsDestroyed)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][空闲的资产处于销毁状态！！][The Asset Name:]" + v2.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                        temp.Add(v2);
                    }
                }
                foreach (var v2 in temp)
                {
                    m_idle[v2.ThePath].Remove(v2);
                }
            }
        }
        private static void Destroy(GameObjectCeil ceil)
        {
            if (m_using.ContainsKey(ceil.ThePath) && m_using[ceil.ThePath].Contains(ceil))
            {
                m_using[ceil.ThePath].Remove(ceil);
            }
            if (m_idle.ContainsKey(ceil.ThePath) && m_idle[ceil.ThePath].Contains(ceil))
            {
                m_idle[ceil.ThePath].Remove(ceil);
            }
            ResourceLoader.UnloadObject(ceil.ThePath);
        }
        private static void BackToPool(GameObjectCeil ceil)
        {
            CreatDict(ceil.ThePath);
            if (!ceil.IsBeingUse)
            {
#if UNITY_EDITOR
                Debug.LogError("[资产非法][资产回池异常！！][重复回池][The Asset Name:]" + ceil.ThePath);
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            ceil.SetBeingUse(false);
            if (null != ceil.TheTransform)
            {
                ceil.TheTransform.parent = m_root;
            }

            if (m_using[ceil.ThePath].Contains(ceil))
            {
                m_using[ceil.ThePath].Remove(ceil);
            }
            if (!m_idle[ceil.ThePath].Contains(ceil))
            {
                m_idle[ceil.ThePath].Add(ceil);
            }
        }
        private static GameObjectCeil GetFromPool(string path)
        {
            if (!m_idle.ContainsKey(path))
            {
                return null;
            }

            GameObjectCeil ceil = null;
            int index = m_idle[path].Count;
            while (true)
            {
                index--;
                if (index < 0)
                {
                    break;
                }

                ceil = m_idle[path][index];
                m_idle[path].RemoveAt(index);
                if (null != ceil)
                {
                    if (ceil.IsDestroyed)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][资产销毁异常！！][The Asset Name:]" + ceil.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                    }
                    else if (ceil.IsBeingUse)
                    {
#if UNITY_EDITOR
                        Debug.LogError("[资产非法][资产使用异常！！][The Asset Name:]" + ceil.ThePath);
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ceil;
        }
        private static GameObjectCeil GetFromLoad(string path)
        {
            UnityEngine.Object obj = ResourceLoader.LoadObject(path, typeof(GameObject));
            if (null == obj)
            {
                return null;
            }

            GameObject target = GameObject.Instantiate(obj) as GameObject;
            GameObjectCeil ceil = target.AddComponentIfNotExist<GameObjectCeil>();
            ceil.Init(path, Destroy);
            ceil.TheTransform.parent = m_root;
            return ceil;
        }
        private static void GetFromAysncLoad(string path, Action<GameObjectCeil> cb)
        {
            ResourceLoader.AsyncLoadObject(path, typeof(GameObject), (obj) =>
            {
                if (null == obj)
                {
                    cb?.Invoke(null);
                    return;
                }

                GameObject target = GameObject.Instantiate(obj) as GameObject;
                GameObjectCeil ceil = target.AddComponentIfNotExist<GameObjectCeil>();
                ceil.Init(path, Destroy);
                ceil.TheTransform.parent = m_root;
                cb?.Invoke(ceil);
            });
        }
        private static void CreatDict(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
#if UNITY_EDITOR
                Debug.LogError("[资产非法][资产池创建异常！！][The Asset Name:]" + path);
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
            if (!m_idle.ContainsKey(path))
            {
                m_idle.Add(path, new List<GameObjectCeil>());
            }
            if (!m_using.ContainsKey(path))
            {
                m_using.Add(path, new List<GameObjectCeil>());
            }
        }
    }
}
