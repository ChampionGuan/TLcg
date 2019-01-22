local IManager = function()
    local t = {}
    -- 初始化
    function t.Initialize()
    end

    -- 更新
    function t.CustomUpdate()
    end

    -- 固定更新
    function t.CustomFixedUpdate()
    end

    -- 销毁
    function t.CustomDestroy()
    end

    -- 焦点
    function t.OnAppFocus(hasfocus)
    end

    return t
end

return IManager
