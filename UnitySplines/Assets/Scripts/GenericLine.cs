using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VDV.Spline
{
    public abstract class GenericLine<T> : MonoBehaviour where T : LineVertex, new()
    {
        [SerializeField] protected List<T> points;

        [SerializeField]
        private bool loop;

        protected virtual int PointsPerSegment { get { return 2; } }

        public T GetPoint(int i)
        {
            return points[i];
        }

        public void SetPoint(int i, T point)
        {
            if (loop)
            {
                if (i == 0)
                {
                    points[points.Count - 1] = point;
                }
                else if (i == points.Count - 1)
                {
                    points[0] = point;
                }
            }
            points[i] = point;
        }

        public int PointCount
        {
            get { return points == null ? 0 : points.Count; }
        }

        public bool Loop
        {
            get { return loop; }
            set
            {
                loop = value;
                if (loop)
                {
                    SetPoint(0, points[0]);
                }
            }
        }

        public void Reset()
        {
            points = new List<T>();
            for (var i = 0; i < PointsPerSegment; i++)
            {
                points.Add(new T { Position = new Vector3(i + 1, 0, 0) });
            }
        }

        public void AddSegment()
        {
            if (points == null || points.Count == 0)
            {
                Reset();
            }
            else
            {
                T point = points[points.Count - 1];
                Vector3 pos = point.Position;
                for (var i = 0; i < PointsPerSegment - 1; i++)
                {
                    pos.x += 1f;
                    points.Add(new T {Position = pos});
                }
                SetPoint(points.Count - 1, points[points.Count - 1]);
            }
        }

        public void DeleteSegment(int i)
        {
            if (points.Count == PointsPerSegment)
            {
                points.Clear();
            }
            else if (PointsPerSegment == 2)
            {
                points.RemoveAt(i);
            }
            // TODO: Generic Removal?
        }

        public virtual Helper.LinePoint ClosestPoint(Vector3 pos)
        {
            return new Helper.LinePoint {Distance = Mathf.Infinity};
        }
    }

}