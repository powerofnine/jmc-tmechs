// Inspector Gadgets // Copyright 2019 Kybernetik //

#if UNITY_EDITOR

using InspectorGadgets.Editor.PropertyDrawers;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InspectorGadgets.Editor
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    internal sealed class TransformEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        public PositionDrawer Position { get; private set; }
        public RotationDrawer Rotation { get; private set; }
        public ScaleDrawer Scale { get; private set; }

        public Transform[] Targets { get; private set; }
        public bool CurrentIsLocalMode { get; private set; }
        public bool CurrentFreezeChildTransforms { get; private set; }
        public bool UseUniformScale { get; internal set; }

        /************************************************************************************************************************/

        public static readonly AutoPrefs.EditorBool
            DefaultToUniformScale = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "DefaultToUniformScale", true),
            IsLocalMode = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "IsLocalMode", true),
            OverrideTransformGizmos = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "OverrideTransformGizmos", true),
            FreezeChildTransforms = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "FreezeChildTransforms", false),
            DrawAllGizmos = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "DrawAllGizmos", false);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        private void OnEnable()
        {
            if (Targets == null || Targets.Length != targets.Length)
                Targets = new Transform[targets.Length];
            for (int i = 0; i < Targets.Length; i++)
                Targets[i] = targets[i] as Transform;

            if (Position == null)
            {
                Position = new PositionDrawer(this);
                Rotation = new RotationDrawer(this);
                Scale = new ScaleDrawer(this);
            }

            Position.OnEnable(serializedObject);
            Rotation.OnEnable(serializedObject);
            Scale.OnEnable(serializedObject);

            CurrentIsLocalMode = IsLocalMode;
            CurrentFreezeChildTransforms = FreezeChildTransforms;
            UseUniformScale = DefaultToUniformScale;
        }

        private void OnDisable()
        {
            Tools.hidden = false;

            IsLocalMode.Value = CurrentIsLocalMode;
            FreezeChildTransforms.Value = CurrentFreezeChildTransforms;

            Position.OnDisable();
            Rotation.OnDisable();
            Scale.OnDisable();
        }

        /************************************************************************************************************************/

        public override void OnInspectorGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                Rotation.CheckIfArbitrarilyRotated();
                if (!CurrentIsLocalMode && Rotation.IsArbitrarilyRotated)
                    UseUniformScale = false;
            }

            var height = Position.Height + Rotation.Height + Scale.Height + IGEditorUtils.Spacing;
            var area = EditorGUILayout.GetControlRect(true, height);

            DoModeButtons(ref area);

            area.height = Position.Height;
            Position.DoInspectorGUI(area);

            area.y += area.height + IGEditorUtils.Spacing;
            area.height = Rotation.Height;
            Rotation.DoInspectorGUI(area);

            area.y += area.height + IGEditorUtils.Spacing;
            area.height = Scale.Height;
            Scale.DoInspectorGUI(area);

            DoWarnings();
        }

        /************************************************************************************************************************/
        #region Mode Buttons
        /************************************************************************************************************************/

        public static readonly float ModeButtonHeight = EditorGUIUtility.singleLineHeight * 3 + 2;
        public static readonly Quaternion ModeLabelRotation = Quaternion.Euler(0, 0, -90);

        /************************************************************************************************************************/

        private void DoModeButtons(ref Rect area)
        {
            var xMin = area.xMin;
            xMin -= IGEditorUtils.IndentSize - 1;
            if (xMin < IGEditorUtils.Spacing)
                xMin = IGEditorUtils.Spacing;
            area.xMin = xMin;

            var buttonArea = area;
            buttonArea.width = InternalGUI.SmallButtonStyle.fixedWidth;

            area.xMin += buttonArea.width;

            var guiEnabled = GUI.enabled;

            buttonArea.height = InternalGUI.SmallButtonStyle.fixedHeight;

            // Local / World.
            if (GUI.Button(buttonArea, CurrentIsLocalMode ? Strings.GUI.LocalMode : Strings.GUI.WorldMode, InternalGUI.SmallButtonStyle))
            {
                CurrentIsLocalMode = !CurrentIsLocalMode;
                Rotation.CacheEulerAngles();
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
            }

            var height = InternalGUI.SmallButtonStyle.fixedHeight + IGEditorUtils.Spacing;
            buttonArea.y += height;

            if (!OverrideTransformGizmos)
                GUI.enabled = false;

            // Freeze Child Transforms.
            EditorGUI.BeginChangeCheck();
            CurrentFreezeChildTransforms = GUI.Toggle(buttonArea, CurrentFreezeChildTransforms,
                Strings.GUI.FreezeChildTransforms, InternalGUI.SmallButtonStyle);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            // Draw All Gizmos.

            buttonArea.y += height;

            if (GUI.Toggle(buttonArea, DrawAllGizmos, Strings.GUI.DrawAllGizmos, InternalGUI.SmallButtonStyle) != DrawAllGizmos)
            {
                DrawAllGizmos.Invert();
                SceneView.RepaintAll();
            }

            GUI.enabled = guiEnabled;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        private void DoWarnings()
        {
            if (!CurrentIsLocalMode && Rotation.IsArbitrarilyRotated)
            {
                EditorGUILayout.HelpBox(Strings.GUI.ScaleSkewWarning, MessageType.Warning);
                GUILayout.Space(-IGEditorUtils.Spacing);
            }

            const float PositionWarningThreshold = 100000f;

            var position = Targets[0].position;
            if (Mathf.Abs(position.x) > PositionWarningThreshold ||
                Mathf.Abs(position.y) > PositionWarningThreshold ||
                Mathf.Abs(position.z) > PositionWarningThreshold)
            {
                EditorGUILayout.HelpBox(Strings.GUI.PrecisionWarning, MessageType.Warning);
                GUILayout.Space(-IGEditorUtils.Spacing);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Scene GUI
        /************************************************************************************************************************/

        private void OnSceneGUI()
        {
            if (!OverrideTransformGizmos)
                return;

            //Tools.hidden = false; return;

            TransformPropertyDrawer currentTool;
            switch (Tools.current)
            {
                case Tool.Move: currentTool = Position; break;
                case Tool.Rotate: currentTool = Rotation; break;
                case Tool.Scale: currentTool = Scale; break;
                default:
                    Tools.hidden = false;
                    return;
            }

            // Ignore controls while holding alt so it can control the camera without clicking on handles.
            if (Event.current.alt && Event.current.type != EventType.Repaint)
                return;

            Tools.hidden = true;

            if (DrawAllGizmos || target == Selection.activeTransform)
            {
                DrawHandles(target as Transform, currentTool);
            }
        }

        /************************************************************************************************************************/

        private static readonly Color
            FrozenChildLineColor = new Color(1, 0, 0, 0.25f),
            FrozenChildDotColor = new Color(1, 0, 0, 0.5f);

        private void DrawHandles(Transform target, TransformPropertyDrawer currentTool)
        {
            var handlePosition = GetHandlePosition(target);

            EditorGUI.BeginDisabledGroup(ShouldDisableSceneTools());
            {
                currentTool.DrawTool(target, handlePosition);

                if (CurrentFreezeChildTransforms && Event.current.type == EventType.Repaint)
                {
                    var color = Handles.color;

                    var parentPosition = target.position;

                    for (int i = 0; i < target.childCount; i++)
                    {
                        var child = target.GetChild(i);
                        var childPosition = child.position;

                        Handles.color = FrozenChildLineColor;
                        Handles.DrawLine(parentPosition, childPosition);
                        Handles.color = FrozenChildDotColor;

                        Handles.SphereHandleCap(0, childPosition, child.rotation,
                            HandleUtility.GetHandleSize(childPosition) * 0.1f, EventType.Repaint);
                    }

                    Handles.color = color;
                }

                if (EditorApplication.isPlaying && target.gameObject.isStatic)
                    ShowStaticLabel(handlePosition);
            }
            EditorGUI.EndDisabledGroup();
        }

        /************************************************************************************************************************/

        private static readonly FieldInfo
            HandleOffset = typeof(Tools).GetField("handleOffset", IGEditorUtils.StaticBindings),
            LocalHandleOffset = typeof(Tools).GetField("localHandleOffset", IGEditorUtils.StaticBindings);

        public static Vector3 GetHandlePosition(Transform target)
        {

            if (target == null)
                return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

            Vector3 offset;
            if (HandleOffset != null)
                offset = (Vector3)HandleOffset.GetValue(null);
            else
                offset = Vector3.zero;

            if (LocalHandleOffset != null)
                offset += Tools.handleRotation * (Vector3)LocalHandleOffset.GetValue(null);

            switch (Tools.pivotMode)
            {
                case PivotMode.Center:
                    if (DrawAllGizmos)
                        return CalculateBounds(target).center + offset;
                    return InternalEditorUtility.CalculateSelectionBounds(true, false).center + offset;

                case PivotMode.Pivot:
                default:
                    return target.position + offset;
            }
        }

        /************************************************************************************************************************/

        private static Bounds CalculateBounds(Transform target)
        {
            var bounds = new Bounds();
            var hasOrigin = false;

            var colliders = target.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                EncapsulateBounds(ref bounds, colliders[i].bounds, ref hasOrigin);

            var colliders2D = target.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders2D.Length; i++)
                EncapsulateBounds(ref bounds, colliders2D[i].bounds, ref hasOrigin);

            var renderers = target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                EncapsulateBounds(ref bounds, renderers[i].bounds, ref hasOrigin);

            if (!hasOrigin)
                bounds.center = target.position;

            return bounds;
        }

        private static void EncapsulateBounds(ref Bounds bounds, Bounds encapsulate, ref bool hasOrigin)
        {
            if (!hasOrigin)
            {
                hasOrigin = true;
                bounds = encapsulate;
            }
            else
            {
                bounds.Encapsulate(encapsulate);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true in play mode if any of the selected objects are marked as static.
        /// </summary>
        private bool ShouldDisableSceneTools()
        {
            if (!EditorApplication.isPlaying)
                return false;

            for (int i = 0; i < Targets.Length; i++)
            {
                if (Targets[i].gameObject.isStatic)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        private static GUIContent _StaticLabel;
        private static GUIStyle _StaticLabelStyle;

        // UnityEditor.Handles.ShowStaticLabel.
        private static void ShowStaticLabel(Vector3 position)
        {
            if (_StaticLabel == null)
            {
                _StaticLabel = new GUIContent("Static");
                _StaticLabelStyle = "SC ViewAxisLabel";
                _StaticLabelStyle.alignment = TextAnchor.MiddleLeft;
                _StaticLabelStyle.fixedWidth = 0f;
            }

            Handles.color = Color.white;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.BeginGUI();
            var rect = HandleUtility.WorldPointToSizedRect(position, _StaticLabel, _StaticLabelStyle);
            rect.x += 10f;
            rect.y += 10f;
            GUI.Label(rect, _StaticLabel, _StaticLabelStyle);
            Handles.EndGUI();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif
