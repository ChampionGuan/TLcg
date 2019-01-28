-- 返回
local UIBack = {}

-- 无操作的面板
local NoHandleUI = {
    [UIConfig.ControllerName.Loading] = true,
    [UIConfig.ControllerName.Battle] = true
}
-- 退出app的面板
local QuitAppUI = {
    [UIConfig.ControllerName.Login] = true,
    [UIConfig.ControllerName.Maincity] = true
}
-- 切场景的面板
local LoadSceneUI = {}

function UIBack.CustomUpdate()
    -- 只在安卓和编辑器平台启用
    if CSharp.Application.platform ~= CSharp.RuntimePlatform.Android and not Utils.IsEditor() then
        return
    end

    if CSharp.Input.GetKeyUp(CSharp.KeyCode.Escape) then
        -- 引导中,则retrun

        local ctrl = UIManager.GetTopCtrl()
        if nil == ctrl or not ctrl.IsShow then
            return
        end

        -- 无操作的面板
        if nil ~= UIManager.WaitSyncing() then
            return
        end

        -- 不做任何处理
        for k, v in pairs(NoHandleUI) do
            if ctrl.ControllerName == k then
                return
            end
        end

        -- 退出app
        for k, v in pairs(QuitAppUI) do
            if ctrl.ControllerName == k then
                UIManager.openController(UIConfig.ControllerName.QuitApp)
                return
            end
        end

        -- 切换场景
        for k, v in pairs(LoadSceneUI) do
            if ctrl.ControllerName == k then
                LevelManager.LoadLastScene()
                ctrl:close()
                return
            end
        end

        -- back
        ctrl:close()
    end
end

return UIBack
