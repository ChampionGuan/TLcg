--------------------------------------------------------------
------------------------计时器--------------------------------
--------------------------------------------------------------
local timeNomralId = 0
local timeIgnoreId = 0

local function Timer(maxCd, isAutoReset, isIgnoreTimeScale, funcStart, funcUpdate, funcComplete, target, isAscend)
    local t = {}
    -- 实例化Id--
    t.InstanceId = 0

    -- 最大值
    t.MaxCd = 0
    -- 当前值
    t.CurCd = 0
    -- 计时倍率
    t.Speed = 1

    -- 是否自动重置
    t.IsAutoReset = false
    -- 是否忽略timescaler
    t.IsIgnoreTimeScale = false
    -- 是否开始
    t.IsStart = false
    -- 是否暂停
    t.IsPause = false
    -- 计时对象--
    t.Target = nil
    -- 是否为正计时，默认为倒计时
    t.IsAscend = false

    -- 开始
    t.onStart = nil
    -- 进行中
    t.onUpdate = nil
    -- 结束
    t.onComplete = nil

    function t:init(maxCd, isAutoReset, isIgnoreTimeScale, funcStart, funcUpdate, funcComplete, target, isAscend)
        if isIgnoreTimeScale then
            timeIgnoreId = timeIgnoreId + 1
            self.InstanceId = timeIgnoreId
        else
            timeNomralId = timeNomralId + 1
            self.InstanceId = timeNomralId
        end

        self.CurCd = 0
        self.Speed = 1
        self.MaxCd = maxCd
        self.IsAutoReset = isAutoReset
        self.IsIgnoreTimeScale = isIgnoreTimeScale
        self.IsStart = false
        self.IsPause = true
        self.onStart = funcStart
        self.onUpdate = funcUpdate
        self.onComplete = funcComplete
        self.Target = target
        self.IsAscend = isAscend or false
    end

    -- 开始计时
    -- <param name="startTime" type="number">可以指定起始时间，否则以默认的时间开始计时</param>
    function t:start(startTime)
        -- 倒计时
        if not self.IsAscend then
            -- 正计时
            -- 为数字且大于等于0
            if type(startTime) == "number" and startTime >= 0 then
                self.CurCd = startTime
            else
                self.CurCd = self.MaxCd
            end
        else
            -- 为数字且小于等于MaxCd
            if type(startTime) == "number" and startTime <= self.MaxCd then
                self.CurCd = startTime
            else
                self.CurCd = 0
            end
        end

        if nil ~= self.onStart then
            self.onStart(self.Target)
        end
        self.IsStart = true
        self.IsPause = false
    end

    function t:addCd(f)
        self.CurCd = self.CurCd + f
        self.MaxCd = self.MaxCd + f
    end

    -- 重置
    -- <param name="isSkipComplete" type="bool">重置时是否跳过调用Complete的回调方法，默认跳过</param>
    function t:reset(isSkipComplete)
        if nil ~= self.onComplete and isSkipComplete ~= nil and not isSkipComplete then
            self.onComplete(self.Target)
        end

        -- 正序计时，从0开始；倒序，从max开始
        if self.IsAscend == true then
            self.CurCd = 0
        else
            self.CurCd = self.MaxCd
        end
    end
    -- 重置MaxCD
    -- <param name="count" type="number">重置最大cd</param>
    function t:resetMax(count)
        self.MaxCd = count
        self:reset()
    end

    function t:setSpeed(speed)
        self.Speed = speed
    end

    function t:pause()
        self.IsPause = true
    end

    function t:resume()
        self.IsPause = false
    end

    function t:update(f)
        if not self.IsStart or self.IsPause then
            return
        end

        f = f * self.Speed

        -- 正序计时
        if self.IsAscend == true then
            self.CurCd = self.CurCd + f
        else
            self.CurCd = self.CurCd - f
        end

        if nil ~= self.onUpdate then
            self.onUpdate(self.Target, self.CurCd)
        end

        -- 计时是否结束
        if self.IsAscend == true then
            if self.CurCd < self.MaxCd then
                return
            end
        else
            if self.CurCd > 0 then
                return
            end
        end

        self.IsStart = false
        if nil ~= self.onComplete then
            self.onComplete(self.Target)
        end

        if self.IsAutoReset then
            self:reset()
            self:start()
        end
    end

    function t:recycle()
        self.IsAutoReset = false
        self.IsIgnoreTimeScale = false
        self.IsStart = false
        self.IsPause = false
        self.IsAscend = false
        self.Target = nil
        self.onStart = nil
        self.onUpdate = nil
        self.onComplete = nil
    end

    t:init(maxCd, isAutoReset, isIgnoreTimeScale, funcStart, funcUpdate, funcComplete, target, isAscend)

    return t
end

return Timer
