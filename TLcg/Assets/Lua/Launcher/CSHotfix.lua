-- 不同的方法补丁，参考链接:https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/hotfix.md
-- 当前版本不支持Assembly-CSharp.dll之外的dll打补丁。如需要，需升级xlua（亲测通过！）
-- 非编辑器模式下执行
if
    CS.UnityEngine.Application.platform == CS.UnityEngine.RuntimePlatform.WindowsEditor or
        CS.UnityEngine.Application.platform == CS.UnityEngine.RuntimePlatform.OSXEditor
 then
    return
else
    -- CS.LCG.LuaEnv.Instance:DoString(
    --     "xlua.hotfix(CS.LCG.CameraControllerMainCity, 'Update', function(self) print('maincity Camera') end)"
    -- )
    CS.LCG.LuaEnv.Instance:DoString(
        -- "xlua.hotfix(CS.LCG.Bootup, 'HotfixTest', function(self) print('lua hotfix log') end)"
    )
end
