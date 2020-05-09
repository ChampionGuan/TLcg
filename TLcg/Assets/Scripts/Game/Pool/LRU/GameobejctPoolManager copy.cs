using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class GameobejctPoolManager
    {
        // 注意！！ idle队列采用栈逻辑处理，索引靠前为最早空闲
        private static Dictionary<string, List<GameobejctPoolCeil>> m_idle = new Dictionary<string, List<GameobejctPoolCeil>>();
        private static Dictionary<string, List<GameobejctPoolCeil>> m_using = new Dictionary<string, List<GameobejctPoolCeil>>();
        private static List<GameobejctPoolCeil> m_idleSort = new List<GameobejctPoolCeil>();
        private static Dictionary<string, int> m_rfc = new Dictionary<string, int>();
        private static List<GameobejctPoolCeil> m_temp = new List<GameobejctPoolCeil>();
        private static float m_tick = 0;
        public static void Update()
        {
            m_temp.Clear();
            foreach (var v in m_using)
            {
                foreach (var v2 in v.Value)
                {
                    if (!v2.IsValid || !v2.InUse)
                    {
                        m_temp.Add(v2);
                    }
                }
            }
            foreach (var v in m_temp)
            {
                m_using[v.ThePath].Remove(v);
                m_idle[v.ThePath].Add(v);
            }

            LRUCheck();
        }
        public static GameObject Get(string path)
        {
            GameobejctPoolCeil ceil = GetFromPool(path);
            if (null == ceil)
            {
                ceil = GetFromLoad(path);
            }
            if (null == ceil)
            {
                return null;
            }
            CreatDir(path);
            ceil.SetInUse(true);
            m_using[path].Add(ceil);

            return ceil.TheGameobject;
        }
        public static void AsyncGet(string path, Action<GameObject> cb)
        {
            GameobejctPoolCeil ceil = GetFromPool(path);
            Action<GameobejctPoolCeil> go = (obj) =>
            {
                if (obj = null)
                {
                    return;
                }

                CreatDir(path);
                ceil.SetInUse(true);
                m_using[path].Add(ceil);

                cb.Invoke(obj.TheGameobject);
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
            GameobejctPoolCeil ceil = obj.GetComponent<GameobejctPoolCeil>();
            if (null == ceil)
            {
                UnityEngine.GameObject.Destroy(obj);
            }
            else
            {
                ceil.SetInUse(false);
            }
        }
        private static void Back(GameobejctPoolCeil ceil)
        {
            if (m_using[ceil.ThePath].Contains(ceil))
            {
                m_using[ceil.ThePath].Remove(ceil);
            }
            if (m_idle[ceil.ThePath].Contains(ceil))
            {
                m_idle[ceil.ThePath].Remove(ceil);
            }

            int c = m_using[ceil.ThePath].Count + m_idle[ceil.ThePath].Count;
            while (m_rfc[ceil.ThePath] - c > 0)
            {
                ResourceLoader.UnloadObject(ceil.ThePath);
            }
        }
        private static void LRUCheck()
        {
            // TODO 多长时间检查、达到多少内存检查、达到多少个资源检查
            // TODO 检查空闲队列，找出空闲较久资源，然后销毁

            m_tick += Time.deltaTime;
            // 5分钟检查一次
            if (m_tick > 5 * 60)
            {
                m_tick = 0;
                // 按照空闲时间排序
                m_idleSort.Clear();
                foreach (var v in m_idle)
                {
                    foreach (var v2 in v.Value)
                    {
                        m_idleSort.Add(v2);
                    }
                }
                m_idleSort.Sort((a, b) => a.IdleDuration().CompareTo(b.IdleDuration()));
            }
        }
        private static GameobejctPoolCeil GetFromPool(string path)
        {
            if (!m_idle.ContainsKey(path))
            {
                return null;
            }
            if (m_idle[path].Count < 1)
            {
                return null;
            }
            GameobejctPoolCeil ceil = null;
            int count = m_idle[path].Count - 1;
            do
            {
                ceil = m_idle[path][count];
                m_idle[path].RemoveAt(count);
            }
            while (null == ceil && m_idle[path].Count > 0);

            return ceil;
        }
        private static GameobejctPoolCeil GetFromLoad(string path)
        {
            UnityEngine.Object obj = ResourceLoader.LoadObject(path, typeof(GameObject));
            if (null == obj)
            {
                return null;
            }

            if (!m_rfc.ContainsKey(path))
            {
                m_rfc.Add(path, 1);
            }
            else
            {
                m_rfc[path]++;
            }

            GameObject target = GameObject.Instantiate(obj) as GameObject;
            GameobejctPoolCeil ceil = target.AddComponentIfNotExist<GameobejctPoolCeil>();
            ceil.Init(path, Back);

            return ceil;
        }
        private static void GetFromAysncLoad(string path, Action<GameobejctPoolCeil> cb)
        {
            ResourceLoader.AsyncLoadObject(path, typeof(GameObject), (obj) =>
            {
                if (null == obj)
                {
                    cb.Invoke(null);
                    return;
                }

                if (!m_rfc.ContainsKey(path))
                {
                    m_rfc.Add(path, 1);
                }
                else
                {
                    m_rfc[path]++;
                }

                GameObject target = GameObject.Instantiate(obj) as GameObject;
                GameobejctPoolCeil ceil = target.AddComponentIfNotExist<GameobejctPoolCeil>();
                ceil.Init(path, Back);
                cb.Invoke(ceil);
            });
        }
        private static void CreatDir(string path, bool scene = false)
        {
            if (!m_idle.ContainsKey(path))
            {
                m_idle.Add(path, new List<GameobejctPoolCeil>());
            }
            if (!m_using.ContainsKey(path))
            {
                m_using.Add(path, new List<GameobejctPoolCeil>());
            }
        }
    }
}
