local _V = UIManager.View()
_V.PkgPath = "UI/ServerList/ServerList"
_V.PkgName = "ServerList"
_V.ComName = "ServerList"

function _V:OnCreat()
    self.BtnBack = self.UI:GetChild("Button_back")
    self.BtnBack.onClick:Set(function() self:DispatchEvent("BtnBack") end)
end

function _V:OnDestroy()
end

return _V
