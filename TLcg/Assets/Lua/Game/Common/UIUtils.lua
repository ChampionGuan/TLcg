UIUtils = {}

-- 加载ui资源
function UIUtils.LoadUI(path)
    if nil == path then
        return
    end
    CSharp.ResourceLoader.LoadUI(path)
end

-- 卸载ui资源
function UIUtils.UnloadUI(path)
    if nil == path then
        return
    end
    CSharp.ResourceLoader.UnloadUI(path, true)
end

-- 生成界面
-- <param name="path" type="string">路径</param>
-- <param name="fileName" type="string">package文件名</param>
-- <param name="panelName" type="string">panel名</param>
-- <param name="isFullScreen" type="boolean">全屏适应</param>
function UIUtils.SpawnUICom(path, fileName, panelName, isFullScreen)
    if path == nil or fileName == nil or panelName == nil then
        return nil
    end

    UIUtils.LoadUI(path)
    local ui = CSharp.UIPackage.CreateObject(fileName, panelName).asCom
    ui = CSharp.GRoot.inst:AddChild(ui)
    ui.visible = false
    ui.fairyBatching = true
    if isFullScreen then
        ui:SetSize(UIManager.RootSize.x, UIManager.RootSize.y)
    end

    return ui
end

-- 销毁界面
-- <param name="path" type="string">路径</param>
-- <param name="target" type="Object">对象</param>
function UIUtils.DisposeUICom(path, target)
    if path == nil or target == nil then
        return
    end
    UIUtils.UnloadUI(path)
    CSharp.GRoot.inst:RemoveChild(target, true)
end

--_conrtoller FairyGui.Controller,_index number
function UIUtils.SetControllerIndex(_conrtoller, _index)
    if _conrtoller == nil or _index == nil or type(_index) ~= "number" then
        Utils.DebugWarning(
            "setControllerIndex,has invalid value. _conrtoller,_index:",
            pcall(_conrtoller.pageCount),
            _index
        )
        return
    end
    if _index >= 0 and _index < _conrtoller.pageCount then
        _conrtoller.selectedIndex = _index
    end
end
