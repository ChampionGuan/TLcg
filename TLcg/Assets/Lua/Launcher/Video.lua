-- Video
local Video = {}

local VideoConfig = nil
local VideoCtrl = nil

local m_videoRoot = nil
local m_onPlayCallback = nil
local m_onOverCallback = nil

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
    if VideoCtrl:Play(config.VideoUrl, config.Loop, config.Skip) then
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
    VideoConfig = LuaHandle.Load("Game.Config.VideoConfig")

    m_videoRoot = CSharp.ABManager.Load("Prefabs/Misc/VideoPlayer", "VideoPlayer", typeof(CSharp.GameObject))
    m_videoRoot = CSharp.UObject.Instantiate(m_videoRoot)

    VideoCtrl = m_videoRoot:GetComponent(typeof(CSharp.Video))
    VideoCtrl.PreparedCallback = VideoPrepared
    VideoCtrl.OverCallback = VideoOver

    Video.Stop()
end

function Video.Destroy()
    CSharp.UObject.Destroy(m_videoRoot)
    CSharp.ABManager.UnloadAb("Prefabs/Misc/VideoPlayer")
    LuaHandle.Unload("Game.Config.VideoConfig")
end

return Video
