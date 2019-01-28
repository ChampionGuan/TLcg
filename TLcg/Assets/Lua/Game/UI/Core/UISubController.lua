-----------------------------------------------------
-------------------定义SubController-----------------
-----------------------------------------------------
local uiCenter = nil

-- 实例化
local function UISubController(ctrlName, viewName)
    if nil == uiCenter then
        uiCenter = LuaHandle.Load("Game.UI.Core.UICenter")
    end

    local t = (LuaHandle.Load("Game.UI.Core.UIController"))(ctrlName, viewName)
    -- 界面引用
    t.SubView = nil
    -- 父ctrl
    t.ParentCtrl = nil
    -- 创建界面--
    t.Creat = function(self, parentCtrl)
        self.ParentCtrl = parentCtrl
        self.View = parentCtrl.View
        if nil ~= self.SubView and not Utils.uITargetIsNil(self.SubView.UI) then
            return
        end
        if self.ViewName ~= nil then
            self.SubView = LuaHandle.Load(self.ViewName)
            self.SubView:Creat()
        else
            if nil ~= parentCtrl.SubView then
                self.SubView = parentCtrl.SubView
            else
                self.SubView = parentCtrl.View
            end
        end
        if nil == self.Timers then
            self.Timers = uiTimer()
        end

        self:OnCreat()
        -- 子ctrl创建
        for k, v in pairs(self.SubCtrl) do
            v:Creat(self)
        end
    end
    -- 重建界面--
    t.ReCreat = function(self)
        if self.ViewName ~= nil and (nil == self.SubView or Utils.uITargetIsNil(self.SubView.UI)) then
            self:Creat(self.ParentCtrl)
            self:Open(self.ParentCtrl, self.Data)
        end
    end
    -- 打开界面--
    t.Open = function(self, parentCtrl, data)
        self.Data = data
        self.ParentCtrl = parentCtrl
        self:ReCreat()

        -- 子ctrl打开
        for k, v in pairs(self.SubCtrl) do
            v:Open(self, data)
        end
        self.IsOpen = true
    end
    -- 关闭界面--
    t.Close = function(self, parentCtrl)
        if parentCtrl ~= self.ParentCtrl then
            return
        end
        self.ParentCtrl = parentCtrl
        self.IsOpen = false

        -- 子ctrl关闭
        for k, v in pairs(self.SubCtrl) do
            v:Close(self)
        end
    end
    -- 显示界面--
    t.Show = function(self, parentCtrl)
        -- 如果被销毁则重新创建
        self.ParentCtrl = parentCtrl
        self:ReCreat()

        -- 子ctrl显示
        for k, v in pairs(self.SubCtrl) do
            v:Show(self)
            v:SetParent(self.SubView.UI)
        end

        if not self:IsDispose() then
            self.SubView:Show()
        end

        self.IsOpen = true
        self.IsShow = true
    end
    -- 隐藏界面--
    t.Hide = function(self, parentCtrl)
        if parentCtrl ~= self.ParentCtrl then
            return
        end
        self.ParentCtrl = parentCtrl
        self.IsShow = false

        -- 子ctrl隐藏
        for k, v in pairs(self.SubCtrl) do
            v:Hide(self)
        end
        if not self:IsDispose() then
            self.SubView:Hide()
        end
    end
    -- 通知界面是否可交互--
    t.SetInteractive = function(self, parentCtrl, isok)
        self.ParentCtrl = parentCtrl

        -- 子ctrl交互
        for k, v in pairs(self.SubCtrl) do
            v:SetInteractive(self, isok)
        end
        if not self:IsDispose() then
            self.SubView:SetInteractive(isok)
        end
        self:OnInteractive(isok)
    end
    -- 销毁界面--
    t.Destroy = function(self, parentCtrl, deep)
        -- 父对象为空时
        if nil == self.ParentCtrl then
            return
        end

        -- 非强制销毁，且父对象不一致
        if not deep and parentCtrl ~= self.ParentCtrl then
            return
        end

        -- 非强制销毁，且不允许销毁 （比如一些共用的ctrl(聊天缩略框等)）
        if not deep and self.IsCannotDestroy then
            if not self.IsShow then
                self:SetParent(CSharp.GRoot.inst)
                self:Hide(parentCtrl)
            end
            return
        end

        -- 子ctrl销毁
        for k, v in pairs(self.SubCtrl) do
            v:Destroy(self, deep)
        end

        if nil ~= self.SubView and not Utils.uITargetIsNil(self.SubView.UI) then
            self:OnDestroy()
        end
        if not self:IsDispose() then
            self.SubView:Destroy()
        end

        self.SubView = nil
        self.ParentCtrl = nil
        self.IsOpen = false
        self.IsShow = false
        self.Events = nil
        self.Timers:DisposeAll()
        -- 移除ctrl
        uiCenter.RemoveSubController(self)
    end
    -- 是否已销毁--
    t.IsDispose = function(self)
        if nil == self.ViewName or nil == self.SubView or Utils.uITargetIsNil(self.SubView.UI) then
            return true
        else
            return false
        end
    end
    -- 设置父对象
    t.SetParent = function(self, parent, index)
        if nil == parent or self:IsDispose() then
            return
        end

        local sOrder = parent.sortingOrder
        parent.sortingOrder = 0
        if nil ~= index then
            parent:AddChildAt(self.SubView.UI, index)
        else
            parent:AddChild(self.SubView.UI)
        end
        parent.sortingOrder = sOrder

        self.SubView.UI.visible = true
        self.SubView.UI.position = CSharp.Vector3.zero
    end

    uiCenter.RegisterSubController(t)
    return t
end

return UISubController
