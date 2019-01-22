TimerManager = LuaHandle.Load("Game.Manager.IManager")()

TimerManager.deltaTime = 0
TimerManager.fixedDeltaTime = 0

-- 当前时间戳(服务器给)
TimerManager.CurTimestamp = 0
-- 客户端时区（客户端计算）
TimerManager.CurTimeZone = 0
-- 客户端与服务器时区差
TimerManager.TimeZoneDiff = 0

-- 初始化
function TimerManager.Initialize()
    local ostime = os.time()
    TimerManager.CurTimestamp = ostime
    TimerManager.CurTimeZone = os.difftime(ostime, os.time(os.date("!*t", ostime))) -- “!*t” 得到的是一个 UTC 时间
    TimerManager.TimeZoneDiff = 0
    TimerManager.fixedDeltaTime = CSharp.Time.fixedDeltaTime
end

-- 更新
function TimerManager.CustomUpdate()
    TimerManager.deltaTime = CSharp.Time.deltaTime
end

-- 固定更新
function TimerManager.CustomFixedUpdate()
    TimerManager.CurTimestamp = TimerManager.CurTimestamp + TimerManager.fixedDeltaTime
end

-- 销毁
function TimerManager.CustomDestroy()
end
