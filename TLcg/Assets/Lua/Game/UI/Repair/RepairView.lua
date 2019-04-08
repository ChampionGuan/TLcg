local _V = UIManager.View()
_V.PkgPath = "UI/Login/Login"
_V.PkgName = "Login"
_V.ComName = "AutofixMain"

function _V:OnCreat()
    self.BtnBack = self.UI:GetChild("Button_Back")
    self.BtnFix = self.UI:GetChild("Button_Fix")
    self.BtnDeepFix = self.UI:GetChild("Button_DeepFix")

    self.BtnBack.onClick:Set(
        function()
            self:DispatchEvent("BtnBack")
        end
    )
    self.BtnFix.onClick:Set(
        function()
            self:DispatchEvent("BtnFix")
        end
    )
    self.BtnDeepFix.onClick:Set(
        function()
            self:DispatchEvent("BtnDeepFix")
        end
    )
end

function _V:OnDestroy()
end

return _V
