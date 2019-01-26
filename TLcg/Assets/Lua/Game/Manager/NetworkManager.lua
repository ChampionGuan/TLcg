NetworkManager = LuaHandle.Load("Game.Manager.IManager")()

local httpHandle = nil

function NetworkManager.Initialize()
    if nil == httpHandle then
        httpHandle = LuaHandle.Load("Game.Network.HttpHandle")
    end
    httpHandle.Initialize()
end

-- 请求网络连接--
function NetworkManager.Connect(ip, port, token, key, callback)
end

-- 断开连接
function NetworkManager.DisConnect()
end

-- 发送消息
function NetworkManager.SendMsg(msg, moduleId, msgId)
end

-- 收到消息
function NetworkManager.ReceiveMsg(msg)
end

-- httpGet
-- url:远端
-- callback:成功回调
-- data:{
--     -- 菊花同步
--     sync = true,
--     -- 天荒地老，等待消息回来
--     keepWaiting = true,
--     -- 错误弹框
--     errorPopup = true,
--     -- 错误飘字
--     errorTip = true
-- }
function NetworkManager.HttpGet(url, callback, data)
end

-- httpPost
function NetworkManager.HttpPost(url, from, callback, data)
end

-- httpGet
function NetworkManager.HttpImage(url, callback)
end
