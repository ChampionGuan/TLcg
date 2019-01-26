_C = UIManager.Controller(UIConfig.ControllerName.Loading, UIConfig.ViewName.Loading)

local function LoadingOk()
    _C:Close()
end

local function LoadingP(p)
    _C.View.ProgressBar.value = p
end

function _C:OnCreat()
    _C:AddEvent(UIConfig.Event.LOADING_P, LoadingP)
    _C:AddEvent(UIConfig.Event.LOADING_OK, LoadingOk)
end

function _C:OnOpen()
    _C.View.ProgressBar.max = 100
    _C.View.ProgressBar.value = 0
end
