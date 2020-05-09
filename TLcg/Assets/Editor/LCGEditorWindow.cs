using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Timeline;
using UnityEngine.Events;
using System.Reflection;

namespace LCG
{
    public class LCGEditorWindow : BaseEditorWindow<LCGEditorWindow>
    {
        static LCGEditorWindow m_window;
        static EditorWindow m_window_project;
        //static TimelineWindow m_window_timeline;

        [MenuItem("Tools/editor测试")]
        static void Build()
        {
            m_window = LCGEditorWindow.GetWindow<LCGEditorWindow>();
            m_window_project = EditorHelper.OpenProjectEditor();
            //m_window_timeline = EditorHelper.OpenTimelineEditor() as TimelineWindow;

            m_window.DockWindow();
            m_window.Focus();
        }

        private void OnGUI()
        {
            DockWindow();
            EditorHelper.Label(this.position.size.x.ToString() + ":" + this.position.size.y.ToString(), 1000);
            EditorHelper.Label(this.m_LastPostion.size.x.ToString() + ":" + this.m_LastPostion.size.y.ToString(), 1000);
        }

        private void DockWindow()
        {
            if ((bool)EditorHelper.isDockedMethod.Invoke(m_window_project, null) == false)
            {
                m_window.DockWindow((EditorWindow)m_window_project, Docker.DockPosition.Right);
            }
            //if ((bool)EditorHelper.isDockedMethod.Invoke(m_window_timeline, null) == false)
            //{
            //    m_window.DockWindow((EditorWindow)m_window_timeline, Docker.DockPosition.Bottom);
            //}
        }
        private void AddTabWindow()
        {
            // 加页签
            // var parent = EditorHelper.ParentField.GetValue(m_window);
            // EditorHelper.AddTabMethod.Invoke(parent, new object[] { m_window_project, false });
            // EditorHelper.AddTabMethod.Invoke(parent, new object[] { m_window_timeline, false });
        }

        [OnOpenAsset(1)]
        static bool OpenToEdit(int _instanceID, int _line)
        {
            return false;
        }
    }

    #region  EditorHelper

    public class EditorHelper
    {
        public static EditorWindow OpenProjectEditor()
        {
            // Retrieve the existing Inspector tab, or create a new one if none is open
            EditorWindow ProjectBrowser = EditorWindow.GetWindow(ProjectBrowserType);
            // Get the size of the currently window
            // Vector2 size = new Vector2(ProjectBrowser.position.width, ProjectBrowser.position.height);
            // Clone the inspector tab (optionnal step)
            ProjectBrowser = UnityEngine.Object.Instantiate(ProjectBrowser);
            // Set min size, and focus the window
            //ProjectBrowser.minSize = size;
            ProjectBrowser.Show();
            //ProjectBrowser.Focus();
            return ProjectBrowser;
        }
        //public static EditorWindow OpenTimelineEditor()
        //{
        //    return EditorWindow.GetWindow(TimelineWindowType) as TimelineWindow;
        //}

        // === Reflection ===
        public static Type ProjectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        public static Type TimelineWindowType = Type.GetType("UnityEditor.Timeline.TimelineWindow, Unity.Timeline.Editor");
        public static Type EditorClipType = Type.GetType("UnityEditor.Timeline.EditorClip, Unity.Timeline.Editor");
        public static Type TimelineTreeViewGUIType = Type.GetType("UnityEditor.Timeline.TimelineTreeViewGUI, Unity.Timeline.Editor");

        public static Type DockAreaType = typeof(Editor).Assembly.GetType("UnityEditor.DockArea");
        public static MethodInfo AddTabMethod = DockAreaType.GetMethod("AddTab", new Type[] { typeof(EditorWindow), typeof(bool) });
        public static FieldInfo ParentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);

        public static PropertyInfo TimelineEditorTreeview = TimelineWindowType.GetProperty("treeView");
        public static MethodInfo CalculateRowRectsMethod = TimelineTreeViewGUIType.GetMethod("CalculateRowRects");

