using System.Collections.Generic;
using System.Linq;
using MonoFSMEditor;
using UnityEditor;
using UnityEngine;
using static MonoFSM.GUIExtensions.EventExtensions;

namespace MonoFSM.Editor.MonoAnimationWindow
{
    public static class EditorIcons
        {
            public static Texture2D GetIcon(string iconNameOrPath, bool returnNullIfNotFound = false)
            {
                iconNameOrPath ??= "";

                if (icons_byName.TryGetValue(iconNameOrPath, out var cachedResult) && cachedResult) return cachedResult;


                Texture2D icon = null;

                void getCustom()
                {
                    if (icon) return;
                    if (!customIcons.ContainsKey(iconNameOrPath)) return;

                    var pngBytesString = customIcons[iconNameOrPath];
                    var pngBytes = pngBytesString.Split("-").Select(r => System.Convert.ToByte(r, 16)).ToArray();

                    icon = new Texture2D(1, 1);

                    icon.LoadImage(pngBytes);

                }
                void getBuiltin()
                {
                    if (icon) return;

                    icon = EditorGUIUtility.LoadIcon(iconNameOrPath) as Texture2D;
                    // icon = typeof(EditorGUIUtility).InvokeMethod<Texture2D>("LoadIcon", iconNameOrPath) as Texture2D;

                }

                getCustom();
                getBuiltin();

                icons_byName[iconNameOrPath] = icon;

                if (icon == null && !returnNullIfNotFound) return Texture2D.grayTexture;
                else return icon;

            }

            static Dictionary<string, Texture2D> icons_byName = new();

