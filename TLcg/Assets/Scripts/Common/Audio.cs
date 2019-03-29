using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class Audio : MonoBehaviour
    {
        public static Audio Instance
        {
            get; private set;
        }

        private Transform m_root;
        private int m_audioInsIndex = 0;
        private float m_musicVolume = 0;
        private float m_effectVolume = 0;
        private AudioListener m_curAudioListerner;
        private AudioListener m_defaultAudioListener;
        private UnityEngine.Audio.AudioMixer m_audioMixer;

        // 预加载列表
        private List<string> m_preloadList = new List<string>();
        // 空闲音源
        private List<AudioSourceInfo> m_idleAudio = new List<AudioSourceInfo>();
        // 待移除音源
        private List<AudioSourceInfo> m_removeAudio = new List<AudioSourceInfo>();
        // 音乐音源
        private Dictionary<int, AudioSourceInfo> m_musicAudio = new Dictionary<int, AudioSourceInfo>();
        // 音效音源
        private Dictionary<int, AudioSourceInfo> m_effectAudio = new Dictionary<int, AudioSourceInfo>();
        // 同组音源队列
        private Dictionary<string, List<AudioSourceInfo>> m_groupAudio = new Dictionary<string, List<AudioSourceInfo>>();
        // 同路径音源队列
        private Dictionary<string, List<AudioSourceInfo>> m_pathAudio = new Dictionary<string, List<AudioSourceInfo>>();

        void Awake()
        {
            Instance = this;
            m_root = transform;
            GameObject.DontDestroyOnLoad(gameObject);
            m_defaultAudioListener = gameObject.GetComponent<AudioListener>();
            m_curAudioListerner = GameObject.FindObjectOfType<AudioListener>();
            m_audioMixer = ResourceLoader.LoadObject("Prefabs/Misc/AudioMixer", typeof(UnityEngine.Audio.AudioMixer)) as UnityEngine.Audio.AudioMixer;
            SetAudioListener(null);
        }
        void Update()
        {
            foreach (var v in m_musicAudio.Values)
            {
                v.Update();
                if (v.CheckIdle())
                {
                    m_removeAudio.Add(v);
                }
            }
            foreach (var v in m_removeAudio)
            {
                m_musicAudio.Remove(v.InsId);
                m_idleAudio.Add(v);
                if (m_groupAudio.ContainsKey(v.TheGroupName) && m_groupAudio[v.TheGroupName].Contains(v))
                {
                    m_groupAudio[v.TheGroupName].Remove(v);
                }
                if (m_pathAudio.ContainsKey(v.ThePath) && m_pathAudio[v.ThePath].Contains(v))
                {
                    m_pathAudio[v.ThePath].Remove(v);
                }
            }
            m_removeAudio.Clear();
            foreach (var v in m_effectAudio.Values)
            {
                v.Update();
                if (v.CheckIdle())
                {
                    m_removeAudio.Add(v);
                }
            }
            foreach (var v in m_removeAudio)
            {
                m_effectAudio.Remove(v.InsId);
                m_idleAudio.Add(v);
                if (m_groupAudio.ContainsKey(v.TheGroupName) && m_groupAudio[v.TheGroupName].Contains(v))
                {
                    m_groupAudio[v.TheGroupName].Remove(v);
                }
                if (m_pathAudio.ContainsKey(v.ThePath) && m_pathAudio[v.ThePath].Contains(v))
                {
                    m_pathAudio[v.ThePath].Remove(v);
                }
            }
            m_removeAudio.Clear();
        }
        public void CustomDestroy()
        {
            foreach (var v in m_musicAudio.Values)
            {
                v.Destroy();
            }
            foreach (var v in m_effectAudio.Values)
            {
                v.Destroy();
            }
            ResourceLoader.UnloadObject("Prefabs/Misc/AudioMixer");
        }
        public void Preload(string path)
        {
            if (m_preloadList.Contains(path))
            {
                return;
            }
            ResourceLoader.LoadObject(path);
            m_preloadList.Add(path);
        }
        public void Unload(string path)
        {
            if (!m_preloadList.Contains(path))
            {
                return;
            }
            m_preloadList.Remove(path);
            ResourceLoader.UnloadObject(path);
        }
        public void UnloadAll()
        {
            foreach (var v in m_preloadList)
            {
                ResourceLoader.UnloadObject(v);
            }
            m_preloadList.Clear();
        }
        public int? Play(
            AudioState state,
            int? insId,
            string path,
            bool pathMutex,
            string group,
            bool groupMutex,
            bool isEffect,
            bool isFade,
            bool isLoop,
            float initialVolume,
            float minDistance,
            float maxDistance,
            Transform follower,
            Action onComplete)
        {
            if (null != insId && ChangeAudioState(state, insId.Value))
            {
                return insId.Value;
            }
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(group))
            {
                return null;
            }

            AudioSourceInfo audio = null;

            // 同路径互斥
            if (pathMutex && m_pathAudio.ContainsKey(path))
            {
                foreach (var v in m_pathAudio[path])
                {
                    if (ChangeAudioState(state, v.InsId))
                    {
                        return v.InsId;
                    }
                }
            }

            audio = GetIdleAudio(null != follower);
            audio.IsLoading = true;
            audio.IsFade = isFade;
            audio.IsGroupMutex = groupMutex;
            audio.IsPathMutex = pathMutex;
            audio.ThePath = path;
            audio.TheGroupName = group;
            audio.TheInitialVolume = initialVolume;
            audio.TheComplete = onComplete;
            audio.TheAudio.loop = isLoop;
            audio.TheAudio.minDistance = minDistance;
            audio.TheAudio.maxDistance = maxDistance;

            // 音效、音乐区分
            if (isEffect)
            {
                audio.TheAudio.volume = initialVolume * m_effectVolume;
                m_effectAudio.Add(audio.InsId, audio);
            }
            else
            {
                audio.TheAudio.volume = initialVolume * m_musicVolume;
                m_musicAudio.Add(audio.InsId, audio);
            }

            ResourceLoader.AsyncLoadObject(audio.ThePath, typeof(AudioClip), (clip) =>
            {
                SetAudioGroup(audio, clip as AudioClip, state);
            });

            return audio.InsId;
        }
        public void PlayByGroupName(string name, AudioState state)
        {
            if (!m_groupAudio.ContainsKey(name))
            {
                return;
            }
            foreach (var v in m_groupAudio[name])
            {
                SetAudioState(v, state);
            }
        }
        public void PlayByPathName(string name, AudioState state)
        {
            if (!m_pathAudio.ContainsKey(name))
            {
                return;
            }
            foreach (var v in m_pathAudio[name])
            {
                SetAudioState(v, state);
            }
        }
        public bool ChangeAudioState(AudioState state, int insId)
        {
            if (m_musicAudio.ContainsKey(insId))
            {
                SetAudioState(m_musicAudio[insId], state);
                return true;
            }
            if (m_effectAudio.ContainsKey(insId))
            {
                SetAudioState(m_effectAudio[insId], state);
                return true;
            }
            return false;
        }
        public void SetAudioGroup(AudioSourceInfo audio, AudioClip clip, AudioState state)
        {
            if (null == clip)
            {
                return;
            }
            audio.IsLoading = false;
            audio.TheAudio.clip = clip;
            if (!m_groupAudio.ContainsKey(audio.TheGroupName))
            {
                m_groupAudio.Add(audio.TheGroupName, new List<AudioSourceInfo>());
            }
            if (!m_pathAudio.ContainsKey(audio.ThePath))
            {
                m_pathAudio.Add(audio.ThePath, new List<AudioSourceInfo>());
            }
            if (audio.IsGroupMutex)
            {
                foreach (var v in m_groupAudio[audio.TheGroupName])
                {
                    v.Stop();
                }
            }
            if (audio.IsPathMutex)
            {
                foreach (var v in m_pathAudio[audio.ThePath])
                {
                    v.Stop();
                }
            }
            m_groupAudio[audio.TheGroupName].Add(audio);
            m_pathAudio[audio.ThePath].Add(audio);

            UnityEngine.Audio.AudioMixerGroup[] groups = m_audioMixer.FindMatchingGroups(audio.TheGroupName);
            if (groups.Length > 0)
            {
                audio.TheAudio.outputAudioMixerGroup = groups[0];
            }

            SetAudioState(audio, state);
        }
        public void SetAudioState(AudioSourceInfo audio, AudioState state)
        {
            if (null == audio)
            {
                return;
            }
            switch (state)
            {
                case AudioState.Play:
                    audio.Play();
                    break;
                case AudioState.Stop:
                    audio.Stop();
                    break;
                case AudioState.Pause:
                    audio.Pause();
                    break;
                case AudioState.UnPause:
                    audio.UnPause();
                    break;
                default: break;
            }
        }
        public void SetAudioVolume(float volume, bool isEffect)
        {
            if (isEffect)
            {
                foreach (var v in m_effectAudio.Values)
                {
                    v.TheAudio.volume = v.TheInitialVolume * volume;
                }
                m_effectVolume = volume;
            }
            else
            {
                foreach (var v in m_musicAudio.Values)
                {
                    v.TheAudio.volume = v.TheInitialVolume * volume;
                }
                m_musicVolume = volume;
            }
        }
        public void SetAudioListener(AudioListener listener)
        {
            if (null != m_curAudioListerner)
            {
                m_curAudioListerner.enabled = false;
            }

            m_curAudioListerner = null == listener ? m_defaultAudioListener : listener;

            if (null != m_curAudioListerner)
            {
                m_curAudioListerner.enabled = true;
            }
        }
        private AudioSourceInfo GetIdleAudio(bool is3d)
        {
            AudioSourceInfo audio = null;

            for (int i = m_idleAudio.Count - 1; i >= 0; i--)
            {
                if (m_idleAudio[i].Is3d == is3d)
                {
                    audio = m_idleAudio[i];
                    m_idleAudio.RemoveAt(i);
                    break;
                }
            }

            if (null == audio)
            {
                audio = new AudioSourceInfo();
                if (is3d)
                {
                    audio.TheSelf = new GameObject("3dsound").transform;
                    audio.TheAudio = audio.TheSelf.gameObject.AddComponent<AudioSource>();
                    audio.TheAudio.spatialBlend = 1;
                    audio.TheAudio.rolloffMode = AudioRolloffMode.Linear;
                    audio.TheSelf.parent = m_root;
                }
                else
                {
                    audio.TheSelf = m_root;
                    audio.TheAudio = audio.TheSelf.gameObject.AddComponent<AudioSource>();
                    audio.TheAudio.spatialBlend = 0;
                }
                audio.Is3d = is3d;
            }
            audio.IsFade = false;
            audio.InsId = ++m_audioInsIndex;
            audio.TheAudio.playOnAwake = false;

            return audio;
        }
        public enum AudioState
        {
            None = -1,
            Play,
            Stop,
            Pause,
            UnPause
        }
        public class AudioSourceInfo
        {
            public int InsId;
            public bool Is3d;
            public bool IsFade;
            public bool IsLoading;
            public bool IsGroupMutex;
            public bool IsPathMutex;
            public float TheInitialVolume;
            public string ThePath;
            public string TheGroupName;
            public Transform TheSelf;
            public Transform TheFollow;
            public AudioSource TheAudio;
            public Action TheComplete = null;
            private Coroutine CoroutineFade;

            public bool CheckIdle()
            {
                if (IsLoading || TheAudio.isPlaying)
                {
                    return false;
                }
                Destroy();
                return true;
            }
            public void Play()
            {
                if (TheAudio.isPlaying)
                {
                    return;
                }
                if (null != CoroutineFade)
                {
                    Audio.Instance.StopCoroutine(CoroutineFade);
                    CoroutineFade = null;
                }
                if (IsFade)
                {
                    CoroutineFade = Audio.Instance.StartCoroutine(OnFade(true));
                }
            }
            public void Stop()
            {
                if (null != CoroutineFade)
                {
                    Audio.Instance.StopCoroutine(CoroutineFade);
                    CoroutineFade = null;
                }
                if (IsFade)
                {
                    CoroutineFade = Audio.Instance.StartCoroutine(OnFade(false));
                }
            }
            public void Pause()
            {
                TheAudio.Pause();
            }
            public void UnPause()
            {
                TheAudio.UnPause();
            }
            public void Update()
            {
                if (IsLoading)
                {
                    return;
                }
                if (null != TheComplete && !TheAudio.isPlaying)
                {
                    TheComplete.Invoke();
                    TheComplete = null;
                }
                if (!Is3d || null == TheFollow)
                {
                    TheFollow = null;
                }
                if (null == TheFollow)
                {
                    return;
                }
                if (TheAudio.isPlaying)
                {
                    TheSelf.position = TheFollow.position;
                }
            }
            public void Destroy()
            {
                if (null != CoroutineFade)
                {
                    Audio.Instance.StopCoroutine(CoroutineFade);
                }

                if (null != TheAudio.clip)
                {
                    TheAudio.Stop();
                    TheAudio.clip = null;
                    ResourceLoader.UnloadObject(ThePath);
                }

                IsLoading = false;
                TheFollow = null;
                TheComplete = null;
                CoroutineFade = null;
            }
            private IEnumerator OnFade(bool fadein)
            {
                if (fadein)
                {
                    int frame = 0;
                    float volume = 0;
                    float interval = TheAudio.volume * 0.04f;
                    TheAudio.volume = volume;
                    TheAudio.Play();
                    while (frame <= 25)
                    {
                        frame++;
                        volume = volume + interval;
                        TheAudio.volume = volume;
                        yield return null;
                    }
                }
                else
                {
                    int frame = 0;
                    float volume = TheAudio.volume;
                    float interval = volume * 0.04f;
                    while (frame <= 25)
                    {
                        frame++;
                        volume = volume - interval;
                        TheAudio.volume = volume;
                        yield return null;
                    }
                    TheFollow = null;
                    TheAudio.Stop();
                    TheAudio.clip = null;
                    ResourceLoader.UnloadObject(ThePath);
                }
                yield return null;
            }
        }
    }
}