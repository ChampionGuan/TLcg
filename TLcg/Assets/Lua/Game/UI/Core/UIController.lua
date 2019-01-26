-----------------------------------------------------
-------------------定义controller--------------------
-----------------------------------------------------
-- 界面销毁配置
local uiDisposeConfig = nil
-- uiCenter
local uiCenter = nil
-- Blur
local blurEffect = nil
-- layer
local uiCullingMask = nil
-- uiTimer
local uiTimer = nil

-- 实例化
local function UIController(ctrlName, viewName, isRegister)
    if nil == uiDisposeConfig then
        uiDisposeConfig = LuaHandle.Load("Game.Config.UIDisposeConfig")
    end
    if nil == uiCenter then
        uiCenter = LuaHandle.Load("Game.UI.Core.UICenter")
    end
    if nil == blurEffect then
        blurEffect = LuaHandle.Load("Game.UI.Core.UIBlur")
    end
    if nil == uiCullingMask then
        uiCullingMask = LuaHandle.Load("Game.UI.Core.UICullingMask")
    end
    if nil == uiTimer then
        uiTimer = LuaHandle.Load("Game.UI.Core.UITimer")
    end

    local t = {}
    -- 外部传入数据
    t.Data = nil
    -- 类型
    t.Type = 1
    -- 渲染顺序
    t.SortingOrder = nil
    -- 交互性
    t.Interactive = true
    -- 模糊(默认为关闭)
    t.PreCtrlBlur = false
    -- 上一个Ctrl的交互性(默认为关闭)
    t.PreCtrlInteractive = false
    -- 不允许被销毁
    t.IsCannotDestroy = false
    -- 是否预处理面板
    t.IsPreHandle = false
    -- 是否打开
    t.IsOpen = false
    -- 是否显示
    t.IsShow = false
    -- 是否交互
    t.IsInteractive = false
    -- 是否被销毁
    t.IsDestroyed = true
    -- ctrl名称
    t.ControllerName = ctrlName
    -- view名称
    t.ViewName = viewName
    -- view引用
    t.View = nil
    -- 存活时间
    t.AliveTime = 5
    -- 计时器
    t.Timers = nil
    -- 事件
    t.Events = nil
    -- 子ctrl
    t.SubCtrl = {}

    -- 预处理界面--
    t.PreHandle = function(self, data)
        self.IsPreHandle = true
        self.Data = data
        return self:OnPreHandle()
    end
    -- 创建界面--
    t.Creat = function(self)
        if not self.IsDestroyed then
            return
        end
        if nil ~= self.ViewName and nil == self.View then
            self.View = LuaHandle.Load(self.ViewName)
        end
        self.View:Creat()

        self.IsDestroyed = false
        self:OnCreat()

        -- 子ctrl创建
        for k, v in pairs(self.SubCtrl) do
            v:Creat(self)
        end

        if nil == self.Timers then
            self.Timers = uiTimer()
        end

        -- 初始交互性
        self:SetInteractive(self.IsInteractive)

        -- 不允许被销毁
        if uiDisposeConfig[self.ControllerName] == -1 then
            self.IsCannotDestroy = true
        end
    end
    -- 打开界面--
    t.Open = function(self, data, isPushStack)
        -- 如果被销毁
        if self.IsDestroyed then
            self:Creat()
        end

        self.Data = data
        self.AliveTime = uiDisposeConfig[self.ControllerName] or 3
        -- 子ctrl打开
        for k, v in pairs(self.SubCtrl) do
            v:Open(self, data)
        end
        self.IsOpen = true
        self.IsPreHandle = false
        self:OpenOver()
        -- 判断是否要推入栈中
        if nil == isPushStack or isPushStack then
            uiCenter:PushingStack(self)
        end
    end
    -- 关闭界面--
    t.Close = function(self)
        if not self.IsOpen then
            return
        end
        -- 子ctrl关闭
        for k, v in pairs(self.SubCtrl) do
            v:Close(self)
        end
        self.IsOpen = false
        self:CloseOver()
        -- 移除模糊
        blurEffect.RemoveBlur(self.ControllerName)
        uiCenter:PopingStack(self)
    end
    -- 显示界面--
    t.Show = function(self)
        -- 如果被销毁
        if self.IsDestroyed then
            self:Open(self.Data, false)
        end
        -- 未open状态，不处理
        if not self.IsOpen then
            return false
        end
        -- 相机渲染
        self:SetMask()
        -- 模糊
        self:SetBlur(true)

        if self.IsShow then
            return false
        end
        -- 子ctrl显示
        for k, v in pairs(self.SubCtrl) do
            v:Show(self)
            v:SetParent(self.View.UI)
        end

        if not self.IsDestroyed then
            self.View:Show()
        end

        self.IsShow = true
        self:ShowOver()
        return true
    end
    -- 隐藏界面--
    t.Hide = function(self)
        -- 未show状态，不处理
        if not self.IsShow then
            return false
        end
        -- 模糊
        self:SetBlur(false)

        -- 子ctrl隐藏
        for k, v in pairs(self.SubCtrl) do
            v:Hide(self)
        end

        if not self.IsDestroyed then
            self.View:Hide()
        end

        self.IsShow = false
        self.IsPreHandle = false
        self:HideOver()
        return true
    end
    -- 开始打开界面--
    t.OpenOver = function(self)
        self:OnOpen(self.Data)
        -- 子ctrl打开
        for k, v in pairs(self.SubCtrl) do
            v:OpenOver()
        end
    end
    -- 结束关闭界面--
    t.CloseOver = function(self)
        -- 子ctrl关闭
        for k, v in pairs(self.SubCtrl) do
            if not v.IsOpen then
                v:CloseOver()
            end
        end
        self:OnClose()
    end
    -- 结束显示界面--
    t.ShowOver = function(self)
        -- 子ctrl显示
        for k, v in pairs(self.SubCtrl) do
            v:ShowOver()
        end
        self:OnShow()
    end
    -- 开始隐藏界面--
    t.HideOver = function(self)
        self:OnHide()
        -- 子ctrl隐藏
        for k, v in pairs(self.SubCtrl) do
            if not v.IsShow then
                v:HideOver()
            end
        end
    end
    -- 通知界面是否可交互--
    t.SetInteractive = function(self, isok)
        -- 没有交互性
        if not self.Interactive and isok then
            isok = self.Interactive
        end
        self.IsInteractive = isok
        -- 子ctrl交互
        for k, v in pairs(self.SubCtrl) do
            v:SetInteractive(self, isok)
        end
        if not self.IsDestroyed then
            self.View:SetInteractive(isok)
        end
        self:OnInteractive(isok)
    end
    -- 通知界面是否可交互--
    t.InteractiveBySelf = function(self, isok)
        self.Interactive = isok
        self:SetInteractive(isok)
    end
    -- 是否模糊--
    t.SetBlur = function(self, isBlur)
        if not self.PreCtrlBlur then
            return
        end
        if not self.IsDestroyed then
            blurEffect.Blur(isBlur, self, self.View.UI.sortingOrder - 1)
        end
    end
    -- 相机渲染层级
    t.SetMask = function(self)
        uiCullingMask.Mask(self)
    end
    -- 通知界面设置渲染顺序--
    t.SetSortingOrder = function(self, order)
        -- 如果拥有自己的渲染顺序,则使用自己的渲染值
        if nil ~= self.SortingOrder then
            order = self.SortingOrder
        end
        if not self.IsDestroyed then
            self.View:SetSortingOrder(order)
        end
        if self.IsShow then
            self:SetBlur(true)
        end
    end
    -- 销毁界面--
    t.DestroyBySelf = function(self, deep)
        deep = nil == deep and false or deep
        -- 未被销毁
        if not self.IsDestroyed then
            -- 置为销毁
            self.IsDestroyed = true

            -- 在非强制条件下，才允许关闭自己（重要）
            -- 上层已将数据清空，不需再进行关闭
            if not deep and self.IsOpen then
                self:Close()
            end

            -- 子ctrl销毁
            for k, v in pairs(self.SubCtrl) do
                v:Destroy(self, deep)
            end
            -- 销毁
            if self.View ~= nil then
                self:OnDestroy()
                self.View:Destroy()
                self.View = nil
            end
        end

        self.Timers:DisposeAll()
        self.Events = nil
        self.IsPreHandle = false
        self.IsOpen = false
        self.IsShow = false
        self.Data = nil

        -- 移除模糊
        blurEffect.RemoveBlur(self.ControllerName)
        -- 移除ctrl
        uiCenter:RemoveController(self)
    end
    -- 销毁界面--
    t.DestroyByOther = function(self, deep)
        -- 强制销毁
        if deep then
            self:DestroyBySelf(deep)
            return
        end

        -- 判断已被销毁
        if self.IsDestroyed then
            return
        end
        -- 正在显示和不允许被销毁
        if self.IsCannotDestroy or self.IsShow then
            return
        end
        -- 置为销毁
        self.IsDestroyed = true

        -- 子ctrl销毁
        for k, v in pairs(self.SubCtrl) do
            v:Destroy(self, deep)
        end
        -- 销毁
        if self.View ~= nil then
            self:OnDestroy()
            self.View:Destroy()
            self.View = nil
        end
        self.Timers:DisposeAll()
        self.Events = nil
    end
    -- 添加事件
    t.AddEvent = function(self, type, event)
        if nil == self.Events then
            self.Events = {}
        end
        self.Events[type] = event
    end
    -- 执行事件--
    t.DispatchEvent = function(self, type, ...)
        if not self.IsOpen then
            return
        end
        -- 子ctrl广播
        for k, v in pairs(self.SubCtrl) do
            v:DispatchEvent(type, ...)
        end
        if nil ~= self.Events and nil ~= self.Events[type] then
            self.Events[type](...)
        end
    end
    -- 更新--
    t.Update = function(self)
        if self.IsOpen then
            self:OnUpdate()
            self.Timers:Update()
        end
    end
    -- 更新--
    t.FixedUpdate = function(self)
        if self.IsOpen then
            self:OnFixedUpdate()
            self.Timers:FixedUpdate()
        elseif not self.IsDestroyed and self.AliveTime ~= -1 and not self.IsCannotDestroy then
            self.AliveTime = self.AliveTime - TimerManager.fixedDeltaTime
            if self.AliveTime < 0 then
                self:DestroyBySelf()
            end
        end
    end
    -- 刷新--
    t.Refresh = function(self)
        -- 子ctrl更新
        for k, v in pairs(self.SubCtrl) do
            v:Refresh()
        end
        self:OnRefresh()
    end
    -- 子ctrl--
    t.RequireSubCtrl = function(self, name)
        if nil == name then
            return nil
        end

        local sub = nil
        for k, v in pairs(self.SubCtrl) do
            if v.ControllerName == name then
                sub = v
                break
            end
        end
        if nil == sub then
            sub = LuaHandle.Load(name)
            table.insert(self.SubCtrl, sub)
        end
        return sub
    end
    --------当xx方法处理，子类重写----------
    t.OnPreHandle = function(self)
        return true
    end
    t.OnCreat = function(self)
    end
    t.OnOpen = function(self, data)
    end
    t.OnClose = function(self)
    end
    t.OnShow = function(self)
    end
    t.OnInteractive = function(self, isOk)
    end
    t.OnHide = function(self)
    end
    t.OnUpdate = function(self)
    end
    t.OnFixedUpdate = function(self)
    end
    t.OnDestroy = function(self)
    end
    t.OnRefresh = function(self)
    end

    if nil == isRegister or isRegister then
        uiCenter:RegisterController(t)
    end
    return t
end

return UIController
