require "Common.LuaHandle"
LuaHandle.Load("Common.Common")
LuaHandle.Load("Common.CSUtils")

-- 更新检测
local m_abHotfix = LuaHandle.Load("Launcher.ABHotfix")

-- 初始化
function Initialize(type)
    if type == CSharp.EBootup.Launcher then
        m_abHotfix.Bootup(Over)
    elseif type == CSharp.EBootup.Check then
        m_abHotfix.Check(Over)
    elseif type == CSharp.EBootup.Repair then
        m_abHotfix.Repair(false, Over)
    elseif type == CSharp.EBootup.DeepRepair then
        m_abHotfix.Repair(true, Over)
    end
end

-- 结束
function Over()
    print("启动器结束！！")
    m_abHotfix.Destroy()
    LuaHandle.Load("Launcher.CSHotfix")
    CSharp.Main.Instance:StartupGame(CSharp.EBootup.Game)
end

-- 固定更新
function FixedUpdate()
    m_abHotfix.FixedUpdate()
end

-- 销毁
function OnDestroy()
    m_abHotfix.Destroy()
    LuaHandle.UnloadAll()
    LuaHandle.Unload("Launcher.Main")
end
