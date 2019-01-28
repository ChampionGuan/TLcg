local UISync = {}

-- 菊花面板
local m_view = nil
-- 等待时间
local m_tick = nil
-- 多种菊花
local m_syncTab = {}

function UISync.Show(type, value)
    if value then
        if not UISync.Showing() then
            m_tick = 0.5
        end

        m_syncTab[type] = true
        m_view.UI.visible = true
        m_view.UI.sortingOrder =
            type == Define.SyncType.NetSync and UIDefine.SortingOrder.NetSignal or UIDefine.SortingOrder.Sync
    else
        if not UISync.Showing() then
            return
        end

        if type == Define.SyncType.All then
            m_syncTab = {}
        else
            m_syncTab[type] = false
        end

        if UISync.Showing() then
            m_view.UI.sortingOrder =
                type == Define.SyncType.NetSync and UIDefine.SortingOrder.Sync or UIDefine.SortingOrder.NetSignal
        else
            m_view.ShowStat.selectedIndex = 1
            m_view.UI.visible = false
            m_tick = nil
        end
    end
end

function UISync.Showing()
    for _, v in pairs(m_syncTab) do
        if v then
            return true
        end
    end
    return false
end

function UISync.Clear()
    m_view.ShowStat.selectedIndex = 1
    m_view.UI.visible = false
    m_syncTab = {}
    m_tick = nil
end

function UISync.Initialize()
    m_view = {}
    m_view.UI = UIUtils.SpawnUICom("UI/Sync/Sync", "Sync", "Sync")
    m_view.ShowStat = m_view.UI:GetController("Show_C")
    m_view.ShowStat.selectedIndex = 1
end

function UISync.CustomUpdate()
    if nil == m_tick then
        return
    end
    m_tick = m_tick - TimerManager.deltaTime
    if m_tick < 0 then
        m_view.ShowStat.selectedIndex = 0
        m_tick = nil
    end
end

function UISync.CustomDestroy()
    UIUtils.DisposeUICom("UI/Sync/Sync", m_view.UI)
    m_syncTab = {}
    m_view = nil
end

return UISync