        public static EventInfo TimelineWindow_OnGUIEvent = TimelineWindowType.GetEvent("OnGUIEvent");
        public static EventInfo TimelineWindow_OnTimeChangeEvent = TimelineWindowType.GetEvent("OnTimeChangeEvent");
        public static EventInfo TimelineWindow_OnRebuildGraphEvent = TimelineWindowType.GetEvent("OnRebuildGraphEvent");
        public static MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetGetMethod(true);
        public static PropertyInfo TimelineEditor_clip = EditorClipType.GetProperty("clip");
        public static PropertyInfo EditorClip_locked = TimelineWindowType.GetProperty("locked");

        //public static PropertyInfo TimelineWindowState = TimelineWindowType.GetProperty("state");
        // ==================

        public static bool GoToPath(string path)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj == null)
            {
                return false;
            }
            UnityEditor.EditorGUIUtility.PingObject(obj);
            AssetDatabase.ReleaseCachedFileHandles();
            return true;
        }

        public static void DelChildren(Transform tf, bool force = false)
        {
            if (tf == null)
                return;

            if (!force && UnityEditor.PrefabUtility.IsPartOfAnyPrefab(tf.gameObject))
                return;

            for (var i = tf.childCount - 1; i >= 0; i--)
            {
                var child = tf.GetChild(i);
                child.SetParent(null);
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        /// <summary>
        /// 是否在Prefab编辑环境下
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static bool IsPrefabEditMode(GameObject root)
        {
#if UNITY_EDITOR
            var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(root);
            //Debug.LogFormat("OnDisable {0}, {1}", prefabStage.scene.name, prefabStage.IsPartOfPrefabContents(gameObject));
            //about to exit prefab edit mode
            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(root))
            {
                return true;
            }
#endif
            return false;
        }

        static GUIStyle s_BtnMid;
        public static GUIStyle GUIStyleBtnMid
        {
            get
            {
                if (s_BtnMid == null)
                {
                    s_BtnMid = new GUIStyle(GUI.skin.button);
                    s_BtnMid.fontSize = 15;
                    s_BtnMid.richText = true;
                }
                return s_BtnMid;
            }
        }

        static GUIStyle s_PopupMid;
        public static GUIStyle GUIStylePopupMid
        {
            get
            {
                if (s_PopupMid == null)
                {
                    s_PopupMid = new GUIStyle(EditorStyles.popup);
                    s_PopupMid.fontSize = 16;
                    s_PopupMid.fixedHeight = 22;
                    s_PopupMid.richText = true;
                }
                return s_PopupMid;
            }
        }

        static GUIStyle s_TextFieldMid;
        public static GUIStyle GUIStyleTextFieldMid
        {
            get
            {
                if (s_TextFieldMid == null)
                {
                    s_TextFieldMid = new GUIStyle(EditorStyles.textField);
                    s_TextFieldMid.fontSize = 14;
                    s_TextFieldMid.fixedHeight = 22;
                }
                return s_TextFieldMid;
            }
        }

        static GUIStyle s_ToggleMid;
        public static GUIStyle GUIStyleToggleMid
        {
            get
            {
                if (s_ToggleMid == null)
                {
                    s_ToggleMid = new GUIStyle(EditorStyles.toggle);
                    s_ToggleMid.fontSize = 14;
                    s_ToggleMid.fixedHeight = 22;
                }
                return s_ToggleMid;
            }
        }

        static GUIStyle s_ToggleButtonMid;
        public static GUIStyle GUIStyleToggleButtonMid
        {
            get
            {
                if (s_ToggleButtonMid == null)
                {
                    s_ToggleButtonMid = new GUIStyle(UnityEngine.GUI.skin.button);
                    s_ToggleButtonMid.fontSize = 14;
                    s_ToggleButtonMid.fixedHeight = 22;
                }
                return s_ToggleButtonMid;
            }
        }

        static GUIStyle s_LabelMid;
        public static GUIStyle GUIStyleLabelMid
        {
            get
            {
                if (s_LabelMid == null)
                {
                    s_LabelMid = new GUIStyle(UnityEngine.GUI.skin.label);
                    s_LabelMid.fontSize = 14;
                    s_LabelMid.fixedHeight = 22;
                    s_LabelMid.richText = true;
                }
                return s_LabelMid;
            }
        }

        static GUIStyle s_LabelBig;
        public static GUIStyle GUIStyleLabelBig
        {
            get
            {
                if (s_LabelBig == null)
                {
                    s_LabelBig = new GUIStyle(UnityEngine.GUI.skin.label);
                    s_LabelBig.fontSize = 16;
                    s_LabelBig.fixedHeight = 22;
                    s_LabelBig.richText = true;
                }
                return s_LabelBig;
            }
        }

        static GUIStyle s_MultiLabel;
        public static GUIStyle GUIStyleMultiLabel
        {
            get
            {
                if (s_MultiLabel == null)
                {
                    s_MultiLabel = new GUIStyle(UnityEngine.GUI.skin.label);
                    s_MultiLabel.fontSize = 14;
                    s_MultiLabel.richText = true;
                    s_MultiLabel.wordWrap = true;
                }
                return s_MultiLabel;
            }
        }

        static GUIStyle s_Vertical;
        public static GUIStyle GUIStyleVertical
        {
            get
            {
                if (s_Vertical == null)
                {
                    s_Vertical = new GUIStyle();
                    s_Vertical.padding = new RectOffset(10, 10, 10, 10);
                }
                return s_Vertical;
            }
        }

        static GUIStyle s_ObjFieldMid;
        public static GUIStyle GUIStyleObjFieldMid
        {
            get
            {
                if (s_ObjFieldMid == null)
                {
                    s_ObjFieldMid = new GUIStyle(EditorStyles.objectField);
                    s_ObjFieldMid.fixedHeight = 22;
                }
                return s_ObjFieldMid;
            }
        }

        static GUIStyle s_ErrorStyle;
        public static GUIStyle GUIStyleErrorStyle
        {
            get
            {
                if (s_ErrorStyle == null)
                {
                    s_ErrorStyle = new GUIStyle();
                    s_ErrorStyle.fontSize = 16;
                    s_ErrorStyle.fixedHeight = 22;
                    s_ErrorStyle.fontStyle = FontStyle.Bold;
                    s_ErrorStyle.normal.textColor = Color.red;
                }
                return s_ErrorStyle;
            }
        }

        public static int IntPopup(string label, int selectedValue, string[] displayedOptions, int[] optionValues)
        {
            return EditorGUILayout.IntPopup(label, selectedValue, displayedOptions, optionValues, GUIStylePopupMid, GUILayout.Height(22));
        }

        public static int IntPopup(int selectedValue, string[] displayedOptions, int[] optionValues)
        {
            return EditorGUILayout.IntPopup(selectedValue, displayedOptions, optionValues, GUIStylePopupMid, GUILayout.Height(22));
        }

        public static int IntField(string label, int value)
        {
            return EditorGUILayout.IntField(label, value, GUIStyleTextFieldMid, GUILayout.Height(22));
        }

        public static string TextField(string label, string text)
        {
            return EditorGUILayout.TextField(label, text, GUIStyleTextFieldMid, GUILayout.Height(22));
        }

        public static bool ToggleButton(string label, bool value, int width = 0)
        {
            if (width > 0)
                return GUILayout.Toggle(value, label, GUIStyleToggleButtonMid, GUILayout.Height(22), GUILayout.Width(width));
            else
                return GUILayout.Toggle(value, label, GUIStyleToggleButtonMid, GUILayout.Height(22));
        }

        public static bool Toggle(string label, bool value)
        {
            return EditorGUILayout.Toggle(label, value, GUIStyleToggleMid, GUILayout.Height(22));
        }

        public static bool Button(string label, int width = 0)
        {
            if (width > 0)
                return GUILayout.Button(label, GUIStyleBtnMid, GUILayout.Width(width));
            return GUILayout.Button(label, GUIStyleBtnMid);
        }

        public static void Label(string label, int width = 0)
        {
            if (width > 0)
                GUILayout.Label(label, GUIStyleLabelMid, GUILayout.Width(width));
            else
                GUILayout.Label(label, GUIStyleLabelMid);
        }

        public static void BigLabel(string label, int width = 0)
        {
            if (width > 0)
                GUILayout.Label(label, GUIStyleLabelBig, GUILayout.Width(width));
            else
                GUILayout.Label(label, GUIStyleLabelBig);
        }

        public static void MultiLabel(string label, int width = 0)
        {
            if (width > 0)
                GUILayout.Label(label, GUIStyleMultiLabel, GUILayout.Width(width));
            else
                GUILayout.Label(label, GUIStyleMultiLabel);
        }

        public static void HLine(float height, Color color, float progress = 1.0f)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * progress, rect.height), color);
        }
    }
    #endregion

    #region  BaseEditorWindow

    public class BaseEditorWindow<T> : EditorWindow where T : EditorWindow
    {
        [EditorPrefsField]
        protected Rect m_LastPostion = new Rect(0, 0, 0, 0);

        static EditorPrefsObject<T> s_EditorPrefsObject = new EditorPrefsObject<T>();

        public static T GetWindow()
        {
            T window = (T)EditorWindow.GetWindow(typeof(T));
            return window;
        }

        virtual protected void OnEnable()
        {
            s_EditorPrefsObject.ReadObjectEditorPrefs(this);
            if (m_LastPostion.width * m_LastPostion.height == 0)
            {
                var attr = GetAttribute();
                if (attr != null)
                    m_LastPostion.size = attr.DefaultSize;
                else
                    m_LastPostion.size = new Vector2(400, 400);
            }
            this.position = m_LastPostion;
        }

        virtual protected void OnDisable()
        {
            m_LastPostion = this.position;
            s_EditorPrefsObject.WriteObjectEditorPrefs(this);
        }

        virtual protected void SetPositon()
        {
            this.position = m_LastPostion;
            s_EditorPrefsObject.WriteObjectEditorPrefs(this);
        }

        static EditorWindowAttribute GetAttribute()
        {
            Attribute[] attrs = Attribute.GetCustomAttributes(typeof(T));
            if (attrs == null || attrs.Length == 0)
                return null;
            foreach (var attr in attrs)
            {
                if (attr is EditorWindowAttribute)
                {
                    return (EditorWindowAttribute)attr;
                }
            }
            return null;
        }

        #region EditorPrefs Operation

        protected static string GetPrefs(string key)
        {
            var val = EditorPrefs.GetString(typeof(T).Name + "_" + key);
            return val;
        }

        protected static void SetPrefs(string key, string val)
        {
            EditorPrefs.SetString(typeof(T).Name + "_" + key, val);
        }

        protected static void SetPrefs(string key, int val)
        {
            EditorPrefs.SetInt(typeof(T).Name + "_" + key, val);
        }

        protected static void SetPrefs(string key, float val)
        {
            EditorPrefs.SetFloat(typeof(T).Name + "_" + key, val);
        }

        protected static void SetPrefs(string key, bool val)
        {
            EditorPrefs.SetBool(typeof(T).Name + "_" + key, val);
        }

        #endregion

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorWindowAttribute : Attribute
    {
        public Vector2Int DefaultSize { private set; get; }

        public EditorWindowAttribute(int width, int height)
        {
            DefaultSize = new Vector2Int(width, height);
        }
    }

    #endregion

    #region  BaseInspector
    public interface IInspectorWindowEditor
    {
        bool WindowMode { set; get; }
    }
    public class BaseInspector<T> : Editor, IInspectorWindowEditor where T : UnityEngine.Object
    {
        bool m_WindowMode = false;
        public bool WindowMode { set { m_WindowMode = value; } get { return m_WindowMode; } }

        protected T m_Target;

        protected virtual void OnEnable()
        {
            m_Target = target as T;
            Init();
        }


        protected virtual void Init()
        {

        }

        protected T Target { get { return m_Target; } }


    }
    #endregion

    #region  EditorPrefsObject

    /// <summary>
    /// A wrapper can automatically read and write target's fields which have EditorPrefsFieldAttribute attribute from and to EditorPrefs.
    /// T is target type
    /// </summary>
    public class EditorPrefsObject<T> where T : UnityEngine.Object
    {
        protected Dictionary<System.Reflection.FieldInfo, string> m_FieldPrefKeyDict;

        /// <summary>
        /// Write target's field-values to EditorPrefs
        /// </summary>
        /// <param name="obj">Object.</param>
        public void WriteObjectEditorPrefs(UnityEngine.Object obj)
        {
            var t = typeof(T);
            var fields = t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            foreach (var f in fields)
            {
                bool hasEditorPrefsFieldAttr = HasEditorPrefsFieldAttr(f);
                if (!hasEditorPrefsFieldAttr)
                    continue;
                WriteFieldPrefs(obj, f);
            }
        }

        /// <summary>
        /// Retrived target's field-values from EditorPrefs
        /// </summary>
        /// <param name="obj">Object.</param>
        public void ReadObjectEditorPrefs(UnityEngine.Object obj)
        {
            var t = typeof(T);
            var fields = t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            foreach (var f in fields)
            {
                bool hasEditorPrefsFieldAttr = HasEditorPrefsFieldAttr(f);
                if (!hasEditorPrefsFieldAttr)
                    continue;
                ReadFieldPrefs(obj, f);
            }
        }

        public void WriteFieldPrefs(UnityEngine.Object obj, System.Reflection.FieldInfo f)
        {
            string key = GetFieldPrefKey(f);
            if (f.FieldType == typeof(string))
            {
                EditorPrefs.SetString(key, f.GetValue(obj) as string);
            }
            else if (f.FieldType == typeof(int))
            {
                EditorPrefs.SetInt(key, (int)f.GetValue(obj));
            }
            else if (f.FieldType == typeof(float))
            {
                EditorPrefs.SetFloat(key, (float)f.GetValue(obj));
            }
            else if (f.FieldType == typeof(bool))
            {
                EditorPrefs.SetBool(key, (bool)f.GetValue(obj));
            }
            else if (f.FieldType == typeof(Vector2))
            {
                Vector2 val = (Vector2)f.GetValue(obj);
                EditorPrefs.SetString(key, Vec2ToString(ref val));
            }
            else if (f.FieldType == typeof(Vector3))
            {
                Vector3 val = (Vector3)f.GetValue(obj);
                EditorPrefs.SetString(key, Vec3ToString(ref val));
            }
            else if (f.FieldType == typeof(Vector4))
            {
                Vector4 val = (Vector4)f.GetValue(obj);
                EditorPrefs.SetString(key, Vec4ToString(ref val));
            }
            else if (f.FieldType == typeof(Rect))
            {
                Rect val = (Rect)f.GetValue(obj);
                EditorPrefs.SetString(key, RectToString(ref val));
            }
        }

        public void ReadFieldPrefs(UnityEngine.Object obj, System.Reflection.FieldInfo f)
        {
            string key = GetFieldPrefKey(f);
            if (!EditorPrefs.HasKey(key))
                return;
            if (f.FieldType == typeof(string))
            {
                var val = EditorPrefs.GetString(key);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(int))
            {
                var val = EditorPrefs.GetInt(key);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(float))
            {
                var val = EditorPrefs.GetFloat(key);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(bool))
            {
                var val = EditorPrefs.GetBool(key);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(Vector2))
            {
                Vector2 val = Vector2.zero;
                StringToVec2(EditorPrefs.GetString(key), ref val);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(Vector3))
            {
                Vector3 val = Vector3.zero;
                StringToVec3(EditorPrefs.GetString(key), ref val);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(Vector4))
            {
                Vector4 val = Vector3.zero;
                StringToVec4(EditorPrefs.GetString(key), ref val);
                f.SetValue(obj, val);
            }
            else if (f.FieldType == typeof(Rect))
            {
                Rect val = Rect.zero;
                StringToRect(EditorPrefs.GetString(key), ref val);
                f.SetValue(obj, val);
            }
        }

        public static bool HasEditorPrefsFieldAttr(System.Reflection.FieldInfo f)
        {
            var attr = Attribute.GetCustomAttribute(f, typeof(EditorPrefsFieldAttribute));
            return attr != null;
        }

        public string GetFieldPrefKey(System.Reflection.FieldInfo f)
        {
            if (m_FieldPrefKeyDict == null)
                m_FieldPrefKeyDict = new Dictionary<System.Reflection.FieldInfo, string>();
            string key = null;
            if (!m_FieldPrefKeyDict.TryGetValue(f, out key))
            {
                key = string.Format("{0}_{1}", typeof(T).Name, f.Name);
                m_FieldPrefKeyDict.Add(f, key);
            }

            return key;
        }

        static void StringToVec2(string str, ref Vector2 outVec)
        {
            outVec.x = outVec.y = 0;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            List<float> list = ListPool<float>.Get();
            list.Clear();
            StringToFloatList(str, list);
            if (list.Count > 0)
            {
                outVec.x = list[0];
            }
            if (list.Count > 1)
            {
                outVec.y = list[1];
            }
            ListPool<float>.Release(list);
        }

        static void StringToVec3(string str, ref Vector3 outVec)
        {
            outVec.x = outVec.y = outVec.z = 0;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            List<float> list = ListPool<float>.Get();
            list.Clear();
            StringToFloatList(str, list);
            if (list.Count > 0)
            {
                outVec.x = list[0];
            }
            if (list.Count > 1)
            {
                outVec.y = list[1];
            }
            if (list.Count > 2)
            {
                outVec.z = list[2];
            }
            ListPool<float>.Release(list);
        }

        static void StringToVec4(string str, ref Vector4 outVec)
        {
            outVec.x = outVec.y = outVec.z = outVec.w = 0;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            List<float> list = ListPool<float>.Get();
            list.Clear();
            StringToFloatList(str, list);
            if (list.Count > 0)
            {
                outVec.x = list[0];
            }
            if (list.Count > 1)
            {
                outVec.y = list[1];
            }
            if (list.Count > 2)
            {
                outVec.z = list[2];
            }
            if (list.Count > 3)
            {
                outVec.w = list[3];
            }
            ListPool<float>.Release(list);
        }

        static void StringToRect(string str, ref Rect outRect)
        {
            outRect = Rect.zero;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            List<float> list = ListPool<float>.Get();
            list.Clear();
            StringToFloatList(str, list);
            if (list.Count > 3)
            {
                outRect = new Rect(list[0], list[1], list[2], list[3]);
            }
            ListPool<float>.Release(list);
        }

        static void StringToFloatList(string str, List<float> outList)
        {
            string[] arr = str.Split(SPLIT_CHAR);
            foreach (var s in arr)
            {
                float val = 0;
                float.TryParse(s, out val);
                outList.Add(val);
            }
        }

        static string Vec2ToString(ref Vector2 val)
        {
            return string.Format("{0},{1}", val.x, val.y);
        }

        static string Vec3ToString(ref Vector3 val)
        {
            return string.Format("{0},{1},{2}", val.x, val.y, val.z);
        }

        static string Vec4ToString(ref Vector4 val)
        {
            return string.Format("{0},{1},{2},{3}", val.x, val.y, val.z, val.w);
        }

        static string RectToString(ref Rect val)
        {
            return string.Format("{0},{1},{2},{3}", val.x, val.y, val.width, val.height);
        }

        static char[] SPLIT_CHAR = new Char[1] { ',' };
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EditorPrefsFieldAttribute : Attribute
    {
        public EditorPrefsFieldAttribute()
        {

        }
    }

    #endregion

    #region  ListPool

    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());
        public static List<T> Get()
        {
            List<T> ret = s_ListPool.Get();
            ret.Clear();
            return ret;
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    #endregion

    #region  ObjectPool

    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int CountAll { get; private set; }
        public int CountActive { get { return CountAll - CountInactive; } }
        public int CountInactive { get { return m_Stack.Count; } }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get(System.Func<T> func = null)
        {
            T element;
            if (m_Stack.Count == 0)
            {
                if (func == null)
                    element = new T();
                else
                    element = func();
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }

    #endregion

    #region  Docker

    public static class Docker
    {
        public enum DockPosition
        {
            Left,
            Top,
            Right,
            Bottom
        }

        private static Vector2 GetFakeMousePosition(EditorWindow wnd, DockPosition position)
        {
            Vector2 mousePosition = Vector2.zero;

            // The 20 is required to make the docking work.
            // Smaller values might not work when faking the mouse position.
            switch (position)
            {
                case DockPosition.Left: mousePosition = new Vector2(20, wnd.position.size.y / 2); break;
                case DockPosition.Top: mousePosition = new Vector2(wnd.position.size.x / 2, 20); break;
                case DockPosition.Right: mousePosition = new Vector2(wnd.position.size.x - 20, wnd.position.size.y / 2); break;
                case DockPosition.Bottom: mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - 20); break;
            }

            return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
        }

        /// <summary>
        /// Docks the "docked" window to the "anchor" window at the given position
        /// </summary>
        public static void DockWindow(this EditorWindow anchor, EditorWindow docked, DockPosition position)
        {
            var anchorParent = GetParentOf(anchor);

            SetDragSource(anchorParent, GetParentOf(docked));
            PerformDrop(GetWindowOf(anchorParent), docked, GetFakeMousePosition(anchor, position));
        }

        public static void DockWindow(this EditorWindow anchor, EditorWindow docked, DockPosition position, Rect dockedPosition)
        {
            var anchorParent = GetParentOf(anchor);

            SetDragSource(anchorParent, GetParentOf(docked));
            PerformDrop(GetWindowOf(anchorParent), docked, GetFakeMousePosition(anchor, position), dockedPosition);
        }

        static object GetParentOf(object target)
        {
            var field = target.GetType().GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(target);
        }

        static object GetWindowOf(object target)
        {
            var property = target.GetType().GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
            return property.GetValue(target, null);
        }

        static void SetDragSource(object target, object source)
        {
            var field = target.GetType().GetField("s_OriginalDragSource", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, source);
        }

        static void PerformDrop(object window, EditorWindow child, Vector2 screenPoint)
        {
            var rootSplitViewProperty = window.GetType().GetProperty("rootSplitView", BindingFlags.Instance | BindingFlags.Public);
            object rootSplitView = rootSplitViewProperty.GetValue(window, null);

            var dragMethod = rootSplitView.GetType().GetMethod("DragOver", BindingFlags.Instance | BindingFlags.Public);
            var dropMethod = rootSplitView.GetType().GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);

            var dropInfo = dragMethod.Invoke(rootSplitView, new object[] { child, screenPoint });
            if (dropInfo == null) return;
            FieldInfo fi = dropInfo.GetType().GetField("dropArea");
            if (fi != null && fi.GetValue(dropInfo) == null) return;

            dropMethod.Invoke(rootSplitView, new object[] { child, dropInfo, screenPoint });
        }

        static void PerformDrop(object window, EditorWindow child, Vector2 screenPoint, Rect dropRect)
        {
            var rootSplitViewProperty = window.GetType().GetProperty("rootSplitView", BindingFlags.Instance | BindingFlags.Public);
            object rootSplitView = rootSplitViewProperty.GetValue(window, null);

            var dragMethod = rootSplitView.GetType().GetMethod("DragOver", BindingFlags.Instance | BindingFlags.Public);
            var dropMethod = rootSplitView.GetType().GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);

            var dropInfo = dragMethod.Invoke(rootSplitView, new object[] { child, screenPoint });
            if (dropInfo == null) return;
            FieldInfo fi = dropInfo.GetType().GetField("dropArea");
            if (fi != null && fi.GetValue(dropInfo) == null) return;

            FieldInfo windowPosition = dropInfo.GetType().GetField("rect");
            if (windowPosition == null) return;

            // override docked window position
            windowPosition.SetValue(dropInfo, dropRect);

            dropMethod.Invoke(rootSplitView, new object[] { child, dropInfo, screenPoint });
        }
    }
    #endregion

}