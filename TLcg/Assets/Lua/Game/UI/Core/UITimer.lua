local index = 1
local function Timer()
    local t = {}

    function t:Init(cdMax, funcStart, funcUpdate, funcComplete, host)
        index = index + 1
        self.InsId = index
        self.CurCd = 0
        self.CdMax = cdMax
        self.Start = funcStart
        self.Update = funcUpdate
        self.Complete = funcComplete
        self.Host = host
        self.IsIdle = false

        if nil ~= self.Start then
            self.Start(self.Host)
        end
    end

    function t:AddCd(cdAdd)
        self.CdMax = self.CdMax + cdAdd
        self.CurCd = self.CurCd + cdAdd
    end

    function t:Tick(value)
        self.CurCd = self.CurCd - value

        if self.CurCd > 0 then
            if nil ~= self.Update then
                self.Update(self.Host, self.CurCd)
            end
            return
        end

        if nil ~= self.Complete then
            self.Complete(self.Host)
        end
        self:Dispose()
    end

    function t:Dispose()
        self.Start = nil
        self.Update = nil
        self.Complete = nil
        self.Host = nil
        self.IsIdle = true
        return
    end

    return t
end

local function UITimer()
    local t = {}
    t.Idle = {}
    t.UsingNormal = {}
    t.UsingIgnore = {}

    function t:NewTimer(cdMax, ignoreTimeScale, funcStart, funcUpdate, funcComplete, host)
        if cdMax == nil or action == nil then
            return
        end
        local tab = table.remove(self.Idle, 1)
        if nil == tab then
            tab = Timer()
        end
        tab:Init(cdMax, funcStart, funcUpdate, funcComplete, host)
        if ignoreTimeScale then
            self.UsingIgnore[tab.InsId] = tab
            self.UsingNormal[tab.InsId] = nil
        else
            self.UsingNormal[tab.InsId] = tab
            self.UsingIgnore[tab.InsId] = nil
        end
        return tab.InsId
    end

    function t:AddCd(insId, cdAdd)
        local tab = self.UsingNormal[insId]
        if nil == tab then
            tab = self.UsingIgnore[insId]
        end
        if nil == tab then
            return false
        end
        tab:AddCd(cdAdd)
        return true
    end

    function t:DisposeTimer(insId)
        local tab = self.UsingNormal[insId]
        if nil == tab then
            tab = self.UsingIgnore[insId]
        end
        if nil == tab then
            return
        end
        tab:Dispose()
        return
    end

    function t:DisposeAll()
        for _, v in pairs(self.UsingNormal) do
            v:Dispose()
        end
        for _, v in pairs(self.UsingIgnore) do
            v:Dispose()
        end
    end

    function t:Update()
        for _, v in pairs(self.UsingIgnore) do
            v:Tick(TimerManager.fixedDeltaTime)
            if v.IsIdle then
                self.UsingIgnore[v.InsId] = nil
                table.insert(self.Idle, v)
            end
        end
    end

    function t:FixedUpdate()
        for _, v in pairs(self.UsingNormal) do
            v:Tick(TimerManager.deltaTime)
            if v.IsIdle then
                self.UsingNormal[v.InsId] = nil
                table.insert(self.Idle, v)
            end
        end
    end

    return t
end

return UITimer
