_C = UIManager.Controller(UIConfig.ControllerName.Autofix, UIConfig.ViewName.Autofix)

local function BtnBack()
    _C:Close()
end

local function BtnFix()
end

local function BtnDeepFix()
end

function _C:OnCreat()
    _C.View:AddEvent("BtnBack", BtnBack)
    _C.View:AddEvent("BtnFix", BtnFix)
    _C.View:AddEvent("BtnDeepFix", BtnDeepFix)
end
