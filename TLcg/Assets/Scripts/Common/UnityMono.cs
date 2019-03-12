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
        public bool IsValid()
        {
            return null != theGameobject;
        }
        public bool IsActive()
        {
            if (!IsValid())
            {
                return false;
            }
            return theGameobject.activeSelf;
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
        public T AddComponentIfNotExist<T>() where T : Component
        {
            if (!IsValid())
            {
                return null;
            }
            T t = theGameobject.GetComponent<T>();
            if (null != t)
            {
                return t;
            }
            return theGameobject.AddComponent<T>();
        }
        public Transform GetChild(string path)
        {
            if (!IsValid())
            {
                return null;
            }
            return theTransform.Find(path);
        }
        public Vector3 GetPosition()
        {
            if (!IsValid())
            {
                return Vector3.zero;
            }
            return theTransform.position;
        }
        public float GetPositionX()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.position.x;
        }
        public float GetPositionY()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.position.y;
        }
        public float GetPositionZ()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.position.z;
        }
        public Vector3 GetRotation()
        {
            if (!IsValid())
            {
                return Vector3.zero;
            }
            return theTransform.eulerAngles;
        }
        public float GetRotationX()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.rotation.x;
        }
        public float GetRotationY()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.rotation.y;
        }
        public float GetRotationZ()
        {
            if (!IsValid())
            {
                return 0;
            }
            return theTransform.rotation.z;
        }
        public void SetPosition(Vector3 v3)
        {
            if (!IsValid())
            {
                return;
            }
            theTransform.position = v3;
        }
        public void SetPosition(int x, int y, int z)
        {
            if (!IsValid())
            {
                return;
            }
            theTransform.position = new Vector3(x, y, z);
        }
        public void SetEulerAngles(Vector3 v3)
        {
            if (!IsValid())
            {
                return;
            }
            theTransform.eulerAngles = v3;
        }
        public void SetEulerAngles(int x, int y, int z)
        {
            if (!IsValid())
            {
                return;
            }
            theTransform.eulerAngles = new Vector3(x, y, z);
        }
        public void SetPos2Rot(Vector3 pos, Vector3 rot)
        {
            if (!IsValid())
            {
                return;
            }
            if (null != pos)
            {
                theTransform.position = pos;
            }
            if (null != rot)
            {
                theTransform.eulerAngles = rot;
            }
        }
        public void SetLayer(string layerName)
        {
            SetLayer(theTransform, LayerMask.NameToLayer(layerName));
        }

        public void SetPos2Rot(Vector3 pos, Quaternion rot)
        {
            if (!IsValid())
            {
                return;
            }
            if (null != pos)
            {
                theTransform.position = pos;
            }
            if (null != rot)
            {
                theTransform.rotation = rot;
            }
        }
        public void SetRotByDir(Vector3 dir)
        {
            if (!IsValid())
            {
                return;
            }
            if (null == dir || dir == Vector3.zero)
            {
                return;
            }
            float angle = Vector3.Angle(dir, theTransform.forward);
            float dc = Vector3.Dot(theTransform.up, Vector3.Cross(theTransform.forward, dir));
            theTransform.Rotate(theTransform.up, angle * (dc < 0 ? -1 : 1));
        }
        private void SetLayer(Transform target, int layerId)
        {
            if (null == target)
            {
                return;
            }
            target.gameObject.layer = layerId;
            foreach (Transform child in target)
            {
                child.gameObject.layer = layerId;
                SetLayer(child, layerId);
            }
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
                    StopAllCoroutines();
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
                StopAllCoroutines();
                theBack.Invoke(this);
            }
        }
    }
}