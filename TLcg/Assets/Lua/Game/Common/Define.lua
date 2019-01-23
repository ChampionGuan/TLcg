Define = {}

-- 场景类型
Define.LevelType = {
    Bootup = 1,
    MainCity = 2,
    Battle = 3,
}

-- sync类型
Define.SyncType = {
    All = 0,
    -- http请求
    HttpSync = 1,
    -- 消息同步
    MsgSync = 2,
    -- 重连同步
    NetSync = 3,
    -- 登录时的菊花
    LoginSync = 4,
    -- 其他
    OtherSync = 5
}
