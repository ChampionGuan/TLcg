-- 此脚本由工具生成
-- Path：             音源路径
-- PathMutex：        是否同路径音源互斥
-- IsEffect：         是否为音效
-- IsLoop：           是否循环播放
-- IsFade：           是否淡入淡出
-- GroupName：        所在组名称
-- GroupMutex：       是否同组互斥
-- TrackName：        所在轨名称
-- TrackMutex：       是否同轨互斥
-- UnloadType:        卸载模式，0：及时卸载，1：切换场景后卸载
-- InitialVolume：    初始音量
-- MinDistance：      如果为3d音源，最短距离
-- MaxDistance：      如果为3d音源，最远距离

local AudioConfig = {
    BGM_Login = {
        Path = "Audio/BGM_Login",
        PathMutex = false,
        IsEffect = false,
        IsLoop = true,
        IsFade = true,
        GroupName = "Default",
        GroupMutex = true,
        TrackName = "Bg",
        TrackMutex = true,
        DefaultVolume = 1,
        MinDistance = 0,
        MaxDistance = 500
    },
    BGM_City01 = {
        Path = "Audio/BGM_City01",
        PathMutex = false,
        IsEffect = false,
        IsLoop = false,
        IsFade = true,
        GroupName = "Default",
        GroupMutex = true,
        TrackName = "Bg",
        TrackMutex = true,
        DefaultVolume = 1,
        MinDistance = 0,
        MaxDistance = 500
    },
    BGM_Field01 = {
        Path = "Audio/BGM_Field01",
        PathMutex = false,
        IsEffect = false,
        IsLoop = true,
        IsFade = true,
        GroupName = "Default",
        GroupMutex = true,
        TrackName = "Bg",
        TrackMutex = true,
        DefaultVolume = 0.9,
        MinDistance = 0,
        MaxDistance = 500
    },
    BGM_Battle01 = {
        Path = "Audio/BGM_Battle01",
        PathMutex = false,
        IsEffect = false,
        IsLoop = true,
        IsFade = true,
        GroupName = "Default",
        GroupMutex = true,
        TrackName = "Bg",
        TrackMutex = true,
        DefaultVolume = 0.6,
        MinDistance = 0,
        MaxDistance = 500
    }
}
return AudioConfig
