-----------------------------------------------------
-------------------定义blur--------------------------
-----------------------------------------------------
-- 模糊
local UIRoot = {}
local TextureList = {}
local TextureTab = {}
local BlurEffect = {}

BlurEffect.AddBlur = function(ctrl)
    local fsCtrl = UIManager.GetTopFullScreenCtrl()
    local fsTrue = (nil ~= fsCtrl and fsCtrl.IsShow) or ctrl.Type == UIDefine.CtrlType.FullScreen

    if fsTrue and true == TextureTab[ctrl.ControllerName] then
        return
    end
    if not fsTrue and false == TextureTab[ctrl.ControllerName] then
        return
    end

    BlurEffect.RemoveBlur(ctrl.ControllerName)
    if fsTrue or nil == CSharp.Camera.main or CSharp.Camera.main.cullingMask == 0 then
        TextureList[ctrl.ControllerName] = CSharp.NTexture(CSharp.BlurEffect.Instance:GetBlurTexture(CSharp.StageCamera.main))
        TextureTab[ctrl.ControllerName] = true
    else
        TextureList[ctrl.ControllerName] = CSharp.NTexture(CSharp.BlurEffect.Instance:GetBlurTexture(CSharp.Camera.main, CSharp.StageCamera.main))
        TextureTab[ctrl.ControllerName] = false
    end
end

BlurEffect.RemoveBlur = function(name)
    if nil ~= TextureList[name] then
        CSharp.UObject.Destroy(TextureList[name].nativeTexture)
    end
    TextureList[name] = nil
    TextureTab[name] = nil
end

BlurEffect.Destroy = function()
    for k, v in pairs(TextureList) do
        BlurEffect.RemoveBlur(k)
    end
    TextureList = {}
    TextureTab = {}
    CSharp.BlurEffect.Instance:Destroy()

    if Utils.uITargetIsNil(UIRoot.Root) then
        return
    end

    UIUtils.disposeView("UI/BlurEffect/BlurEffect", "BlurEffect", UIRoot.Root)
end

BlurEffect.Blur = function(isBlur, ctrl, sortingOrder)
    if isBlur and Utils.uITargetIsNil(UIRoot.Root) then
        UIRoot.Root = UIUtils.creatView("UI/BlurEffect/BlurEffect", "BlurEffect", "BlurMain")
        UIRoot.Icon = UIRoot.Root:GetChild("icon")
        UIRoot.Root.touchable = false
    end

    if Utils.uITargetIsNil(UIRoot.Root) then
        return
    end

    if isBlur then
        BlurEffect.AddBlur(ctrl)
        UIRoot.Icon.texture = TextureList[ctrl.ControllerName]
        UIRoot.Root.sortingOrder = sortingOrder
    end
    UIRoot.Root.visible = isBlur
end

return BlurEffect
