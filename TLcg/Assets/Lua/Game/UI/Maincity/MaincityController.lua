local _C = UIManager.Controller(UIConfig.ControllerName.Maincity, UIConfig.ViewName.Maincity)

local function BtnToBootup()
    LevelManager.LoadScene(Define.LevelType.Bootup)
end

local function BtnToBattle()
    LevelManager.LoadScene(Define.LevelType.Battle)
end

function _C:OnCreat()
    _C.View:AddEvent("BtnToBootup", BtnToBootup)
    _C.View:AddEvent("BtnToBattle", BtnToBattle)
end

function _C:OnClose()
    print("maincity close!!!")
end