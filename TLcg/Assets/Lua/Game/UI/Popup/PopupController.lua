local _C = UIManager.Controller(UIConfig.ControllerName.Popup, UIConfig.ViewName.Popup)

local function BtnConfirm()
    _C:Close()
end

function _C:OnCreat()
    _C.View:AddEvent("BtnConfirm", BtnConfirm)
end
