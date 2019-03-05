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
        private GameObject m_subtitleTarget = null;
        private Text m_subtitleText = null;
        private VideoPlayer m_videoPlayer = null;
        private AudioSource m_audioSource = null;
        private Dictionary<string, VideoClip> m_videoClips = new Dictionary<string, VideoClip>();

        public System.Action PreparedCallback
        {
            private get; set;
        }
        public System.Action OverCallback
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
            m_subtitleTarget = transform.Find("Canvas/Subtitle").gameObject;
            m_subtitleText = m_subtitleTarget.transform.Find("SubtitleBg/Text").GetComponent<Text>();

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
            m_subtitleTarget.SetActive(false);
            GameObject.DontDestroyOnLoad(m_root);
        }

        public bool Play(string videoUrl, bool isLoop, bool isSkip)
        {
            if (string.IsNullOrEmpty(videoUrl))
            {
                return false;
            }

            VideoClip clip = null;
            if (m_videoClips.ContainsKey(videoUrl))
            {
                clip = m_videoClips[videoUrl];
            }
            if (null == clip)
            {
                clip = ResourceLoader.LoadObject(videoUrl, typeof(VideoClip)) as VideoClip;
                m_videoClips[videoUrl] = clip;
            }
            m_btnSkip.SetActive(isSkip);
            m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
            m_videoPlayer.clip = clip;
            m_videoPlayer.isLooping = isLoop;
            m_videoPlayer.Prepare();

            return true;
        }

        public void Stop()
        {
            if (null == m_videoPlayer.clip)
            {
                return;
            }

            m_videoPlayer.Stop();
            m_videoPlayer.clip = null;
            if (null != OverCallback)
            {
                OverCallback.Invoke();
            }
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

        public void Release()
        {
            if (null != m_videoPlayer)
            {
                m_videoPlayer.clip = null;
                // 释放掉RenderImage占用的硬件资源
                m_videoPlayer.targetTexture.Release();
            }
            if (null != m_videoClips)
            {
                // 释放掉缓存的视频
                m_videoClips.Clear();
            }
        }

        public void CustomDestroy()
        {
            Release();
            OverCallback = null;
            PreparedCallback = null;
            if (null != m_videoPlayer)
            {
                m_videoPlayer.loopPointReached -= LoopPointReached;
                m_videoPlayer.prepareCompleted -= PrepareCompleted;
            }
            if (null != m_root)
            {
                GameObject.Destroy(m_root);
            }
            m_videoPlayer = null;
            m_audioSource = null;
        }

        private void LoopPointReached(VideoPlayer vp)
        {
            if (vp.isLooping)
            {
                return;
            }
            m_videoPlayer.clip = null;
            StartCoroutine(Wait(OverCallback));
        }

        private void PrepareCompleted(VideoPlayer vp)
        {
            vp.Play();
            StartCoroutine(Wait(PreparedCallback));
        }

        private IEnumerator Wait(System.Action action)
        {
            yield return null;
            if (null != action)
            {
                action.Invoke();
            }
        }
    }
}