-- 检测unity对象是否被销毁(Object类型)
local function UnityTargetIsNil(unityTarget)
    if nil == unityTarget or CSharp.Utils.IsNull(unityTarget) then
        return true
    else
        return false
    end
end

-- 音源数据
local AudioSourceInfo = function()
    local t = {
        -- 实例化id
        insId = 0,
        -- 配置id
        configId = 0,
        -- 是否为3d音源
        is3d = false,
        -- 淡入淡出
        isFadeIn = false,
        -- 自身
        theSelf = nil,
        -- 跟随对象（3d音源,才会有跟随对象）
        theFollow = nil,
        -- 音源组件
        audioSource = nil,
        -- 默认音量
        defaultVolume = 1,
        -- 资源路径
        audioPath = "",
        -- 所在组
        groupName = "",
        -- 下一个音源
        nextAuidoGroupId = nil,
        -- 是否在空闲状态
        IsIdle = function(self)
            if self.audioSource.isPlaying then
                return false
            end
            self.onUpdate = nil
            return true
        end,
        -- 播放
        Play = function(self)
            if self.audioSource.isPlaying then
                return
            end

            self.onUpdate = nil
            -- 淡入
            if self.isFadeIn then
                local frame = 0
                local curValue = 0
                local interval = self.audioSource.volume / 30
                self.audioSource.volume = 0
                -- 30帧结束
                self.onUpdate = function(t)
                    if frame > 30 then
                        t.onUpdate = nil
                    end
                    frame = frame + 1
                    curValue = curValue + interval
                    t.audioSource.volume = curValue
                end
            end
            self.audioSource:Play()
        end,
        -- 停止
        Stop = function(self)
            self.onUpdate = nil
            -- 淡出
            if self.isFadeIn then
                local frame = 0
                local curValue = self.audioSource.volume
                local interval = curValue / 30
                -- 30帧结束
                self.onUpdate = function(t)
                    if frame > 30 then
                        t.onUpdate = nil
                        t.theFollow = nil
                        t.audioSource:Stop()
                        -- 卸载掉资源
                        CSharp.ResourceLoader.UnloadObject(t.audioPath)
                    end
                    frame = frame + 1
                    curValue = curValue - interval
                    t.audioSource.volume = curValue
                end
            else
                self.theFollow = nil
                self.audioSource:Stop()
                -- 卸载掉资源
                CSharp.ResourceLoader.UnloadObject(self.audioPath)
            end
            self.nextAuidoGroupId = nil
        end,
        -- 暂停
        Pause = function(self)
            self.onUpdate = nil
            self.audioSource:Pause()
        end,
        -- 恢复
        UnPause = function(self)
            self.onUpdate = nil
            self.audioSource:UnPause()
        end,
        -- 方法
        Update = function(self)
            -- 淡入淡出
            if nil ~= self.onUpdate then
                self:onUpdate()
            end

            -- 连续播放的处理
            if nil ~= self.nextAuidoGroupId and not self.audioSource.isPlaying then
                AudioManager.PlaySound(self.nextAuidoGroupId, nil, AudioStat.Play, self.theFollow)
                self.nextAuidoGroupId = nil
            end

            -- 3d音源的跟随处理
            if not self.is3d or UnityTargetIsNil(self.theFollow) then
                return
            end
            if not self.theFollow.gameObject.activeSelf then
                self.theFollow = nil
            end
            if UnityTargetIsNil(self.theFollow) then
                return
            end
            if self.audioSource.isPlaying then
                self.theSelf.transform.position = self.theFollow.position
            end
        end
    }
    return t
end

-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
-- -- -- -- -- -- -- -- -- -- -- -- 分割线 -- -- -- -- -- -- -- -- -- -- -- -- -
-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

AudioStat = {
    Play = 1,
    Pause = 2,
    UnPause = 3,
    Stop = 4
}

AudioManager = LuaHandle.Load("Game.Manager.IManager")()

-- 配置
local AudioConfig = nil
local AudioGroupConfig = nil

-- 音源路径前缀
local m_audioPathPrefix = "AudioManager/"
-- 场景的组名称(有几个特殊的组，需要注意，比如Scene,BgUI等)
local m_sceneGroupName = "Scene"
-- 系统背景音组名称
local m_bgUIGroupName = "BgUI"

