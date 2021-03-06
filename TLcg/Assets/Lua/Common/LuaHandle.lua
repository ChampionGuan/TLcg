local loadedLua = CS.LCG.ResourceLoader.LoadedLua

-- lua白名单（此名单下脚本不卸载，比如一些公共数据存储）
local dontUnloadList = {
    ["Game.Main"] = 1,
    ["Launcher.Main"] = 1,
    ["Common.Common"] = 1,
    ["Common.LuaHandle"] = 1,
    ["bit"] = 1,
    ["pb"] = 1
}

LuaHandle = {}

-- 加载lua
function LuaHandle.Load(path)
    if nil == path then
        return
    end

    -- 加载
    return require(path)
end

-- 卸载lua
function LuaHandle.Unload(path)
    if nil == path then
        return
    end

    package.loaded[path] = nil
    if loadedLua:Contains(path) then
        loadedLua:Remove(path)
    end
end

-- 卸载所有lua
function LuaHandle.UnloadAll()
    local luaPath = ""
    for i = loadedLua.Count - 1, 0, -1 do
        luaPath = loadedLua[i]
        if nil == dontUnloadList[luaPath] then
            LuaHandle.Unload(luaPath)
        end
    end
end
