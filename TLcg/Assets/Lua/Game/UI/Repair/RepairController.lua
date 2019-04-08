local _C = UIManager.Controller(UIConfig.ControllerName.Repair, UIConfig.ViewName.Repair)

local function BtnBack()
    _C:Close()
end

local function BtnCheck()
    if not CSharp.Main.Instance:CheckNeedHotfix("0.0.2") then
        return
    end
    Common.Version.CheckResVersion = "0.0.2"
    CSharp.Main.Instance:StartupLauncher(CSharp.EBootup.Check)
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
