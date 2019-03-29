-- http连接的超时处理
HttpManager = LuaHandle.Load("Game.Manager.IManager")()

-- 当前http请求个数
local m_syncCount = 0
-- 请求的http消息
local m_httpMsg = {}

-- 返回登录
local function BackToLogin()
    UIManager.WaitSync(Define.SyncType.HttpSync, false)
    LevelManager.LoadScene(Define.LevelType.Bootup)
end

-- http递增
local function CountAsc()
    if m_syncCount <= 0 then
        UIManager.WaitSync(Define.SyncType.HttpSync, true)
    end
    m_syncCount = m_syncCount + 1
end

-- http递减
local function CountDec()
    m_syncCount = m_syncCount - 1
    if m_syncCount <= 0 then
        UIManager.WaitSync(Define.SyncType.HttpSync, false)
    end
end

-- 检测是否再次请求
local function CheckRerequst(error, msg)
    if error ~= CSharp.HttpErrorCode.RequstTimeOut then
        return false
    end
    -- 判断为奇数次超时
    msg.timeoutCount = msg.timeoutCount + 1
    if math.fmod(msg.timeoutCount, 2) == 0 then
        return false
    end

    if nil == msg.fromArgs then
        HttpHandle.Get(msg.urlArgs)
    else
        HttpHandle.Post(msg.urlArgs, msg.fromArgs)
    end
    return true
end

-- 检测是否等待
local function CheckKeepWaiting(msg)
    if nil == msg then
        return false
    end
    if not msg.isKeepWaiting then
        return false
    end

    -- 只给两次机会
    local data = nil
    if msg.syncCount >= 3 then
        data = {
            content = Localization.LoginFailure,
            btnRightTitle = Localization.BackToLogin,
            btnRightFunc = BackToLogin
        }
    else
        local toRehttp = function()
            if nil == msg.fromArgs then
                HttpHandle.Get(msg.urlArgs)
            else
                HttpHandle.Post(msg.urlArgs, msg.fromArgs)
            end
        end
        data = {
            content = Localization.NetTimeOut,
            btnLeftTitle = Localization.BackToLogin,
            btnRightTitle = Localization.KeepToWait,
            btnLeftFunc = BackToLogin,
            btnRightFunc = toRehttp
        }
    end
    -- 弹出二级弹框
    UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)
    return true
end

-- 错误处理
local function ErrorCode(error, msg)
    local desc = ""
    if error == CSharp.HttpErrorCode.Unreachable then
        desc = Localization.NetUnreachable
    elseif error == CSharp.HttpErrorCode.RequstTimeOut then
        desc = Localization.NetTimeOut
    elseif error == CSharp.HttpErrorCode.ReceiveZero then
        desc = Localization.NetReceiveZero
    elseif error == CSharp.HttpErrorCode.ReceiveUndone then
        desc = Localization.NetReceiveZero
    else
        desc = Localization.NetError
    end

    -- 网络异常
    if not msg.isErrorPopup then
        UIManager.ShowTip(desc, false)
    else
        UIManager.OpenController(UIConfig.ControllerName.Popup, {content = desc})
    end
end

-- http请求失败
local function SyncError(index, error)
    local msg = m_httpMsg[index]
    if nil == msg then
        return
    end
    -- 不用菊花的不做处理！！
    if not msg.isSync then
        m_httpMsg[index] = nil
        return
    end

    -- 再次请求
    if CheckRerequst(error, msg) then
        return
    end

    CountDec()

    -- 是否等待
    if CheckKeepWaiting(msg) then
        return
    else
        ErrorCode(error, msg)
    end
    m_httpMsg[index] = nil
end

-- http请求成功
local function SyncSucceed(index, res)
    local msg = m_httpMsg[index]
    if nil == msg then
        return
    end
    -- 是否为页面错误
    if nil ~= tonumber(Utils.stringSub_1(res, 0, 3)) then
        SyncError(index, Localization.NetReceiveZero)
        return
    end

    if msg.isSync then
        CountDec()
    end
    msg.onSucceed(res)
    m_httpMsg[index] = nil
end

-- 新增http消息
local function AddHttpMsg(url, from, callback, data)
    -- 无效的远端地址
    if nil == url or "" == url then
        UIManager.ShowTip(Localization.InvaildUrl, false)
        return false
    end

    -- 此处的拼接为约定！！
    local index = nil ~= from and url .. "?" .. from or url
    if nil == m_httpMsg[index] then
        m_httpMsg[index] = {
            isKeepWaiting = nil ~= data and data.keepWaiting or false,
            isErrorPopup = nil ~= data and data.errorPopup or false,
            isErrorTip = nil ~= data and data.errorTip or false,
            isSync = nil ~= data and data.sync or false,
            urlArgs = url,
            fromArgs = from,
            onSucceed = callback,
            syncCount = 1,
            timeoutCount = 0
        }
    else
        m_httpMsg[index].syncCount = m_httpMsg[index].syncCount + 1
    end

    -- 需要菊花同步，且为偶数次超时
    if m_httpMsg[index].isSync and math.fmod(m_httpMsg[index].timeoutCount, 2) == 0 then
        CountAsc()
    end

    return true
end

-- Get
-- url:远端
-- callback:成功回调
-- data:{
--     -- 菊花同步
--     sync = true,
--     -- 天荒地老，等待消息回来
--     keepWaiting = true,
--     -- 错误弹框
--     errorPopup = true,
--     -- 错误飘字
--     errorTip = true
-- }
function HttpManager.Get(url, callback, data)
    print("http请求：", url)
    if AddHttpMsg(url, nil, callback, data) then
        CSharp.Http.Get(url, SyncSucceed, SyncError)
    end
end

-- Post
function HttpManager.Post(url, from, callback, data)
    print("http请求：", url, from)
    if AddHttpMsg(url, from, callback, data) then
        CSharp.Http.Post(url, from, SyncSucceed, SyncError)
    end
end

-- Image
function HttpManager.Image(url, succeedCallBack)
    print("http iamge：", url)
    CSharp.ABHttpImg.Instance:GetImage(url, succeedCallBack)
end

-- 初始化
function HttpManager.Initialize()
    m_syncCount = 0
    m_httpMsg = {}
end

-- httpClear
function HttpManager.CustomDestroy()
    m_syncCount = 0
    m_httpMsg = {}
    UIManager.WaitSync(Define.SyncType.HttpSync, false)
end
