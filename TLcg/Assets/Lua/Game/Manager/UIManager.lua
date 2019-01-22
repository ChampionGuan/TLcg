UIManager = LuaHandle.Load("Game.Manager.IManager")()
-- UIStage放大系数
UIManager.StageScaleFactor = 1
-- UIRoot放大系数
UIManager.RootScaleFactor = 1
-- UIRoot size
UIManager.RootSize = {x = 0, y = 0}

-- ctrl--
local ctrl = nil
-- subCtrl--
local subCtrl = nil
-- view--
local view = nil
-- ctrlCenter
local ctrlCenter = nil
-- 多点触摸
local multiTouchConfig = nil
-- 面板互斥
local mutexesConfig = nil

-- 初始化fairyGui
local function InitFairyGui()
    -- 字体库
    CSharp.FUIConfig.defaultFont =
        "Droid Sans Fallback,FZZhengHeiS-DB-GB,Microsoft YaHei,LTHYSZK,Helvetica-Bold,Helvetica-Bold"
    -- ui相机
    CSharp.GameObject.DontDestroyOnLoad(CSharp.StageCamera.main)
    CSharp.StageCamera.main.transform.position = CSharp.StageCamera.main.transform.position - CSharp.Vector3(0, 0, 1)
    CSharp.StageCamera.main.nearClipPlane = -10
    CSharp.StageCamera.main.farClipPlane = 10
    CSharp.StageCamera.main.depth = 100
    CSharp.StageCamera.main.allowHDR = false
end

-- 分辨率设置
local function SetResolution()
    -- 屏幕自适应
    CSharp.GRoot.inst:SetContentScaleFactor(Common.UIResolution.x, Common.UIResolution.y)
    -- UIRoot放大系数
    local screenWidthScale = Common.ScreenResolution.x / Common.UIResolution.x
    local screenHeightScale = Common.ScreenResolution.y / Common.UIResolution.y
    if screenWidthScale > screenHeightScale then
        UIManager.RootScaleFactor = 1 + ((screenWidthScale - screenHeightScale) / screenHeightScale)
    else
        UIManager.RootScaleFactor = 1 + ((screenHeightScale - screenWidthScale) / screenWidthScale)
    end
    -- 舞台放大系数
    UIManager.StageScaleFactor = 1 / (CSharp.Stage.inst.scaleX * CSharp.GRoot.contentScaleFactor)
    -- root尺寸
    UIManager.RootSize = {x = CSharp.GRoot.inst.width, y = CSharp.GRoot.inst.height}
end

-- 多点触摸设置
local function SetUIMultiTouch(name)
    CSharp.Input.multiTouchEnabled = multiTouchConfig[name] or false
    CSharp.Stage.inst:ResetInputState()
end

-- 界面互斥设置
local function SetUIMutextes(name)
    -- 互斥面板
    local config = uiMutextesConfig[name]
    if nil == config then
        return
    end
    for k, v in pairs(config) do
        local mutexCtrl = UIManager.GetController(v)
        -- 如果存在互斥面板，且处于打开状态
        if nil ~= mutexCtrl and mutexCtrl.IsOpen then
            mutexCtrl:Close()
        end
    end
end

-- 关闭主界面之外面板
local function SetUICloseloop(name)
    if name ~= UIConfig.ControllerName.MainCity then
        return
    end
    UIManager.CloseTheBelowCtrl(ctrl.ControllerName)
end

local function OnCtrlOpen(ctrl)
    SetUIMutextes(ctrl.ControllerName)
end

local function OnCtrlClose(ctrl)
end

local function OnCtrlShow(ctrl)
    SetUICloseloop(ctrl.ControllerName)
    SetUIMultiTouch(ctrl.ControllerName)
end

local function OnCtrlHide(ctrl)
end

-- 初始化
function UIManager.Initialize()
    LuaHandle.Load("Game.UI.Common.UIDefine")
    LuaHandle.Load("Game.UI.Common.UIConfig")
    multiTouchConfig = LuaHandle.Load("Game.Config.UIMultiTouchConfig")
    mutexesConfig = LuaHandle.Load("Game.Config.UIMutexesConfig")
    ctrlCenter = LuaHandle.Load("Game.UI.Core.UICenter")
    ctrl = LuaHandle.Load("Game.UI.Core.UIController")
    subCtrl = LuaHandle.Load("Game.UI.Core.UISubController")
    view = LuaHandle.Load("Game.UI.Core.UIView")
    InitFairyGui()
    SetResolution()
    ctrlCenter.OnCtrlShow = OnCtrlShow
    ctrlCenter.OnCtrlHide = OnCtrlHide
    ctrlCenter.OnCtrlOpen = OnCtrlOpen
    ctrlCenter.OnCtrlClose = OnCtrlClose
