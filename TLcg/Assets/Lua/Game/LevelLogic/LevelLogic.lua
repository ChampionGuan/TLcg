-- 场景逻辑
local LevelLogic = function()
    local t = {}

    t.Config = nil
    t.InsInfo = nil
    t.EnterScene = function(self, callBack)
        self.Config = LevelManager.CurrLevelConfig
        self.InsInfo = LevelManager.IncomingInfo
        self:OnEnterScene(callBack)
    end
    t.ExitScene = function(self)
        self:OnExitScene()
        self.Config = nil
        self.InsInfo = nil
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
