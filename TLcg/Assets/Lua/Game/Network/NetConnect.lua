local protoIdConfig = LuaHandle.Load("Game.Config.ProtoIdConfig")
local protoIdBlacklist = protoIdConfig.BlacklistId
local protoIdLogin = protoIdConfig.LoginProtoId

-- 网络连接状态
local SocketStat = {
    None = 0,
    -- 连接中
    Connecting = 1,
    -- 连接上
    Connected = 2,
    -- 已断开连接需要重连
    Off_Reconnect = 3
}

-- 网络处理
local NetConnect = {}

-- 获取焦点
local hasFocus = true
-- 当前连接状态
local curStat = SocketStat.None
-- 连接成功回调
local connectedInvoke = nil

-- 直接重连，断线后之间重连
local reconnectDirect = false
-- 重连次数，超过3次则返回登录
local reconnectNum = 1
-- 重连超时计时器
local connectTimeOutTimer = nil
-- 后台心跳计时器
local backgroundHeartTimeOutTimer = nil

-- 游戏登录状态
local loginSuccess = false

----------------------------------------------------------------------------------------------
----------------------------------------异常重连，返回登录处理----------------------------------
----------------------------------------------------------------------------------------------

-- 重新连接
local function toReconnect()
    if not Common.PreLogin then
        -- 无任何登录时，走标准登录流程
        DataTrunk.PlayerInfo.LoginData.C2SLoginProto()
    else
        -- 在其他界面时，走重新登陆流程
        DataTrunk.PlayerInfo.LoginData.C2SReLoginProto()
    end
end

-- 返回登录
local function backToLogin()
    -- 关闭菊花
    UIManager.WaitSync(Define.SyncType.All, false)

    -- 在登录界面就不处理了
    if not UIManager.CtrlIsShow(UIConfig.ControllerName.LoginMain) then
        -- 返回登录
        LevelManager.LoadScene(Define.LevelType.Bootup)
    end
end

-- 网络重连弹框（返回登录和重新连接）
local function netReconnectPopup()
    -- 暂停计时
    connectTimeOutTimer:pause()
    -- 标记为需要重连
    curStat = SocketStat.Off_Reconnect
    -- 断开连接
    CSharp.NetworkManager.Instance:OnDisconnect()

    -- 未登录状态时
    if not Common.PreLogin then
        UIManager.ShowTip("C_NetError")
        -- 关闭连接状态
        NetworkManager.disConnect()
        return
    end

    -- 奇数时，直接重连
    if math.fmod(reconnectNum, 2) ~= 0 then
        toReconnect()
    else
        local data = nil
        local count = reconnectNum / 2
        -- 超时处理
        if count >= 3 then
            data = {
                content = Localization.NetReloginFailure,
                btnRightTitle = Localization.BtnToLogin,
                btnRightFunc = backToLogin
            }
        else
            data = {
                content = string.format(Localization.Connecting_2, count),
                btnLeftTitle = Localization.BtnToLogin,
                btnRightTitle = Localization.BtnToReconnect,
                btnLeftFunc = backToLogin,
                btnRightFunc = toReconnect
            }
            -- 首次重连
            if count == 1 then
                data.content = Localization.NetFirstConnect
            end
        end
        -- 打开重连弹框
        UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
    end
    -- 重连次数累加
    reconnectNum = reconnectNum + 1
end

-- 初始化
function NetConnect.initialize()
    -- 注册事件
    misc_decoder.RegisterAction(
        misc_decoder.S2C_FAIL_DISCONECT_REASON,
        function(code)
            -- 断开连接
            NetConnect.onDisconnect()
            -- 打开重连弹框
            local errorKey =
                UIManager.getNetworkErrorTip(misc_decoder.ModuleID, misc_decoder.S2C_FAIL_DISCONECT_REASON, code)
            local data = {
                content = Localization[errorKey],
                btnRightTitle = Localization.BtnToLogin,
                btnRightFunc = backToLogin
            }
            UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
        end
    )

    -- 登陆结束
    Event.AddListener(
        EventType.LOGIN_OVER,
        function()
            loginSuccess = true
        end
    )
    -- 返回登录
    Event.AddListener(
        EventType.BEFORE_LOADING,
        function(type)
            if type == Define.LevelType.Bootup then
                curStat = SocketStat.None
            end
        end
    )
    -- 后台心跳包
    local heartBeat = function()
        NetworkManager.sendMsg(misc_decoder.NewC2sBackgroundHeartBeatMsg())
    end
    -- 心跳计时器
    backgroundHeartTimeOutTimer = TimerManager.newTimer(10, true, true, heartBeat)

    -- 5秒连接超时，超时时断开连接（奇数时自动连，偶数时弹框告知玩家）
    connectTimeOutTimer = TimerManager.newTimer(5, false, false, nil, nil, netReconnectPopup)
