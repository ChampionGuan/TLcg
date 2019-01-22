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
    t.SceneReady = function(self)
        self:OnSceneReady()
    end
    t.ExitScene = function(self)
        self:OnExitScene()
    end
    t.Destroy = function(self)
        self.Config = nil
        self.InsInfo = nil
        self:OnDestroy()
    end
    t.Update = function(self)
        self:OnUpdate()
    end
    t.FixedUpdate = function(self)
        self:OnFixedUpdate()
    end
    t.OnSceneReady = function(self)
    end
    t.OnUpdate = function(self)
    end
    t.OnFixedUpdate = function(self)
    end
    t.OnExitScene = function(self)
    end
    t.OnEnterScene = function(self, callBack)
    end
    t.OnDestroy = function(self)
    end

    return t
end

return LevelLogic
