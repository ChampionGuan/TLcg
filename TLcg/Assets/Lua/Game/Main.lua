require "Common.LuaHandle"
LuaHandle.Load("bit")
LuaHandle.Load("Common.Common")
LuaHandle.Load("Common.CSUtils")
LuaHandle.Load("Common.PlayerPrefs")
LuaHandle.Load("Common.bezier")
LuaHandle.Load("Common.json")
LuaHandle.Load("Game.Config.Localization")
LuaHandle.Load("Game.Common.Define")
LuaHandle.Load("Game.Common.Event")
LuaHandle.Load("Game.Common.Utils")
LuaHandle.Load("Game.Common.UIUtils")
LuaHandle.Load("Game.Manager.DataManager")
LuaHandle.Load("Game.Manager.AudioManager")
LuaHandle.Load("Game.Manager.VideoManager")
LuaHandle.Load("Game.Manager.TimerManager")
LuaHandle.Load("Game.Manager.UIManager")
LuaHandle.Load("Game.Manager.LevelManager")
LuaHandle.Load("Game.Manager.NetworkManager")

-- 初始化
function Initialize(type)
    print("开始游戏")
    DataManager.Initialize()
    AudioManager.Initialize()
    VideoManager.Initialize()
    TimerManager.Initialize()
    UIManager.Initialize()
    LevelManager.Initialize()
    NetworkManager.Initialize()
end

-- 更新
function Update()
    DataManager.CustomUpdate()
    AudioManager.CustomUpdate()
    VideoManager.CustomUpdate()
    TimerManager.CustomUpdate()
    UIManager.CustomUpdate()
    LevelManager.CustomUpdate()
    NetworkManager.CustomUpdate()
end

-- 固定更新
function FixedUpdate()
    DataManager.CustomFixedUpdate()
    AudioManager.CustomFixedUpdate()
    VideoManager.CustomFixedUpdate()
    TimerManager.CustomFixedUpdate()
    UIManager.CustomFixedUpdate()
    LevelManager.CustomFixedUpdate()
    NetworkManager.CustomFixedUpdate()
end

-- 焦点
function OnAppFocus(hasfocus)
    DataManager.OnAppFocus(hasfocus)
    AudioManager.OnAppFocus(hasfocus)
    VideoManager.OnAppFocus(hasfocus)
    TimerManager.OnAppFocus(hasfocus)
    UIManager.OnAppFocus(hasfocus)
    LevelManager.OnAppFocus(hasfocus)
    NetworkManager.OnAppFocus(hasfocus)
end

-- 收到消息
function OnReceiveMsg(msg)
    NetwrokManager.ReceiveMsg(msg)
end

-- 销毁
function OnDestroy()
    DataManager.CustomDestroy()
    AudioManager.CustomDestroy()
    VideoManager.CustomDestroy()
    TimerManager.CustomDestroy()
    UIManager.CustomDestroy()
    LevelManager.CustomDestroy()
    NetworkManager.CustomDestroy()
    LuaHandle.UnloadAll()
    LuaHandle.Unload("Game.Main")
end
