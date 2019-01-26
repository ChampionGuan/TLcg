-- 本地化配置
return {
    -- 时间单位秒，-1 表示永驻内容 ，0 表示即时销毁，>0表示延时,
    -- 此处未配置的面板默认5秒后关闭
    [UIConfig.ControllerName.Popup] = -1,
    [UIConfig.ControllerName.Maincity] = -1,
    [UIConfig.ControllerName.Battle] = 10
}
