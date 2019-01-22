-- http连接的超时处理
local HttpHandle = {}

-- 当前http请求个数
local httpCount = 0
-- 请求的http消息
local httpMsg = {}

-- 返回登录
local function BackToLogin()
    -- 关闭菊花
    UIManager.WaitSync(Define.SyncType.HttpSync, false)

    -- 在登录界面就不处理了
    if not UIManager.CtrlIsShow(UIConfig.ControllerName.LoginMain) then
        -- 返回登录
        LevelManager.loadScene(Define.LevelType.Bootup)
    end
end

-- http递增
local function HttpCountAsc()
    if httpCount <= 0 then
        UIManager.WaitSync(Define.SyncType.HttpSync, true)
    end
    httpCount = httpCount + 1
end

-- http递减
local function HttpCountDec()
    httpCount = httpCount - 1
    if httpCount <= 0 then
        -- 关闭菊花
        UIManager.WaitSync(Define.SyncType.HttpSync, false)
    end
end

-- 检测持续性等待
local function CheckStillWait(index)
    local msg = httpMsg[index]
    if nil == msg then
        return false
    end
    if not msg.isWait then
        return false
    end

    local toReHttp = function()
        if nil == msg.fromArgs then
            HttpHandle.Get(msg.urlArgs)
        else
            HttpHandle.Post(msg.urlArgs, msg.fromArgs)
        end
    end

    -- 弹框
    local data = nil
    -- 只给两次机会
    if msg.syncCount >= 3 then
        data = {
            content = Localization.NetReloginFailure,
            btnRightTitle = Localization.BtnToLogin,
            btnRightFunc = BackToLogin
        }
    else
        data = {
            content = Localization.NetTimeOut,
            btnLeftTitle = Localization.BtnToLogin,
            btnRightTitle = Localization.BtnToWait,
            btnLeftFunc = BackToLogin,
            btnRightFunc = toReHttp
        }
    end
    -- 弹出二级弹框
    UIManager.OpenController(UIConfig.ControllerName.NetErrorPopup, data)

    return true
end

-- http请求失败
local function HttpSyncError(index, error)
    local msg = httpMsg[index]
    if nil == msg then
        return
    end
    -- 无需提示！！！
    if not msg.isErrorTip then
        httpMsg[index] = nil
        return
    end

    -- 记录超时次数
    if error == CSharp.HttpErrorMsg.RequstTimeOut then
        msg.timeOutCount = msg.timeOutCount + 1

        -- 判断为奇数次超时
        if math.fmod(msg.timeOutCount, 2) ~= 0 then
            if nil == msg.fromArgs then
                HttpHandle.Get(msg.urlArgs)
            else
                HttpHandle.Post(msg.urlArgs, msg.fromArgs)
            end
            return
        end
    end

    -- 递减！！
    if msg.isWaitSync then
        HttpCountDec()
    end

    -- 检测是否需要弹框，进行持续性等待！！！
    if CheckStillWait(index) then
        return
    end

    local desc = ""
    if error == CSharp.HttpErrorMsg.Unreachable then
        -- 网络不可达
        desc = Localization.NetUnreachable
    elseif error == CSharp.HttpErrorMsg.RequstTimeOut then
        -- 连接超时
        desc = Localization.NetTimeOut
    elseif error == CSharp.HttpErrorMsg.ReceiveZero then
        -- 接收错误
        desc = Localization.ReceiveZero
    elseif error == CSharp.HttpErrorMsg.ReceiveUndone then
        -- 接收未完成
        desc = Localization.ReceiveZero
    else
        -- 其他异常
        desc = Localization.NetError
    end

    -- 网络异常
    if nil == msg or not msg.isErrorPopup then
        UIManager.ShowTip(desc, false)
    else
        UIManager.OpenController(UIConfig.ControllerName.Popup, {content = desc})
    end

    httpMsg[index] = nil
end

-- http请求成功
local function HttpSyncSucceed(index, res)
    local msg = httpMsg[index]
    if nil == msg then
        return
    end
    -- 是否为页面错误
    if nil ~= tonumber(Utils.stringSub_1(res, 0, 3)) then
        HttpSyncError(index, Localization.ReceiveZero)
        return
    end

    if msg.isWaitSync then
        HttpCountDec()
    end
    msg.onSucceed(res)
    httpMsg[index] = nil