end

-- 更新
function UIManager.CustomUpdate()
    ctrlCenter:CustomUpdate()
end

-- 固定更新
function UIManager.CustomFixedUpdate()
    ctrlCenter:CustomFixedUpdate()
end

-- 销毁
function UIManager.CustomDestroy()
    UIManager.DestroyAllCtrl(true)
    CSharp.Stage.inst:RemoveEventListeners()
end

-- 焦点
function UIManager.OnAppFocus(hasfocus)
end

-- uiCtrl
function UIManager.Controller(ctrlName, viewName)
    local value = ctrlCenter:GetController(ctrlName)
    if nil ~= value then
        return value
    else
        return ctrl(ctrlName, viewName)
    end
end

-- uiSubCtrl
function UIManager.SubController(ctrlName, viewName)
    local value = ctrlCenter:GetSubController(ctrlName)
    if nil ~= value then
        return value
    else
        return subCtrl(ctrlName, viewName)
    end
end

-- uiView
function UIManager.View()
    return view()
end

-- 打开队列
local opening = false
local openQueue = {}
-- 打开界面
function UIManager.OpenController(name, data)
    -- 正在打开中，等待！！
    if opening then
        table.insert(openQueue, {name, data})
        return
    end

    opening = true
    ctrlCenter:OpenController(name, data)
    opening = false

    -- 打开在等待的队列
    local new = table.remove(openQueue, 1)
    if nil ~= new then
        UIManager.OpenController(new[1], new[2])
    end
end

-- 获取面板
function UIManager.GetCtrl(name)
    return ctrlCenter:GetController(name)
end

-- 获取顶部面板
function UIManager.GetTopCtrl()
    return ctrlCenter:GetTopCtrl()
end

-- 获取顶部的非弹框面板
function UIManager.GetTopCtrl2NotPopup()
    return ctrlCenter:GetTopCtrl2NotPopup()
end

-- 获取顶部的全屏面板
function UIManager.GetTopFullScreenCtrl()
    return ctrlCenter:GetTopFullScreenCtrl()
end

-- 界面是否打开
function UIManager.CtrlIsOpen(name)
    local ctrl = ctrlCenter:GetController(name)
    if nil == ctrl then
        return false
    else
        return ctrl.IsOpen
    end
end

-- 界面是否为显示状态
function UIManager.CtrlIsShow(name)
    local ctrl = ctrlCenter:GetController(name)
    if nil == ctrl then
        return false
    else
        return ctrl.IsShow
    end
end

-- 关闭此界面之上的所有面板
function UIManager.CloseTheAboveCtrl(name)
    local ctrl = ctrlCenter:getTopCtrl()
    if nil ~= ctrl and ctrl.ControllerName ~= name then
        ctrl:Close()
        UIManager.CloseTheAboveCtrl(name)
    end
end

-- 关闭此界面之下的所有面板
function UIManager.CloseTheBelowCtrl(name)
    local ctrl = ctrlCenter:GetBottomCtrl()
    if nil ~= ctrl and ctrl.ControllerName ~= name then
        ctrl:Close()
        UIManager.CloseTheBelowCtrl(name)
    end
end

-- 刷新所有面板
function UIManager.RefreshAllCtrl(name)
    ctrlCenter:RefreshAllCtrl()
end

-- 销毁指定面板
function UIManager.DestroyCtrl(name)
    ctrlCenter:DestroyCtrl(name)
end

-- 销毁所有面板
function UIManager.DestroyAllCtrl(deep)
    ctrlCenter:DestroyAllCtrl(deep)
end

-- 发送广播
function UIManager.SendNtfMessage(ntfType, ...)
    Event.Dispatch(ntfType, ...)
    ctrlCenter:SendNtfMessage(ntfType, ...)
end

-- 消息等待
function UIManager.WaitSync(type, value)
end

-- 消息等待中
function UIManager.WaitSyncing()
end
