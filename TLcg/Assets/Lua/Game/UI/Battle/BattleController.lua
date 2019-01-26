local _C = UIManager.Controller(UIConfig.ControllerName.Battle, UIConfig.ViewName.Battle)

local function BtnToMaincity()
    _C:Close()
    LevelManager.LoadScene(Define.LevelType.MainCity)
end

function _C:OnCreat()
    _C.View:AddEvent("BtnToMaincity", BtnToMaincity)
end

function _C:OnOpen(data)
end

function _C:OnClose()
end
