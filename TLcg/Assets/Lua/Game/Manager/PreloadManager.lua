local XluaUtils = LuaHandle.Load("Common.XluaUtils")
LuaHandle.Load("Game.Manager.VideoManager")

local UI = {}
UI.Root = nil
UI.ProgressBar = nil
UI.VersionText = nil

function UI.LoadUI()
    CSharp.ResourceLoader.LoadUI("UI/Loading/Loading")
    UI.Root = CSharp.UIPackage.CreateObject("Loading", "LoadAssets").asCom
    UI.ProgressBar = UI.Root:GetChild("ProgressBar")
    UI.VersionText = UI.Root:GetChild("Text_Version")
    UI.VersionText.text = Common.Version.ServerResVersion
    CSharp.GRoot.inst:AddChild(UI.Root)
    UI.Root:MakeFullScreen()
end

function UI.Destroy()
    if nil == UI.Root then
        return
    end
    UI.Root:Dispose()
    CSharp.GRoot.inst:RemoveChild(UI.Root, true)
    UI.Root = nil
end

local Preload = {}

function Preload.LoadLua(complete)
    luaConfig = LuaHandle.Load("Game.Config.LuaPreloadConfig")
    local index, max = 0, #luaConfig
    local wait = CSharp.WaitForEndOfFrame()
    UI.ProgressBar.max = max
    UI.ProgressBar.value = 0
    while (index <= max) do
        index = index + 1
        LuaHandle.Load(luaConfig[index])
        UI.ProgressBar.value = index
        if index % 1 == 0 then
            coroutine.yield(wait)
        end
    end

    complete()
end

function Preload.LoadAssets(complete)
    local wait = CSharp.WaitForEndOfFrame()
    coroutine.yield(wait)
    complete()
end

function Preload.Load()
    VideoManager.Play(
        2,
        function()
            UI.LoadUI()
            CSharp.Main.Instance:StartCoroutine(
                XluaUtils.cs_generator(
                    Preload.LoadLua,
                    function()
                        CSharp.Main.Instance:StartCoroutine(
                            XluaUtils.cs_generator(Preload.LoadAssets, PreloadManager.LoadComplete)
                        )
                    end
                )
            )
        end
    )
end

PreloadManager = LuaHandle.Load("Game.Manager.IManager")()
PreloadManager.Initialized = false
PreloadManager.OnLoadComplete = nil

function PreloadManager.Initialize(over)
    print("进行预加载")
    PreloadManager.Initialized = false
    PreloadManager.OnLoadComplete = over
    VideoManager.Initialize()
    Preload.Load()
end

function PreloadManager.LoadComplete()
    PreloadManager.Initialized = true
    PreloadManager.OnLoadComplete()
    UI.Destroy()
end

function PreloadManager.CustomDestroy()
    UI.Destroy()
    VideoManager.CustomDestroy()
end

return Preload
