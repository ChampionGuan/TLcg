using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace LCG
{
    public class Gyroscope : MonoBehaviour
    {
        private Transform theTarget;

        private Vector3 posOffset = Vector3.zero;
        private Vector3 posAttitude = Vector3.zero;

        public float posFactorX = 1;
        public float posFactorY = 1;
        public float lerpTime = 0.5f;

        private static Gyroscope gyro = null;
        public static void SetFactor(float x, float y, float t)
        {
            if (null == gyro)
            {
                return;
            }
            gyro.posFactorX = x;
            gyro.posFactorY = y;
            gyro.lerpTime = t;
        }

        public static bool Visible
        {
            set
            {
                if (null == gyro)
                {
                    return;
                }
                gyro.enabled = value;
            }
        }

        void Start()
        {
            gyro = this;
            theTarget = transform;
            posOffset = theTarget.localPosition;

            GetAccelerationnfo();
            theTarget.DOLocalMove(posAttitude + posOffset, 0);
        }

        void OnDestroy()
        {
            gyro = null;
        }

        void Update()
        {
            GetAccelerationnfo();
            theTarget.DOLocalMove(posAttitude + posOffset, lerpTime);
        }

        void GetAccelerationnfo()
        {
            posAttitude.z = 0;
            posAttitude.x = -posFactorX * Input.acceleration.x;
            posAttitude.y = -posFactorY * Input.acceleration.y;
        }

// #if UNITY_EDITOR
//         string lerp = "0.5";
//         string factorX = "1";
//         string factorY = "1";

//         string show1 = "0";
//         string show2 = "0";

//         float height = 0;
//         float weight = 0;

//         void OnGUI()
//         {
//             height = Screen.height / 10;
//             weight = Screen.width / 10;

//             GUI.Label(new Rect(0, height * 1, weight, height), "重力信息");
//             show2 = GUI.TextField(new Rect(weight * 1, height * 1, weight * 3, height), string.Format("x:{0}   y:{1}   z:{2}", Input.acceleration.x, Input.acceleration.y, Input.acceleration.z));

//             GUI.Label(new Rect(0, height * 3, weight, height), "缓动时间");
//             lerp = GUI.TextField(new Rect(weight * 1, height * 3, weight, height), lerp);

//             GUI.Label(new Rect(0, height * 4, weight, height), "取值范围");
//             factorX = GUI.TextField(new Rect(weight * 1, height * 4, weight, height), factorX);
//             factorY = GUI.TextField(new Rect(weight * 2, height * 4, weight, height), factorY);

//             if (GUI.RepeatButton(new Rect(weight * 2, height * 3, weight, height), "使用设置"))
//             {
//                 lerpTime = float.Parse(lerp);
//                 posFactorX = float.Parse(factorX);
//                 posFactorY = float.Parse(factorY);
//             }
//         }
// #endif
    }
}