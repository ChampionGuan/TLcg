LuaHandle.Load("Common.CSUtils")
LuaHandle.Load("Common.json")
LuaHandle.Load("Common.PlayerPrefs")
local Tips = LuaHandle.Load("Launcher.Tips")
local Video = LuaHandle.Load("Launcher.Video")
local Audio = LuaHandle.Load("Launcher.Audio").Audio
local XluaUtils = LuaHandle.Load("Common.XluaUtils")

local ABHotfix = {}
local m_mainUI = {}
local m_popupUI = {}

local function QuitApp()
    CSharp.Application.Quit()
end

local function InitializeUI()
    -- 分辨率
    CSharp.GRoot.inst:SetContentScaleFactor(Common.UIResolution.x, Common.UIResolution.y)

    -- ui相机
    local uiCamera = CSharp.StageCamera.main
    uiCamera.transform.position = uiCamera.transform.position - CSharp.Vector3(0, 0, 1)
    uiCamera.nearClipPlane = -10
    uiCamera.farClipPlane = 10
    uiCamera.depth = 100
    uiCamera.allowHDR = false
end

local function CreatUI(path, fileName, panelName)
    -- 包引入
    CSharp.ABManager.LoadUI(path)
    -- 创建UI
    local ui = CSharp.UIPackage.CreateObject(fileName, panelName).asCom
    CSharp.GRoot.inst:AddChild(ui)

    -- 不可见
    ui.visible = false
    -- 设置全屏适应
    ui:MakeFullScreen()

    return ui
end

local function ShowPopupUI(isShow)
    isShow = type(isShow) == "boolean" and isShow or false
    if nil == m_popupUI.UI then
        local ui = CreatUI("UI/FixUpdate/FixUpdate", "FixUpdate", "FixTipsMain")
        m_popupUI.UI = ui
        m_popupUI.Desc = ui:GetChild("title")
        m_popupUI.BtnState = ui:GetController("State_C")
        m_popupUI.BtnConfirm = ui:GetChild("Button_Confirm")
        m_popupUI.BtnCancel = ui:GetChild("Button_Cancel")
        m_popupUI.BtnCancel.onClick:Set(QuitApp)
    end
    m_popupUI.UI.visible = isShow

    -- 播个动效
    if isShow then
        m_popupUI.UI.pivot = CSharp.Vector2(0.5, 0.5)
        m_popupUI.UI.scale = CSharp.Vector2(0.85, 0.85)
        m_popupUI.UI:TweenScale(CSharp.Vector2(1, 1), 0.1)

        -- 清除确认按钮的事件监听
        m_popupUI.BtnConfirm.onClick:Clear()
        m_popupUI.BtnConfirm.onClick:Add(ShowPopupUI)

        -- 清除取消按钮的事件监听
        m_popupUI.BtnCancel.onClick:Clear()
        m_popupUI.BtnCancel.onClick:Add(ShowPopupUI)
    end
end

local function ShowMainUI(isShow)
    isShow = type(isShow) == "boolean" and isShow or false
    if nil == m_mainUI.UI then
        m_mainUI.UI = CreatUI("UI/FixUpdate/FixUpdate", "FixUpdate", "FixUpdateMain")
        m_mainUI.Desc = m_mainUI.UI:GetChild("Text_News")
        m_mainUI.Version = m_mainUI.UI:GetChild("Text_Version")
        m_mainUI.ProgressBar = m_mainUI.UI:GetChild("ProgressBar_Loading")
        m_mainUI.ProgressBar.max = 100
        m_mainUI.ProgressBar.value = 0
    end
    m_mainUI.UI.visible = isShow

    -- 关闭弹框
    if isShow then
        ShowPopupUI(false)
    end
end

local function DisposeUI()
    if nil ~= m_mainUI.UI then
        m_mainUI.UI:Dispose()
        m_mainUI.UI = nil
    end
    if nil ~= m_popupUI.UI then
        m_popupUI.UI:Dispose()
        m_popupUI.UI = nil
    end
end

local function AddBtnEvent(btn, event, args)
    if nil == btn or nil == event then
        return
    end
    btn.onClick:Add(
        function()
            if nil ~= args then
                event(args)
            else
                event()
            end
        end
    )
end

local function ProgressBar(value)
    if m_mainUI.ProgressBar.value > value then
        m_mainUI.ProgressBar.value = 0
    end
    if value ~= 0 and m_mainUI.ProgressBar.value ~= value then
        m_mainUI.ProgressBar:TweenValue(value, 0.5):SetEase(CSharp.EaseType.CubicOut)
    end
