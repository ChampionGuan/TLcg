-----------------------------------------------------
-------------------定义View--------------------------
-----------------------------------------------------

local function UIView()
    local t = {}
    t.UI = nil
    t.PkgPath = nil
    t.PkgName = nil
    t.IsFullScreen = true
    t.ComName = nil
    t.Events = nil

    t.AddEvent = function(self, _key, _callback)
        if nil == _key or nil == _callback then
            return
        end

        if self.Events == nil then
            self.Events = {}
        end

        if nil ~= self.Events[_key] then
            print("重复注册ui事件", _key)
        end
        self.Events[_key] = _callback
    end

    t.DispatchEvent = function(self, _key, ...)
        if nil ~= self.Events and nil ~= self.Events[_key] then
            self.Events[_key](...)
        end
    end

    t.Creat = function(self)
        self.UI = UIUtils.SpawnUICom(self.PkgPath, self.PkgName, self.ComName, self.IsFullScreen)
        self:OnCreat()
    end

    t.Interactive = function(self, isok)
        self.UI.touchable = isok
    end

    t.SortingOrder = function(self, order)
        self.UI.sortingOrder = order
    end

    t.Show = function(self)
        self.UI.visible = true
        self:OnShow()
    end

    t.Hide = function(self)
        self.UI.visible = false
        self:OnHide()
    end

    t.Destroy = function(self)
        self.Events = nil
        self:OnDestroy()
        UIUtils.DisposeUICom(self.PkgPath, self.PkgName, self.UI)
    end

    t.IsDispose = function(self)
        if nil == self.UI or nil == self.UI.displayObject or self.UI.displayObject.isDisposed then
            return true
        else
            return false
        end
    end
    t.OnCreat = function(self)
    end
    t.OnDestroy = function(self)
    end
    t.OnShow = function(self)
    end
    t.OnHide = function(self)
    end
    t.OnInteractive = function(self)
    end
    return t
end
return UIView
