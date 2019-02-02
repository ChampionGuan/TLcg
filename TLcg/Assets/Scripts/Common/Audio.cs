using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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