            static Dictionary<string, string> customIcons = new()
            {
                ["Search_"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-02-00-49-44-41-54-78-01-ED-56-3D-4F-02-41-10-1D-3E-22-C6-C4-CA-D0-9A-58-6B-87-8D-12-6B-0B-1B-7F-07-85-89-95-F4-34-D6-F2-07-2C-FD-23-D0-41-A2-8D-FF-00-8A-0B-0D-7A-70-7C-1C-EB-7B-B8-47-16-C4-70-7B-77-1C-0D-2F-B9-DC-DE-DD-CE-CE-DB-99-D9-37-27-B2-C7-8E-91-B1-99-EC-38-CE-71-A1-50-38-E0-78-34-1A-8D-8B-C5-E2-97-6C-1B-4A-A9-43-38-AB-8E-C7-E3-CF-D9-6C-E6-29-0D-DF-F7-5D-BE-C3-B7-1A-E7-C8-36-E0-BA-EE-1D-1C-39-6A-03-40-CC-E5-5C-49-12-9E-E7-55-B0-B0-AF-1D-F8-93-C9-A4-81-77-0F-B8-97-79-E9-71-DB-20-E1-83-44-45-92-00-77-13-38-47-04-BA-83-C1-E0-FA-BF-B9-20-71-85-39-3D-83-44-BC-48-30-9F-0C-A9-5E-B0-D7-E9-74-8E-36-D9-70-0E-89-6A-C2-4E-AC-9A-D0-45-35-07-43-1D-D6-0E-51-2A-07-51-63-D1-4A-54-B0-B2-B5-F3-B6-58-02-36-4D-DA-72-8D-B0-36-59-F3-81-E7-3C-9B-CD-9E-72-8C-75-5E-C5-12-B0-79-E3-3D-9F-CF-9F-71-AD-30-36-4B-04-28-32-B9-5C-6E-9E-F3-4C-26-D3-12-4B-4C-A7-D3-96-B6-2D-04-82-65-45-60-17-58-22-40-79-65-0D-71-8C-7B-49-2C-81-D0-97-B4-AD-CB-B5-24-0A-8C-22-6C-8A-25-70-04-DB-B6-45-F8-07-3C-42-86-FA-59-1D-43-E3-F8-D6-24-2A-28-22-81-FE-53-5C-C2-0A-11-45-2B-E8-0B-2A-6E-73-32-A5-98-0B-87-90-E2-6E-62-52-6C-90-58-34-A3-40-98-90-DB-D5-66-D4-80-F3-45-C3-62-03-A3-ED-70-38-BC-C1-AB-0B-89-0B-1D-09-57-6D-00-53-16-EC-1C-24-6E-F1-3C-82-DD-37-08-5F-4A-5C-A8-DF-1F-92-1A-2B-1B-42-E3-1A-ED-D7-D3-3F-24-D5-20-E7-DC-39-9D-1B-73-26-F8-7E-2F-49-81-F2-DA-EF-F7-4F-78-AD-93-5A-38-3B-E7-CE-D5-F2-CF-CA-14-A9-8A-DE-9C-6C-C1-B0-73-E7-AB-69-42-6A-5E-24-2D-30-EC-DC-F9-1A-12-8F-92-16-50-0B-4F-6B-8A-F5-5D-D2-04-76-5C-37-09-20-3D-75-49-1B-28-C0-67-5C-1F-74-0E-0E-79-D9-63-8F-15-FC-00-17-02-EB-AB-1A-4B-B3-E7-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Cross"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-00-C5-49-44-41-54-78-01-ED-96-D1-0D-83-30-0C-44-9D-4E-D0-51-BA-02-13-B5-23-A4-1B-A4-13-31-42-3B-4A-37-70-8D-6A-04-42-E0-D8-88-E0-1F-3F-29-8A-50-1C-DF-05-48-62-80-20-08-9C-49-D2-20-22-5E-A9-BB-53-1B-FA-67-4A-E9-0B-0A-66-F3-06-5E-DA-79-6B-89-32-4E-BC-39-71-55-9C-63-47-B2-14-7F-01-3D-37-6A-BD-64-82-C7-7A-8E-1D-A9-9A-06-29-E1-62-35-9B-6F-C2-12-7B-B8-89-66-E2-1A-81-E6-E2-0A-13-ED-C5-2B-26-CE-11-57-98-D8-25-6E-D9-86-FE-B8-7E-02-D7-9F-10-3D-B7-21-7A-1E-44-96-C4-4D-4C-D0-E4-62-49-B8-61-22-4B-1A-B5-6D-38-BF-C7-3F-D4-3A-E9-6E-E7-B1-8E-63-55-68-0A-92-07-3F-16-63-41-92-E1-BF-80-B2-BB-20-09-82-E0-0C-7E-54-36-6A-69-F6-3F-13-EF-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Plus"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-00-A7-49-44-41-54-78-01-ED-D5-01-09-84-30-14-06-E0-7F-C7-05-B8-06-77-0D-CE-08-46-31-C2-1A-68-04-4D-A0-51-8C-A0-0D-B4-81-0D-E6-13-14-C6-10-C5-4D-11-E1-FF-60-F8-78-0C-F7-B3-E1-04-88-02-18-63-B4-8C-04-77-98-17-5F-64-F0-F4-82-BF-C8-AA-BF-F0-14-12-E0-14-0C-C0-00-0C-A0-D6-9A-72-B1-A4-F2-F8-61-5B-6C-CD-E9-64-D4-3B-F3-1B-A5-54-81-3D-D3-D5-6A-AE-A3-DD-F5-D6-8E-E0-83-EB-0C-6E-E3-ED-36-64-9B-72-49-3A-95-7F-6C-8B-71-EC-08-7A-79-77-85-B3-48-C8-CA-DA-DA-12-9E-F8-19-32-00-03-3C-3A-40-67-D5-2D-EE-30-FF-37-34-88-02-8C-19-03-9B-84-46-97-A8-ED-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Star"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-01-16-49-44-41-54-78-01-ED-94-6D-0D-C2-30-10-86-DF-11-04-20-61-12-70-C0-1C-50-07-AB-03-90-80-04-50-00-28-19-0E-90-00-0E-C0-C1-71-CD-BA-B0-B1-86-B5-BD-0E-FE-EC-49-2E-6D-2E-D7-CB-F5-BE-80-89-09-01-44-94-1B-81-80-19-64-28-16-0D-01-73-C8-28-59-9E-F8-07-36-FD-0D-39-22-91-94-40-B5-EE-1A-91-48-02-58-B7-EE-2B-FC-92-8F-F4-37-2C-10-41-6C-06-0A-87-4E-23-82-DE-14-F0-4F-0A-3E-F2-81-77-1B-87-AE-E4-B7-43-13-71-CF-B2-EC-F2-D5-C2-A4-92-E5-44-E9-39-05-95-89-8D-B7-2C-0F-92-63-7C-6C-11-03-D5-CD-76-A3-78-AE-24-5C-D5-4D-20-3B-0A-67-8F-94-B0-43-E5-99-0D-63-53-60-0C-D8-71-E5-11-40-85-31-A0-7A-3A-7C-30-4D-E7-DD-ED-21-8B-48-79-DA-2D-02-6C-83-02-58-3B-74-07-96-B3-43-5F-22-25-8E-F4-77-66-9B-EF-9A-BA-3B-23-A8-0C-3E-01-E8-96-73-E7-6C-53-7F-67-68-A4-82-9D-1D-AD-D3-C1-D9-A6-F7-CE-38-22-15-F6-D7-45-80-BD-B2-6F-E4-65-60-27-4B-8A-58-A7-B6-24-E9-FA-60-62-62-2C-5E-30-1D-6B-34-83-5B-F0-2B-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Star Hollow"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-01-5A-49-44-41-54-78-01-ED-96-FD-6D-C2-30-10-C5-2F-15-03-B0-41-33-42-47-48-37-C8-06-64-03-BA-41-D9-80-6E-00-1B-B4-9D-20-DD-20-EA-04-C9-06-65-83-EB-3B-F1-2C-8C-04-F9-B0-AD-F0-4F-7E-D2-29-16-3A-5F-EC-77-1F-41-64-61-21-02-55-CD-CD-24-82-27-89-A3-84-55-12-C1-4A-E2-D8-C0-4E-F2-08-28-BF-23-97-40-62-52-60-F2-77-72-56-A0-92-40-32-09-04-B7-AE-79-00-8B-F1-9C-65-D9-AB-CC-85-27-7F-09-2B-B8-5E-CB-5C-E0-65-15-EC-8F-EB-B5-AD-61-6F-12-C0-EA-46-F0-02-8F-7C-60-DF-16-F6-65-0B-48-7F-C2-9E-6F-2C-37-78-0E-75-44-07-FF-9F-5E-0F-DE-E8-A8-C3-94-FE-A1-47-F8-1F-27-A5-C9-24-A5-B4-6D-48-9B-B1-4E-9A-98-F4-B8-20-ED-D4-20-F0-DD-72-4F-A3-91-A3-DA-05-DC-51-C6-43-5F-40-A6-EF-40-DF-0F-49-09-5B-CE-D4-68-7B-7C-1A-FA-14-32-92-D1-93-10-D5-6B-55-DF-C1-7E-7B-DC-AC-0B-86-2B-3D-04-CA-6B-54-DE-6F-57-9F-63-AF-70-D3-0F-25-3D-0F-1F-75-2F-64-EB-B9-2E-A9-EE-1D-32-E5-01-3E-61-35-5F-B2-77-85-A6-97-99-F1-4E-3F-F3-A9-25-25-DE-CD-76-B7-7A-9B-EA-38-35-F6-C9-D3-E0-C9-AF-F7-7A-DB-9B-19-9A-3C-0D-53-7A-5B-BD-99-21-A9-E0-AD-8B-09-FE-25-F7-C4-A7-01-41-5E-34-FC-5B-30-DF-7F-84-85-85-50-FE-01-12-E7-01-A3-5F-51-F9-4C-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Collapse"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-00-CE-49-44-41-54-78-01-ED-95-01-0D-C2-30-10-45-7F-A7-60-12-2A-01-09-93-80-04-1C-80-03-EA-60-16-70-30-1C-20-01-09-93-C0-1C-1C-BF-61-24-CD-18-0C-9A-DE-1A-92-BE-A4-49-D7-25-F7-7F-72-FF-5A-A0-90-19-33-3D-10-91-1D-F4-18-8C-31-E7-B7-7F-29-7E-10-7D-5C-A8-59-4D-3C-D4-D0-67-08-3F-E6-5A-B0-D5-34-C2-16-9C-50-48-05-DB-B5-F7-C1-45-0E-28-BC-19-53-7D-F3-7B-AC-09-05-2D-57-1F-8C-96-DF-5B-AC-05-C5-AE-33-F3-ED-CF-F4-C7-98-22-ED-87-4B-A6-85-26-14-38-CA-32-3A-A1-64-E1-46-BE-A7-41-4A-E4-35-74-4B-F8-C9-B0-48-01-0B-D5-3F-8A-3F-E9-25-45-28-59-A4-93-78-3A-68-C1-E2-2E-10-72-88-A4-42-66-8A-81-62-A0-18-C8-6E-20-1A-79-BC-0F-97-71-59-14-FE-95-3B-1B-9A-F9-A3-61-43-3F-A9-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Plus Thicker"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-00-A1-49-44-41-54-78-01-ED-96-D1-09-84-30-10-44-27-C7-15-70-D7-81-25-D9-C1-9D-95-A8-1D-D9-89-25-C4-0E-4C-07-71-85-80-12-22-91-B8-E8-CF-3C-98-8F-2C-21-79-10-D8-0D-40-48-21-DE-FB-5A-32-4B-AC-E4-87-BB-91-4B-47-BF-61-51-C8-0B-E5-7C-A0-C0-15-01-15-28-40-01-0A-98-B8-10-BA-5A-87-3C-55-B4-9E-32-FB-9D-A4-37-C6-0C-39-01-9B-38-5C-0B-27-02-DF-7D-21-F5-04-2A-1D-EE-48-20-2E-BC-13-9B-1A-49-7B-42-A4-8A-D6-13-F2-F4-D0-22-4C-C1-47-87-91-0A-14-A0-00-05-B4-04-1C-EE-46-9A-CF-3F-34-A3-F5-6B-5E-83-90-42-16-B4-42-4C-CD-3F-8F-0E-C4-00-00-00-00-49-45-4E-44-AE-42-60-82",
                ["Dropdown"] = "89-50-4E-47-0D-0A-1A-0A-00-00-00-0D-49-48-44-52-00-00-00-20-00-00-00-20-08-06-00-00-00-73-7A-7A-F4-00-00-00-09-70-48-59-73-00-00-0B-13-00-00-0B-13-01-00-9A-9C-18-00-00-00-01-73-52-47-42-00-AE-CE-1C-E9-00-00-00-04-67-41-4D-41-00-00-B1-8F-0B-FC-61-05-00-00-00-78-49-44-41-54-78-01-ED-D0-B1-0D-80-20-14-84-E1-D3-D2-15-98-82-41-18-C6-51-5C-85-41-98-C2-15-AC-F1-E8-2C-50-91-04-8C-F1-BE-E4-35-0A-FC-04-40-44-44-FE-6E-28-59-14-09-15-06-BA-5B-33-A2-CC-82-E7-6A-F6-9C-E3-23-F8-58-CE-A3-05-1E-1C-0A-E2-01-AD-F0-F0-89-B3-5E-C4-D3-BF-09-2D-31-60-38-5B-26-9E-BE-19-F4-C0-90-CD-5C-C0-A2-27-06-DD-21-EE-F0-06-86-E7-34-10-11-91-2F-DB-01-40-06-CB-42-92-53-E4-4B-00-00-00-00-49-45-4E-44-AE-42-60-82",
            };
        }
    public static class GUIExtension
    {
        // public static Rect AddWidthFromMid(this Rect rect, float f) => rect.SetWidthFromMid(rect.width + f);
        // public static Rect AddWidthFromRight(this Rect rect, float f) => rect.SetWidthFromRight(rect.width + f);
        public static int Pow(this int f, int pow) => (int)Mathf.Pow(f, pow);
        public static float Pow(this float f, float pow) => Mathf.Pow(f, pow);

