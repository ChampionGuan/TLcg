Debug = {}

function Debug.Format(...)
    local tab = {...}
    local output = ""
    for i = 1, #tab do
        output = string.format("%s\t%s", output, tab[i])
    end
    return output .. "\n" .. debug.traceback()
end

function Debug.Log(...)
    print("<color=#00FF39>[Log] " .. Debug.Format(...) .. " </color>")
end

function Debug.Waring(...)
    print("<color=0041FF>[Warning] " .. Debug.Format(...) .. " </color>")
end

function Debug.Error(...)
    error("<color=#C700FF>[Error] " .. Debug.Format(...) .. " </color>")
end

Utils = {}

-- 获取y轴向角度--
function Utils.HorizontalAngle(direction)
    return CSharp.Mathf.Atan2(direction.x, direction.z) * CSharp.Mathf.Rad2Deg
end

-- 计算两向量夹角--
function Utils.AngleAroundAxis(direA, direB, axis)
    direA = direA - CSharp.Vector3.Project(driA, axis)
    direB = direB - CSharp.Vector3.Project(direB, axis)
    local angle = CSharp.Vector3.Angle(direA, direB)

    local factor = 1
    if CSharp.Vector3.Dot(axis, CSharp.Vector3.Cross(direA, direB)) < 0 then
        factor = -1
    end

    return angle * factor
end

-- 点绕轴旋转某角度后的点位置--
function Utils.RotateRound(position, center, axis, angle)
    local point = CSharp.Quaternion.AngleAxis(angle, axis) * (position - center)
    local resultVec3 = center + point
    return resultVec3
end

-- 秒数转换(x天x小时x分钟x秒)
function Utils.SecondConversion(second)
    if type(second) ~= "number" or second < 0 then
        return
    end

    local desc, day, hour, minute = ""
    second = math.floor(second)

    day = math.modf(second / 86400)
    second = second - day * 86400

    hour = math.modf(second / 3600)
    second = second - hour * 3600

    minute = math.modf(second / 60)
    second = second - minute * 60

    if day > 0 then
        desc = day .. Localization.Day
    end
    if hour > 0 then
        desc = desc .. hour .. Localization.Hour
    end
    if minute > 0 then
        desc = desc .. minute .. Localization.Minute
    end
    if second > 0 then
        desc = desc .. second .. Localization.Second
    end
    return desc
end

-- 获得key值不规律的table的长度
function Utils.TableLength(table)
    if type(table) ~= "table" then
        return 0
    end

    local count = 0

    for k, v in pairs(table) do
        if v ~= nil then
            count = count + 1
        end
    end

    return count
end

