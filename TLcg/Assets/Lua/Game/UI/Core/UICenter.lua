local UICenter = {}
-- 当界面显示时
UICenter.OnCtrlShow = nil
-- 当界面隐藏时
UICenter.OnCtrlHide = nil
-- 当界面打开时
UICenter.OnCtrlOpen = nil
-- 当界面打开时
UICenter.OnCtrlClose = nil

-- 本地存储--
local ControllerCenter = {}
local ControllerStack = {}
local SubControllerCenter = {}

local function PrintCtrlName()
    for k, v in pairs(ControllerStack) do
        print(v.ControllerName, v.Type, "in stack !!!")
    end
end
-- 当界面显示时
local function OnCtrlShow(ctrl)
    if nil ~= UICenter.OnCtrlShow then
        UICenter.OnCtrlShow(ctrl)
    end
end
-- 当界面隐藏时
local function OnCtrlHide(ctrl)
    if nil ~= UICenter.OnCtrlHide then
        UICenter.OnCtrlHide(ctrl)
    end
end
-- 当界面打开时
local function OnCtrlOpen(ctrl)
    if nil ~= UICenter.OnCtrlOpen then
        UICenter.OnCtrlOpen(ctrl)
    end
end
-- 当界面关闭时
local function OnCtrlClose(ctrl)
    if nil ~= UICenter.OnCtrlClose then
        UICenter.OnCtrlClose(ctrl)
    end
end
-- 比较两ctrl的渲染顺序
local function CompareBothCtrlSortingOrder(ctrlA, ctrlB)
    if
        (nil ~= ctrlA.SortingOrder and nil == ctrlB.SortingOrder) or
            (nil ~= ctrlA.SortingOrder and nil ~= ctrlB.SortingOrder and ctrlA.SortingOrder > ctrlB.SortingOrder)
     then
        return true
    end
    return false
end

-- 栈中ctrl排序
local function SortCtrlStack()
    -- 冒泡
    for m = 1, #ControllerStack do
        for n = m + 1, #ControllerStack do
            -- 两ctrl渲染顺序比较
            if CompareBothCtrlSortingOrder(ControllerStack[m], ControllerStack[n]) then
                -- 交换位置
                local temp = ControllerStack[m]
                ControllerStack[m] = ControllerStack[n]
                ControllerStack[n] = temp
            end
        end
    end
end
-- 关闭栈中之前界面--
local function HidePreCtrl(id)
    if id <= 0 then
        return
    end
    local ctrl = ControllerStack[id]
    if nil == ctrl then
        return
    end
    ctrl:Hide()
    OnCtrlHide(ctrl)

    if ctrl.Type == UIDefine.CtrlType.PopupBox then
        id = id - 1
        HidePreCtrl(id)
    end
end
-- 打开栈中之前界面--
local function ShowPreCtrl(id)
    if id <= 0 then
        return
    end
    local ctrl = ControllerStack[id]
    if nil == ctrl then
        return
    end
    ctrl:Show()
    ctrl:SetSortingOrder(id * 10)

    if ctrl.Type == UIDefine.CtrlType.PopupBox then
        id = id - 1
        ShowPreCtrl(id)
    end
end
-- 交互栈中之前界面--
local function InteractivePreCtrl(id)
    if id <= 0 then
        return
    end
    local ctrl = ControllerStack[id]
    if nil == ctrl then
        return
    end
    ctrl:SetInteractive(true)

    if ctrl.PreCtrlInteractive then
        id = id - 1
        InteractivePreCtrl(id)
    end
end
-- 模糊制定界面--
local function BlurPreCtrl(id)
    local ctrl = ControllerStack[id]
    if nil == ctrl then
        return
    end
    if ctrl.IsShow then
        ctrl:SetBlur(true)
    end
    BlurPreCtrl(id + 1)
