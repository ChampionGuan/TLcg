-- Video
local Video = {}

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
end

local function VideoError()
    if nil ~= m_onPlayCallback then
        m_onPlayCallback()
    end
    if nil ~= m_onOverCallback then
        m_onOverCallback()
    end
end

function Video.Play(id, onPlay, onOver)
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

function Video.Stop()
    VideoCtrl:Stop()
    VideoCtrl:SetActive(false)
end

function Video.Release()
    VideoCtrl:Stop()
    VideoCtrl:Release()
    VideoCtrl:SetActive(false)
end

function Video.Initialize()
    if nil ~= m_videoRoot then
        return
    end
    VideoConfig = LuaHandle.Load("Game.Config.VideoConfig")

    m_videoRoot = CSharp.ABManager.Load("Prefabs/Misc/VideoPlayer", "VideoPlayer", typeof(CSharp.GameObject))
    m_videoRoot = CSharp.UObject.Instantiate(m_videoRoot)
    m_videoRoot.name = "VideoPlayer"

    VideoCtrl = m_videoRoot:GetComponent(typeof(CSharp.Video))
    VideoCtrl.PreparedCallback = VideoPrepared
    VideoCtrl.OverCallback = VideoOver

    Video.Stop()
end

function Video.Destroy()
    if nil == m_videoRoot then
        return
    end
    VideoCtrl:CustomDestroy()
    LuaHandle.Unload("Game.Config.VideoConfig")
    CSharp.ABManager.UnloadAb("Prefabs/Misc/VideoPlayer")
    m_videoRoot = nil
end

return Video