-- 参数:待分割的字符串,分割字符
-- 返回:子串表.(含有空串)
function Utils.StringSplit(str, split_char)
    local sub_str_tab = {}
    while (true) do
        local pos = string.find(str, split_char)
        if (not pos) then
            sub_str_tab[#sub_str_tab + 1] = str
            break
        end
        local sub_str = string.sub(str, 1, pos - 1)
        sub_str_tab[#sub_str_tab + 1] = sub_str
        str = string.sub(str, pos + 1, #str)
    end

    return sub_str_tab
end

-- 深拷贝
function Utils.DeepCopy(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local newObject = {}
        lookup_table[object] = newObject
        for key, value in pairs(object) do
            newObject[_copy(key)] = _copy(value)
        end
        return setmetatable(newObject, getmetatable(object))
    end
    return _copy(object)
end

-- 检测fgui对象是否被销毁（uiObject类型）
function Utils.UITargetIsNil(uiTarget)
    if
        nil == uiTarget or nil == uiTarget.displayObject or uiTarget.displayObject.isDisposed or
            uiTarget.displayObject.gameObject:Equals(nil)
     then
        return true
    else
        return false
    end
end

-- 检测unity对象是否被销毁(Object类型)
function Utils.UnityTargetIsNil(unityTarget)
    if nil == unityTarget or (CSharp.LogicUtils.IsNil(unityTarget)) then
        return true
    else
        return false
    end
end

-- 是否处在编辑器中
function Utils.IsEditor()
    if
        CSharp.Application.platform == CSharp.RuntimePlatform.WindowsEditor or
            CSharp.Application.platform == CSharp.RuntimePlatform.OSXEditor or
            CSharp.Application.platform == CSharp.RuntimePlatform.LinuxEditor
     then
        return true
    end

    return false
end

-- 检测是否在范围
function Utils.CheckInRange(point, range)
    if point.x >= range.x and point.x <= range.y and point.y >= range.z and point.y <= range.w then
        return true
    end
    return false
end

-- bytes 转int数字
function Utils.BytesToInt(bytes)
    if bytes == nil then
        Utils.DebugError("bytes is nil")
        return nil
    end
    local byteTab = {}
    for i = 1, string.len(bytes) do
        table.insert(byteTab, string.byte(string.sub(bytes, i, i)))
    end
    if #byteTab == 0 then
        return 0
    elseif #byteTab == 1 then
        if byteTab[0] == 1 then
            return -9223372036854775808
        end
    elseif #byteTab > 8 then
        return 0
    end
    local p = 1
    local a = 0
    for i = 1, #byteTab do
        a = a + byteTab[i] * p
        p = p * 256
    end
    local x = tonumber(a / 2)
    if a % 2 == 1 then
        x = -x
    end
    return x
end

-- int 转byte数字
-- _int:必须是number类型
function Utils.IntToBytes(_int)
    if _int == nil then
        Utils.DebugError("_int is nil by Utils in 865")
        return nil
    end
    -- 保护下
    if type(_int) ~= "number" then
        local ok, int = pcall(tonumber, _int)
        if ok then
            _int = int
        else
            Utils.DebugError("_int is nil by Utils in 864")
            return nil
        end
    end

    if _int == 0 then
        return nil
    end

    local tab = ""
    if _int == -9223372036854775808 then
        tab = "1"
        return tab
    end

    local x = 0
    if _int < 0 then
        x = (-_int * 2) + 1
    else
        x = _int * 2
    end

    local bn = 8
    local n = 256
    for i = 1, 8, 1 do
        if x < n then
            bn = i
            break
        else
            n = n * 256
        end
    end

    for i = 1, bn do
        tab = tab .. string.char(math.floor(x % 256))
        x = x / 256
    end
    return tab
end

-- 协程开始
-- Utils.CoroutineStart(function() coroutine.yield(CSharp.WaitForSeconds(5)) end)
function Utils.CoroutineStart(...)
    if nil == xluaUtils then
        xluaUtils = LuaHandle.Load("Game.Common.xluaUtils")
    end
    return CSharp.GameEngine.Instance:StartCoroutine(xluaUtils.cs_generator(...))
end

-- 协程结束
-- Utils.CoroutineStop(参数为协程开始时返回的值)
function Utils.CoroutineStop(coroutine)
    if nil == coroutine then
        return
    end
    return CSharp.GameEngine.Instance:StopCoroutine(coroutine)
end

--// The Save Function
function Utils.save(tbl, filename)
    local charS, charE = "   ", "\n"
    local file, err = io.open(filename, "wb")
    if err then
        return err
    end

    -- initiate variables for save procedure
    local tables, lookup = {tbl}, {[tbl] = 1}
    file:write("return {" .. charE)

    for idx, t in ipairs(tables) do
        file:write("-- Table: {" .. idx .. "}" .. charE)
        file:write("{" .. charE)
        local thandled = {}

        for i, v in ipairs(t) do
            thandled[i] = true
            local stype = type(v)
            -- only handle value
            if stype == "table" then
                if not lookup[v] then
                    table.insert(tables, v)
                    lookup[v] = #tables
                end
                file:write(charS .. "{" .. lookup[v] .. "}," .. charE)
            elseif stype == "string" then
                file:write(charS .. exportstring(v) .. "," .. charE)
            elseif stype == "number" then
                file:write(charS .. tostring(v) .. "," .. charE)
            end
        end

        for i, v in pairs(t) do
            -- escape handled values
            if (not thandled[i]) then
                local str = ""
                local stype = type(i)
                -- handle index
                if stype == "table" then
                    if not lookup[i] then
                        table.insert(tables, i)
                        lookup[i] = #tables
                    end
                    str = charS .. "[{" .. lookup[i] .. "}]="
                elseif stype == "string" then
                    str = charS .. "[" .. exportstring(i) .. "]="
                elseif stype == "number" then
                    str = charS .. "[" .. tostring(i) .. "]="
                end

                if str ~= "" then
                    stype = type(v)
                    -- handle value
                    if stype == "table" then
                        if not lookup[v] then
                            table.insert(tables, v)
                            lookup[v] = #tables
                        end
                        file:write(str .. "{" .. lookup[v] .. "}," .. charE)
                    elseif stype == "string" then
                        file:write(str .. exportstring(v) .. "," .. charE)
                    elseif stype == "number" then
                        file:write(str .. tostring(v) .. "," .. charE)
                    end
                end
            end
        end
        file:write("}," .. charE)
    end
    file:write("}")
    file:close()
end

--// The Load Function
function Utils.load(sfile)
    local ftables, err = loadfile(sfile)
    if err then
        return _, err
    end
    local tables = ftables()
    for idx = 1, #tables do
        local tolinki = {}
        for i, v in pairs(tables[idx]) do
            if type(v) == "table" then
                tables[idx][i] = tables[v[1]]
            end
            if type(i) == "table" and tables[i[1]] then
                table.insert(tolinki, {i, tables[i[1]]})
            end
        end
        -- link indices
        for _, v in ipairs(tolinki) do
            tables[idx][v[2]], tables[idx][v[1]] = tables[idx][v[1]], nil
        end
    end
    return tables[1]
end
