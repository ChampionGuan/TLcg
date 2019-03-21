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

        private List<AudioSourceInfo> m_idleAudio = new List<AudioSourceInfo>();
        private Dictionary<int, AudioSourceInfo> m_musicAudio = new Dictionary<int, AudioSourceInfo>();
        private Dictionary<int, AudioSourceInfo> m_effectAudio = new Dictionary<int, AudioSourceInfo>();
        private Dictionary<string, Dictionary<int, AudioSourceInfo>> m_groupAudio = new Dictionary<string, Dictionary<int, AudioSourceInfo>>();
        private List<AudioSourceInfo> m_removeList = new List<AudioSourceInfo>();

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
                if (v.IsIdle())
                {
                    m_removeList.Add(v);
                }
            }
            foreach (var v in m_removeList)
            {
                m_musicAudio.Remove(v.InsId);
                m_idleAudio.Add(v);
                if (m_groupAudio.ContainsKey(v.TheGroupName) && m_groupAudio[v.TheGroupName].ContainsKey(v.InsId))
                {
                    m_groupAudio[v.TheGroupName].Remove(v.InsId);
                }
            }
            m_removeList.Clear();
            foreach (var v in m_effectAudio.Values)
            {
                v.Update();
                if (v.IsIdle())
                {
                    m_removeList.Add(v);
                }
            }
            foreach (var v in m_removeList)
            {
                m_effectAudio.Remove(v.InsId);
                m_idleAudio.Add(v);
                if (m_groupAudio.ContainsKey(v.TheGroupName) && m_groupAudio[v.TheGroupName].ContainsKey(v.InsId))
                {
                    m_groupAudio[v.TheGroupName].Remove(v.InsId);
                }
            }
            m_removeList.Clear();
        }
        public void CustomDestroy()
        {
            foreach (var v in m_musicAudio.Values)
            {
                v.OnComplete = null;
            }
            foreach (var v in m_effectAudio.Values)
            {
                v.OnComplete = null;
            }
            ResourceLoader.UnloadObject("Prefabs/Misc/AudioMixer");
        }
        public int? Play(AudioState state, int? insId, string audioPath, string groupName, bool groupMutex, bool isEffect, bool isFade, bool isLoop, float defaultVolume, float minDistance, float maxDistance, Transform follower)
        {
            if (null != insId)
            {
                if (m_musicAudio.ContainsKey(insId.Value))
                {
                    SetAudioState(m_musicAudio[insId.Value], state);
                    return insId.Value;
                }
                if (m_effectAudio.ContainsKey(insId.Value))
                {
                    SetAudioState(m_effectAudio[insId.Value], state);
                    return insId.Value;
                }
            }

            if (string.IsNullOrEmpty(audioPath))
            {
                return null;
            }
            if (string.IsNullOrEmpty(groupName))
            {
                return null;
            }
            AudioSourceInfo audio = GetIdleAudio(null != follower);
            audio.ThePath = audioPath;
            audio.TheGroupName = groupName;
            audio.IsFade = isFade;
            audio.DefaultVolume = defaultVolume;
            audio.TheAudio.loop = isLoop;
            audio.TheAudio.minDistance = minDistance;
            audio.TheAudio.maxDistance = maxDistance;

            if (isEffect)
            {
                audio.TheAudio.volume = defaultVolume * m_effectVolume;
                m_effectAudio.Add(audio.InsId, audio);
            }
            else
            {
                audio.TheAudio.volume = defaultVolume * m_musicVolume;
                m_musicAudio.Add(audio.InsId, audio);
            }

            ResourceLoader.AsyncLoadObject(audio.ThePath, typeof(AudioClip), (clip) =>
            {
                SetAudioGroup(audio, clip as AudioClip, state, groupMutex);
            });

            return audio.InsId;
        }
        public void PlayByGroupName(string name, AudioState state)
        {
            if (!m_groupAudio.ContainsKey(name))
            {
                return;
            }
            foreach (var v in m_groupAudio[name].Values)
            {
                SetAudioState(v, state);
            }
        }
        public void SetAudioGroup(AudioSourceInfo audio, AudioClip clip, AudioState state, bool groupMutex)
        {
            if (null == clip)
            {
                return;
            }
            audio.TheAudio.clip = clip;
            if (!m_groupAudio.ContainsKey(audio.TheGroupName))
            {
                m_groupAudio.Add(audio.TheGroupName, new Dictionary<int, AudioSourceInfo>());
            }
            if (groupMutex)
            {
                foreach (var v in m_groupAudio[audio.TheGroupName].Values)
                {
                    v.Stop();
                }
            }
            m_groupAudio[audio.TheGroupName].Add(audio.InsId, audio);

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
                    v.TheAudio.volume = v.DefaultVolume * volume;
                }
                m_effectVolume = volume;
            }
            else
            {
                foreach (var v in m_musicAudio.Values)
                {
                    v.TheAudio.volume = v.DefaultVolume * volume;
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
            public float DefaultVolume;
            public string ThePath;
            public string TheGroupName;
            public Transform TheSelf;
            public Transform TheFollow;
            public AudioSource TheAudio;
            public Action OnComplete = null;
            private Coroutine CoroutineFade;

            public bool IsIdle()
            {
                if (TheAudio.isPlaying)
                {
                    return false;
                }
                if (null != CoroutineFade)
                {
                    Audio.Instance.StopCoroutine(CoroutineFade);
                }
                TheFollow = null;
                CoroutineFade = null;
                OnComplete = null;
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
                TheAudio.Play();
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
                OnComplete = null;
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
                if (null != OnComplete && !TheAudio.isPlaying)
                {
                    OnComplete.Invoke();
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
            private IEnumerator OnFade(bool fadein)
            {
                if (fadein)
                {
                    int frame = 0;
                    float volume = 0;
                    float interval = TheAudio.volume * 0.04f;
                    TheAudio.volume = volume;
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
                    ResourceLoader.UnloadObject(ThePath);
                }
                yield return null;
            }
        }
    }
}