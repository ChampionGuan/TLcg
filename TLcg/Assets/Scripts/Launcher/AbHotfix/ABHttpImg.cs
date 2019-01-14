using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace LCG
{
    public class ABHttpImg : SingletonMonobehaviour<ABHttpImg>
    {
        public static string LocalStorgePath
        {
            get; private set;
        }

        void Awake()
        {
            LocalStorgePath = Application.temporaryCachePath + "/image/";
        }

        public void GetImage(string url, Action<string, Texture2D> callback)
        {
            if (String.IsNullOrEmpty(url) || null == callback)
            {
                return;
            }

            if (!File.Exists(LocalStorgePath + url.GetHashCode()))
            {
                StartCoroutine(DownloadImage(url, callback));
            }
            else
            {
                StartCoroutine(LoadLocalImage(url, callback));
            }
        }

        public void Clear()
        {
            StartCoroutine(ClearImage());
        }

        private IEnumerator LoadLocalImage(string url, Action<string, Texture2D> callback)
        {
            string filePath = "file:///" + LocalStorgePath + url.GetHashCode();
            WWW www = new WWW(filePath);
            yield return www;

            if (String.IsNullOrEmpty(www.error) && null != www.textureNonReadable)
            {
                callback(url, www.textureNonReadable);
            }
            else
            {
                // 重新下载
                DownloadImage(url, callback);
            }
        }

        private IEnumerator DownloadImage(string url, Action<string, Texture2D> callback)
        {
            WWW www = new WWW(url);
            yield return www;

            if (String.IsNullOrEmpty(www.error) && null != www.textureNonReadable)
            {
                callback(url, www.textureNonReadable);
                // 保存本地
                if (!Directory.Exists(LocalStorgePath))
                {
                    Directory.CreateDirectory(LocalStorgePath);
                }
                File.WriteAllBytes(LocalStorgePath + url.GetHashCode(), www.bytes);
            }
        }

        private IEnumerator ClearImage()
        {
            if (Directory.Exists(LocalStorgePath))
            {
                Directory.Delete(LocalStorgePath, true);
            }
            yield return null;
        }
    }
}