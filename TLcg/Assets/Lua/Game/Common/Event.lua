-------------------------------------------------------------------------
------------------------------EventListener------------------------------
-------------------------------------------------------------------------

-- 事件类型
EventType = {}

-- socket
EventType.CONNECTING = 1
EventType.CONNECTED = 2
EventType.MUST_RELOGIN = 3
EventType.MUST_RECONNECT = 4
EventType.MUST_CLOSE = 5

-- 关闭网络异常弹框
EventType.CLOSE_NET_ERROR_POPUP = "CLOSE_NET_ERROR_POPUP"
-- 进入场景
EventType.ENTER_SCENCE = "ENTER_SCENCE"
-- 退出场景
EventType.EXIT_SCENCE = "EXIT_SCENCE"

-- 事件分发器
Event = {}
function Event.AddListener(etype, func)
    if etype == nil or func == nil then
        return
    end

    local a = Event[etype]
    if not a then
        a = {}
        Event[etype] = a
    end
    table.insert(a, 1, func)
end

function Event.RemoveListener(etype, func)
    local a = Event[etype]
    if (a == nil) then
        return
    end
    for k, v in pairs(a) do
        if (v == func) then
            a[k] = nil
        end
    end
end

function Event.Dispatch(etype, ...)
    -- print("dispatching event", etype, " thread id ", CS.System.Threading.Thread.CurrentThread.ManagedThreadId)
    local a = Event[etype]
    if not a then
        return
    end
    for k, v in pairs(a) do
        v(...)
    end
end

function Event.Clear(etype)
    local a = Event[etype]
    if not a then
        return
    end
    Event[etype] = nil
end
