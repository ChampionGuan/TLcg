local UISync = {}

-- 菊花面板
local view = nil
-- 等待时间
local tick = nil
-- 多种菊花
local syncTab = {}

function UISync.Show(type, value)
    if value then
        if not UISync.Showing() then
            tick = 0.5
        end

        syncTab[type] = true
        view.UI.visible = true
        view.UI.sortingOrder =
            type == Define.SyncType.NetSync and UIDefine.SortingOrder.NetSignal or UIDefine.SortingOrder.Sync
    else
        if not UISync.Showing() then
            return
        end

        if type == Define.SyncType.All then
            syncTab = {}
        else
            syncTab[type] = false
        end

        if UISync.Showing() then
            view.UI.sortingOrder =
                type == Define.SyncType.NetSync and UIDefine.SortingOrder.Sync or UIDefine.SortingOrder.NetSignal
        else
            view.ShowStat.selectedIndex = 1
            view.UI.visible = false
            tick = nil
        end
    end
end

function UISync.Showing()
    for _, v in pairs(syncTab) do
        if v then
            return true
        end
    end
    return false
end

function UISync.Clear()
    view.ShowStat.selectedIndex = 1
    view.UI.visible = false
    syncTab = {}
    tick = nil
end

function UISync.Initialize()
    view = {}
    view.UI = UIUtils.SpawnUICom("UI/Sync/Sync", "Sync", "Sync")
    view.ShowStat = view.UI:GetController("Show_C")
    view.ShowStat.selectedIndex = 1
end

function UISync.CustomUpdate()
    if nil == tick then
        return
    end
    tick = tick - TimerManager.deltaTime
    if tick < 0 then
        view.ShowStat.selectedIndex = 0
        tick = nil
    end
end

function UISync.CustomDestroy()
    UIUtils.DisposeUICom("UI/Sync/Sync", view.UI)
    syncTab = {}
    view = nil
end

return UISync
