local _V = UIManager.View()
_V.PkgPath = "UI/Battle/Battle"
_V.PkgName = "Battle"
_V.ComName = "Battle"

function _V:OnCreat()
    self.BtnToMaincity = self.UI:GetChild("Button_ToMaincity")

    self.BtnToMaincity.onClick:Set(
        function()
            self:DispatchEvent("BtnToMaincity")
        end
    )
end

function _V:OnDestroy()
end

return _V