-- 根对象
local m_audioRoot = nil
-- 音源实例化Id
local m_audioInsId = 0

-- 空闲音源
local m_idleAudio = nil
-- 各种音源
local m_musicAudio = nil
local m_effectAudio = nil
local m_groupAuido = nil

-- 音源音量值
local m_musicVolume = nil
local m_effectVolume = nil

-- 音量混合器
local m_audioMixer = nil
-- 音源监听对象
local m_curAudioListerner = nil
local m_defaultAudioListener = nil

-- 获取空闲音源
local function GetIdleAudioSource(followTarget)
    local asData = nil
    local is3d = not UnityTargetIsNil(followTarget) or false

    -- 从空闲音源里获取
    if nil ~= m_idleAudio then
        for k, v in pairs(m_idleAudio) do
            if v.is3d == is3d and v:IsIdle() then
                asData = v
                m_idleAudio[k] = nil
                break
            end
        end
    end

    -- 重新生成
    if nil == asData then
        asData = AudioSourceInfo()
        if is3d then
            asData.theSelf = CSharp.GameObject("3dSound")
            asData.audioSource = asData.theSelf:AddComponent(typeof(CSharp.AudioSource))
            asData.audioSource.spatialBlend = 1
            asData.audioSource.rolloffMode = CSharp.AudioRolloffMode.Linear
            asData.theSelf.transform.parent = m_audioRoot.transform
        else
            asData.theSelf = m_audioRoot
            asData.audioSource = asData.theSelf:AddComponent(typeof(CSharp.AudioSource))
            asData.audioSource.spatialBlend = 0
        end
        asData.is3d = is3d
    end

    m_audioInsId = m_audioInsId + 1
    asData.insId = m_audioInsId
    asData.isFadeIn = false
    asData.theFollow = followTarget
    asData.audioSource.playOnAwake = false

    return asData
end

-- 置音源状态
local function DoAuidoState(asData, state)
    if nil == asData then
        return
    end

    if state == AudioStat.Play then
        asData:Play()
    elseif state == AudioStat.Stop then
        asData:Stop()
    elseif state == AudioStat.Pause then
        asData:Pause()
    elseif state == AudioStat.UnPause then
        asData:UnPause()
    end
end

-- 置音源设置
local function DoAudioGroup(asData, clip, state, groupMutex)
    if nil == clip then
        return
    end
    if nil == asData.groupName or "" == asData.groupName or UnityTargetIsNil(m_audioMixer) then
        return
    end
    asData.audioSource.clip = clip

    -- 组音频保存
    if nil == m_groupAuido[asData.groupName] then
        m_groupAuido[asData.groupName] = {}
    end

    -- 是否同组互斥
    if groupMutex and nil ~= m_groupAuido[asData.groupName] then
        for k, v in pairs(m_groupAuido[asData.groupName]) do
            v:Stop()
        end
    end
    m_groupAuido[asData.groupName][asData.insId] = asData

    -- 设置组
    local groups = m_audioMixer:FindMatchingGroups(asData.groupName)
    if groups.Length > 0 then
        asData.audioSource.outputAudioMixerGroup = groups[0]
    end

    -- 置音源状态
    DoAuidoState(asData, state)
end

-- 设置监听者
local function SetAudioListener(listener)
    if not UnityTargetIsNil(m_curAudioListerner) then
        m_curAudioListerner.enabled = false
    end

    local curListener = nil
    if UnityTargetIsNil(listener) then
        curListener = m_defaultAudioListener
    else
        curListener = listener
    end

    m_curAudioListerner = curListener
    if m_curAudioListerner ~= nil then
        m_curAudioListerner.enabled = true
    end
end

-- 设置音源音量
local function SetAudioVolume(volume, isEffect)
    if isEffect then
        CSharp.Stage.inst.soundVolume = volume

        for k, v in pairs(m_effectAudio) do
            v.audioSource.volume = v.defaultVolume * volume
        end
    else
        for k, v in pairs(m_musicAudio) do
            v.audioSource.volume = v.defaultVolume * volume
        end
    end
end

