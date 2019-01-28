-- 消息池
local MsgsCenter = {}
-- 消息队列
MsgsCenter.Queue = {}
-- 出
function MsgsCenter.Dequeue()
    if #MsgsCenter.Queue > 0 then
        return table.remove(MsgsCenter.Queue, 1)
    end
    return nil
end
-- 进
function MsgsCenter.Enqueue(msg)
    table.insert(MsgsCenter.Queue, msg)
end

-- 飘字池
local TipsCenter = {}
-- 空闲
TipsCenter.Idle = {}
-- 使用
TipsCenter.Using = {}
-- index
TipsCenter.Index = 1
-- 获取
function TipsCenter.Get()
    TipsCenter.Index = TipsCenter.Index + 1

    local tip = nil
    if #TipsCenter.Idle > 0 then
        tip = table.remove(TipsCenter.Idle, 1)
    else
        local ui = UIUtils.SpawnUICom("UI/Tips/Tips", "Tips", "Tip")
        ui.touchable = false
        tip = {}
        tip.Root = ui
        tip.Effect = ui:GetTransition("T_0")
        tip.Type = ui:GetController("Type_C")
        tip.Content = ui:GetChild("Text_Content")
    end
    tip.Id = TipsCenter.Index
    tip.Root.visible = true
    tip.Root.sortingOrder = UIDefine.SortingOrder.TipsMsg
    TipsCenter.Using[tip.Id] = tip
    return tip
end

-- 回池
function TipsCenter.Back(tip)
    tip.Root.sortingOrder = UIDefine.SortingOrder.TipsMsg - 1
    tip.Root.visible = false
    TipsCenter.Using[tip.Id] = nil
    table.insert(TipsCenter.Idle, tip)
end

-- 飘字
local UITips = {}
-- 计时中(计时过程中不会再出现飘字))
local m_tick = nil
-- 最后一条tip
local m_lastTip = nil

-- content 表示显示文本内容
-- type 表示显示类型，true为成功，false为警示
function UITips.Show(content, type)
    if m_tick ~= nil then
        if m_lastTip == content then
            return
        end
        MsgsCenter.Enqueue({content, type})
    else
        m_lastTip = content
        m_tick = 1

        local tip = TipsCenter.Get()
        tip.Content.text = content
        tip.Type.selectedIndex = type and 0 or 1
        tip.Effect:Play(
            function()
                TipsCenter.Back(tip)
            end
        )
    end
end

function UITips.Clear()
    for _, v in pairs(TipsCenter.Using) do
        v.Effect:Stop()
    end
    m_tick = nil
    TipsCenter.Using = {}
    MsgsCenter.Queue = {}
end

function UITips.CustomDestroy()
    for _, v in pairs(TipsCenter.Using) do
        v.Root:Dispose()
    end
    for _, v in pairs(TipsCenter.Idle) do
        v.Root:Dispose()
    end
    TipsCenter.Idle = {}
    UITips.Clear()
end

function UITips.CustomUpdate()
    if nil == m_tick then
        return
    end
    m_tick = m_tick - TimerManager.deltaTime
    if m_tick < 0 then
        m_tick = nil
        local msg = MsgsCenter.Dequeue()
        if nil ~= msg then
            UITips.Show(msg[1], msg[2])
        end
    end
end

function UITips.Initialize()
end

return UITips
