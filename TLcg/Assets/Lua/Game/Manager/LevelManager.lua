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

-- 场景配置
local levelConfig = nil

local function OnSceneLoadStart()
end

local function OnSceneLoadUpdate(p)
end

local function OnSceneLoadComplete()
end

-- 初始化
function LevelManager.Initialize()
    levelConfig = LuaHandle.Load("Game.Config.LevelConfig")
    CSharp.ResourceLoader.ScenePathPrefix = "Scene"
    CSharp.SceneLoader.Instance.LoadStart = OnSceneLoadStart
    CSharp.SceneLoader.Instance.LoadUpdate = OnSceneLoadUpdate
    CSharp.SceneLoader.Instance.LoadComplete = OnSceneLoadComplete
end

-- 更新
function LevelManager.CustomUpdate()
end

-- 固定更新
function LevelManager.CustomFixedUpdate()
end

-- 销毁
function LevelManager.CustomDestroy()
end

-- 加载指定场景
-- <param name="id" type="number">场景id</param>
-- <param name="info" type="table">外部传入的参数</param>
function LevelManager.LoadScene(id, info)
    local nextConfig = levelConfig:getConfigByKey(id)
end
