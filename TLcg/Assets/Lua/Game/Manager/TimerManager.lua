TimerManager = LuaHandle.Load("Game.Manager.IManager")()

-- 当前帧时间
local theCurrRealtime = 0
-- 上一帧时间
local theLastRealtime = 0

TimerManager.deltaTime = 0
TimerManager.ignoreDeltaTime = 0
TimerManager.fixedDeltaTime = 0

-- 当前时间戳(服务器给)
TimerManager.CurTimestamp = 0
-- 客户端时区（客户端计算）
TimerManager.CurTimeZone = 0
-- 客户端与服务器时区差
TimerManager.TimeZoneDiff = 0

-- 正常计时--
local function TimerNormalUpdate()
end

-- 忽略timeScaler计时--
local function TimerIgnoreUpdate(f)
end

-- 初始化
function TimerManager.Initialize()
    local ostime = os.time()
    TimerManager.CurTimestamp = ostime
    TimerManager.CurTimeZone = os.difftime(ostime, os.time(os.date("!*t", ostime))) -- “!*t” 得到的是一个 UTC 时间
    TimerManager.TimeZoneDiff = 0
    TimerManager.fixedDeltaTime = CSharp.Time.fixedDeltaTime
    theLastRealtime = CSharp.Time.realtimeSinceStartup
end

-- 更新
function TimerManager.CustomUpdate()
    TimerManager.deltaTime = CSharp.Time.deltaTime
    theCurrRealtime = CSharp.Time.realtimeSinceStartup
    TimerManager.ignoreDeltaTime = theCurrRealtime - theLastRealtime
    theLastRealtime = theCurrRealtime
    TimerIgnoreUpdate(TimerManager.ignoreDeltaTime)
end

-- 固定更新
function TimerManager.CustomFixedUpdate()
    TimerManager.CurTimestamp = TimerManager.CurTimestamp + TimerManager.fixedDeltaTime
    TimerNormalUpdate(TimerManager.fixedDeltaTime)
end

-- 销毁
function TimerManager.CustomDestroy()
end

-- 获取客户端显示，时间戳转换(时:分)
function TimerManager.GetShowTime_HM(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    return os.date("!%H:%M", timestamp + TimerManager.CurTimeZone)
end

-- 获取客户端显示，时间戳转换(时:分:秒)
function TimerManager.GetShowTime_HMS(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    return os.date("!%H:%M:%S", timestamp + TimerManager.CurTimeZone)
end

-- 获取客户端显示，时间戳转换(年-月-日 小时:分钟)
function TimerManager.GetShowTime_YMDHM(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    return os.date("!%Y-%m-%d %H:%M", timestamp + TimerManager.CurTimeZone)
end

-- 获取客户端显示，时间戳转换(年-月-日)
function TimerManager.GetShowTime_YMD(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    return os.date("!%Y-%m-%d", timestamp + TimerManager.CurTimeZone)
end

-- 获取客户端显示，时间戳转换(年-月-日 小时:分钟:秒)
function TimerManager.GetShowTime_YMDHMS(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    return os.date("!%Y-%m-%d %H:%M:%S", timestamp + TimerManager.CurTimeZone)
end

-- 获取客户端显示，时间戳转换自定义
function TimerManager.GetShowTime_T(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end
    -- tab.year, tab.month, tab.day, tab.hour, tab.min
    local tab = os.date("!*t", timestamp + TimerManager.CurTimeZone)
    return tab
end

-- 获取客户端显示，时间戳转周
function TimerManager.GetShowTime_Weekend(timestamp)
    if type(timestamp) ~= "number" or timestamp <= 0 then
        return ""
    end

    return os.date("!%w", timestamp + TimerManager.CurTimeZone)
end
