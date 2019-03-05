local MaincityLevelLogic = LuaHandle.Load("Game.LevelLogic.LevelLogic")()

-- 进入场景--
function MaincityLevelLogic:OnEnterScene(callback)
    -- 准备好了，回调吧
    if nil ~= callback then
        callback()
    end
    UIManager.OpenController(UIConfig.ControllerName.Maincity)
end

function MaincityLevelLogic:OnExitScene()
end

return MaincityLevelLogic
