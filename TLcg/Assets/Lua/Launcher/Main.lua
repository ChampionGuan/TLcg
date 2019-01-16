require "Common.Common"
require "Common.LuaHandle"

-- 更新检测
local m_abHotfix = nil

-- 初始化
function Initialize(type)
    m_abHotfix = LuaHandle.Load("Launcher.ABHotfix")

    if type == 1 then
        m_abHotfix.Bootup(Over)
    elseif type == 2 then
        m_abHotfix.Check(Over)
    elseif type == 3 then
        m_abHotfix.Repair(false, Over)
    elseif type == 4 then
        m_abHotfix.Repair(true, Over)
    end
end

-- 结束
function Over()
    print("启动器结束！！")
    OnDestroy()
    LuaHandle.Load("Launcher.CSHotfix")
    CSharp.Main.Instance:StartupGame()
    LuaHandle.Unload("Launcher.Main")
end

-- 更新
function Update()
end

-- 固定更新
function FixedUpdate()
    m_abHotfix.FixedUpdate()
end

-- 焦点
function OnAppFocus(hasfocus)
end

-- 收到消息
function OnReceiveMsg(msg)
end

-- 销毁
function OnDestroy()
    m_abHotfix.Destroy()
end