end

----------------------------------------------------------------------------------------------
----------------------------------------消息处理机制-------------------------------------------
----------------------------------------------------------------------------------------------

-- 创建连接--
-- <param name="ip" type="string">ip地址</param>
-- <param name="port" type="number">端口</param>
-- <param name="token" type="string">认证登陆信息</param>
-- <param name="key" type="string">认证登陆信息</param>
-- <param name="callBack" type="function">登录完成回调</param>
function NetConnect.connect(ip, port, token, key, callBack)
    if curStat == SocketStat.Connecting then
        return
    end

    -- 网络不可达
    if not CSharp.NetworkManager.Instance.NetAvailable then
        -- 未登录时，飘字提示
        if not Common.PreLogin then
            UIManager.ShowTip("C_NetUnreachable")
            return
        end

        -- 打开重连弹框
        local data = {
            content = Localization.NetUnreachable,
            btnLeftTitle = Localization.BtnToLogin,
            btnRightTitle = Localization.BtnToReconnect,
            btnLeftFunc = backToLogin,
            btnRightFunc = toReconnect
        }
        UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
        return
    end

    -- 置为未登录状态
    loginSuccess = false
    -- 开启超时计时
    connectTimeOutTimer:start()
    -- 成功回调
    connectedInvoke = callBack
    -- 打开菊花
    UIManager.WaitSync(Define.SyncType.NetSync)

    -- 断开连接
    CSharp.NetworkManager.Instance:OnDisconnect()
    -- 重新连接
    CSharp.NetworkManager.Instance:Connect(ip, port, token, key)

    print(string.format("第%d次尝试连接", reconnectNum))
end

-- 发送消息
-- <param name="msg" type="string">消息包体</param>
function NetConnect.sendMsg(msg, moduleId, msgId)
    -- 打印当前网络状态
    print("当前连接状态：", curStat, "当前登录状态：", loginSuccess)

    -- 👆处在强制关闭状态
    if curStat == SocketStat.None then
        return false
    end
    if curStat == SocketStat.Connecting then
        return false
    end

    -- 处在需重连状态
    if curStat == SocketStat.Off_Reconnect then
        -- 如果未登录过
        if not Common.PreLogin then
            return false
        end
        -- 不是所有的协议都需要立马重连，比如后台的同步服务器时间，心跳包等
        if
            nil ~= moduleId and nil ~= msgId and nil ~= protoIdBlacklist[moduleId .. "-" .. msgId] and
                not protoIdBlacklist[moduleId .. "-" .. msgId]
         then
            return false
        end
        -- 重新连接
        if not UIManager.CtrlIsShow(UIConfig.ControllerName.NetErrorPopup) then
            if reconnectDirect then
                reconnectDirect = false
                toReconnect()
            else
                netReconnectPopup()
            end
        end
        return false
    end

    -- 处在已连接状态
    if curStat == SocketStat.Connected then
        -- 如果出来未登录成功状态，只处理部分协议
        if not loginSuccess and nil ~= moduleId and nil ~= msgId and nil == protoIdLogin[moduleId .. "-" .. msgId] then
            return false
        end

        CSharp.NetworkManager.Instance:SendMessage(msg)
        return true
    end
end

-- 接收到消息
function NetConnect.receiveS2CMsgId(moduleId, msgId)
end

----------------------------------------------------------------------------------------------
----------------------------------------socket连接状态监测-------------------------------------
----------------------------------------------------------------------------------------------

-- 主动断开连接
function NetConnect.onDisconnect(stat)
    print("断开所有连接！！", stat)

    curStat = nil == stat and SocketStat.None or stat
    loginSuccess = false
    reconnectNum = 1
    UIManager.WaitSync(Define.SyncType.NetSync, false)
    Event.Dispatch(Event.MUST_CLOSE)

    -- 断开连接
    CSharp.NetworkManager.Instance:OnDisconnect()
