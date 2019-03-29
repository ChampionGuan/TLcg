local protoIdMap = nil
local protoIdBlacklist = nil

-- 消息同步时的菊花处理
local MsgHandle = {}

-- 同步的消息id
local syncMsgId = 0
-- 同步消息次数
local syncMsgCount = 0
-- 同步超时次数
local syncTimeoutCount = 1
-- 同步超时计时器
local syncTimeoutTimer = nil

-- protoId
local function GetProtoId(moduleId, msgId)
    return msgId * 10000 + moduleId
end

-- 返回登录
local function BackToLogin()
    UIManager.WaitSync(Define.SyncType.Sync, false)
    LevelManager.LoadScene(Define.LevelType.Bootup)
end

-- 继续等待
local function ContinueWait()
    -- 重新计时
    if syncMsgCount > 0 then
        syncTimeoutTimer:start()
        NetworkManager.SendMsg(misc_decoder.NewC2sPingMsg())
    end
end

-- 菊花超时
local function SyncTimeOut()
    -- 未完全登录时
    if not Common.PreLogin then
        NetworkManager.disConnect()
        UIManager.ShowTip("C_NetError")
        return
    end

    -- 超时返回登录处理
    local data = nil
    if syncTimeoutCount >= 3 then
        data = {
            content = Localization.LoginFailure,
            btnRightTitle = Localization.BackToLogin,
            btnRightFunc = BackToLogin,
            syncType = Define.SyncType.Sync
        }
    else
        data = {
            content = Localization.NetTimeOut,
            btnLeftTitle = Localization.BackToLogin,
            btnRightTitle = Localization.KeepToWait,
            btnLeftFunc = BackToLogin,
            btnRightFunc = ContinueWait,
            syncType = Define.SyncType.Sync
        }
    end
    -- 弹出二级弹框
    UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
    -- 超时计数+1
    syncTimeoutCount = syncTimeoutCount + 1
end

-- msg递增
local function AddC2SMsgId(moduleId, msgId)
    -- 黑名单下的不给转菊花
    if nil ~= protoIdBlacklist[moduleId .. "-" .. msgId] then
        return
    end

    local c2sId = GetProtoId(moduleId, msgId)
    -- 判断此id是否有回复id
    if nil == protoIdMap.C2SMap[c2sId] then
        return
    end

    syncMsgCount = syncMsgCount + 1
    syncTimeoutCount = 1
    syncTimeoutTimer:start()
    UIManager.WaitSync(Define.SyncType.Sync)

    -- 发送菊花同步的协议
    local msg, newModuleId, newMsgId = misc_decoder.NewC2sPingMsg()
    syncMsgId = GetProtoId(newModuleId, newMsgId)
    NetworkManager.SendMsg(msg)
end

-- msg递减
function MsgHandle.ReceiveMsg(moduleId, msgId)
    -- 消息id不为空
    if nil == moduleId or nil == msgId then
        return
    end
    -- s2cid
    local s2cId = GetProtoId(moduleId, msgId)
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
    UIManager.WaitSync(Define.SyncType.Sync, false)
    UIManager.DispatchEvent(Event.ERROR_SYNC_CLOSE, Define.SyncType.Sync)
    syncTimeoutTimer:pause()
end

-- 初始化
function MsgHandle.Initialize()
    if nil == protoIdMap then
        protoIdMap = LuaHandle.Load("Game.autogen.msgmap")
    end
    if nil == protoIdBlacklist then
        protoIdBlacklist = LuaHandle.Load("Game.Config.ProtoIdConfig").BlacklistId
    end

    syncMsgCount = 0
    syncTimeoutCount = 1
    syncTimeoutTimer = TimerManager.newTimer(10, false, false, nil, nil, SyncTimeOut)
end

-- 发送消息
function MsgHandle.SendMsg(moduleId, msgId)
    -- 消息id为空，表示不需要转菊花
    if nil == moduleId or nil == msgId then
        return
    end

    AddC2SMsgId(moduleId, msgId)
end

-- 清除
function MsgHandle.Destroy()
    syncMsgCount = 0
    syncTimeoutCount = 1
    syncTimeoutTimer:pause()
    UIManager.WaitSync(Define.SyncType.Sync, false)
end
Event.AddListener(EventType.MUST_RECONNECT, MsgHandle.Destroy)
Event.AddListener(EventType.MUST_RELOGIN, MsgHandle.Destroy)
Event.AddListener(EventType.MUST_CLOSE, MsgHandle.Destroy)

return MsgHandle