end
-- 从栈中移除指定界面--
local function RemoveFromStack(ctrl)
    local removeSucceed = false
    for i = (#(ControllerStack)), 1, -1 do
        if ControllerStack[i] == nil or ControllerStack[i].ControllerName == ctrl.ControllerName then
            removeSucceed = true
            table.remove(ControllerStack, i)
            break
        end
    end

    return removeSucceed
end
-- 获取打开的ctrl
local function GetOpenCtrl(id, add)
    local ctrl = ControllerStack[id]
    if nil == ctrl or ctrl.IsOpen then
        return ctrl, id
    else
        return GetOpenCtrl(id + add, add)
    end
end
-- 将界面压栈--
function UICenter.PushingStack(ctrl)
    -- 隐藏之前面板
    local ctrlNum = #ControllerStack
    if ctrl.Type ~= UIDefine.CtrlType.PopupBox then
        HidePreCtrl(ctrlNum)
    end

    -- 新面板加入栈中
    RemoveFromStack(ctrl)
    table.insert(ControllerStack, ctrl)
    SortCtrlStack()

    ctrlNum = #ControllerStack

    -- 重置交互性和渲染顺序
    for k, v in pairs(ControllerStack) do
        v:SetInteractive(false)
        v:SetSortingOrder(k * 10)
    end
    -- 重新设置交互性
    InteractivePreCtrl(ctrlNum)

    -- 界面打开
    local theLastCtrl = ControllerStack[ctrlNum]
    OnCtrlShow(theLastCtrl)

    -- 处理最后一个面板
    if theLastCtrl.ControllerName == ctrl.ControllerName then
        ctrl:Show()
    else
        ShowPreCtrl(ctrlNum)
    end
end
-- 将界面出栈--
function UICenter.PopingStack(ctrl)
    -- 关闭自身
    ctrl:Hide()
    ctrl:SetInteractive(false)
    OnCtrlHide(ctrl)

    -- 判断已无面板
    local ctrlNum = (#ControllerStack)
    if ctrlNum <= 0 then
        return
    end

    -- 未移除成功
    if not RemoveFromStack(ctrl) then
        return
    end

    -- 栈顶面板
    local theLastCtrl = nil
    theLastCtrl, ctrlNum = UICenter.GetTopCtrl()
    if nil == theLastCtrl then
        return
    end

    -- 重置交互性（在打开面板之前）
    InteractivePreCtrl(ctrlNum)
    -- 打开之前面板
    if ctrl.Type ~= UIDefine.CtrlType.PopupBox then
        ShowPreCtrl(ctrlNum)
    end
    -- 界面显示
    OnCtrlShow(theLastCtrl)
    -- 界面关闭
    OnCtrlClose(ctrl)
    -- 背景模糊
    BlurPreCtrl(1)
end
-- 打开指定界面--
function UICenter.OpenController(name, data)
    local ctrl = UICenter.GetController(name)
    if ctrl == nil then
        LuaHandle.Load(name)
        ctrl = UICenter.GetController(name)
    end
    -- 存在预处理逻辑，默认都是预处理成功
    if ctrl:PreHandle(data) then
        ctrl:Creat()
        ctrl:Open(data)
        OnCtrlOpen(ctrl)
    end
end
-- 向所有打开界面广播消息--
function UICenter.DispatchEvent(type, ...)
    for k, v in pairs(ControllerStack) do
        if v ~= nil and v.IsOpen then
            v:DispatchEvent(type, ...)
        end
    end
end
-- 注册界面--
function UICenter.RegisterController(ctrl)
    if nil == ctrl.ControllerName then
        error("ctrl cannot have no name !!!")
    end
    ControllerCenter[ctrl.ControllerName] = ctrl
end
-- 移除界面--
function UICenter.RemoveController(ctrl)
    if nil == ctrl then
        return
    end
    -- ControllerCenter[ctrl.ControllerName] = nil
end
-- 注册sub界面--
function UICenter.RegisterSubController(ctrl)
    if nil == ctrl.ControllerName then
        error("ctrl cannot have no name !!!")
    end
    SubControllerCenter[ctrl.ControllerName] = ctrl
end
-- 移除sub界面--
function UICenter.RemoveSubController(ctrl)
    if nil == ctrl then
        return
    end
    -- SubControllerCenter[ctrl.ControllerName] = nil
end
-- 获取顶部界面--
function UICenter.GetTopCtrl()
    return GetOpenCtrl(#ControllerStack, -1)
end
-- 获取底部界面--
function UICenter.GetBottomCtrl()
    return GetOpenCtrl(1, 1)
end
-- 获取指定界面--
function UICenter.GetController(name)
    if name == nil then
        return nil
    end

    return ControllerCenter[name]
end
-- 获取指定界面--
function UICenter.GetSubController(name)
    if name == nil then
        return nil
    end

    return SubControllerCenter[name]
end
-- 获取顶部的非弹框界面--
function UICenter.GetTopCtrl2NotPopup()
    local ctrl = nil
    for i = #ControllerStack, 1, -1 do
        if ControllerStack[i].Type ~= UIDefine.CtrlType.PopupBox then
            ctrl = ControllerStack[i]
            break
        end
    end
    return ctrl
end
-- 获取顶部的全屏界面--
function UICenter.GetTopFullScreenCtrl()
    local ctrl = nil
    for i = #ControllerStack, 1, -1 do
        if ControllerStack[i].Type == UIDefine.CtrlType.FullScreen then
            ctrl = ControllerStack[i]
            break
        end
    end
    return ctrl
end
-- 关闭指定界面--
function UICenter.DestroyCtrl(name)
    local ctrl = ControllerCenter[name]
    if nil ~= ctrl then
        ctrl:DestroyBySelf()
    end
end
-- 关闭所有的界面--
function UICenter.DestroyAllCtrl(deep)
    if nil == deep then
        deep = false
    end
    if deep then
        ControllerStack = {}
    end
    for k, v in pairs(ControllerCenter) do
        if not v.IsCannotDestroy or deep then
            v:DestroyByOther(deep)
        end
    end
end
-- 更新所有打开的界面--
function UICenter.CustomUpdate()
    for k, v in pairs(ControllerCenter) do
        v:Update()
    end
    for k, v in pairs(SubControllerCenter) do
        v:Update()
    end
end
-- 更新所有打开的界面--
function UICenter.CustomFixedUpdate()
    for k, v in pairs(ControllerCenter) do
        v:FixedUpdate()
    end
    for k, v in pairs(SubControllerCenter) do
        v:FixedUpdate()
    end
end

-- 刷新所有打开的界面--
function UICenter.RefreshAllCtrl()
    for i = #ControllerStack, 1, -1 do
        if ControllerStack[i].IsOpen and not ControllerStack[i].IsDestroyed then
            ControllerStack[i]:Refresh()
        end
    end
end

return UICenter
