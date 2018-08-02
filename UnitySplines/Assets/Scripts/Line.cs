using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VDV.Spline
{
    public class Line : MonoBehaviour
    {
        [SerializeField]
        private List<Vector3> points;

        [SerializeField]
        private bool loop;

        protected virtual int PointsPerSegment { get { return 2; } }

        public Vector3 GetPoint(int i)
        {
            return points[i];
        }

        public void SetPoint(int i, Vector3 point)
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
            get { return points.Count; }
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
            points = new List<Vector3>();
            for (var i = 0; i < PointsPerSegment; i++)
            {
                points.Add(new Vector3(i + 1, 0, 0));
            }
        }

        public void AddSegment()
        {
            if (points.Count == 0)
            {
                Reset();
            }
            else
            {
                Vector3 point = points[points.Count - 1];
                for (var i = 0; i < PointsPerSegment - 1; i++)
                {
                    point.x += 1f;
                    points.Add(point);
                }
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
    }

}