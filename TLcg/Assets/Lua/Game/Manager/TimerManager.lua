TimerManager = LuaHandle.Load("Game.Manager.IManager")()

TimerManager.deltaTime = 0
TimerManager.fixedDeltaTime = 0

-- 当前时间戳(服务器给)
TimerManager.CurTimestamp = 0
-- 客户端时区（客户端计算）
TimerManager.CurTimeZone = 0
-- 客户端与服务器时区差
TimerManager.TimeZoneDiff = 0

-- 计时器中心
local TimerCenter = nil

-- 初始化
function TimerManager.Initialize()
    if nil == TimerCenter then
        TimerCenter = LuaHandle.Load("Game.Timer.Timer")()
    end

    local ostime = os.time()
    TimerManager.CurTimestamp = ostime
    TimerManager.CurTimeZone = os.difftime(ostime, os.time(os.date("!*t", ostime))) -- “!*t” 得到的是一个 UTC 时间
    TimerManager.TimeZoneDiff = 0
end

-- 更新
function TimerManager.CustomUpdate()
    TimerManager.deltaTime = CSharp.Time.deltaTime
    TimerCenter:Update()
end

-- 固定更新
function TimerManager.CustomFixedUpdate()
    TimerManager.CurTimestamp = TimerManager.CurTimestamp + TimerManager.fixedDeltaTime
    TimerCenter:FixedUpdate()
end

-- 销毁
function TimerManager.CustomDestroy()
    TimerManager.DisposeAllTimer()
end

-- 新计时器
-- cdMax:计时值
-- ignoreTimescale:忽略timescale
-- isCycle:循环使用，到时重置
-- funcStart:计时开始回调
-- funcUpdate:计时进行回调
-- funcComplete:计时结束回调
-- host:计时器宿主
function TimerManager.NewTimer(cdMax, ignoreTimescale, isCycle, funcStart, funcUpdate, funcComplete, host)
    return TimerCenter:New(cdMax, ignoreTimescale, isCycle, funcStart, funcUpdate, funcComplete, host)
end

-- 计时器加时
function TimerManager.AddCd(insId, cdAdd)
    return TimerCenter:AddCd(insId, cdAdd)
end

-- 计时器销毁
function TimerManager.DisposeTimer(insId)
    return TimerCenter:DisposeTimer(insId)
end

-- 销毁所有计时器
function TimerManager.DisposeAllTimer()
    return TimerCenter:DisposeAll()
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
