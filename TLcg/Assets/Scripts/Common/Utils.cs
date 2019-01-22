using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public static class Utils
    {
        /// <summary>
        /// 判空检测
        /// </summary>
        public static bool IsNull(UnityEngine.Object target)
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
        /// <summary>
        /// bytes转texture2D
        /// </summary>
        public static void TextureParse(int width, int height, byte[] bytes, Action<string, Texture2D> callBack)
        {
            string desc = "";
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(width, height);
                tex.LoadImage(bytes);
            }
            catch (Exception e)
            {
                desc = e.Message;
            }
            if (null != callBack)
            {
                callBack(desc, tex);
            }
        }
    }
}