        private static readonly Stack<Color> _guiColorStack = new();
        public static void SetGUIColor(Color c)
        {
            _guiColorStack.Push(GUI.color);
            GUI.color *= c;

        }
        public static void ResetGUIColor()
        {
            GUI.color = _guiColorStack.Pop();
        }
        public static Color Greyscale(float brightness, float alpha = 1) => new(brightness, brightness, brightness, alpha);
        public static Color SetAlpha(this Color color, float alpha) { color.a = alpha; return color; }
        
        public static bool IconButton(Rect rect, string iconName, float iconSize = default, Color color = default, Color colorHovered = default, Color colorPressed = default)
        {
            var id = EditorGUIUtility.GUIToScreenRect(rect).GetHashCode();// GUIUtility.GetControlID(FocusType.Passive, rect);
            var isPressed = id == _pressedIconButtonId;

            var wasActivated = false;

            void icon()
            {
                if (!curEvent.isRepaint) return;


                if (color == default)
                    color = Color.white;

                if (colorHovered == default)
                    colorHovered = Color.white;

                if (colorPressed == default)
                    colorPressed = Color.white.SetAlpha(.6f);


                if (rect.IsHovered())
                    color = colorHovered;

                if (isPressed)
                    color = colorPressed;


                if (iconSize == default)
                    iconSize = Mathf.Min(rect.width,rect.height);

                var iconRect = rect.SetSizeFromMid(iconSize);



                SetGUIColor(color);

                GUI.DrawTexture(iconRect, EditorIcons.GetIcon(iconName));

                ResetGUIColor();


            }
            void mouseDown()
            {
                if (!curEvent.isMouseDown) return;
                if (!rect.IsHovered()) return;

                _pressedIconButtonId = id;

                curEvent.Use();

            }
            void mouseUp()
            {
                if (!curEvent.isMouseUp) return;
                if (!isPressed) return;

                _pressedIconButtonId = 0;

                if (rect.IsHovered())
                    wasActivated = true;

                curEvent.Use();

            }
            void mouseDrag()
            {
                if (!curEvent.isMouseDrag) return;
                if (!isPressed) return;

                curEvent.Use();

            }

            // rect.MarkInteractive();

            icon();
            mouseDown();
            mouseUp();
            mouseDrag();

            return wasActivated;

        }

        static int _pressedIconButtonId;
    }
}