end

-- socket必须重连
local function socketReconnect()
    if curStat == SocketStat.Connecting then
        return
    end
    print("socket必须重连！！！")

    -- 断开
    CSharp.NetworkManager.Instance:OnDisconnect()
    curStat = SocketStat.Off_Reconnect
    loginSuccess = false

    -- 未获取到焦点
    if not hasFocus then
        return
    end

    -- 在需要直接重连时
    if reconnectDirect then
        reconnectDirect = false
        toReconnect()
        return
    end

    -- 在转菊花的界面，断开了连接，弹个框告诉玩家！！
    if nil ~= UIManager.sync and UIManager.sync.IsShow then
        netReconnectPopup()
        return
    end
end
Event.AddListener(EventType.MUST_RECONNECT, socketReconnect)
Event.AddListener(EventType.MUST_RELOGIN, socketReconnect)

-- socket连接中
local function socketConnecting()
    curStat = SocketStat.Connecting
end
Event.AddListener(EventType.CONNECTING, socketConnecting)

-- socket连接成功
local function socketConnected()
    -- 连接结束时需要关闭菊花
    UIManager.WaitSync(Define.SyncType.All, false)

    -- 置连接状态
    curStat = SocketStat.Connected
    -- 暂停超时计时
    connectTimeOutTimer:pause()
    -- 清连接次数
    reconnectNum = 1

    -- 连接成功回调
    if nil ~= connectedInvoke then
        connectedInvoke()
        -- 清空
        connectedInvoke = nil
    end
    print("socket连接成功")
end
Event.AddListener(EventType.CONNECTED, socketConnected)

----------------------------------------------------------------------------------------------
----------------------------------------后台切入时的网络处理------------------------------------
----------------------------------------------------------------------------------------------

-- 失去焦点时间
local loseFocusTime = os.time()
-- 是否为wifi连接
local isWifi = CSharp.NetworkManager.Instance.IsWifi

-- app退出
local function onAppQuit()
    curStat = SocketStat.None
end
Event.AddListener(EventType.APP_QUIT, onAppQuit)

-- app得焦点
local function onAppFocus(focus)
    local wait = CSharp.WaitForEndOfFrame()
    -- 等二帧
    coroutine.yield(wait)
    coroutine.yield(wait)

    -- 标识
    hasFocus = focus
    -- 在后台时间计算
    local interval = os.time() - loseFocusTime
    -- 存储焦点切换时间
    loseFocusTime = os.time()
    -- 无网络服务,切换网络（wifi切换4g网络时,要求重新登录;4g切wifi就没问题,坑爹！！）
    local netChange = not CSharp.NetworkManager.Instance.NetAvailable or isWifi ~= CSharp.NetworkManager.Instance.IsWifi
    -- 存储wifi状态
    isWifi = CSharp.NetworkManager.Instance.IsWifi

    -- 获取焦点
    if not focus then
        backgroundHeartTimeOutTimer:start()
        return
    else
        backgroundHeartTimeOutTimer:pause()
        if netChange then
            socketReconnect()
        end

        if not UIManager.CtrlIsShow(UIConfig.ControllerName.NetErrorPopup) then
            reconnectDirect = true
            NetworkManager.sendMsg(misc_decoder.NewC2sSyncTimeMsg(TimerManager.curServerTimestamp))
            NetworkManager.sendMsg(misc_decoder.NewC2sBackgroundWeakupMsg())
            -- 非编辑器模式下后台切入转菊花
            if not Utils.isEditor() then
                NetworkManager.sendMsg(misc_decoder.NewC2sPingMsg())
            end
        end
    end
end

-- app焦点捕获
local focusCoroutine = nil
function NetConnect.onAppFocus(focus)
    print(
        "focus:",
        focus,
        "syncComplete:",
        Common.PreLogin,
        "curStat:",
        curStat,
        "isWifi:",
        CSharp.NetworkManager.Instance.IsWifi,
        "netAvailable",
        CSharp.NetworkManager.Instance.NetAvailable
    )
    -- 未登录状态时
    if not Common.PreLogin or curStat == SocketStat.None then
        return
    end

    if nil ~= focusCoroutine then
        Utils.CoroutineStop(focusCoroutine)
    end
    focusCoroutine = Utils.CoroutineStart(onAppFocus, focus)
end

return NetConnect
