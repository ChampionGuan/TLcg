local timeInsId = 0

-- 短途计时器
local function ShortTime(cdMax, action)
    local t = {}
    -- 实例化id
    t.InsId = 0
    -- 最大cd
    t.CdMax = 0
    -- 到站时间
    t.ArriveTime = 0
    -- 到站事件
    t.ArriveEvent = nil
    -- 中途更改到站
    function t:init(cdMax, action)
        timeInsId = timeInsId + 1
        self.InsId = timeInsId
        self.CdMax = cdMax
        self.ArriveEvent = action
        self.ArriveTime = TimerManager.curServerTimestamp + cdMax
    end
    -- 中途更改到站
    function t:addCd(cdAdd)
        self.CdMax = self.CdMax + cdAdd
        self.ArriveTime = self.ArriveTime + cdAdd
    end
    -- 中途更改到站
    function t:update()
        if nil == self.ArriveEvent then
            return
        end
        if TimerManager.curServerTimestamp > self.ArriveTime then
            self.ArriveEvent()
        end
    end
    -- 废弃
    function t:dispose()
        self.ArriveEvent = nil
        return nil
    end
    -- 是否已过站
    function t:missArrive()
        if nil == self.ArriveEvent then
            return true
        end
        if TimerManager.curServerTimestamp > self.ArriveTime then
            return true
        end
        return false
    end

    if cdMax ~= nil and action ~= nil then
        t:init(cdMax, action)
    end

    return t
end

-- 公车计时类（名字就是这么特殊）--
local BusTimer = {}
local timerUsed = {}
local timerInUse = {}

function BusTimer.addEvent(cdMax, action)
    local t = table.remove(timerUsed, 1)
    if nil ~= t then
        t:init(cdMax, action)
    else
        t = ShortTime(cdMax, action)
    end
    table.insert(timerInUse, t)
    return t
end

function BusTimer.update()
    if #timerInUse <= 0 then
        return
    end
    -- 遍历
    for i = #timerInUse, 1, -1 do
        timerInUse[i]:update()
        -- 是否已经过站
        if timerInUse[i]:missArrive() then
            local t = table.remove(timerInUse, i)
            table.insert(timerUsed, t)
        end
    end
end

return BusTimer
