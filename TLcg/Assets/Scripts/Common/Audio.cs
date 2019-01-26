using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LCG
{
    public class Audio : MonoBehaviour
    {
        void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}