end

local function RemoteUrl()
    -- 服务器id
    local serverId = PlayerPrefs.GetLoginS()
    -- 远端地址
    local url =
        string.format(
        "http://193.112.227.230:8080/info?os=%d&t=%s&sid=%d",
        Common.LoginInfo.OSType,
        Common.LoginInfo.Identification,
        (serverId == "" and 0 or serverId)
    )
    return url
end

local function DownloadSize(value)
    if value <= 0 then
        return "0K"
    end
    if value > 1073741824 then
        return math.ceil(value / 1073741824 * 100) / 100 .. "G"
    elseif value > 1048576 then
        return math.ceil(value / 1048576 * 100) / 100 .. "M"
    elseif value > 1024 then
        return math.ceil(value / 1024 * 100) / 100 .. "K"
    else
        return value .. "B"
    end
end

local function LoadLua(complete)
    luaConfig = LuaHandle.Load("Launcher.Preload")

    local index, max = 0, #luaConfig
    local wait = CSharp.WaitForEndOfFrame()

    m_mainUI.ProgressBar.max = max
    m_mainUI.ProgressBar.value = 0

    while (index <= max) do
        index = index + 1
        LuaHandle.Load(luaConfig[index])
        m_mainUI.ProgressBar.value = index
        if index % 5 == 0 then
            coroutine.yield(wait)
        end
    end

    complete()
end

