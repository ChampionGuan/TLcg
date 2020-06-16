using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class GameObjectCeil : MonoBehaviour
    {
        public GameObject TheGameobject
        {
            get; private set;
        }
        public Transform TheTransform
        {
            get; private set;
        }
        public string ThePath
        {
            get; private set;
        }
        public bool IsBeingUse
        {
            get; private set;
        }
        public bool IsDestroyed
        {
            get; private set;
        }
        public float UsedDuration
        {
            get
            {
                if (!IsBeingUse || IsDestroyed)
                {
                    return -1;
                }
                return Time.realtimeSinceStartup - m_beingUse_TS;
            }
        }
        public float IdleDuration
        {
            get
            {
                if (IsBeingUse || IsDestroyed)
                {
                    return -1;
                }
                return Time.realtimeSinceStartup - m_backIdle_TS;
            }
        }


        private Action<GameObjectCeil> m_onDestroy;
        private float m_beingUse_TS;
        private float m_backIdle_TS;

        public void Init(string path, Action<GameObjectCeil> back)
        {
            ThePath = path;
            TheGameobject = gameObject;
            TheTransform = transform;
            IsBeingUse = false;
            IsDestroyed = false;
            m_onDestroy = back;
        }
        public void SetBeingUse(bool value)
        {
            IsBeingUse = value;
            SetActive(value);

            if (value)
            {
                m_beingUse_TS = Time.realtimeSinceStartup;
            }
            else
            {
                m_backIdle_TS = Time.realtimeSinceStartup;
            }
        }
        void SetActive(bool active)
        {
            if (IsDestroyed)
            {
                return;
            }
            TheGameobject.SetActive(active);
        }
        void Awake()
        {
            TheGameobject = gameObject;
            TheTransform = transform;
            DontDestroyOnLoad(TheGameobject);
            SetActive(false);
        }
        void OnDestroy()
        {
            IsBeingUse = false;
            IsDestroyed = true;
            TheGameobject = null;
            TheTransform = null;
            if (null != m_onDestroy) m_onDestroy(this);
            m_onDestroy = null;
        }
    }
}