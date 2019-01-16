-- cshape脚本
-- 注意，此脚本只是对csharp的类进行引用保存
-- 任何实例化的类这里不允许处理，例如CS.SDKManager.Instance.TheCSDKHandler
CSharp = {}

-- unityEngine
CSharp.WaitForSeconds = CS.UnityEngine.WaitForSeconds
CSharp.WaitForEndOfFrame = CS.UnityEngine.WaitForEndOfFrame
CSharp.UObject = CS.UnityEngine.Object
CSharp.Vector2 = CS.UnityEngine.Vector2
CSharp.Vector3 = CS.UnityEngine.Vector3
CSharp.Vector4 = CS.UnityEngine.Vector4
CSharp.Quaternion = CS.UnityEngine.Quaternion
CSharp.Color = CS.UnityEngine.Color
CSharp.Time = CS.UnityEngine.Time
CSharp.Screen = CS.UnityEngine.Screen
CSharp.Camera = CS.UnityEngine.Camera
CSharp.LayerMask = CS.UnityEngine.LayerMask
CSharp.GameObject = CS.UnityEngine.GameObject
CSharp.Transform = CS.UnityEngine.Transform
CSharp.Animator = CS.UnityEngine.Animator
CSharp.Application = CS.UnityEngine.Application
CSharp.RuntimePlatform = CS.UnityEngine.RuntimePlatform
CSharp.AudioRolloffMode = CS.UnityEngine.AudioRolloffMode
CSharp.AudioSource = CS.UnityEngine.AudioSource
CSharp.AudioListener = CS.UnityEngine.AudioListener
CSharp.AudioClip = CS.UnityEngine.AudioClip
CSharp.AudioMixer = CS.UnityEngine.Audio.AudioMixer
CSharp.Input = CS.UnityEngine.Input
CSharp.KeyCode = CS.UnityEngine.KeyCode

-- fairyGui
CSharp.Stage = CS.FairyGUI.Stage
CSharp.GRoot = CS.FairyGUI.GRoot
CSharp.ScrollPane = CS.FairyGUI.ScrollPane
CSharp.FUIConfig = CS.FairyGUI.UIConfig
CSharp.EventListener = CS.FairyGUI.EventListener
CSharp.EventDispatcher = CS.FairyGUI.EventDispatcher
CSharp.GGraph = CS.FairyGUI.GGraph
CSharp.GLoader = CS.FairyGUI.GLoader
CSharp.GComponent = CS.FairyGUI.GComponent
CSharp.GLabel = CS.FairyGUI.GLabel
CSharp.GButton = CS.FairyGUI.GButton
CSharp.GComboBox = CS.FairyGUI.GComboBox
CSharp.GProgressBar = CS.FairyGUI.GProgressBar
CSharp.GSlider = CS.FairyGUI.GSlider
CSharp.UIPackage = CS.FairyGUI.UIPackage
CSharp.UIPanel = CS.FairyGUI.UIPanel
CSharp.FontManager = CS.FairyGUI.FontManager
CSharp.GoWrapper = CS.FairyGUI.GoWrapper
CSharp.LongPressGesture = CS.FairyGUI.LongPressGesture
CSharp.StageCamera = CS.FairyGUI.StageCamera
CSharp.BlurFilter = CS.FairyGUI.BlurFilter
CSharp.UIContentScaler = CS.FairyGUI.UIContentScaler
CSharp.RelationType = CS.FairyGUI.RelationType
CSharp.Timers = CS.FairyGUI.Timers
CSharp.TimerCallback = CS.FairyGUI.TimerCallback
CSharp.NTexture = CS.FairyGUI.NTexture
CSharp.AlignType = CS.FairyGUI.AlignType
CSharp.EaseType = CS.FairyGUI.EaseType

-- custom
CSharp.Main = CS.LCG.Main
CSharp.LauncherEngine = CS.LCG.LauncherEngine
CSharp.GameEngine = CS.LCG.GameEngine
CSharp.PlayerPrefs = CS.LCG.PlayerPrefs
CSharp.ABManager = CS.LCG.ABManager
CSharp.ABHelper = CS.LCG.ABHelper
CSharp.EVersionState = CSharp.ABHelper.EVersionState
CSharp.Utils = CS.LCG.Utils
CSharp.Video = CS.LCG.Video


