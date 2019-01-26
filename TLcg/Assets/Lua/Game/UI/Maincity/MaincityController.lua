_C = UIManager.Controller(UIConfig.ControllerName.Maincity, UIConfig.ViewName.Maincity)

local function BtnToBootup()
    _C:Close()
    LevelManager.LoadScene(Define.LevelType.Bootup)
end

local function BtnToBattle()
    _C:Close()
    LevelManager.LoadScene(Define.LevelType.Battle)
end

function _C:OnCreat()
    _C.View:AddEvent("BtnToBootup", BtnToBootup)
    _C.View:AddEvent("BtnToBattle", BtnToBattle)
end
