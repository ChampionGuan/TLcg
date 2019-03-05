using UnityEngine;

namespace LCG
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        protected static T _instance;
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
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    string name = typeof(T).Name;
                    GameObject parent = GameObject.Find(name);
                    if (null == parent)
                    {
                        parent = new GameObject(name);
                    }
                    _instance = parent.AddComponent<T>();
                    _instance.OnInstance();
                    DontDestroyOnLoad(parent);
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

        public static bool IsNull<t>(this UnityEngine.Object target) where t : UnityEngine.Object
        {
            if (null == target)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}