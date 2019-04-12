using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LCG
{
    public class Video : MonoBehaviour
    {
        private GameObject m_root = null;
        private GameObject m_btnSkip = null;
        private GameObject m_subtitleRoot = null;
        private Text m_subtitleText = null;
        private VideoPlayer m_videoPlayer = null;
        private AudioSource m_audioSource = null;
        private List<string> m_videoList = new List<string>();
        private bool m_isAutoUnload = false;

        public System.Action PreparedCallback
        {
            private get; set;
        }
        public System.Action CompleteCallback
        {
            private get; set;
        }
        public System.Action<float> ProcessCallback
        {
            private get; set;
        }
        public bool IsPlaying
        {
            get { return m_videoPlayer.isPlaying; }
        }

        void Awake()
        {
            m_root = gameObject;
            m_btnSkip = transform.Find("Canvas/BtnSkip").gameObject;
            m_subtitleRoot = transform.Find("Canvas/Subtitle").gameObject;
            m_subtitleText = m_subtitleRoot.transform.Find("SubtitleBg/Text").GetComponent<Text>();

            m_audioSource = m_root.AddComponentIfNotExist<AudioSource>();
            m_audioSource.playOnAwake = false;

            m_videoPlayer = m_root.AddComponentIfNotExist<VideoPlayer>();
            m_videoPlayer.playOnAwake = false;
            m_videoPlayer.waitForFirstFrame = true;
            m_videoPlayer.skipOnDrop = true;
            m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            m_videoPlayer.EnableAudioTrack(0, true);
            m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
            m_videoPlayer.loopPointReached += LoopPointReached;
            m_videoPlayer.prepareCompleted += PrepareCompleted;

            m_btnSkip.SetActive(false);
            m_subtitleRoot.SetActive(false);
            GameObject.DontDestroyOnLoad(m_root);
        }

        public bool AsyncPlay(string videoUrl, bool isLoop, bool isSkip, bool isAutoUnload = true)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return false;
            }

            ResourceLoader.AsyncLoadObject(videoUrl, typeof(VideoClip), (clip) =>
            {
                Play(videoUrl, isLoop, isSkip, isAutoUnload, clip as VideoClip);
            }, LoadProcess);
            return true;
        }
        public bool Play(string videoUrl, bool isLoop, bool isSkip, bool isAutoUnload = true)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return false;
            }

            VideoClip clip = ResourceLoader.LoadObject(videoUrl, typeof(VideoClip)) as VideoClip;
            if (null == clip)
            {
                return false;
            }

            return Play(videoUrl, isLoop, isSkip, isAutoUnload, clip);
        }

        public void Stop()
        {
            if (null == m_videoPlayer.clip)
            {
                return;
            }

            UnloadThePreVideo();
            m_videoPlayer.Stop();
            m_videoPlayer.clip = null;
            if (null != CompleteCallback)
            {
                CompleteCallback.Invoke();
            }
        }

        public void PlaySubtitle(float start, float end, string text)
        {
            m_subtitleRoot.SetActive(true);
            m_subtitleText.text = string.Empty;
            StartCoroutine(Subtitle(start, end, text));
        }

        public void StopSubtitle()
        {
            if (null == m_root)
            {
                return;
            }
            StopAllCoroutines();
            m_subtitleText.text = string.Empty;
            m_subtitleRoot.SetActive(false);
        }

        public void UnloadAll()
        {
            foreach (var v in m_videoList)
            {
                ResourceLoader.UnloadObject(v);
            }
            m_videoList.Clear();
        }

        public void SetActive(bool value)
        {
            m_root.SetActive(value);
        }

        public void SetVolume(float value)
        {
            value = value < 0 ? 0 : value;
            value = value > 1 ? 1 : value;
            m_audioSource.volume = value;
        }

        public void CustomDestroy()
        {
            // 移除所有缓存
            UnloadAll();
            // 释放掉RenderImage占用的硬件资源
            if (null != m_videoPlayer)
            {
                m_videoPlayer.clip = null;
                m_videoPlayer.targetTexture.Release();
            }

            // 清空回调
            CompleteCallback = null;
            PreparedCallback = null;
            ProcessCallback = null;
            if (null != m_videoPlayer)
            {
                m_videoPlayer.loopPointReached -= LoopPointReached;
                m_videoPlayer.prepareCompleted -= PrepareCompleted;
            }
            // 销毁根对象
            if (null != m_root)
            {
                StopAllCoroutines();
                GameObject.Destroy(m_root);
            }
            m_videoPlayer = null;
            m_audioSource = null;
        }
        private bool Play(string videoUrl, bool isLoop, bool isSkip, bool isAutoUnload, VideoClip clip)
        {
            if (null == clip)
            {
                return false;
            }
            // 停止所有协程
            StopAllCoroutines();
            // 上一自动销毁
            UnloadThePreVideo();

            // 有相同则移至队尾
            if (m_videoList.Contains(videoUrl))
            {
                m_videoList.Remove(videoUrl);
            }
            m_videoList.Add(videoUrl);

            // 允许缓存两个视频源
            if (m_videoList.Count > 2)
            {
                string url = m_videoList[0];
                ResourceLoader.UnloadObject(url);
            }

            m_isAutoUnload = isAutoUnload;
            m_btnSkip.SetActive(isSkip);
            m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
            m_videoPlayer.clip = clip as VideoClip;
            m_videoPlayer.isLooping = isLoop;
            m_videoPlayer.Prepare();

            return true;
        }
        private void UnloadThePreVideo()
        {
            if (!m_isAutoUnload || m_videoList.Count <= 0)
            {
                return;
            }

            int count = m_videoList.Count - 1;
            string url = m_videoList[count];
            m_videoList.RemoveAt(count);
            ResourceLoader.UnloadObject(url);
            m_isAutoUnload = false;
        }

        private void LoopPointReached(VideoPlayer vp)
        {
            if (vp.isLooping)
            {
                return;
            }
            UnloadThePreVideo();
            m_videoPlayer.clip = null;
            StartCoroutine(Wait(CompleteCallback));
        }

        private void PrepareCompleted(VideoPlayer vp)
        {
            vp.Play();
            StartCoroutine(Wait(PreparedCallback));
        }
        private void LoadProcess(float p)
        {
            if (null != ProcessCallback)
            {
                ProcessCallback.Invoke(p);
            }
        }
        private IEnumerator Wait(System.Action action)
        {
            yield return null;
            if (null != action)
            {
                action.Invoke();
            }
        }
        private IEnumerator Subtitle(float start, float end, string text)
        {
            if (start >= end)
            {
                yield break;
            }

            if (start > 0)
            {
                yield return new WaitForSeconds(start);
            }
            m_subtitleText.text = text;

            if (end > 0)
            {
                yield return new WaitForSeconds(end);
            }
            m_subtitleText.text = string.Empty;
        }
    }
}