local m_downloadSize = nil
local m_downloadSpeed = nil
local m_hotfixComplete = nil
local function HotfixHandle(value)
    -- 客户端初始版本号
    if value.state == CSharp.EVersionState.OriginalVersionId then
        Common.Version.OriginalResVersion = value.sValue
        return
    end

    -- 客户端版本号
    if value.state == CSharp.EVersionState.ClientVersionId then
        m_mainUI.Version.text = value.sValue
        return
    end

    -- 开始检测本地（进度条滚动到50%，此处刻意！！）
    if value.state == CSharp.EVersionState.CheckLocalVersion then
        m_mainUI.ProgressBar.value = 0
        m_mainUI.ProgressBar:TweenValue(50, 2):SetEase(CSharp.EaseType.CubicOut)
        m_mainUI.Desc.text = Tips.Hotter_1
        return
    end

    -- 检测本地结束（进度条滚动到100%，此处刻意！！）
    if value.state == CSharp.EVersionState.CheckLocalVersionOver then
        m_mainUI.ProgressBar:TweenValue(100, 0.5):SetEase(CSharp.EaseType.CubicOut)
        return
    end

    -- 服务器所需资源号检测
    if value.state == CSharp.EVersionState.AckServerVersionId then
        local ok, decode = pcall(json.decode, value.sValue)
        if not ok then
            ShowPopupUI(true)
            AddBtnEvent(m_popupUI.BtnConfirm, ABHotfix.ReCheck)
            AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
            m_popupUI.Desc.text = Tips.NetError
            m_popupUI.BtnState.selectedIndex = 2
            return
        end

        -- 登陆服地址
        Common.LoginInfo.LoginAddr = decode.LoginAddr
        -- 热更新地址
        Common.LoginInfo.HotAddr = decode.HotAddr .. "/"
        -- 客户端版本号
        Common.LoginInfo.ResVersion = decode.ClientVersion
        -- true表示没有注册
        Common.LoginInfo.HaveNotRegister = decode.HaveNotRegister
        -- true表示有ip输入框
        Common.LoginInfo.HaveIpInput = decode.HaveIpInput
        -- 0都可以 1内网登陆 2渠道登陆
        Common.LoginInfo.LoginMode = decode.LoginMode

        -- 此服需要的版号
        if not CSharp.ABHelper.VersionNumMatch(Common.LoginInfo.ResVersion) then
            value.callBack(Common.Version.OriginalResVersion .. " " .. Common.LoginInfo.HotAddr)
        else
            value.callBack(Common.LoginInfo.ResVersion .. " " .. Common.LoginInfo.HotAddr)
        end
        -- value.callBack( "0.0.1.0" .. " " .. "http://192.168.1.110:100/ab_TAccumulation/" )
        return
    end

    -- 下载文件大小确认
    if value.state == CSharp.EVersionState.DownloadConfirm then
        m_downloadSize = DownloadSize(value.fValue)
        -- 更新内容很小，则直接更新(wifi：10m,移动数据：1m)
        if value.fValue < 10485760 then
            if CSharp.ABHelper.IsWifi then
                value.callBack()
                return
            elseif value.fValue < 1048576 then
                value.callBack()
                return
            end
        end
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, value.callBack)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = string.format(Tips.Hotter_3, m_downloadSize)
        m_popupUI.BtnState.selectedIndex = 1
        return
    end

    -- 下载使用wifi确认
    if value.state == CSharp.EVersionState.DownloadWifi then
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, value.callBack)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = Tips.Hotter_4
        m_popupUI.BtnState.selectedIndex = 1
        return
    end

    -- 下载速度
    if value.state == CSharp.EVersionState.DownloadSpeed then
        m_downloadSpeed = value.sValue
        return
    end

    -- 下载进度
    if value.state == CSharp.EVersionState.DownloadProgress then
        ProgressBar(value.fValue * 100)
        if nil ~= m_downloadSpeed then
            m_mainUI.Desc.text = string.format(Tips.Hotter_5, m_downloadSize, m_downloadSpeed)
        else
            m_mainUI.Desc.text = string.format(Tips.Hotter_6, m_downloadSize)
        end
        return
    end

    -- 解压进度
    if value.state == CSharp.EVersionState.UnzipProgress then
        ProgressBar(value.fValue * 100)
        m_mainUI.Desc.text = Tips.Hotter_7
        return
    end

    -- 无网络提示
    if value.state == CSharp.EVersionState.Unreachable then
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, ABHotfix.ReCheck)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = Tips.Hotter_8
        m_popupUI.BtnState.selectedIndex = 1
        return
    end

    -- 未知异常
    if value.state == CSharp.EVersionState.UnknowError then
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, ABHotfix.ReCheck)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = Tips.Hotter_9 .. value.sValue
        m_popupUI.BtnState.selectedIndex = 2
        print("更新时网络异常！！", value.sValue)
        return
    end

    -- 需要更新大版本
    if value.state == CSharp.EVersionState.NeedGoStore then
        ShowPopupUI(true)
        AddBtnEvent(
            m_popupUI.BtnConfirm,
            function()
                print("需要store网址！！前往下载了！！！")
                QuitApp()
            end
        )
        m_popupUI.Desc.text = Tips.Hotter_11
        m_popupUI.BtnState.selectedIndex = 3
        return
    end

    -- 当前版本号
    if value.state == CSharp.EVersionState.ServerVersionId then
        m_mainUI.Version.text = value.sValue
        Common.Version.ServerResVersion = value.sValue
        return
    end

    -- 热更完成，开始预加载lua
    if value.state == CSharp.EVersionState.HotfixComplete then
        m_mainUI.Desc.text = Tips.Hotter_2
        -- 如果检测到上次未完成重度修复，则继续
        if PlayerPrefs.GetAutoDeepfix() then
            ABHotfix.Repair(true, m_hotfixComplete)
        else
            CSharp.Main.Instance:StartCoroutine(
                XluaUtils.cs_generator(
                    LoadLua,
                    function()
                        m_hotfixComplete()
                        DisposeUI()
                    end
                )
            )
        end
        return
    end
end

