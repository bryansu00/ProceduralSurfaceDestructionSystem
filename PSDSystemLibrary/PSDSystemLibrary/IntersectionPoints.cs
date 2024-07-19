#if USING_GODOT
    using Godot;
#else
using System.Numerics;
#endif

namespace PSDSystem
{
    /// <summary>
    /// Stores and keeps tracks of intersection points within the CutSurface() operation.
    /// </summary>
    /// <typeparam name="T">Class that is a PolygonVertex and IHasBooleanVertexProperties</typeparam>
    public class IntersectionPoints<T> where T : PolygonVertex, IHasBooleanVertexProperties<T>
    {
        /// <summary>
        /// The Polygon that the Cutter is intersecting with.
        /// </summary>
        public Polygon<T> Polygon { get; }

        /// <summary>
        /// The intersection results.
        /// Tuple 1 is the intersection point in Vector2.
        /// Tuple 2 is a reference to a VertexNode of the Polygon it is intersecting.
        /// Tuple 3 is the T value of the intersection done using the PartialLineIntersection() function.
        /// Tuple 4 is a reference to a VertexNode of the Cutter.
        /// Tuple 5 is the U value of the intersection done using the PartialLineIntersection() function.
        /// </summary>
        public List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> Intersections { get; }

        /// <summary>
        /// Construct IntersectionPoints.
        /// </summary>
        /// <param name="polygon">Reference to the Polygon it is intersecting.</param>
        /// <param name="intersections">The list of intersections.</param>
        public IntersectionPoints(Polygon<T> polygon, List<Tuple<Vector2, VertexNode<T>, float, VertexNode<T>, float>> intersections)
        {
            Polygon = polygon;
            Intersections = intersections;
        }
    }
}
