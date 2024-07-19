#nullable enable

namespace PSDSystem
{
    /// <summary>
    /// Class needed to perform boolean operations for between two or more Polygons.
    /// </summary>
    public class BooleanVertex : PolygonVertex, IHasBooleanVertexProperties<BooleanVertex>
    {
        /// <summary>
        /// Whether or not the BooleanVertex is outside of the Polygon it is intersecting
        /// </summary>
        public bool IsOutside { get; set; }

        /// <summary>
        /// Reference to another VertexNode belonging to the Polygon it is intersecting.
        /// If the BooleanVertex is an intersection point, then Cross will NOT be null.
        /// </summary>
        public VertexNode<BooleanVertex>? Cross { get; set; }

        /// <summary>
        /// Whether or not the BooleanVertex has been processed as part of the boolean operations.
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Whether or not the BooleanVertex is on the edge of the Polygon it is intersecting.
        /// </summary>
        public bool OnEdge { get; set; }

        /// <summary>
        /// Whether the not the BooleanVertex is a Vertex created as part of the boolean operations.
        /// </summary>
        public bool IsAnAddedVertex { get; set; }

        /// <summary>
        /// Construct a BooleanVertex with the given index.
        /// </summary>
        /// <param name="index">The index the BooleanVertex is referring to.</param>
        public BooleanVertex(int index) : base(index)
        {
            IsOutside = true;
            Cross = null;
            Processed = false;
            OnEdge = false;
            IsAnAddedVertex = false;
        }
    }

    /// <summary>
    /// An interface containing the required properties needed to execute the Boolean operations.
    /// If a class is extended from this interface, then IsOutside = false, Cross = null, Processed = false, OnEdge = false, IsAnAddedVertex = false,
    /// and the class must have a constructor that takes an integer as an argument.
    /// </summary>
    /// <typeparam name="T">Class that is a PolygonVertex.</typeparam>
    public interface IHasBooleanVertexProperties<T> where T : PolygonVertex
    {
        /// <summary>
        /// Whether or not the BooleanVertex is outside of the Polygon it is intersecting
        /// </summary>
        public bool IsOutside { get; set; }

        /// <summary>
        /// Reference to another VertexNode belonging to the Polygon it is intersecting.
        /// If the BooleanVertex is an intersection point, then Cross will NOT be null.
        /// </summary>
        public VertexNode<T>? Cross { get; set; }

        /// <summary>
        /// Whether or not the BooleanVertex has been processed as part of the boolean operations.
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Whether or not the BooleanVertex is on the edge of the Polygon it is intersecting.
        /// </summary>
        public bool OnEdge { get; set; }

        /// <summary>
        /// Whether the not the BooleanVertex is a Vertex created as part of the boolean operations.
        /// </summary>
        public bool IsAnAddedVertex { get; set; }
    }
}
