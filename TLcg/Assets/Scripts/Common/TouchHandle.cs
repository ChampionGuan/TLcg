using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class TouchHandle : Singleton<TouchHandle>, Define.IMonoBase
    {
        // touch个数
        private int m_touchCount = 0;
        // touch类型
        private TouchType m_touchType = TouchType.None;
        // touch信息
        private List<TouchInfo> m_touchInfo = new List<TouchInfo>();
        // 拖拽距离增量
        private Vector2 m_dragDeltaPos = Vector2.zero;
        // 缩放值增量
        private float m_zoomDeltaValue = 0;

        // 拖拽阻尼
        private float DragDamp;
        // 最大touch个数
        private int MaxTouchCount;
        // 触摸允许事件
        private Func<bool> TouchValidEvent;
        // 点击事件
        private Action<Vector2> ClickEvent;
        // 拖拽事件
        private Action<Vector2> DragEvent;
        // touch事件
        private Action<Vector2> TouchBeginEvent;
        // touch事件
        private Action<Vector2> TouchEndEvent;
        // 缩放事件
        private Action<float> ZoomEvent;
        // 滚轮滚动事件
        private Action<float> ScrollWheelEvent;

        public TouchHandle()
        {
            MaxTouchCount = 1;
            DragDamp = 0.15f;
        }

        public void SetArgs(Func<bool> touchValid = null, Action<Vector2> click = null, Action<Vector2> drag = null, Action<Vector2> touchBegin = null, Action<Vector2> touchEnd = null, Action<float> zoom = null, Action<float> scrollWheel = null, float? dragDamp = null, int? maxTouchCount = null)
        {
            TouchValidEvent = touchValid;
            ClickEvent = click;
            DragEvent = drag;
            TouchBeginEvent = touchBegin;
            TouchEndEvent = touchEnd;
            ZoomEvent = zoom;
            ScrollWheelEvent = scrollWheel;
            DragDamp = null == dragDamp ? 0.15f : dragDamp.Value;
            MaxTouchCount = null == maxTouchCount ? 1 : maxTouchCount.Value;
        }
        public void Clear()
        {
            CustomDestroy();
        }
        #region 触摸屏
        private void CheckTouch()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (i > MaxTouchCount - 1)
                    {
                        break;
                    }
                    var touch = Input.touches[i];
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            AddTouchInfo(touch);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            RefreshTouchInfo(touch);
                            break;
                        case TouchPhase.Canceled:
                        case TouchPhase.Ended:
                            RemoveTouchInfo(touch);
                            break;
                        default: break;
                    }
                }
            }
            else
            {
                ClearTouchInfo();
            }
            HandleTouch();
        }

        private void AddTouchInfo(Touch info)
        {
            if (!IsTouchValid())
            {
                return;
            }

            m_touchCount++;
            m_dragDeltaPos.x = 0;
            m_dragDeltaPos.y = 0;
            TouchInfo touchInfo;
            if (m_touchInfo.Count < m_touchCount)
            {
                touchInfo = new TouchInfo();
                m_touchInfo.Add(touchInfo);
            }
            else
            {
                touchInfo = m_touchInfo[m_touchCount - 1];
            }
            touchInfo.fingerId = info.fingerId;
            touchInfo.Update(info.position, info.deltaPosition);

            if (m_touchCount == 1 && null != TouchBeginEvent)
            {
                TouchBeginEvent.Invoke(info.position);
            }
            if (m_touchCount == 2)
            {
                m_zoomDeltaValue = Vector2.Distance(m_touchInfo[0].downPos, m_touchInfo[1].downPos);
            }
        }
        private void RefreshTouchInfo(Touch info)
        {
            if (m_touchCount < 1)
            {
                return;
            }
            for (int i = 0; i < m_touchCount; i++)
            {
                if (m_touchInfo[i].fingerId == info.fingerId)
                {
                    m_touchInfo[i].Update(info.position, info.deltaPosition);
                    break;
                }
            }
        }
        private void RemoveTouchInfo(Touch info)
        {
            if (m_touchCount < 1)
            {
                return;
            }
            if (m_touchType == TouchType.Click && null != ClickEvent)
            {
                ClickEvent.Invoke(m_touchInfo[0].downPos);
            }

            for (int i = 0; i < m_touchCount; i++)
            {
                if (m_touchInfo[i].fingerId == info.fingerId)
                {
                    if (i != m_touchCount - 1)
                    {
                        m_touchInfo[i].fingerId = m_touchInfo[m_touchCount - 1].fingerId;
                        m_touchInfo[i].Update(m_touchInfo[m_touchCount - 1].downPos, m_touchInfo[m_touchCount - 1].deltaPos);
                    }

                    if (m_touchCount > 2 && i < 2)
                    {
                        m_zoomDeltaValue = Vector2.Distance(m_touchInfo[0].downPos, m_touchInfo[1].downPos);
                    }
                    if (m_touchCount == 1 && null != TouchEndEvent)
                    {
                        TouchEndEvent.Invoke(info.position);
                    }

                    m_touchCount--;
                    break;
                }
            }
        }
        private void ClearTouchInfo()
        {
            if (m_touchType == TouchType.None)
            {
                return;
            }

            m_touchCount = 0;
            m_zoomDeltaValue = 0;
            m_touchType = TouchType.None;
        }
        private void HandleTouch()
        {
            if (m_touchCount < 1)
            {
                m_touchType = TouchType.None;
                return;
            }
            // 一个touch
            if (m_touchCount == 1)
            {
                if (m_touchInfo[0].deltaPos.sqrMagnitude <= 10)
                {
                    if (m_touchType == TouchType.None)
                    {
                        m_touchType = TouchType.Click;
                    }
                }
                else
                {
                    m_touchType = TouchType.Drag;
                }
                return;
            }
            else
            {
                m_touchType = TouchType.Zoom;
            }
        }
        #endregion

        #region 鼠标、滚轮
        private void CheckMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                AddMouseInfo();
            }
            else if (Input.GetMouseButton(0))
            {
                RefreshMouseInfo();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                RemoveMouseInfo();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                m_touchType = TouchType.ScrollWheel;
            }
            else
            {
                ClearMouseInfo();
            }
        }
        private void AddMouseInfo()
        {
            if (!IsTouchValid())
            {
                return;
            }
            if (null != TouchBeginEvent)
            {
                TouchBeginEvent.Invoke(Input.mousePosition);
            }
            m_touchCount++;
            m_dragDeltaPos.x = 0;
            m_dragDeltaPos.y = 0;
            TouchInfo touchInfo;
            if (m_touchInfo.Count < m_touchCount)
            {
                touchInfo = new TouchInfo();
                m_touchInfo.Add(touchInfo);
            }
            else
            {
                touchInfo = m_touchInfo[0];
            }
            touchInfo.fingerId = 0;
            touchInfo.Update(Input.mousePosition, Vector2.zero);
        }
        private void RefreshMouseInfo()
        {
            if (m_touchCount < 1)
            {
                return;
            }
            if (Input.mousePosition.x == m_touchInfo[0].downPos.x && Input.mousePosition.y == m_touchInfo[0].downPos.y)
            {
                if (m_touchType == TouchType.None)
                {
                    m_touchType = TouchType.Click;
                }
                else if (m_touchType == TouchType.Drag)
                {
                    m_touchInfo[0].Update(Input.mousePosition);
                }
            }
            else
            {
                m_touchInfo[0].Update(Input.mousePosition);
                m_touchType = TouchType.Drag;
            }
        }
        private void RemoveMouseInfo()
        {
            if (m_touchCount < 1)
            {
                return;
            }
            if (m_touchType == TouchType.Click && null != ClickEvent)
            {
                ClickEvent.Invoke(m_touchInfo[0].downPos);
            }
            if (null != TouchEndEvent)
            {
                TouchEndEvent.Invoke(Input.mousePosition);
            }
            m_touchCount--;
        }
        private void ClearMouseInfo()
        {
            if (m_touchType == TouchType.None)
            {
                return;
            }

            m_touchCount = 0;
            m_zoomDeltaValue = 0;
            m_touchType = TouchType.None;
        }
        #endregion

        #region other
        private bool IsTouchValid()
        {
            if (null == TouchValidEvent)
            {
                return true;
            }
            else
            {
                return TouchValidEvent();
            }
        }
        private void Handle()
        {
            switch (m_touchType)
            {
                case TouchType.Drag:
                    if (m_touchCount > 1)
                    {
                        break;
                    }
                    if (null != DragEvent)
                    {
                        DragEvent.Invoke(m_touchInfo[0].deltaPos);
                    }
                    m_dragDeltaPos.x = m_touchInfo[0].deltaPos.x;
                    m_dragDeltaPos.y = m_touchInfo[0].deltaPos.y;
                    break;
                case TouchType.ScrollWheel:
                    if (null != ScrollWheelEvent)
                    {
                        ScrollWheelEvent.Invoke(Input.GetAxis("Mouse ScrollWheel"));
                    }
                    break;
                case TouchType.Zoom:
                    if (m_touchCount < 1)
                    {
                        break;
                    }
                    if (null != ZoomEvent)
                    {
                        float dis = Vector2.Distance(m_touchInfo[0].downPos, m_touchInfo[1].downPos);
                        ZoomEvent.Invoke(dis - m_zoomDeltaValue);
                        m_zoomDeltaValue = dis;
                    }
                    break;
                case TouchType.None:
                    if (m_dragDeltaPos.x <= 0.05f && m_dragDeltaPos.y <= 0.05f)
                    {
                        break;
                    }
                    if (null != DragEvent)
                    {
                        m_dragDeltaPos.x = Mathf.Lerp(m_dragDeltaPos.x, 0, DragDamp);
                        m_dragDeltaPos.y = Mathf.Lerp(m_dragDeltaPos.y, 0, DragDamp);
                        DragEvent.Invoke(m_dragDeltaPos);
                    }
                    break;
                default: break;
            }
        }
        public void CustomUpdate()
        {
            if (Input.touchSupported)
            {
                CheckTouch();
            }
            else
            {
                CheckMouse();
            }
            Handle();
        }
        public void CustomFixedUpdate()
        {
        }
        public void CustomDestroy()
        {
            TouchValidEvent = null;
            ClickEvent = null;
            DragEvent = null;
            TouchBeginEvent = null;
            TouchEndEvent = null;
            ZoomEvent = null;
            ScrollWheelEvent = null;
        }
        public void CustomAppFocus(bool focus)
        {
        }
        private enum TouchType
        {
            None = 0,
            Click,
            Drag,
            Zoom,
            ScrollWheel,
        }
        private class TouchInfo
        {
            public int fingerId;
            // 按下坐标
            public Vector2 downPos;
            // 距离上次改变的距离增量
            public Vector2 deltaPos;
            public void Clear()
            {
                fingerId = -1;
                downPos.x = 0;
                downPos.y = 0;
                deltaPos.x = 0;
                deltaPos.y = 0;
            }
            public void Update(Vector2 dPos, Vector2 dePos)
            {
                deltaPos.x = dePos.x;
                deltaPos.y = dePos.y;
                downPos.x = dPos.x;
                downPos.y = dPos.y;
            }
            public void Update(Vector2 dPos)
            {
                deltaPos.x = dPos.x - downPos.x;
                deltaPos.y = dPos.y - downPos.y;
                downPos.x = dPos.x;
                downPos.y = dPos.y;
            }
            #endregion
        }
    }
}