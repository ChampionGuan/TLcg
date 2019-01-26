local _C = UIManager.Controller(UIConfig.ControllerName.Login, UIConfig.ViewName.Login)

local function BtnAutofix()
end

local function BtnRegister()
    print("注册")
    _C.View.CtrlLogin.selectedIndex = 1
end

local function BtnLogin()
    print("登录")
    _C.View.CtrlLogin.selectedIndex = 1
end

local function BtnSwitchAccount()
    print("切换账号")
    _C.View.CtrlLogin.selectedIndex = 0
end

local function BtnStart()
    _C:Close()
    LevelManager.LoadScene(Define.LevelType.MainCity)
end

local function BtnSwitchServer()
    UIManager.OpenController(UIConfig.ControllerName.ServerList)
end

local function BtnAutofix()
    UIManager.OpenController(UIConfig.ControllerName.Autofix)
end

function _C:OnPreHandle()
    if VideoManager.IsPlaying(2) then
        return true
    else
        VideoManager.Play(
            2,
            function()
                UIManager.OpenController(UIConfig.ControllerName.Login)
            end
        )
        return false
    end
end

function _C:OnCreat()
    _C.View:AddEvent("BtnRegister", BtnRegister)
    _C.View:AddEvent("BtnLogin", BtnLogin)
    _C.View:AddEvent("BtnStart", BtnStart)
    _C.View:AddEvent("BtnSwitchAccount", BtnSwitchAccount)
    _C.View:AddEvent("BtnSwitchServer", BtnSwitchServer)
    _C.View:AddEvent("BtnAutofix", BtnAutofix)
end

function _C:OnOpen(data)
    _C.View.CtrlLogin.selectedIndex = 0
end

function _C:OnClose()
end
