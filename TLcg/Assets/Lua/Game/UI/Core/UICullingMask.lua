-----------------------------------------------------
-------------------定义layer--------------------------
-----------------------------------------------------

local cullingMaskConfig = nil

--  layer
local CullingMask = {
    Mask = function(ctrl)
        if nil == cullingMaskConfig then
            cullingMaskConfig = LuaHandle.Load("Game.Config.UICullingMaskConfig")
        end

        -- 无主相机
        if nil == CSharp.Camera.main then
            return
        end
        -- 无渲染layer
        local mask = cullingMaskConfig[LevelManager.CurLevelType]
        if nil == mask then
            return
        end
        -- 主相机的渲染layer
        local fsCtrl = UIManager.GetTopFullScreenCtrl()
        if (nil ~= fsCtrl and fsCtrl.IsShow) or ctrl.Type == UIDefine.CtrlType.FullScreen then
            CSharp.Camera.main.cullingMask = 0
        elseif CSharp.Camera.main.cullingMask == 0 then
            CSharp.Camera.main.cullingMask = mask
        end
    end
}

return CullingMask
