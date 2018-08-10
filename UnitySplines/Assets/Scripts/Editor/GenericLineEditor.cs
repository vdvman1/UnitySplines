using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VDV.Spline.Editor
{
    public class GenericLineEditor<TLine,TVertex> : UnityEditor.Editor where TVertex : LineVertex, new() where TLine : GenericLine<TVertex>
    {
        protected const float HandleSize = 0.04f;
        protected const float PickSize = 0.06f;

        protected TLine Line;
        protected Transform LineTransform;
        protected Quaternion HandleTransform;
        protected int SelectedPointIdx = -1;

        public void OnSceneGUI()
        {
            Line = target as TLine;
            if (Line == null || Line.PointCount == 0)
            {
                return;
            }
            LineTransform = Line.transform;
            HandleTransform = Tools.pivotRotation == PivotRotation.Local ? LineTransform.rotation : Quaternion.identity;
            Handles.color = Color.white;
            TVertex start = RenderPoint(0);
            for (var i = 0; i < Line.PointCount; i++)
            {
                TVertex end = RenderPoint(i);
                Handles.DrawLine(start.Position, end.Position);
                start = end;
            }
        }

        private TVertex RenderPoint(int i)
        {
            TVertex point = Line.GetPoint(i);
            Vector3 pos = LineTransform.TransformPoint(point.Position);
            Handles.color = Color.white;
            float size = HandleUtility.GetHandleSize(pos);
            if (Handles.Button(pos, HandleTransform, size * HandleSize, size * PickSize, Handles.DotHandleCap))
            {
                OnSelectPoint();
                SelectPoint(i);
            }
            OnRenderPoint(i, point, pos, size);
            if (ShouldRenderPositionHandleForPoint(i))
            {
                EditorGUI.BeginChangeCheck();
                pos = Handles.DoPositionHandle(pos, HandleTransform);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Line, "Move Point");
                    EditorUtility.SetDirty(Line);
                    point.Position = pos;
                    Line.SetPoint(i, point);
                }
            }
            return point;
        }

        protected void SelectPoint(int index, bool notify = true)
        {
            SelectedPointIdx = index;
            Repaint();
            if(notify) OnSelectPoint();
        }

        protected virtual void OnSelectPoint() {}

        protected virtual bool ShouldRenderPositionHandleForPoint(int index)
        {
            return SelectedPointIdx == index;
        }
        protected virtual void OnRenderPoint(int index, TVertex point, Vector3 pos, float handleSize) {}

        public override void OnInspectorGUI()
        {
            Line = target as TLine;
            if (Line == null) return;

            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", Line.Loop);
            bool valid = AfterLoopToggleBox(loop);
            if (valid && EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Line, "Toggle Loop");
                EditorUtility.SetDirty(Line);
                Line.Loop = loop;
            }

            if (SelectedPointIdx >= 0 && SelectedPointIdx < Line.PointCount)
            {
                GUILayout.Label("Selected Point");
                EditorGUI.BeginChangeCheck();
                TVertex point = Line.GetPoint(SelectedPointIdx);
                Vector3 pos = EditorGUILayout.Vector3Field("Position", point.Position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Line, "Move Point");
                    EditorUtility.SetDirty(Line);
                    point.Position = pos;
                    Line.SetPoint(SelectedPointIdx, point);
                }
                OnInspectorPoint(SelectedPointIdx, point);
                if (GUILayout.Button("Delete Selected Segment"))
                {
                    Undo.RecordObject(Line, "Delete Segment");
                    Line.DeleteSegment(SelectedPointIdx);
                    EditorUtility.SetDirty(Line);
                    if (SelectedPointIdx < 0)
                    {
                        SelectedPointIdx = 0;
                    }
                    else if (SelectedPointIdx >= Line.PointCount)
                    {
                        SelectedPointIdx = Line.PointCount - 1;
                    }
                }
            }
            if (GUILayout.Button("Add Segment"))
            {
                Undo.RecordObject(Line, "Add Segment");
                Line.AddSegment();
                EditorUtility.SetDirty(Line);
            }
        }

        protected virtual void OnInspectorPoint(int index, TVertex point) {}

        protected virtual bool AfterLoopToggleBox(bool newLoop)
        {
            return true;
        }
    }
}
