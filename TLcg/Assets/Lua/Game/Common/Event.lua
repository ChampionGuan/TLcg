-------------------------------------------------------------------------
------------------------------EventListener------------------------------
-------------------------------------------------------------------------

Event = {}

Event.CONNECTING = 1
Event.CONNECTED = 2
Event.MUST_RELOGIN = 3
Event.MUST_RECONNECT = 4
Event.MUST_CLOSE = 5

-- 网络错误弹框关闭
Event.ERROR_SYNC_CLOSE = 100

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
