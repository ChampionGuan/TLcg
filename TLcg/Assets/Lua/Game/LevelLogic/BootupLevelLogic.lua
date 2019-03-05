-- 启动场景逻辑脚本
local BootupLevelLogic = LuaHandle.Load("Game.LevelLogic.LevelLogic")()

-- 进入场景--
function BootupLevelLogic:OnEnterScene(callback)
    -- 准备好了，回调吧
    if nil ~= callback then
        callback()
    end
    UIManager.OpenController(UIConfig.ControllerName.Login)
end

function BootupLevelLogic:OnExitScene()
    UIManager.DestroyAllCtrl(false)
end

return BootupLevelLogic
