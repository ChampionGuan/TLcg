local _V = UIManager.View()
_V.PkgPath = "UI/Login/Login"
_V.PkgName = "Login"
_V.ComName = "LoginMain"

function _V:OnCreat()
    self.BtnRegister = self.UI:GetChild("Button_Register")
    self.BtnLogin = self.UI:GetChild("Button_Login")
    self.BtnStart = self.UI:GetChild("Button_Start")
    self.BtnSwitchAccount = self.UI:GetChild("Button_SwitchAccount")
    self.BtnSwitchServer = self.UI:GetChild("Button_SwitchServer")
    self.BtnAutofix = self.UI:GetChild("Button_Autofix")
    self.CtrlLogin = self.UI:GetController("Logic_C")

    self.BtnRegister.onClick:Set(
        function()
            self:DispatchEvent("BtnRegister")
        end
    )
    self.BtnLogin.onClick:Set(
        function()
            self:DispatchEvent("BtnLogin")
        end
    )
    self.BtnStart.onClick:Set(
        function()
            self:DispatchEvent("BtnStart")
        end
    )
    self.BtnSwitchAccount.onClick:Set(
        function()
            self:DispatchEvent("BtnSwitchAccount")
        end
    )
    self.BtnSwitchServer.onClick:Set(
        function()
            self:DispatchEvent("BtnSwitchServer")
        end
    )
    self.BtnAutofix.onClick:Set(
        function()
            self:DispatchEvent("BtnAutofix")
        end
    )
end

function _V:OnDestroy()
end

return _V
