-- csharp的方法补丁，参考链接:https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/hotfix.md
-- 非编辑器模式下执行
if
    CS.UnityEngine.Application.platform == CS.UnityEngine.RuntimePlatform.WindowsEditor or
        CS.UnityEngine.Application.platform == CS.UnityEngine.RuntimePlatform.OSXEditor
 then
    return
else
    CS.LCG.LuaEnv.Instance:DoString(
        "xlua.hotfix(CS.LCG.GameEngine, 'HotfixTest', function(self) print('lua hotfix log') end)"
    )
end
