using System.Collections.Generic;
using Godot;

namespace PSDSystem
{
    public class CoordinateConverter
    {
        public Vector3 Origin { get; }

        public Vector3 XAxis { get; }

        public Vector3 YAxis { get; }

        public CoordinateConverter(Vector3 origin, Vector3 xAxis, Vector3 yAxis)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
        }

        /// <summary>
        /// Convert the given 2D vector to a 3D vector
        /// </summary>
        /// <param name="v">The 2D vector to convert</param>
        /// <returns>3D vector converted from the given 2D vector</returns>
        public Vector3 ConvertTo3D(Vector2 v)
        {
            return (XAxis * v.X) + (YAxis * v.Y) + Origin;
        }

        /// <summary>
        /// Convert a list of 3D vectors to a list of 2D vectors
        /// </summary>
        /// <param name="vectors">List of 2D vectors to convert</param>
        /// <returns>List of 3D vectors</returns>
        public List<Vector3> ConvertListTo3D(List<Vector2> vectors)
        {
            List<Vector3> output = new List<Vector3>();
            foreach (Vector2 v in vectors)
            {
                output.Add(ConvertTo3D(v));
            }
            return output;
        }

        /// <summary>
        /// Convert the given 3D vector to a 2D vector
        /// </summary>
        /// <param name="v">The 3D vector to convert</param>
        /// <returns>2D vector converted from the given 3D vector</returns>
        public Vector2 ConvertTo2D(Vector3 v)
        {
            Vector3 localV = v - Origin;
            float x = localV.Dot(XAxis) / XAxis.Dot(XAxis);
            float y = localV.Dot(YAxis) / YAxis.Dot(YAxis);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Convert a list of 2D vectors to a list of 3D vectors
        /// </summary>
        /// <param name="vectors">List of 3D vectors to convert</param>
        /// <returns>List of 2D vectors</returns>
        public List<Vector2> ConvertListTo2D(List<Vector3> vectors)
        {
            List<Vector2> output = new List<Vector2>();
            foreach (Vector3 v in vectors)
            {
                output.Add(ConvertTo2D(v));
            }
            return output;
        }
    }
}