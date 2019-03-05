using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LCG
{
    public partial class UnityMono : MonoBehaviour, Define.IMonoBase
    {
        public string ThePath { get; private set; }

        private GameObject theGameobject;
        private Transform theTransform;
        private Action<UnityMono> theBack;

        public void Initialize(string path, Action<UnityMono> back)
        {
            ThePath = path;
            theBack = back;
            theGameobject = gameObject;
            theTransform = transform;
        }
        public void SetActive(bool active)
        {
            if (IsValid())
            {
                theGameobject.SetActive(active);
            }
        }
        public void SetParent(Transform parent)
        {
            if (IsValid())
            {
                theTransform.parent = parent;
            }
        }
        public bool IsValid()
        {
            return null != theGameobject;
        }
        public void CustomUpdate()
        {

        }
        public void CustomFixedUpdate()
        {

        }
        public void CustomAppFocus(bool focus)
        {
        }
        public void CustomDestroy()
        {
            CustomDestroy(false);
        }
        public void CustomDestroy(bool isDeep)
        {
            if (null == theGameobject)
            {
                return;
            }
            if (isDeep)
            {
                GameObject.Destroy(theGameobject);
            }
            else
            {
                if (null != theBack)
                {
                    theBack.Invoke(this);
                }
            }
        }
        private void OnDestroy()
        {
            theGameobject = null;
            theTransform = null;
            if (null != theBack)
            {
                theBack.Invoke(this);
            }
        }
    }
}