local m_isDeepFix = false
local m_autofixComplete = nil
local function AutofixHandle(value)
    -- 客户端版本号
    if value.state == CSharp.EVersionState.ClientVersionId then
        m_mainUI.Version.text = value.sValue
        return
    end

    -- 未知异常
    if value.state == CSharp.EVersionState.UnknowError then
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, ABHotfix.ReRepair)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = Tips.Hotter_14 .. value.sValue
        m_popupUI.BtnState.selectedIndex = 2
        print("更新时网络异常！！", value.sValue)
        return
    end

    -- 下载文件大小确认
    if value.state == CSharp.EVersionState.DownloadConfirm then
        m_downloadSize = DownloadSize(value.fValue)
        -- 更新内容很小，则直接更新(wifi：10m,移动数据：1m)
        if value.fValue < 10485760 then
            if CSharp.ABHelper.IsWifi then
                value.callBack()
                return
            elseif value.fValue < 1048576 then
                value.callBack()
                return
            end
        end
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, value.callBack)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = string.format(Tips.Hotter_3, m_downloadSize)
        m_popupUI.BtnState.selectedIndex = 1
        return
    end

    -- 下载使用wifi确认
    if value.state == CSharp.EVersionState.DownloadWifi then
        ShowPopupUI(true)
        AddBtnEvent(m_popupUI.BtnConfirm, value.callBack)
        AddBtnEvent(m_popupUI.BtnCancel, QuitApp)
        m_popupUI.Desc.text = Tips.Hotter_4
        m_popupUI.BtnState.selectedIndex = 1
        return
    end

    -- 下载修复进度
    if value.state == CSharp.EVersionState.AutofixProgress then
        ProgressBar(value.fValue * 100)
        m_mainUI.Desc.text = Tips.Hotter_13
        return
    end

    -- 检测文件进度
    if value.state == CSharp.EVersionState.CheckFileProgress then
        ProgressBar(value.fValue * 100)
        m_mainUI.Desc.text = Tips.Hotter_12
        return
    end

    -- 删除文件进度
    if value.state == CSharp.EVersionState.AutofixDeleteFile then
        ProgressBar(value.fValue * 100)
        m_mainUI.Desc.text = Tips.Hotter_16
        return
    end

    -- 修复完成，开始预加载lua
    if value.state == CSharp.EVersionState.AutofixComplete then
        m_mainUI.Desc.text = Tips.Hotter_2
        CSharp.Main.Instance:StartCoroutine(
            XluaUtils.cs_generator(
                LoadLua,
                function()
                    PlayerPrefs.SaveAutoDeepfix(false)
                    m_autofixComplete()
                    DisposeUI()
                end
            )
        )
        return
    end
end

function ABHotfix.Bootup(over)
    ABHotfix.Initialize()
    -- 播放闪屏
    Video.Play(
        1,
        nil,
        function()
            ABHotfix.Check(over)
        end
    )
end

local m_checkVersionId = nil
function ABHotfix.Check(over)
    InitializeUI()
    ABHotfix.Initialize()

    -- 背景视频
    Video.Play(
        2,
        function()
            ShowMainUI(true)
            m_hotfixComplete = over
            m_checkVersionId = Common.Version.CheckResVersion

            if nil == Common.LoginInfo.HotAddr then
                CSharp.LauncherEngine.Instance:ABHotfix(nil, RemoteUrl(), nil, HotfixHandle)
            else
                CSharp.LauncherEngine.Instance:ABHotfix(
                    m_checkVersionId,
                    RemoteUrl(),
                    Common.LoginInfo.HotAddr,
                    HotfixHandle
                )
            end
        end
    )
end

function ABHotfix.ReCheck()
    ShowMainUI(true)
    if nil == Common.LoginInfo.HotAddr then
        CSharp.LauncherEngine.Instance:ABHotfix(nil, RemoteUrl(), nil, HotfixHandle)
    else
        CSharp.LauncherEngine.Instance:ABHotfix(m_checkVersionId, RemoteUrl(), Common.LoginInfo.HotAddr, HotfixHandle)
    end
end

function ABHotfix.Repair(isDeep, over)
    InitializeUI()
    ABHotfix.Initialize()

    -- 背景视频
    Video.Play(
        2,
        function()
            ShowMainUI(true)
            m_autofixComplete = over
            m_isDeepFix = isDeep

            PlayerPrefs.SaveAutoDeepfix(true)
            CSharp.LauncherEngine.Instance:ABRepair(m_isDeepFix, AutofixHandle)
        end
    )
end

function ABHotfix.ReRepair()
    ShowMainUI(true)
    PlayerPrefs.SaveAutoDeepfix(true)
    CSharp.LauncherEngine.Instance:ABRepair(m_isDeepFix, AutofixHandle)
end

-- 初始化
function ABHotfix.Initialize()
    Audio.Initialize()
    Video.Initialize()
end

-- 更新
function ABHotfix.FixedUpdate()
    Audio:Update()
end

-- 销毁
function ABHotfix.Destroy()
    if nil ~= m_mainUI.UI then
        m_mainUI.UI:Dispose()
    end
    if nil ~= m_popupUI.UI then
        m_popupUI.UI:Dispose()
    end
    m_mainUI.UI = nil
    m_popupUI.UI = nil
    Audio.Destroy()
    Video.Destroy()
    LuaHandle.Unload("Common.CSUtils")
    LuaHandle.Unload("Common.json")
    LuaHandle.Unload("Common.PlayerPrefs")
    LuaHandle.Unload("Common.XluaUtils")
    LuaHandle.Unload("Launcher.Tips")
    LuaHandle.Unload("Launcher.Video")
    LuaHandle.Unload("Launcher.Audio")
end

return ABHotfix
