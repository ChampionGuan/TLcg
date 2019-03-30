-- 视屏配置
return {
    -- [1] = {
    --     Id = 1,
    --     VideoUrl = "Video/Splash",
    --     DefaultAudioId = 25,
    --     Skip = false,
    --     Loop = false,
    --     AutoDestroy = true,
    -- },
    [2] = {
        Id = 2,
        VideoUrl = "Video/Login",
        Skip = false,
        Loop = true,
        AutoDestroy = false
    },
    [3] = {
        Id = 3,
        VideoUrl = "Video/Login",
        Skip = false,
        Loop = true,
        AutoDestroy = false,
        Subtitle = {
            {
                Start = 0.5,
                End = 3,
                Content = "战国时代，干戈不断"
            },
            {
                Start = 3.2,
                End = 5,
                Content = "神州烽烟弥漫"
            },
            {
                Start = 5.5,
                End = 7,
                Content = "列国伐交频频"
            }
        }
    }
}
