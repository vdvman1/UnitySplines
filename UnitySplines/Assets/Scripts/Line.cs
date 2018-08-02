using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VDV.Spline
{
    public class Line : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] points;

        [SerializeField]
        private bool loop;

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
                    points[points.Length - 1] = point;
                }
                else if (i == points.Length - 1)
                {
                    points[0] = point;
                }
            }
            points[i] = point;
        }

        public int PointCount()
        {
            return points.Length;
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
            points = new[] {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f)
            };
        }

        public void AddSegment()
        {
            Vector3 point = points[points.Length - 1];
            Array.Resize(ref points, points.Length + 1);
            point.x += 1f;
            points[points.Length - 1] = point;
        }
    }

}