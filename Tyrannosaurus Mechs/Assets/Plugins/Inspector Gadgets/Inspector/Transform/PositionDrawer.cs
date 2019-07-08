// Inspector Gadgets // Copyright 2019 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InspectorGadgets.Editor.PropertyDrawers
{
    internal sealed class PositionDrawer : TransformPropertyDrawer
    {
        /************************************************************************************************************************/

        public PositionDrawer(TransformEditor parentEditor)
            : base(parentEditor,
                  "Position",
                  "The local position of this Game Object relative to the parent.",
                  "The world position of this Game Object.")
        { }

        /************************************************************************************************************************/

        public override void OnEnable(SerializedObject transform)
        {
            base.OnEnable(transform);
            _MainSerializedProperty = transform.FindProperty("m_LocalPosition");
            _XSerializedProperty = _MainSerializedProperty.FindPropertyRelative("x");
            _YSerializedProperty = _MainSerializedProperty.FindPropertyRelative("y");
            _ZSerializedProperty = _MainSerializedProperty.FindPropertyRelative("z");
        }

        /************************************************************************************************************************/

        public override Vector3 GetLocalValue(Transform target) { return target.localPosition; }
        public override Vector3 GetWorldValue(Transform target) { return target.position; }

        public override void SetLocalValue(Transform target, Vector3 localPosition)
        {
            if (ParentEditor.CurrentFreezeChildTransforms)
                CacheChildPositions(target);

            target.localPosition = localPosition;

            if (ParentEditor.CurrentFreezeChildTransforms)
                RevertChildPositions(target);
        }

        public override void SetWorldValue(Transform target, Vector3 position)
        {
            if (ParentEditor.CurrentFreezeChildTransforms)
                CacheChildPositions(target);

            target.position = position;

            if (ParentEditor.CurrentFreezeChildTransforms)
                RevertChildPositions(target);
        }

        public override string UndoName { get { return "Move"; } }
        public override Vector3 DefaultValue { get { return Vector3.zero; } }

        public override Vector3 SnapValues { get { return IGEditorUtils.MoveSnapVector; } }
        public override Vector3 SnapValue(Vector3 value)
        {
            return IGEditorUtils.SnapPosition(value);
        }

        /************************************************************************************************************************/

        protected override Vector3 GetDragDirection(Transform target)
        {
            return target.forward;
        }

        /************************************************************************************************************************/

        private static Vector3[] _CachedChildPoistions;

        public static void CacheChildPositions(Transform parent)
        {
            Array.Resize(ref _CachedChildPoistions, parent.childCount);
            for (int i = 0; i < _CachedChildPoistions.Length; i++)
            {
                _CachedChildPoistions[i] = parent.GetChild(i).position;
            }
        }

        public static void RevertChildPositions(Transform parent)
        {
            for (int i = 0; i < _CachedChildPoistions.Length; i++)
            {
                parent.GetChild(i).position = _CachedChildPoistions[i];
            }
        }

        /************************************************************************************************************************/

        private static readonly GUIContent PasteContentValue = new GUIContent("P");
        public override GUIContent PasteContent { get { return PasteContentValue; } }

        private static readonly GUIContent SnapContentValue = new GUIContent("S");
        public override GUIContent SnapContent { get { return SnapContentValue; } }

        protected override string GetSnapTooltip()
        {
            return SnapValues.ToString();
        }

        /************************************************************************************************************************/

        private static readonly NullableVector4 ClipboardValue = new NullableVector4(Vector3.zero);
        public override NullableVector4 Clipboard { get { return ClipboardValue; } }

        /************************************************************************************************************************/

        public override void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            AddPropertyNameItem(menu, property);

            var axis = CurrentVectorAxis;
            if (axis < 0)// Vector Label.
            {
                AddVectorClipboardFunctions(menu);

                menu.AddSeparator("");

                SerializedPropertyContextMenu.AddPropertyModifierFunction(menu, property, "Negate", (targetProperty) => targetProperty.vector3Value *= -1);
                SerializedPropertyContextMenu.AddPropertyModifierFunction(menu, property, "Snap to Grid " + IGEditorUtils.MoveSnapVector, () => property.vector3Value = IGEditorUtils.SnapPosition(property.vector3Value));
                AddSnapToGroundFunctions(menu);
                SerializedPropertyContextMenu.Vector3MenuHandler.AddLogFunction(menu, property);
            }
            else// X, Y, Z.
            {
                AddFloatClipboardFunctions(menu, axis);

                menu.AddSeparator("");

                SerializedPropertyContextMenu.AddPropertyModifierFunction(menu, property, "Negate", (targetProperty) => targetProperty.floatValue *= -1);

                AddSnapFloatToGridItem(menu, property, axis);

                if (axis == 1)// Y.
                    AddSnapToGroundFunctions(menu);

                menu.AddSeparator("");

                SerializedPropertyContextMenu.AddLogValueFunction(menu, property, (targetProperty) => targetProperty.floatValue);
            }
        }

        /************************************************************************************************************************/

        public static void AddSnapFloatToGridItem(GenericMenu menu, SerializedProperty property, int axis)
        {
            var snap = IGEditorUtils.MoveSnapVector[axis];
            SerializedPropertyContextMenu.AddPropertyModifierFunction(menu, property, "Snap to Grid (" + snap + ")",
                () => property.floatValue = IGEditorUtils.Snap(property.floatValue, snap));
        }

        /************************************************************************************************************************/

        protected override string GetCurrentModePropertyPrefix()
        {
            return ParentEditor.CurrentIsLocalMode ? "localPosition" : "position";
        }

        /************************************************************************************************************************/
        #region Snap to Ground
        /************************************************************************************************************************/

        private delegate void GroundPointSelector(float target, ref float best, float current);

        public static readonly AutoPrefs.EditorFloat
            SnapToGroundDistance = new AutoPrefs.EditorFloat(Strings.PrefsKeyPrefix + "SnapToGroundDistance", 10);
        public static readonly AutoPrefs.EditorInt
            SnapToGroundLayers = new AutoPrefs.EditorInt(Strings.PrefsKeyPrefix + "SnapToGroundLayers", Physics.DefaultRaycastLayers & ~AutoHideUI.UILayerMask.Value);

        /************************************************************************************************************************/

        private void AddSnapToGroundFunctions(GenericMenu menu)
        {
            AddSnapToGroundFunction(menu, "Snap to Ground (Closest)", GetClosest);
            AddSnapToGroundFunction(menu, "Snap to Ground (Highest)", GetHighest);
        }

        private void AddSnapToGroundFunction(GenericMenu menu, string label, GroundPointSelector selectGroundPoint)
        {
            menu.AddItem(new GUIContent(label), false, () =>
            {
                var targets = _MainSerializedProperty.serializedObject.targetObjects;
                Undo.RecordObjects(targets, "Snap to Ground");

                // Ignore all selected objects and their children.
                var ignore = new HashSet<object>();
                for (int i = 0; i < targets.Length; i++)
                {
                    var target = targets[i] as Transform;
                    ignore.UnionWith(target.GetComponentsInChildren<Collider>());
                    ignore.UnionWith(target.GetComponentsInChildren<Collider2D>());
                }

                for (int i = 0; i < targets.Length; i++)
                {
                    var target = targets[i] as Transform;

                    // Raycast down from above the target and find the closest hit to them which isn't one of their children.

                    var position = GetBasePosition(target);
                    var basePositionOffset = target.position.y - position.y;

                    var origin = position;
                    origin.y += SnapToGroundDistance;

                    var bestValue = float.PositiveInfinity;

                    var layers = Physics.DefaultRaycastLayers & ~AutoHideUI.UILayerMask.Value;

                    // 2D.
                    var hits2D = Physics2D.RaycastAll(origin, Vector3.down, SnapToGroundDistance.Value * 2, layers);
                    for (int j = 0; j < hits2D.Length; j++)
                    {
                        var hit = hits2D[j];
                        if (ignore.Contains(hit.collider))
                            continue;

                        if (bestValue == float.PositiveInfinity)
                            bestValue = hit.point.y;
                        else
                            selectGroundPoint(position.y, ref bestValue, hit.point.y);
                    }

                    // 3D.
                    var hits3D = Physics.RaycastAll(origin, Vector3.down, SnapToGroundDistance.Value * 2, layers);
                    for (int j = 0; j < hits3D.Length; j++)
                    {
                        var hit = hits3D[j];
                        if (ignore.Contains(hit.collider))
                            continue;

                        if (bestValue == float.PositiveInfinity)
                            bestValue = hit.point.y;
                        else
                            selectGroundPoint(position.y, ref bestValue, hit.point.y);
                    }

                    if (bestValue == float.PositiveInfinity)
                        return;

                    position.y = bestValue + basePositionOffset;
                    target.position = position;
                }
            });
        }

        /************************************************************************************************************************/

        private static Vector3 GetBasePosition(Transform target)
        {
            var position = target.position;

            // If we have any Colliders or Renderers, find the lowest bounds and snap that point to the ground instead of the transform's position.

            var colliders2D = target.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders2D.Length; i++)
            {
                var boundsMin = colliders2D[i].bounds.min.y;
                if (position.y > boundsMin)
                    position.y = boundsMin;
            }

            var colliders = target.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                var boundsMin = colliders[i].bounds.min.y;
                if (position.y > boundsMin)
                    position.y = boundsMin;
            }

            var renderers = target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var boundsMin = renderers[i].bounds.min.y;
                if (position.y > boundsMin)
                    position.y = boundsMin;
            }

            return position;
        }

        /************************************************************************************************************************/

        private static void GetClosest(float target, ref float best, float current)
        {
            var bestDistance = Mathf.Abs(target - best);
            var distance = Mathf.Abs(target - current);
            if (distance < bestDistance)
                best = current;
        }

        private static void GetHighest(float target, ref float best, float current)
        {
            if (best < current)
                best = current;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Custom Handles
        /************************************************************************************************************************/

        private static readonly Dictionary<Transform, Vector3>
            MovingFromPositions = new Dictionary<Transform, Vector3>();
        private static readonly Dictionary<Transform, Quaternion>
            MovingFromRotations = new Dictionary<Transform, Quaternion>();

        public static readonly AutoPrefs.EditorBool
            ShowMovementGuides = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "ShowMovementGuides", true),
            ShowMovementDistance = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "ShowMovementDistance", true),
            ShowMovementDistancePerAxis = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "ShowMovementDistancePerAxis", true),
            ShowPositionLabels = new AutoPrefs.EditorBool(Strings.PrefsKeyPrefix + "ShowPositionLabels", false);

        private static bool
            _HasInitialisedHandles;

        private static FieldInfo
            _VertexDragging;

        private static MethodInfo
            _FindNearestVertex;

        private static object[]
            _FindNearestVertexParameters;

        private static Vector3
            _VertexSnapHandleOffset;

        // UnityEditor.Handles.currentlyDragging.
        private static bool CurrentlyDragging { get { return GUIUtility.hotControl != 0; } }

        /************************************************************************************************************************/

        public override void DrawTool(Transform target, Vector3 handlePosition)
        {
            // Vertex Snapping.
            {
                if (!_HasInitialisedHandles)
                {
                    _HasInitialisedHandles = true;
                    _VertexDragging = typeof(Tools).GetField("vertexDragging", IGEditorUtils.StaticBindings);
                    _FindNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex", IGEditorUtils.StaticBindings);
                }

                if (_VertexDragging != null && _FindNearestVertex != null)
                {
                    try
                    {
                        if ((bool)_VertexDragging.GetValue(null))
                        {
                            if (!CurrentlyDragging)
                            {
                                if (_FindNearestVertexParameters == null)
                                    _FindNearestVertexParameters = new object[3];

                                _FindNearestVertexParameters[0] = Event.current.mousePosition;
                                _FindNearestVertexParameters[1] = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab | SelectionMode.Deep);
                                _FindNearestVertex.Invoke(null, _FindNearestVertexParameters);
                                var position = (Vector3)_FindNearestVertexParameters[2];
                                _VertexSnapHandleOffset = position - GetWorldValue(target);
                                handlePosition = position;
                            }
                            else
                            {
                                handlePosition = GetWorldValue(target) + _VertexSnapHandleOffset;
                            }
                        }
                    }
                    catch { }
                }
            }

            EditorGUI.BeginChangeCheck();
            var movement = DrawHandle(target, handlePosition);
            if (EditorGUI.EndChangeCheck() && GUI.enabled && movement != Vector3.zero)
            {
                RecordTransformForUndo(target, UndoName);

                if (TransformEditor.DrawAllGizmos)
                {
                    SetWorldValue(target, GetWorldValue(target) + movement);
                }
                else
                {
                    for (int i = 0; i < ParentEditor.Targets.Length; i++)
                    {
                        target = ParentEditor.Targets[i];
                        if (!SelectionContainsParent(target))
                            SetWorldValue(target, GetWorldValue(target) + movement);
                    }
                }
            }
        }

        /************************************************************************************************************************/

        private bool SelectionContainsParent(Transform target)
        {
            while (true)
            {
                target = target.parent;
                if (target == null)
                    return false;
                else if (Array.IndexOf(ParentEditor.Targets, target) >= 0)
                    return true;
            }
        }

        /************************************************************************************************************************/

        private static GUIStyle _DistanceLabelStyle, _AxisLabelStyle;

        private Vector3 DrawHandle(Transform target, Vector3 handlePosition)
        {
            var rotation = Tools.pivotRotation == PivotRotation.Local ? target.rotation : Quaternion.identity;

            if (ShowMovementGuides)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        MovingFromPositions[target] = handlePosition;
                        MovingFromRotations[target] = rotation;
                        break;

                    case EventType.MouseUp:
                        MovingFromPositions.Clear();
                        MovingFromRotations.Clear();
                        break;

                    case EventType.Repaint:

                        var cameraTransform = SceneView.currentDrawingSceneView.camera.transform;
                        var forward = cameraTransform.forward;
                        if (Vector3.Dot(forward, handlePosition) < Vector3.Dot(forward, cameraTransform.position))
                            break;

                        Vector3 movingFrom;
                        if (MovingFromPositions.TryGetValue(target, out movingFrom))
                        {
                            var movingFromRotation = MovingFromRotations[target];

                            var xAxis = movingFromRotation * Vector3.right;
                            var yAxis = movingFromRotation * Vector3.up;
                            var zAxis = movingFromRotation * Vector3.forward;

                            Handles.DrawLine(movingFrom, handlePosition);

                            var size = HandleUtility.GetHandleSize(movingFrom) * 0.5f;

                            Handles.color = Handles.xAxisColor;
                            Handles.DrawLine(movingFrom, movingFrom + xAxis * size);
                            Handles.color = Handles.yAxisColor;
                            Handles.DrawLine(movingFrom, movingFrom + yAxis * size);
                            Handles.color = Handles.zAxisColor;
                            Handles.DrawLine(movingFrom, movingFrom + zAxis * size);
                            Handles.color = Color.white;

                            if (ShowMovementDistance)
                            {
                                if (_DistanceLabelStyle == null)
                                {
                                    _DistanceLabelStyle = new GUIStyle(GUI.skin.label);
                                    _DistanceLabelStyle.normal.textColor = Color.white;
                                }

                                _DistanceLabelStyle.normal.background = InternalGUI.SceneLabelBackground;

                                var offset = movingFrom - handlePosition;
                                var distance = offset.sqrMagnitude;
                                if (distance > 0.00001f)
                                {
                                    distance = Mathf.Sqrt(distance);
                                    var label = distance.ToString();

                                    if (ShowMovementDistancePerAxis)
                                    {
                                        label = string.Concat(label,
                                            "\n<color=#", IGEditorUtils.ColorToHex(FieldColorX), ">", Mathf.Abs(offset.x),
                                            "</color>\n<color=#", IGEditorUtils.ColorToHex(FieldColorY), ">", Mathf.Abs(offset.y),
                                            "</color>\n<color=#", IGEditorUtils.ColorToHex(FieldColorZ), ">", Mathf.Abs(offset.z),
                                            "</color>");
                                        _DistanceLabelStyle.richText = true;
                                    }
                                    else _DistanceLabelStyle.richText = false;

                                    Handles.Label(movingFrom, label, _DistanceLabelStyle);
                                }
                            }
                        }

                        if (ShowPositionLabels)
                        {
                            if (_AxisLabelStyle == null)
                            {
                                _AxisLabelStyle = new GUIStyle(GUI.skin.label)
                                {
                                    alignment = TextAnchor.MiddleLeft
                                };
                            }

                            _AxisLabelStyle.normal.background = InternalGUI.SceneLabelBackground;

                            var xAxis = rotation * Vector3.right;
                            var yAxis = rotation * Vector3.up;
                            var zAxis = rotation * Vector3.forward;

                            var size = HandleUtility.GetHandleSize(handlePosition) * 1.35f;

                            _AxisLabelStyle.normal.textColor = FieldColorX;
                            Handles.Label(handlePosition + xAxis * size, handlePosition.x.ToString(), _AxisLabelStyle);
                            _AxisLabelStyle.normal.textColor = FieldColorY;
                            Handles.Label(handlePosition + yAxis * size, handlePosition.y.ToString(), _AxisLabelStyle);
                            _AxisLabelStyle.normal.textColor = FieldColorZ;
                            Handles.Label(handlePosition + zAxis * size, handlePosition.z.ToString(), _AxisLabelStyle);
                        }

                        break;

                    default:
                        break;
                }
            }

            return Handles.PositionHandle(handlePosition, rotation) - handlePosition;
        }

        /************************************************************************************************************************/

        private static bool _IsInspectorDrag;

        protected override void DoVectorField(Rect area, float labelRight)
        {
            if (ShowMovementGuides)
            {
                if (_IsInspectorDrag && GUIUtility.hotControl == 0)
                {
                    _IsInspectorDrag = false;
                    MovingFromPositions.Clear();
                    MovingFromRotations.Clear();
                    SceneView.RepaintAll();
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    for (int i = 0; i < Targets.Length; i++)
                    {
                        var target = Targets[i];
                        if (target == null)
                            continue;

                        MovingFromPositions[target] = TransformEditor.GetHandlePosition(target);
                        MovingFromRotations[target] = target.rotation;
                    }
                    _IsInspectorDrag = true;
                }
            }

            base.DoVectorField(area, labelRight);
        }

        /************************************************************************************************************************/

        public override void OnDisable()
        {
            MovingFromPositions.Clear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

    }
}

#endif
