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
    UIManager.OpenController(UIConfig.ControllerName.Repair)
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
    _C.View.TextVersion.text = Common.Version.ServerResVersion
end

function _C:OnClose()
end

function _C:OnUpdate()
    if CSharp.Input.GetKeyUp(CSharp.KeyCode.A) then
        UIManager.WaitSync(1, true)
        local a = CSharp.Gameobjects.Instance:Spawner("Prefabs/Misc/AudioPlayer", false)
        local b = CSharp.Gameobjects.Instance:Spawner("Prefabs/Misc/AudioPlayer", false)
        local c = CSharp.Gameobjects.Instance:Spawner("Prefabs/Misc/AudioPlayer", false)
        local d = CSharp.Gameobjects.Instance:Spawner("Prefabs/Misc/AudioPlayer", false)
        a:CustomDestroy()
        b:CustomDestroy()
        c:CustomDestroy()
        d:CustomDestroy()
    end

    if CSharp.Input.GetKeyUp(CSharp.KeyCode.S) then
        UIManager.WaitSync(2, true)
        CSharp.Gameobjects.Instance:Destroy(0.5)
    end
    
    if CSharp.Input.GetKeyUp(CSharp.KeyCode.D) then
        UIManager.WaitSync(4, true)
    end

    if CSharp.Input.GetKeyUp(CSharp.KeyCode.Z) then
        UIManager.WaitSync(1, false)
    end
    if CSharp.Input.GetKeyUp(CSharp.KeyCode.X) then
        UIManager.WaitSync(2, false)
    end
    if CSharp.Input.GetKeyUp(CSharp.KeyCode.C) then
        UIManager.WaitSync(4, false)
    end

    if CSharp.Input.GetKeyUp(CSharp.KeyCode.J) then
        TimerManager.NewTimer(
            1,
            false,
            false,
            function()
                Debug.Log("开始")
            end,
            function(p1, p2)
                Debug.Log(p1, p2)
            end,
            function()
                Debug.Log("结束")
            end
        )
    end
    if CSharp.Input.GetKeyUp(CSharp.KeyCode.K) then
        self.Timers:New(
            1,
            false,
            false,
            function()
                Debug.Log("开始")
            end,
            function(p1, p2)
                Debug.Log(p1, p2)
            end,
            function()
                Debug.Log("结束")
            end
        )
    end
end
