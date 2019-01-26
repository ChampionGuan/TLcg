local _V = UIManager.View()
_V.PkgPath = "UI/Popup/Popup"
_V.PkgName = "Popup"
_V.ComName = "Popup"

function _V:OnCreat()
    self.BtnConfirm = self.UI:GetChild("Button_Confirm")
    self.BtnConfirm.onClick:Set(function() self:DispatchEvent("BtnConfirm") end)
end

function _V:OnDestroy()
end

return _V
