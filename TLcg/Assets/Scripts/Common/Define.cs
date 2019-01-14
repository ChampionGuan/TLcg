using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LCG
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    _instance.OnInstance();
                }
                return _instance;
            }
        }
        public virtual void OnInstance() { }
    }

    public class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
    {
        public static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    GameObject parent = GameObject.Find("Define");
                    if (null == parent)
                    {
                        parent = new GameObject("Define");
                    }
                    _instance = parent.AddComponent<T>();
                    _instance.OnInstance();
                }
                return _instance;
            }
        }
        public virtual void OnInstance() { }
    }

    public static class ExtensionMethod
    {
        public static T AddComponentIfNotExist<T>(this GameObject target) where T : Component
        {
            if (null == target)
            {
                return null;
            }
            T t = target.GetComponent<T>();
            if (null != t)
            {
                return t;
            }
            return target.AddComponent<T>();
        }
    }
}