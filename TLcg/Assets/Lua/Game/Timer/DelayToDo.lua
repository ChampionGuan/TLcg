-- 延时类--
local function DelayToDo()
    local DelayToDo = {}
    DelayToDo.Timer = nil
    DelayToDo.Params = nil
    DelayToDo.Target = nil
    DelayToDo.ToDo = nil

    function DelayToDo:recycle()
        self.Timer = TimerManager.disposeTimer(self.Timer)
        self.ToDo = nil
        self.Params = nil
        self.Target = nil
    end

    function DelayToDo:onComplete()
        if self.ToDo ~= nil and type(self.ToDo) == "function" then
            self.ToDo(self.Target, self.Params)
        end

        self:recycle()
    end

    function DelayToDo:start(maxCd, speedRate, isIgnoreTimeScale, toDo, params, target)
        self.Timer = TimerManager.newTimer(0, false, isIgnoreTimeScale, nil, nil, self.onComplete, self)
        self.Timer.Speed = speedRate

        self.ToDo = toDo
        self.Params = params
        self.Target = target

        self.Timer:addCd(maxCd - self.Timer.MaxCd)
        self.Timer:start()
    end

    function DelayToDo:setSpeed(speed)
        if nil ~= self.Timer then
            self.Timer:setSpeed(speed)
        end
    end

    function DelayToDo:pause()
        if nil ~= self.Timer then
            self.Timer:pause()
        end
    end

    function DelayToDo:resume()
        if nil ~= self.Timer then
            self.Timer:resume()
        end
    end

    return DelayToDo
end

-- 延时中心类
local DelayToDoCenter = {}
DelayToDoCenter.pool = {}
-- 新生成
function DelayToDoCenter.newTimer(maxCd, speedRate, isIgnoreTimeScale, func, params, funcHost)
    local t = table.remove(DelayToDoCenter.pool, 1)
    if nil == t then
        t = DelayToDo()
    end
    t:start(maxCd, speedRate, isIgnoreTimeScale, func, params, funcHost)
    return t
end

-- 销毁
function DelayToDoCenter.dispose(delayTodo)
    if nil ~= delayTodo then
        delayTodo:recycle()
        table.insert(DelayToDoCenter.pool, delayTodo)
    end
end

return DelayToDoCenter
