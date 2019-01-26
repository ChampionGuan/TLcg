local _V = UIManager.View()
_V.PkgPath = "UI/Loading/Loading"
_V.PkgName = "Loading"
_V.ComName = "Loading"

function _V:OnCreat()
    self.ProgressBar = self.UI:GetChild("ProgressBar")
end

function _V:OnDestroy()
end

return _V
