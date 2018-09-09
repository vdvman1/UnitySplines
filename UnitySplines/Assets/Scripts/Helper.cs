using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace VDV.Spline
{
    public static class Helper
    {
        public struct LinePoint
        {
            public float ProjectionDistance;
            public float Distance;
            public Vector3 Point;
            public int Index;
        }

        public static LinePoint PointToLine(Vector3 pos, Vector3 a, Vector3 b, int aIndex)
        {
            var line = new LinePoint { Index = aIndex };
            Vector3 APos = pos - a;
            Vector3 AB = b - a;
            line.ProjectionDistance = Vector3.Dot(APos, AB) / AB.sqrMagnitude;
            if (line.ProjectionDistance < 0)
            {
                line.Point = a;
            }
            else if (line.ProjectionDistance > 1)
            {
                line.Point = b;
                line.Index++;
            }
            else
            {
                line.Point = a + AB * line.ProjectionDistance;
            }
            line.Distance = (pos - line.Point).magnitude;
            return line;
        }

        public static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
            return 
                type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(field => field.IsPublic | field.IsDefined(typeof(SerializeField), false));
        }
    }
}
