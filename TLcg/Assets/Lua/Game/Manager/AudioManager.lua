AudioManager = LuaHandle.Load("Game.Manager.IManager")()

local AudioConfig = nil
local AudioGroupConfig = nil
local AudioCtrl = nil

-- 根对象
local m_audioRoot = nil

-- 音源音量值
local m_musicVolume = nil
local m_effectVolume = nil
local m_musicSwitch = nil
local m_effectSwitch = nil

-- 获取声效开关
function AudioManager.GetEffectSwitch()
    return PlayerPrefs.GetEffectSwitch()
end

-- 设置声效开关
function AudioManager.SetEffectSwitch(on)
    m_effectSwitch = on
    PlayerPrefs.SetEffectSwitch(on)
    AudioCtrl:SetAudioVolume(m_effectSwitch * m_effectVolume, true)
end

-- 获取音乐开关
function AudioManager.GetMusicSwitch()
    return PlayerPrefs.GetMusicSwitch()
end

-- 设置音乐开关
function AudioManager.SetMusicSwitch(on)
    m_musicSwitch = on
    PlayerPrefs.SetMusicSwitch(on)
    AudioCtrl:SetAudioVolume(m_musicSwitch * m_musicVolume, false)
end

-- 获取声效音量
function AudioManager.GetEffectVolume()
    return PlayerPrefs.GetEffectVolume()
end

-- 设置声效音量
function AudioManager.SetEffectVolume(volume, save)
    if nil == save or save then
        PlayerPrefs.SetEffectVolume(volume)
    end

    m_effectVolume = volume
    AudioCtrl:SetAudioVolume(m_effectSwitch * m_effectVolume, true)
end

-- 获取音乐音量
function AudioManager.GetMusicVolume()
    return PlayerPrefs.GetMusicVolume()
end

-- 设置音乐音量
function AudioManager.SetMusicVolume(volume, save)
    if nil == save or save then
        PlayerPrefs.SetMusicVolume(volume)
    end

    m_musicVolume = volume
    AudioCtrl:SetAudioVolume(m_musicSwitch * m_musicVolume, false)
end

-- 获取音源配置
function AudioManager.GetConfig(id)
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
        print("audio config is nil ")
        return nil
    end

    return audioConfig
end

-- 播放audio
function AudioManager.Play(state, id, insId, followTarget, onComplete)
    if nil == id and nil == insId then
        return nil
    end

    local audioConfig = AudioManager.GetConfig(id)
    if nil == audioConfig then
        return nil
    end

    return AudioCtrl:Play(
        state,
        insId,
        audioConfig.Path,
        audioConfig.PathMutex,
        audioConfig.TrackName,
        audioConfig.TrackMutex,
        audioConfig.GroupName,
        audioConfig.GroupMutex,
        audioConfig.IsEffect,
        audioConfig.IsFade,
        audioConfig.IsLoop,
        audioConfig.InitialVolume,
        audioConfig.MinDistance,
        audioConfig.MaxDistance,
        followTarget,
        onComplete
    )
end

-- 预加载
function AudioManager.Preload(id)
    local audioConfig = AudioManager.GetConfig(id)
    if nil == audioConfig then
        return nil
    end

    AudioCtrl:Preload(audioConfig.Path)
end

-- 卸载所有预加载
function AudioManager.UnloadAll()
    AudioCtrl:UnloadAll()
end

function AudioManager.Initialize()
    if nil ~= m_audioRoot then
        return
    end
    AudioConfig = LuaHandle.Load("Game.Config.AudioConfig")
    AudioGroupConfig = LuaHandle.Load("Game.Config.AudioGroupConfig")

    m_audioRoot = CSharp.Gameobjects.Instance:Spawner("Prefabs/Misc/AudioPlayer", false)
    m_audioRoot:SetActive(true)
    AudioCtrl = m_audioRoot:GetComponent(typeof(CSharp.Audio))

    m_musicSwitch = AudioManager.GetMusicSwitch()
    m_effectSwitch = AudioManager.GetEffectSwitch()

    -- 设置全局音量
    AudioManager.SetEffectVolume(AudioManager.GetEffectVolume())
    AudioManager.SetMusicVolume(AudioManager.GetMusicVolume())
end

function AudioManager.CustomDestroy()
    if nil == m_audioRoot then
        return
    end
    AudioCtrl:CustomDestroy()
    LuaHandle.Unload("Game.Config.AudioConfig")
    LuaHandle.Unload("Game.Config.AudioGroupConfig")
    CSharp.Gameobjects.Instance:Destroy("Prefabs/Misc/AudioPlayer")
    m_audioRoot = nil
end
