NetworkManager = LuaHandle.Load("Game.Manager.IManager")()

function NetworkManager.Initialize()
    
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
