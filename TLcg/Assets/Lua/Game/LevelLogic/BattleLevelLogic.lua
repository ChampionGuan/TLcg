local BattleLevelLogic = LuaHandle.Load("Game.LevelLogic.LevelLogic")()

-- 进入场景--
function BattleLevelLogic:OnEnterScene(callback)
    -- 准备好了，回调吧
    if nil ~= callback then
        callback()
    end
    UIManager.OpenController(UIConfig.ControllerName.Battle)
end

function BattleLevelLogic:OnExitScene()
end

return BattleLevelLogic
