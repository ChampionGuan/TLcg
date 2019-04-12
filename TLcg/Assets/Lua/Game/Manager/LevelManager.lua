LevelManager = LuaHandle.Load("Game.Manager.IManager")()
-- 上一个场景配置
LevelManager.TheLastLevelConfig = nil
-- 下一个场景配置
LevelManager.TheNextLevelConfig = nil

-- 当前场景逻辑
LevelManager.CurLevelLogic = nil
-- 当前场景配置
LevelManager.CurLevelConfig = nil
-- 当前场景类型
LevelManager.CurLevelType = nil
-- 外部传入当前场景数据
LevelManager.IncomingInfo = nil
-- 场景加载中
LevelManager.Loading = false

-- 场景配置
local levelConfig = nil

local function OnSceneLoadStart()
    LevelManager.Loading = true
    if nil ~= LevelManager.CurLevelConfig then
        UIManager.OpenController(UIConfig.ControllerName.Loading)
    end
    if nil ~= LevelManager.CurLevelLogic then
        LevelManager.CurLevelLogic:ExitScene()
    end
    if nil ~= LevelManager.CurLevelConfig and LevelManager.CurLevelConfig.Id ~= LevelManager.TheNextLevelConfig.Id then
        LevelManager.TheLastLevelConfig = LevelManager.CurLevelConfig
    end
end

local function OnSceneLoadUpdate(p)
    UIManager.DispatchEvent(UIConfig.Event.LOADING_P, p)
end

local function OnSceneLoadComplete()
    LevelManager.Loading = false
    LevelManager.CurLevelConfig = LevelManager.TheNextLevelConfig
    LevelManager.CurLevelType = LevelManager.CurLevelConfig.Type
    LevelManager.TheNextLevelConfig = nil

    if LevelManager.CurLevelConfig.LogicScript ~= nil then
        LevelManager.CurLevelLogic = LuaHandle.Load(LevelManager.CurLevelConfig.LogicScript)
    end
    if nil ~= LevelManager.CurLevelLogic then
        LevelManager.CurLevelLogic:EnterScene(
            function()
                -- 进入成功后，关闭loading
                UIManager.DispatchEvent(UIConfig.Event.LOADING_OK)
            end
        )
    end
end

-- 初始化
function LevelManager.Initialize()
    levelConfig = LuaHandle.Load("Game.Config.LevelConfig")
    CSharp.ResourceLoader.ScenePathPrefix = "Scenes/"
    CSharp.SceneLoader.Instance.LoadStart = OnSceneLoadStart
    CSharp.SceneLoader.Instance.LoadUpdate = OnSceneLoadUpdate
    CSharp.SceneLoader.Instance.LoadComplete = OnSceneLoadComplete

    -- 初始时进入启动界面
    LevelManager.TheNextLevelConfig = levelConfig[Define.LevelType.Bootup]
    OnSceneLoadStart()
    OnSceneLoadComplete()
end

-- 更新
function LevelManager.CustomUpdate()
    if nil ~= LevelManager.CurLevelLogic then
        LevelManager.CurLevelLogic:Update()
    end
end

-- 固定更新
function LevelManager.CustomFixedUpdate()
    if nil ~= LevelManager.CurLevelLogic then
        LevelManager.CurLevelLogic:FixedUpdate()
    end
end

-- 销毁
function LevelManager.CustomDestroy()
end

-- 加载指定场景
-- <param name="id" type="number">场景id</param>
-- <param name="info" type="table">外部传入的参数</param>
function LevelManager.LoadScene(id, ...)
    if LevelManager.Loading then
        return
    end
    local nextConfig = levelConfig[id]
    if nil == nextConfig then
        return
    end

    LevelManager.IncomingInfo = ...
    LevelManager.TheNextLevelConfig = nextConfig
    if nextConfig.Id == LevelManager.CurLevelConfig.Id then
        OnSceneLoadStart()
        OnSceneLoadUpdate(100)
        OnSceneLoadComplete()
    else
        CSharp.SceneLoader.Instance:LoadScene(LevelManager.TheNextLevelConfig.SceneName)
    end
end

-- 加载上一个场景
function LevelManager.LoadLastScene(...)
    if nil == LevelManager.TheLastLevelConfig then
        return
    end
    LevelManager.LoadScene(LevelManager.TheLastLevelConfig.Id, ...)
end