-- 当场景退出
local function OnSceneExit()
    AudioManager.PlayGroupSound(m_bgUIGroupName, AudioStat.Stop)
    AudioManager.PlayGroupSound(m_sceneGroupName, AudioStat.Stop)
    SetAudioListener(nil)
end

-- 当场景进入
local function OnSceneEnter()
    local target = CSharp.Camera.main
    if nil ~= target then
        target = target.gameObject
    end

    -- 新的音源监听对象
    if UnityTargetIsNil(target) then
        SetAudioListener(nil)
        return
    end
    local listener = target:GetComponent(typeof(CSharp.AudioListener))
    if UnityTargetIsNil(listener) then
        listener = target:AddComponent(typeof(CSharp.AudioListener))
    end
    SetAudioListener(listener)
end

-- 获取声效开关
function AudioManager.GetSwitchAudioEffect()
    return PlayerPrefs.GetAudioEffectSwitch()
end

-- 获取音乐开关
function AudioManager.GetSwitchAudioMusic()
    return PlayerPrefs.GetAudioMusicSwitch()
end

-- 全局声效
function AudioManager.GetAudioEffectVolume()
    return PlayerPrefs.GetAudioEffectVolume()
end

-- 全局音乐
function AudioManager.GetAudioMusicVolume()
    return PlayerPrefs.GetAudioMusicVolume()
end

-- 全局声效开关
function AudioManager.SwitchAudioEffect(on)
    audioEffectSwitch = on
    PlayerPrefs.SaveAudioEffectSwitch(on)
    SetAudioVolume(audioEffectSwitch * m_effectVolume, true)
end

-- 全局音乐开关
function AudioManager.SwitchAudioMusic(on)
    audioMusicSwitch = on
    PlayerPrefs.SaveAudioMusicSwitch(on)
    SetAudioVolume(audioMusicSwitch * m_musicVolume, false)
end

-- 全局声效
function AudioManager.SetAudioEffectVolume(volume, save)
    if nil == save or save then
        PlayerPrefs.SaveAudioEffectVolume(volume)
    end

    m_effectVolume = volume
    SetAudioVolume(audioEffectSwitch * m_effectVolume, true)
end

-- 全局音乐
function AudioManager.SetAudioMusicVolume(volume, save)
    if nil == save or save then
        PlayerPrefs.SaveAudioMusicVolume(volume)
    end

    m_musicVolume = volume
    SetAudioVolume(audioMusicSwitch * m_musicVolume, false)
end

-- 播放组audio
function AudioManager.PlayGroupSound(groupName, state)
    if nil == m_groupAuido then
        return
    end
    if nil == groupName then
        return
    end
    if nil == m_groupAuido[groupName] then
        return
    end

    for k, v in pairs(m_groupAuido[groupName]) do
        DoAuidoState(v, state)
    end
end