end

-- http图片请求成功
local function HttpTextureSyncSucceed(index, res)
    local msg = httpMsg[index]
    if nil == msg then
        return
    end
    -- 是否为页面错误
    if nil ~= tonumber(Utils.stringSub_1(res, 0, 3)) then
        HttpSyncError(index, Localization.ReceiveZero)
        return
    end

    -- 解析
    local parse = function(e, tex)
        if "" ~= e then
            HttpSyncError(index, e)
            return
        end
        if msg.isWaitSync then
            HttpCountDec()
        end
        msg.onSucceed(tex)
        httpMsg[index] = nil
    end
    CSharp.Utils.TextureParse(msg.width, msg.height, res, parse)
end

-- 新增http消息
local function AddHttpMsg(url, from, succeedCallBack, stillWait, errorPopup, isWaitSync, isErrorTip, width, height)
    -- 无效的远端地址
    if nil == url or "" == url then
        UIManager.ShowTip(Localization.InvaildUrl, false)
        return false
    end

    stillWait = nil == stillWait and false or stillWait
    errorPopup = nil == errorPopup and false or errorPopup
    isErrorTip = nil == isErrorTip and true or isErrorTip
    isWaitSync = nil == isWaitSync and true or isWaitSync
    height = nil == height and 0 or height
    width = nil == width and 0 or width

    -- 此处的拼接为约定！！
    local index = nil ~= from and url .. "?" .. from or url
    if nil == httpMsg[index] then
        httpMsg[index] = {
            onSucceed = succeedCallBack,
            isWait = stillWait,
            isErrorPopup = errorPopup,
            isErrorTip = isErrorTip,
            isWaitSync = isWaitSync,
            width = width,
            height = height,
            urlArgs = url,
            fromArgs = from,
            syncCount = 1,
            timeOutCount = 0
        }
    else
        httpMsg[index].syncCount = httpMsg[index].syncCount + 1
    end

    -- 需要菊花同步，且为偶数次超时
    if httpMsg[index].isWaitSync and math.fmod(httpMsg[index].timeOutCount, 2) == 0 then
        HttpCountAsc()
    end

    return true
end

-- Get
-- url:远端地址
-- succeedCallBack:成功回调
-- stillWait:是否一直等待
-- errorPopup:是否错误时弹框
-- isWaitSync:是否需要菊花
-- isErrorTip:是否需要错误时飘字
function HttpHandle.Get(url, succeedCallBack, stillWait, errorPopup, isWaitSync, isErrorTip)
    print("http请求：", url)
    if AddHttpMsg(url, nil, succeedCallBack, stillWait, errorPopup, isWaitSync, isErrorTip) then
        CSharp.Http.Get(url, HttpSyncSucceed, HttpSyncError)
    end
end

-- Post
function HttpHandle.Post(url, from, succeedCallBack, stillWait, errorPopup, isWaitSync, isErrorTip)
    print("http请求：", url, from)
    if AddHttpMsg(url, from, succeedCallBack, stillWait, errorPopup, isWaitSync, isErrorTip) then
        CSharp.Http.Post(url, from, HttpSyncSucceed, HttpSyncError)
    end
end

-- GetTextrue
function HttpHandle.GetTextrue(url, succeedCallBack, stillWait, errorPopup, isWaitSync, width, height)
    print("http请求：", url)
    if AddHttpMsg(url, nil, succeedCallBack, stillWait, errorPopup, isWaitSync, true, width, height) then
        CSharp.Http.Get(url, HttpTextureSyncSucceed, HttpSyncError)
    end
end

-- 初始化
function HttpHandle.Initialize()
    httpCount = 0
    httpMsg = {}
end

-- httpClear
function HttpHandle.Destroy()
    httpCount = 0
    httpMsg = {}
    CSharp.Http.CustomDestroy()
    UIManager.WaitSync(Define.SyncType.HttpSync, false)
end

return HttpHandle
