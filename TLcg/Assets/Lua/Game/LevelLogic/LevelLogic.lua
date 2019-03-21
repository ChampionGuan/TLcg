-- 场景逻辑
local LevelLogic = function()
    local t = {}

    t.TheConfig = nil
    t.TheInsInfo = nil
    t.EnterScene = function(self, callBack)
        self.TheConfig = LevelManager.CurLevelConfig
        self.TheInsInfo = LevelManager.IncomingInfo
        AudioManager.Play(CSharp.AudioState.Play, self.TheConfig.AudioId)
        Event.Dispatch(EventType.ENTER_SCENCE)
        self:OnEnterScene(callBack)
    end
    t.ExitScene = function(self)
        Event.Dispatch(EventType.EXIT_SCENCE)
        self:OnExitScene()
        self.TheConfig = nil
        self.TheInsInfo = nil
    end
    t.Update = function(self)
        self:OnUpdate()
    end
    t.FixedUpdate = function(self)
        self:OnFixedUpdate()
    end

    t.OnUpdate = function(self)
    end
    t.OnFixedUpdate = function(self)
    end
    t.OnExitScene = function(self)
    end
    t.OnEnterScene = function(self, callBack)
    end

    return t
end

return LevelLogic
