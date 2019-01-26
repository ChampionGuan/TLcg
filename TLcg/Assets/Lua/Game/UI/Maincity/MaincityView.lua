local _V = UIManager.View()
_V.PkgPath = "UI/Maincity/Maincity"
_V.PkgName = "Maincity"
_V.ComName = "Maincity"

function _V:OnCreat()
    self.BtnToBootup = self.UI:GetChild("Button_ToBootup")
    self.BtnToBattle = self.UI:GetChild("Button_ToBattle")

    self.BtnToBootup.onClick:Set(
        function()
            self:DispatchEvent("BtnToBootup")
        end
    )
    self.BtnToBattle.onClick:Set(
        function()
            self:DispatchEvent("BtnToBattle")
        end
    )
end

function _V:OnDestroy()
end

return _V
