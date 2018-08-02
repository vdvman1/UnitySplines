using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VDV.Spline.Editor
{
    [CustomEditor(typeof(Line))]
    public class LineEditor : UnityEditor.Editor
    {
        protected const float HandleSize = 0.04f;
        protected const float PickSize = 0.06f;

        protected Line Line;
        protected Transform LineTransform;
        protected Quaternion HandleTransform;
        protected int SelectedPointIdx = -1;

        private void OnSceneGUI()
        {
            Line = target as Line;
            if (Line == null || Line.PointCount() == 0)
            {
                return;
            }
            LineTransform = Line.transform;
            HandleTransform = Tools.pivotRotation == PivotRotation.Local ? LineTransform.rotation : Quaternion.identity;
            Handles.color = Color.white;
            Vector3 start = RenderPoint(0);
            for (var i = 0; i < Line.PointCount(); i++)
            {
                Vector3 end = RenderPoint(i);
                Handles.DrawLine(start, end);
                start = end;
            }
        }

        private Vector3 RenderPoint(int i)
        {
            Vector3 point = Line.GetPoint(i);
            point = LineTransform.TransformPoint(point);
            Handles.color = Color.white;
            float size = HandleUtility.GetHandleSize(point);
            if (Handles.Button(point, HandleTransform, size * HandleSize, size * PickSize, Handles.DotHandleCap))
            {
                SelectedPointIdx = i;
                Repaint();
            }
            if (SelectedPointIdx == i)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, HandleTransform);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Line, "Move Point");
                    EditorUtility.SetDirty(Line);
                    Line.SetPoint(i, point);
                }
            }
            return point;
        }

        public override void OnInspectorGUI()
        {
            Line = target as Line;
            if (Line == null) return;
            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", Line.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Line, "Toggle Loop");
                EditorUtility.SetDirty(Line);
                Line.Loop = loop;
            }
            if (SelectedPointIdx > 0 && SelectedPointIdx < Line.PointCount())
            {
                GUILayout.Label("Selected Point");
                EditorGUI.BeginChangeCheck();
                Vector3 point = EditorGUILayout.Vector3Field("Position", Line.GetPoint(SelectedPointIdx));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Line, "Move Point");
                    EditorUtility.SetDirty(Line);
                    Line.SetPoint(SelectedPointIdx, point);
                }
            }
            if (GUILayout.Button("Add Segment"))
            {
                Undo.RecordObject(Line, "Add Segment");
                Line.AddSegment();
                EditorUtility.SetDirty(Line);
            }
        }
    } 
}
