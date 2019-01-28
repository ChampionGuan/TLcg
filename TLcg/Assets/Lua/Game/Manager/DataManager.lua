DataManager = LuaHandle.Load("Game.Manager.IManager")()
-- 配置信息--
DataManager.ConfigInfo = nil
-- 玩家信息--
DataManager.PlayerInfo = nil

local function LoadData()
    DataManager.PlayerInfo = {
        MiscData = LuaHandle.Load("Game.Data.MiscData"),
        LoginData = LuaHandle.Load("Game.Data.LoginData")
    }
end

local function LoadConfig()
end

function DataManager.Initialize()
    LoadData()
    LoadConfig()
end

function DataManager.CustomUpdate()
end

function DataManager.CustomFixedUpdate()
end

function DataManager.CustomDestroy()
    DataManager.Clear()
end

function DataManager.Clear()
    if nil == DataManager.PlayerInfo then
        return
    end
    for _, v in pairs(DataManager.PlayerInfo) do
        v:Clear()
    end
end
