local protoIdMap = LuaHandle.Load("autogen.msgmap")
local protoIdBlacklist = LuaHandle.Load("Config.ProtoIdConfig").BlacklistId

-- 消息同步时的菊花处理
local MsgHandle = {}

-- 同步的消息id
local syncMsgId = 0
-- 同步消息次数
local syncMsgCount = 0
-- 同步超时次数
local syncTimeoutCount = 1
-- 同步超时计时器
local syncTimerOut = nil

-- 必须重新连接
local function socketReconnect()
    MsgHandle.clear()
end
Event.AddListener(Event.MUST_RECONNECT, socketReconnect)
Event.AddListener(Event.MUST_RELOGIN, socketReconnect)
Event.AddListener(Event.MUST_CLOSE, socketReconnect)

-- protoId
local function getProtoId(moduleId, msgId)
    return msgId * 10000 + moduleId
end

-- 返回登录
local function backToLogin()
    -- 关闭菊花
    UIManager.WaitSync(Define.SyncType.MsgSync, false)

    -- 在登录界面就不处理了
    if not UIManager.CtrlIsShow(UIConfig.ControllerName.LoginMain) then
        -- 返回登录
        LevelManager.loadScene(Define.LevelType.Bootup)
    end
end

-- 继续等待
local function continueWait()
    -- 重新计时
    if syncMsgCount > 0 then
        syncTimerOut:start()
        NetworkManager.sendMsg(misc_decoder.NewC2sPingMsg())
    end
end

-- 菊花超时
local function syncTimeOut()
    -- 未完全登录时
    if not Common.PreLogin then
        -- 关闭连接状态
        NetworkManager.disConnect()
        -- 提示网络不佳
        UIManager.ShowTip("C_NetError")
        return
    end

    -- 超时返回登录处理
    local data = nil
    if syncTimeoutCount >= 3 then
        data = {
            content = Localization.NetReloginFailure,
            btnRightTitle = Localization.BtnToLogin,
            btnRightFunc = backToLogin,
            syncType = Define.SyncType.MsgSync
        }
    else
        data = {
            content = Localization.NetTimeOut,
            btnLeftTitle = Localization.BtnToLogin,
            btnRightTitle = Localization.BtnToWait,
            btnLeftFunc = backToLogin,
            btnRightFunc = continueWait,
            syncType = Define.SyncType.MsgSync
        }
    end
    -- 弹出二级弹框
    UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
    -- 超时计数+1
    syncTimeoutCount = syncTimeoutCount + 1
end

-- msg递增
local function addC2SMsgId(moduleId, msgId)
    -- 黑名单下的不给转菊花
    if nil ~= protoIdBlacklist[moduleId .. "-" .. msgId] then
        return
    end

    local c2sId = getProtoId(moduleId, msgId)
    -- 判断此id是否有回复id
    if nil == protoIdMap.C2SMap[c2sId] then
        return
    end

    syncMsgCount = syncMsgCount + 1
    syncTimeoutCount = 1
    -- 同步计时器开启
    syncTimerOut:start()
    -- 打开菊花
    UIManager.WaitSync(Define.SyncType.MsgSync)

    -- 发送菊花同步的协议
    local msg, newModuleId, newMsgId = misc_decoder.NewC2sPingMsg()
    syncMsgId = getProtoId(newModuleId, newMsgId)
    NetworkManager.sendMsg(msg)
end

-- msg递减
function MsgHandle.receiveS2CMsgId(moduleId, msgId)
    -- 消息id不为空
    if nil == moduleId or nil == msgId then
        return
    end
    -- s2cid
    local s2cId = getProtoId(moduleId, msgId)
    if nil == s2cId then
        return
    end
    -- c2sid
    local c2sId = protoIdMap.S2CMap[s2cId]
    if nil == c2sId or c2sId ~= syncMsgId then
        return
    end

    syncMsgCount = syncMsgCount - 1
    -- 关闭菊花
    if syncMsgCount > 0 then
        return
    end
    UIManager.WaitSync(Define.SyncType.MsgSync, false)
    UIManager.SendNtfMessage(Event.ERROR_SYNC_CLOSE, Define.SyncType.MsgSync)
    syncTimerOut:pause()
end

-- 初始化
function MsgHandle.initialize()
    syncTimeoutCount = 1
    syncMsgCount = 0

    -- 超时处理
    syncTimerOut = TimerManager.newTimer(10, false, false, nil, nil, syncTimeOut)
end

-- 发送消息
function MsgHandle.sendMsg(moduleId, msgId)
    -- 消息id为空，表示不需要转菊花
    if nil == moduleId or nil == msgId then
        return
    end

    addC2SMsgId(moduleId, msgId)
end

-- 清除
function MsgHandle.Destroy()
    syncTimeoutCount = 1
    syncMsgCount = 0
    syncTimerOut:pause()
    UIManager.WaitSync(Define.SyncType.MsgSync, false)
end

return MsgHandle