-- 播放audio
function AudioManager.PlaySound(id, insId, state, followTarget)
    local asData = nil

    -- 实例化Id不为空
    if nil ~= insId then
        if nil ~= m_musicAudio then
            asData = m_musicAudio[insId]
        end
        if nil == asData and nil ~= m_effectAudio then
            asData = m_effectAudio[insId]
        end
        if nil == id and nil ~= asData then
            DoAuidoState(asData, state)
            return asData.insId
        end
        if nil ~= id and nil ~= asData then
            if id == asData.configId then
                DoAuidoState(asData, state)
                return asData.insId
            else
                DoAuidoState(asData, AudioStat.Stop)
            end
        end
    end

    -- 配置id为空
    if nil == id then
        return nil
    end

    local audioGroupConfig = AudioGroupConfig[id]
    -- 音源组
    if nil == audioGroupConfig or #audioGroupConfig <= 0 then
        print("group key is nil " .. id)
        return nil
    end

    -- 随机种子
    if #audioGroupConfig > 1 then
        math.randomseed(tostring(os.time()):reverse():sub(1, 6))
    end
    -- 随机音源
    local audioConfig = AudioConfig[audioGroupConfig[math.random(1, #audioGroupConfig)]]
    -- 无效配置
    if nil == audioConfig then
        return nil
    end

    asData = GetIdleAudioSource(followTarget)
    asData.configId = id
    asData.groupName = audioConfig.groupName
    asData.defaultVolume = audioConfig.defaultVolume
    asData.isFadeIn = audioConfig.isFadeIn
    asData.nextAuidoGroupId = audioConfig.nextAuidoGroupId
    asData.audioPath = m_audioPathPrefix .. audioConfig.name
    asData.audioSource.loop = audioConfig.isLoop
    asData.audioSource.minDistance = audioConfig.minDistance
    asData.audioSource.maxDistance = audioConfig.maxDistance

    if audioConfig.isBg then
        m_musicAudio[asData.insId] = asData
        asData.audioSource.volume = asData.defaultVolume * audioMusicSwitch * m_musicVolume
    else
        m_effectAudio[asData.insId] = asData
        asData.audioSource.volume = asData.defaultVolume * audioEffectSwitch * m_effectVolume
    end

    CSharp.ResourceLoader.AsyncLoadObject(
        asData.audioPath,
        audioConfig.name,
        typeof(CSharp.AudioClip),
        function(clip)
            DoAudioGroup(asData, clip, state, audioConfig.groupMutex)
        end
    )

    return asData.insId
end

function AudioManager.Initialize()
    if nil ~= m_audioRoot then
        return
    end
    AudioConfig = LuaHandle.Load("Game.Config.AudioConfig")
    AudioGroupConfig = LuaHandle.Load("Game.Config.AudioGroupConfig")

    m_audioRoot = CSharp.GameObject("AudioRoot")
    CSharp.UObject.DontDestroyOnLoad(m_audioRoot)

    -- 当前用的音源监听对象
    m_curAudioListerner = CSharp.GameObject.FindObjectOfType(typeof(CSharp.AudioListener))
    if nil == m_defaultAudioListener then
        m_defaultAudioListener = m_audioRoot:AddComponent(typeof(CSharp.AudioListener))
    end

    -- 事件监听
    if nil ~= Event then
        Event.AddListener(Event.EXIT_SCENCE, OnSceneExit)
        Event.AddListener(Event.ENTER_SCENCE, OnSceneEnter)
    end

    m_audioMixer = CSharp.ResourceLoader.LoadObject("AudioManager/AudioMixer", typeof(CSharp.AudioMixer))
    SetAudioListener(nil)

    m_audioInsId = 100
    m_groupAuido = {}
    m_idleAudio = {}
    m_musicAudio = {}
    m_effectAudio = {}
    audioMusicSwitch = AudioManager.GetSwitchAudioMusic()
    audioEffectSwitch = AudioManager.GetSwitchAudioEffect()

    -- 设置全局音量
    AudioManager.SetAudioEffectVolume(AudioManager.GetAudioEffectVolume())
    AudioManager.SetAudioMusicVolume(AudioManager.GetAudioMusicVolume())
end

function AudioManager.Update()
    if nil ~= m_musicAudio then
        for k, v in pairs(m_musicAudio) do
            v:Update()

            -- 判断是否处于空闲状态
            if v:IsIdle() and nil ~= v.insId and nil ~= m_groupAuido[v.groupName] then
                m_musicAudio[v.insId] = nil
                m_groupAuido[v.groupName][v.insId] = nil
                table.insert(m_idleAudio, v)
            end
        end
    end
    if nil ~= m_effectAudio then
        for k, v in pairs(m_effectAudio) do
            v:Update()

            -- 判断是否处于空闲状态
            if v:IsIdle() and nil ~= v.insId and nil ~= m_groupAuido[v.groupName] then
                m_effectAudio[v.insId] = nil
                m_groupAuido[v.groupName][v.insId] = nil
                table.insert(m_idleAudio, v)
            end
        end
    end
end

function AudioManager.Destroy()
    if nil == m_audioRoot then
        return
    end
    CSharp.UObject.Destroy(m_audioRoot)
    LuaHandle.Unload("Game.Config.AudioConfig")
    LuaHandle.Unload("Game.Config.AudioGroupConfig")
    m_audioRoot = nil
    m_audioInsId = 0
    m_idleAudio = nil
    m_musicAudio = nil
    m_effectAudio = nil
    m_groupAuido = nil
    m_musicVolume = nil
    m_effectVolume = nil
    m_audioMixer = nil
    m_curAudioListerner = nil
    m_defaultAudioListener = nil
end
