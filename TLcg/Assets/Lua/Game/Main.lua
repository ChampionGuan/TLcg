require "Common.LuaHandle"
LuaHandle.Load("Common.Common")
LuaHandle.Load("Common.CSUtils")
LuaHandle.Load("Game.Manager.PreloadManager")

-- 初始化
function Initialize(type)
    PreloadManager.Initialize(
        function()
            print("开始游戏")
            DataManager.Initialize()
            AudioManager.Initialize()
            VideoManager.Initialize()
            TimerManager.Initialize()
            UIManager.Initialize()
            NetworkManager.Initialize()
            HttpManager.Initialize()
            LevelManager.Initialize()
        end
    )
end

-- 更新
function Update()
    if PreloadManager.Initialized then
        DataManager.CustomUpdate()
        AudioManager.CustomUpdate()
        VideoManager.CustomUpdate()
        TimerManager.CustomUpdate()
        UIManager.CustomUpdate()
        LevelManager.CustomUpdate()
        NetworkManager.CustomUpdate()
    end
end

-- 固定更新
function FixedUpdate()
    if PreloadManager.Initialized then
        DataManager.CustomFixedUpdate()
        AudioManager.CustomFixedUpdate()
        VideoManager.CustomFixedUpdate()
        TimerManager.CustomFixedUpdate()
        UIManager.CustomFixedUpdate()
        LevelManager.CustomFixedUpdate()
        NetworkManager.CustomFixedUpdate()
    end
end

-- 焦点
function OnAppFocus(hasfocus)
    if PreloadManager.Initialized then
        DataManager.OnAppFocus(hasfocus)
        AudioManager.OnAppFocus(hasfocus)
        VideoManager.OnAppFocus(hasfocus)
        TimerManager.OnAppFocus(hasfocus)
        UIManager.OnAppFocus(hasfocus)
        LevelManager.OnAppFocus(hasfocus)
        NetworkManager.OnAppFocus(hasfocus)
    end
end

-- 收到消息
function OnReceiveMsg(msg)
    if PreloadManager.Initialized then
        NetworkManager.ReceiveMsg(msg)
    end
end

-- 销毁
function OnDestroy()
    if PreloadManager.Initialized then
        DataManager.CustomDestroy()
        AudioManager.CustomDestroy()
        VideoManager.CustomDestroy()
        TimerManager.CustomDestroy()
        UIManager.CustomDestroy()
        LevelManager.CustomDestroy()
        NetworkManager.CustomDestroy()
        HttpManager.CustomDestroy()
        LuaHandle.UnloadAll()
        LuaHandle.Unload("Game.Main")
    else
        PreloadManager.CustomDestroy()
    end
end
