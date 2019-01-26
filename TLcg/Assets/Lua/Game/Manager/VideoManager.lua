VideoManager = LuaHandle.Load("Game.Manager.IManager")()

local VideoConfig = nil
local VideoCtrl = nil

local m_videoRoot = nil
local m_onPlayCallback = nil
local m_onOverCallback = nil
local m_curVideoConfig = nil

local function VideoPrepared()
    if nil ~= m_onPlayCallback then
        m_onPlayCallback()
    end
end

local function VideoOver()
    if nil ~= m_onOverCallback then
        m_onOverCallback()
    end
    m_curVideoConfig = nil
end

local function VideoError()
    if nil ~= m_onPlayCallback then
        m_onPlayCallback()
    end
    if nil ~= m_onOverCallback then
        m_onOverCallback()
    end
end

function VideoManager.IsPlaying(id)
    if nil == m_curVideoConfig then
        return false
    end
    if nil == id then
        return true
    end
    return m_curVideoConfig.Id == id
end

function VideoManager.Play(id, onPlay, onOver)
    local config = VideoConfig[id]

    m_onPlayCallback = onPlay
    m_onOverCallback = onOver

    if nil == config then
        VideoError()
        return
    end
    if nil ~= m_curVideoConfig and config.VideoUrl == m_curVideoConfig.VideoUrl and VideoCtrl.IsPlaying then
        VideoError()
        return
    end
    if VideoCtrl:Play(config.VideoUrl, config.Loop, config.Skip) then
        m_curVideoConfig = config
        VideoCtrl:SetActive(true)
    else
        VideoError()
    end
end

function VideoManager.Stop()
    VideoCtrl:Stop()
    VideoCtrl:SetActive(false)
end

function VideoManager.Release()
    VideoCtrl:Stop()
    VideoCtrl:Release()
    VideoCtrl:SetActive(false)
end

function VideoManager.Initialize()
    if nil ~= m_videoRoot then
        return
    end
    VideoConfig = LuaHandle.Load("Game.Config.VideoConfig")

    m_videoRoot = CSharp.ResourceLoader.LoadObject("Prefabs/Misc/VideoPlayer", typeof(CSharp.GameObject))
    m_videoRoot = CSharp.UObject.Instantiate(m_videoRoot)
    m_videoRoot.name = "VideoPlayer"

    VideoCtrl = m_videoRoot:GetComponent(typeof(CSharp.Video))
    VideoCtrl.PreparedCallback = VideoPrepared
    VideoCtrl.OverCallback = VideoOver

    VideoManager.Stop()
end

function VideoManager.Destroy()
    if nil == m_videoRoot then
        return
    end
    VideoCtrl:CustomDestroy()
    LuaHandle.Unload("Game.Config.VideoConfig")
    CSharp.ResourceLoader.UnloadObject("Prefabs/Misc/VideoPlayer")
    m_videoRoot = nil
end
