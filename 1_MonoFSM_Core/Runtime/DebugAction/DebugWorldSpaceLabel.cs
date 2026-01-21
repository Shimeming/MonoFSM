using MonoDebugSetting;
using MonoFSM.Foundation;
using MonoFSM.Variable;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace _0_MonoDebug.Gizmo
{
    /// <summary>
    /// FIXME: 可以統一執行？有差嗎？
    /// </summary>
    public class DebugWorldSpaceLabel : AbstractDescriptionBehaviour
    {
        [Header("顯示設定")] public string text = "";
        public Vector3 offset = new Vector3(0, 2f, 0); // 讓文字飄在物體頭頂

        [OnValueChanged(nameof(ResetStyle))] public int fontSize = 24;
        [OnValueChanged(nameof(ResetStyle))] public Color fontColor = Color.green;

        public enum OutlineMode
        {
            None,
            Shadow,
            FourDirections,
            EightDirections,
            Full
        }

        [Header("可讀性設定")] [Tooltip("外框模式。Shadow 最便宜，Full 最貴")]
        public OutlineMode outlineMode = OutlineMode.Shadow;

        [ShowIf("@outlineMode != OutlineMode.None")] [Tooltip("外框顏色，建議使用深色或黑色")]
        public Color outlineColor = Color.black;

        [ShowIf("@outlineMode != OutlineMode.None")] [Tooltip("外框透明度")] [Range(0f, 1f)]
        public float outlineAlpha = 1f;

        [ShowIf("@outlineMode != OutlineMode.None")]
        [Tooltip("外框像素偏移（Shadow / Four / Eight 使用）")]
        [Range(1, 4)]
        public int outlineOffsetPx = 1;

        [ShowIf("outlineMode", OutlineMode.Full)]
        [Tooltip("Full 模式額外半徑（2 代表 5x5-1=24 次，3 代表 7x7-1=48 次）")]
        [Range(1, 3)]
        public int outlineRadius = 1;

        [Space] [Tooltip("使用半透明背景")] public bool useBackground = false;
        [ShowIf("useBackground")] public Color backgroundColor = new Color(0, 0, 0, 0.7f);

        [Space] [Tooltip("在 Scene 視圖中可點擊選取")] public bool clickableInScene = true;

        private GUIStyle _guiStyle;
        private GUIStyle _gizmoStyle;
        private GUIStyle _guiOutlineStyle;
        private GUIStyle _gizmoOutlineStyle;
        private GUIStyle _backgroundStyle;

        private GUIStyle guiStyle
        {
            get
            {
                if (_guiStyle == null)
                {
                    _guiStyle = new GUIStyle();
                    _guiStyle.alignment = TextAnchor.MiddleCenter;
                }

                _guiStyle.fontSize = fontSize;
                _guiStyle.normal.textColor = GetDynamicColor();
                return _guiStyle;
            }
        }

        private GUIStyle gizmoStyle
        {
            get
            {
                if (_gizmoStyle == null)
                {
                    _gizmoStyle = new GUIStyle();
                    _gizmoStyle.alignment = TextAnchor.MiddleCenter;
                }

                _gizmoStyle.fontSize = fontSize;
                _gizmoStyle.normal.textColor = GetDynamicColor();
                return _gizmoStyle;
            }
        }

        private GUIStyle guiOutlineStyle
        {
            get
            {
                if (_guiOutlineStyle == null)
                {
                    _guiOutlineStyle = new GUIStyle(guiStyle);
                }

                _guiOutlineStyle.fontSize = fontSize;
                _guiOutlineStyle.alignment = TextAnchor.MiddleCenter;
                _guiOutlineStyle.normal.textColor = GetOutlineColor();
                return _guiOutlineStyle;
            }
        }

        private GUIStyle gizmoOutlineStyle
        {
            get
            {
                if (_gizmoOutlineStyle == null)
                {
                    _gizmoOutlineStyle = new GUIStyle(gizmoStyle);
                }

                _gizmoOutlineStyle.fontSize = fontSize;
                _gizmoOutlineStyle.alignment = TextAnchor.MiddleCenter;
                _gizmoOutlineStyle.normal.textColor = GetOutlineColor();
                return _gizmoOutlineStyle;
            }
        }

        private Color GetDynamicColor()
        {
            if (_variable is VarBool varBool)
            {
                return varBool.Value ? Color.green : Color.red;
            }

            // 非 VarBool 或沒有 variable 時使用黃色
            return _variable != null ? Color.yellow : fontColor;
        }

        private Color GetOutlineColor()
        {
            var c = outlineColor;
            c.a *= outlineAlpha;
            return c;
        }

        [AutoParent] public AbstractMonoVariable _variable;

        protected override string DescriptionTag => "DebugLabel";
        public override string Description => _variable?.Description;

        private void ResetStyle()
        {
            _guiStyle = null;
            _gizmoStyle = null;
            _guiOutlineStyle = null;
            _gizmoOutlineStyle = null;
            _backgroundStyle = null;
        }


        private void DrawLabelWithOptionalOutline(Rect rect, string displayText, GUIStyle mainStyle,
            GUIStyle outlineStyle)
        {
            if (outlineMode != OutlineMode.None)
            {
                DrawOutline(rect, displayText, outlineStyle);
            }

            GUI.Label(rect, displayText, mainStyle);
        }

        private void DrawOutline(Rect rect, string displayText, GUIStyle outlineStyle)
        {
            int o = Mathf.Max(1, outlineOffsetPx);

            switch (outlineMode)
            {
                case OutlineMode.Shadow:
                    GUI.Label(new Rect(rect.x + o, rect.y + o, rect.width, rect.height),
                        displayText, outlineStyle);
                    return;

                case OutlineMode.FourDirections:
                    GUI.Label(new Rect(rect.x - o, rect.y, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x + o, rect.y, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x, rect.y - o, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x, rect.y + o, rect.width, rect.height), displayText,
                        outlineStyle);
                    return;

                case OutlineMode.EightDirections:
                    GUI.Label(new Rect(rect.x - o, rect.y, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x + o, rect.y, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x, rect.y - o, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x, rect.y + o, rect.width, rect.height), displayText,
                        outlineStyle);
                    GUI.Label(new Rect(rect.x - o, rect.y - o, rect.width, rect.height),
                        displayText, outlineStyle);
                    GUI.Label(new Rect(rect.x + o, rect.y - o, rect.width, rect.height),
                        displayText, outlineStyle);
                    GUI.Label(new Rect(rect.x - o, rect.y + o, rect.width, rect.height),
                        displayText, outlineStyle);
                    GUI.Label(new Rect(rect.x + o, rect.y + o, rect.width, rect.height),
                        displayText, outlineStyle);
                    return;

                case OutlineMode.Full:
                    int r = Mathf.Clamp(outlineRadius, 1, 3);
                    for (int x = -r; x <= r; x++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            if (x == 0 && y == 0)
                                continue;

                            GUI.Label(new Rect(rect.x + x, rect.y + y, rect.width, rect.height),
                                displayText, outlineStyle);
                        }
                    }

                    return;
            }
        }

        void OnGUI()
        {
            if (!RuntimeDebugSetting.IsDebugMode)
                return;

            // 1. 取得主攝影機
            var cam = Camera.main;
            if (cam == null)
                return;

            // 2. 計算物體在世界空間的實際位置 (加上偏移量)
            var worldPos = transform.position + offset;

            // 3. 轉成螢幕座標
            var screenPos = cam.WorldToScreenPoint(worldPos);

            // 如果 z < 0，代表物體在攝影機背後
            if (screenPos.z <= 0)
                return;

            // 4. 修正 Y 軸 (GUI 的 Y 是從上往下算，ScreenPoint 是從下往上算)
            float guiY = cam.pixelHeight - screenPos.y;

            // 5. 準備顯示文字（不要覆蓋 serialized 的 text，避免和其它地方互相影響）
            string displayText = text;
            if (_variable != null)
                displayText = _variable.Description + ": " + _variable.StringValue;

            // 6. 用字體真實尺寸計算 Rect，避免固定寬高導致置中偏移
            Vector2 size = guiStyle.CalcSize(new GUIContent(displayText));
            size.x = Mathf.Max(size.x, 40f);
            size.y = Mathf.Max(size.y, 18f);

            var rect = new Rect(screenPos.x - size.x * 0.5f, guiY - size.y * 0.5f, size.x, size.y);

            if (useBackground)
            {
                DrawBackground(rect);
            }

            DrawLabelWithOptionalOutline(rect, displayText, guiStyle, guiOutlineStyle);
        }

        private void DrawBackground(Rect rect)
        {
            var backgroundRect = new Rect(rect);
            backgroundRect.xMin -= 5;
            backgroundRect.xMax += 5;
            backgroundRect.yMin -= 2;
            backgroundRect.yMax += 2;

            Color oldColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.Label(backgroundRect, GUIContent.none, backgroundStyle);
            GUI.color = oldColor;
        }

        void OnDrawGizmos()
        {
            if (!RuntimeDebugSetting.IsDebugMode)
                return;

            Vector3 labelPosition = transform.position + offset;

            Gizmos.color = GetDynamicColor();
            Gizmos.DrawSphere(labelPosition, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, labelPosition);

            string displayText = text;
            if (_variable != null)
                displayText = _variable.Description + ": " + _variable.StringValue;

            DrawSceneLabel(labelPosition, displayText);
        }

        private void DrawSceneLabel(Vector3 worldPosition, string displayText)
        {
            // Scene 視窗應優先使用 SceneView 的相機
            Camera cam;

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                cam = sceneView.camera;
            }
            else
            {
                cam = Camera.current;
                if (cam == null)
                    return;
            }

            Vector3 sp = cam.WorldToScreenPoint(worldPosition);
            if (sp.z <= 0)
                return;

            Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPosition);
            Vector2 size = gizmoStyle.CalcSize(new GUIContent(displayText));
            var rect = new Rect(guiPos.x - size.x * 0.5f, guiPos.y - size.y * 0.5f, size.x, size.y);
            Gizmos.DrawIcon(worldPosition + Vector3.up * 1f, "sv_label_2", true);
            Handles.BeginGUI();
            // 可點擊區域（背景或文字區域）
            // if (clickableInScene)
            // {
            //     Rect clickRect = useBackground
            //         ? new Rect(rect.x - 5, rect.y - 2, rect.width + 10, rect.height + 4)
            //         : rect;
            //     // Debug.Log("Click Rect: " + clickRect, this);
            //     // 檢測點擊
            //     Event e = Event.current;
            //     if (e.type == EventType.ExecuteCommand)
            //         Debug.Log("Event Type: " + e.type, this);
            //
            //     if (e.type == EventType.ExecuteCommand &&
            //         clickRect.Contains(e.mousePosition))
            //     {
            //         Debug.Log("Scene Label Clicked: " + displayText, this);
            //         e.Use(); // 消費事件，避免被其他處理
            //         OnSceneLabelClicked();
            //         GUI.changed = true;
            //     }
            //
            //     // 顯示 hover 效果（滑鼠在上面時改變游標）
            //     if (clickRect.Contains(e.mousePosition))
            //     {
            //         EditorGUIUtility.AddCursorRect(clickRect, MouseCursor.Link);
            //     }
            // }

            if (useBackground)
            {
                DrawSceneBackground(rect);
            }

            DrawLabelWithOptionalOutline(rect, displayText, gizmoStyle, gizmoOutlineStyle);

            Handles.EndGUI();
        }

        private void OnSceneLabelClicked()
        {
            // 選中這個 GameObject
            Selection.activeGameObject = gameObject;
            Debug.Log($"[DebugWorldSpaceLabel] Clicked on label of '{gameObject.name}'", this);
        }

        private void DrawSceneBackground(Rect rect)
        {
            var backgroundRect = new Rect(rect);
            backgroundRect.xMin -= 5;
            backgroundRect.xMax += 5;
            backgroundRect.yMin -= 2;
            backgroundRect.yMax += 2;

            Color oldColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.Label(backgroundRect, GUIContent.none, backgroundStyle);
            GUI.color = oldColor;
        }

        private GUIStyle backgroundStyle
        {
            get
            {
                if (_backgroundStyle == null)
                {
                    _backgroundStyle = new GUIStyle();
                    _backgroundStyle.normal.background = Texture2D.whiteTexture;
                }

                return _backgroundStyle;
            }
        }
    }
}

#endif
