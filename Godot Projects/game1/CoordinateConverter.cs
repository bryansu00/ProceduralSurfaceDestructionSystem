using System.Collections.Generic;
using Godot;

namespace PSDSystem
{
    public class CoordinateConverter
    {
        private Vector3 Origin { get; }

        private Vector3 XAxis { get; }

        private Vector3 YAxis { get; }

        public CoordinateConverter(Vector3 origin, Vector3 xAxis, Vector3 yAxis)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
        }

        public Vector3 ConvertTo3D(Vector2 v)
        {
            return (XAxis * v.X) + (YAxis * v.Y) + Origin;
        }

        public List<Vector3> ConvertListTo3D(List<Vector2> vectors)
        {
            List<Vector3> output = new List<Vector3>();
            foreach (Vector2 v in vectors)
            {
                output.Add(ConvertTo3D(v));
            }
            return output;
        }

        public Vector2 ConvertTo2D(Vector3 v)
        {
            Vector3 localV = v - Origin;
            float x = localV.Dot(XAxis) / XAxis.Dot(XAxis);
            float y = localV.Dot(YAxis) / YAxis.Dot(YAxis);
            return new Vector2(x, y);
        }
    }
}