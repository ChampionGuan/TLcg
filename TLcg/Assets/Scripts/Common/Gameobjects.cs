using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LCG
{
    public class Gameobjects : Singleton<Gameobjects>, Define.IMonoBase
    {
        private Pool m_pool = new Pool();
        private Pool m_poolScene = new Pool();

        public Gameobjects()
        {
            SceneLoader.Instance.SwtichingScene = SwitchingScene;
        }
        public UnityMono Spawner(string path, bool scene = false)
        {
            return scene ? m_poolScene.Spawner(path) : m_pool.Spawner(path);
        }
        public void Destroy(string path)
        {
            m_pool.Destroy(path);
            m_poolScene.Destroy(path);
        }
        public void CustomUpdate()
        {
            m_pool.CustomUpdate();
            m_poolScene.CustomUpdate();
        }
        public void CustomFixedUpdate()
        {
            m_pool.CustomFixedUpdate();
            m_poolScene.CustomFixedUpdate();
        }
        public void CustomDestroy()
        {
            m_pool.CustomDestroy();
            m_poolScene.CustomDestroy();
        }
        public void CustomAppFocus(bool focus)
        {
            m_pool.CustomAppFocus(focus);
            m_poolScene.CustomAppFocus(focus);
        }
        private void SwitchingScene()
        {
            m_poolScene.CustomDestroy();
        }
        public class Pool : Define.IMonoBase
        {
            private Dictionary<string, List<UnityMono>> m_idle = new Dictionary<string, List<UnityMono>>();
            private Dictionary<string, List<UnityMono>> m_using = new Dictionary<string, List<UnityMono>>();
            public UnityMono Spawner(string path)
            {
                UnityMono mono = GetFromPool(path);
                if (null == mono)
                {
                    mono = GetFromLoad(path);
                }
                if (null == mono)
                {
                    return null;
                }

                CreatDir(path);
                m_using[path].Add(mono);
                return mono;
            }
            public void Destroy(string path)
            {
                if (m_using.ContainsKey(path))
                {
                    for (int i = m_using[path].Count - 1; i >= 0; i--)
                    {
                        m_using[path][i].CustomDestroy(true);
                    }
                    m_using[path].Clear();
                }
                if (m_idle.ContainsKey(path))
                {
                    for (int i = m_idle[path].Count - 1; i >= 0; i--)
                    {
                        m_idle[path][i].CustomDestroy(true);
                    }
                    m_idle[path].Clear();
                }
            }
            private UnityMono GetFromPool(string path)
            {
                if (!m_idle.ContainsKey(path) || m_idle[path].Count < 1)
                {
                    return null;
                }

                UnityMono mono = m_idle[path][0];
                m_idle[path].RemoveAt(0);
                if (!mono.IsValid())
                {
                    return GetFromPool(path);
                }
                else
                {
                    return mono;
                }
            }
            private UnityMono GetFromLoad(string path)
            {
                UnityEngine.Object obj = ResourceLoader.LoadObject(path, typeof(GameObject));
                if (null == obj)
                {
                    return null;
                }
                UnityMono mono = (GameObject.Instantiate(obj) as GameObject).AddComponent<UnityMono>();
                mono.Initialize(path, Back);
                mono.SetParent(Main.Instance.Target);
                mono.SetActive(false);
                return mono;
            }
            private void Back(UnityMono mono)
            {
                if (null == mono || string.IsNullOrEmpty(mono.ThePath))
                {
                    return;
                }
                if (m_using.ContainsKey(mono.ThePath) && m_using[mono.ThePath].Contains(mono))
                {
                    m_using[mono.ThePath].Remove(mono);
                }
                if (mono.IsValid())
                {
                    mono.SetParent(Main.Instance.Target);
                    mono.SetActive(false);

                    if (!m_idle[mono.ThePath].Contains(mono))
                    {
                        m_idle[mono.ThePath].Add(mono);
                    }
                }
                else
                {
                    ResourceLoader.UnloadObject(mono.ThePath);
                }
            }
            public void CustomUpdate()
            {
                foreach (var v in m_using.Values)
                {
                    for (int i = v.Count - 1; i >= 0; i--)
                    {
                        v[i].CustomUpdate();
                    }
                }
            }
            public void CustomFixedUpdate()
            {
                foreach (var v in m_using.Values)
                {
                    for (int i = v.Count - 1; i >= 0; i--)
                    {
                        v[i].CustomFixedUpdate();
                    }
                }
            }
            public void CustomAppFocus(bool focus)
            {
                foreach (var v in m_using.Values)
                {
                    for (int i = v.Count - 1; i >= 0; i--)
                    {
                        v[i].CustomAppFocus(focus);
                    }
                }
            }
            public void CustomDestroy()
            {
                foreach (var v in m_using.Keys)
                {
                    Destroy(v);
                }
            }
            private void CreatDir(string path, bool scene = false)
            {
                if (!m_idle.ContainsKey(path))
                {
                    m_idle.Add(path, new List<UnityMono>());
                }
                if (!m_using.ContainsKey(path))
                {
                    m_using.Add(path, new List<UnityMono>());
                }
            }
        }
    }
}
