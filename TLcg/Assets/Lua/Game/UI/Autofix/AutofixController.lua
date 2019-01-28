local _C = UIManager.Controller(UIConfig.ControllerName.Autofix, UIConfig.ViewName.Autofix)

local function BtnBack()
    _C:Close()
end

local function BtnFix()
    CSharp.Main.Instance:StartupLauncher(CSharp.EBootup.Repair)
end

local function BtnDeepFix()
    CSharp.Main.Instance:StartupLauncher(CSharp.EBootup.DeepRepair)
end

function _C:OnCreat()
    _C.View:AddEvent("BtnBack", BtnBack)
    _C.View:AddEvent("BtnFix", BtnFix)
    _C.View:AddEvent("BtnDeepFix", BtnDeepFix)
end
