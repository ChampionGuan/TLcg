_C = UIManager.Controller(UIConfig.ControllerName.ServerList, UIConfig.ViewName.ServerList)

local function BtnBack()
    _C:Close()
end

function _C:OnCreat()
    _C.View:AddEvent("BtnBack", BtnBack)
end

function _C:OnOpen(data)
end

function _C:OnClose()
end
