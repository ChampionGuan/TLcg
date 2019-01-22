/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;
using FairyGUI;
using DG.Tweening;
using DG.Tweening.Core;

namespace LCG
{
    public static class GenConfigs
    {
        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(System.Object),
#region unityEngine
                typeof(WaitForSeconds),
                typeof(WaitForEndOfFrame),
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Time),
                typeof(Screen),
                typeof(Camera),
                typeof(LayerMask),
                typeof(GameObject),
                typeof(Transform),
                typeof(Animator),
                typeof(Application),
                typeof(RuntimePlatform),
                typeof(AudioRolloffMode),
                typeof(AudioSource),
                typeof(AudioListener),
                typeof(AudioClip),
                typeof(UnityEngine.Audio.AudioMixer),
                typeof(UnityEngine.Input),
                typeof(UnityEngine.KeyCode),
#endregion

#region fairygui
                typeof(EventDispatcher),
                typeof(EventListener),
                typeof(InputEvent),
                typeof(DisplayObject),
                typeof(Container),
                typeof(Stage),
                typeof(Controller),
                typeof(GObject),
                typeof(GGraph),
                typeof(GGroup),
                typeof(GImage),
                typeof(NTexture),
                typeof(GLoader),
                typeof(PlayState),
                typeof(GMovieClip),
                typeof(TextFormat),
                typeof(GTextField),
                typeof(GRichTextField),
                typeof(GTextInput),
                typeof(GComponent),
                typeof(GList),
                typeof(GRoot),
                typeof(GLabel),
                typeof(GButton),
                typeof(GComboBox),
                typeof(GProgressBar),
                typeof(GSlider),
                typeof(PopupMenu),
                typeof(ScrollPane),
                typeof(Transition),
                typeof(UIConfig),
                typeof(UIPackage),
                typeof(Window),
                typeof(GObjectPool),
                typeof(Relations),
                typeof(RelationType),
                typeof(Timers),
                typeof(FontManager),
                typeof(GoWrapper),
                typeof(LongPressGesture),
#endregion


#region custom
                typeof(Main),
                typeof(Video),
                typeof(LauncherEngine),
                typeof(GameEngine),
                typeof(PlayerPrefs),
                typeof(ABHelper),
                typeof(ABHelper.EVersionState),
                typeof(Utils),
                typeof(Network),
                typeof(Http),
                typeof(SceneLoader),
                typeof(ResourceLoader)
#endregion
            };

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        [CSharpCallLua]
        public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(System.Collections.IEnumerator),
                typeof(Func<double, double, double>),
                typeof(Action),
                typeof(Action<string>),
                typeof(Action<float>),
                typeof(Action<bool>),
                typeof(Action<double>),
                typeof(Action<int>),
                typeof(Action<byte[]>),
                typeof(Action<string, string>),
                typeof(Action<string, byte[]>),
                typeof(Action<string, Texture2D>),
                typeof(Action<int, int>),
                typeof(Action<int, int, int, int>),
                typeof(Action<int, int, bool>),
                typeof(Action<int, int, GameObject>),
                typeof(Action<string, int, int>),
                typeof(Action<UnityEngine.Object>),
                typeof(Action<Vector3, bool>),
                typeof(Func<bool>),
                typeof(Action<ABHelper.VersionArgs>),
                typeof(EventCallback0),
                typeof(EventCallback1),
                typeof(PlayCompleteCallback),
                typeof(ListItemRenderer),
                typeof(ListItemProvider),
                typeof(TweenCallback),
                typeof(Application.LogCallback),
                typeof(DOSetter<float>),
                typeof(TimerCallback),
                typeof(UnityEngine.Video.VideoPlayer.EventHandler),
            };

        //黑名单
        [BlackList]
        public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"UnityEngine.WWW", "movie"},
                #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
                #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured","System.String"},
            };
    }
}