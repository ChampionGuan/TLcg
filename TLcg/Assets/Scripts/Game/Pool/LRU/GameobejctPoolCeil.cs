using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class GameobejctPoolCeil : MonoBehaviour
    {
        public GameObject TheGameobject { get; private set; }
        public string ThePath { get; private set; }
        public bool InUse { get; private set; }
        public bool IsValid { get; private set; }

        private float beginUseTS;
        private float backIdleTS;
        private Action<GameobejctPoolCeil> onDestroy;

        public void Init(string path, Action<GameobejctPoolCeil> back)
        {
            ThePath = path;
            onDestroy = back;
        }
        public void SetInUse(bool being)
        {
            InUse = being;
            SetActive(InUse);
            if (InUse)
            {
                beginUseTS = Time.realtimeSinceStartup;
            }
            else
            {
                backIdleTS = Time.realtimeSinceStartup;
            }
        }
        public void SetActive(bool active)
        {
            if (!IsValid)
            {
                return;
            }
            TheGameobject.SetActive(active);
        }
        public float InUseDuration()
        {
            return Time.realtimeSinceStartup - beginUseTS;
        }
        public float IdleDuration()
        {
            return Time.realtimeSinceStartup - backIdleTS;
        }
        void Awake()
        {
            TheGameobject = gameObject;
            SetActive(false);
            IsValid = true;
        }
        void Destroy()
        {
            InUse = false;
            IsValid = false;
            TheGameobject = null;
            if (null != onDestroy) onDestroy(this);
            onDestroy = null;
        }
    }
}