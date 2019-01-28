﻿local index = 1
local function Timer()
    local t = {}

    function t:Init(cdMax, isCycle, funcStart, funcUpdate, funcComplete, host)
        index = index + 1
        self.InsId = index
        self.IsCycle = isCycle or false
        self.CurCd = cdMax
        self.MaxCd = cdMax
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
        self.MaxCd = self.MaxCd + cdAdd
        self.CurCd = self.CurCd + cdAdd
    end

    function t:Tick(value)
        self.CurCd = self.CurCd - value

        if self.CurCd >= 0 then
            if nil ~= self.Update then
                self.Update(self.Host, self.CurCd, self.MaxCd)
            end
            return
        end

        if nil ~= self.Complete then
            self.Complete(self.Host)
        end

        if not self.IsCycle then
            self:Dispose()
        else
            self.CurCd = cdMax
            self.MaxCd = cdMax
        end
    end

    function t:Dispose()
        self.Start = nil
        self.Update = nil
        self.Complete = nil
        self.Host = nil
        self.IsCycle = false
        self.IsIdle = true
        return
    end

    return t
end

local function TimerCenter()
    local t = {}
    t.Idle = {}
    t.UsingNormal = {}
    t.UsingIgnore = {}
    t.CurrRealtime = 0
    t.LastRealtime = CSharp.Time.realtimeSinceStartup

    function t:New(cdMax, ignoreTimescale, isCycle, funcStart, funcUpdate, funcComplete, host)
        if cdMax == nil then
            return
        end

        local tab = table.remove(self.Idle, 1)
        if nil == tab then
            tab = Timer()
        end
        tab:Init(cdMax, isCycle, funcStart, funcUpdate, funcComplete, host)
        if ignoreTimescale then
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
        self.CurrRealtime = CSharp.Time.realtimeSinceStartup
        local deltaTime = self.CurrRealtime - self.LastRealtime
        self.LastRealtime = self.CurrRealtime

        for _, v in pairs(self.UsingIgnore) do
            v:Tick(deltaTime)
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

return TimerCenter
