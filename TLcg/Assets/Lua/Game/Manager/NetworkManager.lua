NetworkManager = LuaHandle.Load("Game.Manager.IManager")()

-- 请求网络连接--
function NetworkManager.Connect(ip, port, token, key, connectedInvoke)
end

-- 断开连接
function NetworkManager.DisConnect()
end

-- 发送消息
function NetworkManager.SendMsg(msg, moduleId, msgId)
end

-- 收到消息
function NetworkManager.OnReceiveMsg(msg)
end

-- httpGet
function NetworkManager.HttpGet(url, succeedCallBack, stillWait, errorPopup, isSync, isErrorTip)
end

-- httpPost
function NetworkManager.HttpPost(url, from, succeedCallBack, stillWait, errorPopup, isSync, isErrorTip)
end

-- httpGet
function NetworkManager.HttpGetTextrue(url, succeedCallBack, stillWait, errorPopup, isSync, width, height)
end

-- httpGet
function NetworkManager.HttpImage(url, succeedCallBack)
end
