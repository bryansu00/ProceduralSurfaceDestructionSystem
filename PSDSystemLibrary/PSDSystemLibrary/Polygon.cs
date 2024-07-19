#nullable enable

#if USING_GODOT
    using Godot;
#else
using System.Numerics;
#endif

namespace PSDSystem
{
    /// <summary>
    /// Class representing a 2D Polygon
    /// </summary>
    /// <typeparam name="T">A vertex class that represent each vertex of a polygon.</typeparam>
    public class Polygon<T> where T : PolygonVertex
    {
        /// <summary>
        /// The head node of a vertex of the Polygon. Returns null if the Polygon is empty.
        /// </summary>
        public VertexNode<T>? Head { get; private set; }

        /// <summary>
        /// The right most vertex of the Polygon. Returns null if the Polygon is empty.
        /// </summary>
        public VertexNode<T>? RightMostVertex
        {
            get
            {
                if (Head == null || Vertices == null)
                {
                    return null;
                }

                VertexNode<T> result = Head;
                VertexNode<T> now = Head.Next;
                while (now != Head)
                {
                    if (Vertices[now.Data.Index].X > Vertices[result.Data.Index].X)
                    {
                        result = now;
                    }
                    now = now.Next;
                }

                return result;
            }
        }

        /// <summary>
        /// The number of vertex in the Polygon
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// The list of vertices the Polygon is referring to
        /// </summary>
        public List<Vector2>? Vertices { get; set; }

        /// <summary>
        /// Construct a Polygon without any vertices or shape
        /// </summary>
        public Polygon()
        {
            Head = null;
            Count = 0;
            Vertices = null;
        }

        /// <summary>
        /// Construct a copy of the given Polygon
        /// </summary>
        /// <param name="polygon"></param>
        public Polygon(Polygon<T> polygon)
        {
            Count = 0;
            Vertices = null;

            // Make a copy here if possible
            if (polygon.Vertices != null)
                Vertices = new List<Vector2>(polygon.Vertices);

            Head = null;

            if (polygon.Head == null)
            {
                return;
            }

            VertexNode<T> now = polygon.Head;
            do
            {
                InsertVertexAtBack(now.Data.Index);
                now = now.Next;
            } while (now != polygon.Head);
        }

        /// <summary>
        /// Inserts a vertex with the given index to the polygon
        /// </summary>
        /// <param name="index">The index that the vertex will refer to</param>
        /// <returns>The vertex inserted at the back.</returns>
        /// <exception cref="Exception">Failed to instantiate a Vertex</exception>
        public VertexNode<T> InsertVertexAtBack(int index)
        {
            T? data = (T?)Activator.CreateInstance(typeof(T), index);
            if (data == null) throw new Exception("Unable to create an instance of a Vertex!");
        
            VertexNode<T> newNode = new VertexNode<T>(this, data);

            if (Head == null)
            {
                Head = newNode;
            }
            else
            {
                // Get tail node
                VertexNode<T> tail = Head.Previous;

                // Connect tail node to new node
                tail.Next = newNode;
                newNode.Previous = tail;

                // Connect new node to Head
                Head.Previous = newNode;
                newNode.Next = Head;
            }

            // Increment size of the polygon
            Count++;

            return newNode;
        }

        /// <summary>
        /// Insert a vertex after the given node
        /// </summary>
        /// <param name="node">The node to insert a vertex after</param>
        /// <param name="index">The index that the inserted vertex will refer to</param>
        /// <returns>The vertex inserted. Returns null if the given node is invalid.</returns>
        public VertexNode<T>? InsertVertexAfter(VertexNode<T> node, int index)
        {
            if (node == null || node.Owner != this) return null;

            T? data = (T?)Activator.CreateInstance(typeof(T), index);
            if (data == null) return null;

            VertexNode<T> newNode = new VertexNode<T>(this, data);

            // Link up new node
            newNode.Previous = node;
            newNode.Next = node.Next;
            // Relink the given node
            node.Next = newNode;
            // Relink the next node
            newNode.Next.Previous = newNode;

            // Increment count of the polygon
            Count++;

            return newNode;
        }

        /// <summary>
        /// Insert a vertex before the given node
        /// </summary>
        /// <param name="node">The node to insert a vertex before</param>
        /// <param name="index">The index that the inserted vertex will refer to</param>
        /// <returns>The vertex inserted. Returns null if the given node is invalid.</returns>
        public VertexNode<T>? InsertVertexBefore(VertexNode<T> node, int index)
        {
            if (node == null || node.Owner != this) return null;

            T? data = (T?)Activator.CreateInstance(typeof(T), index);
            if (data == null) return null;

            VertexNode<T> newNode = new VertexNode<T>(this, data);

            // Link up new node
            newNode.Next = node;
            newNode.Previous = node.Previous;
            // Relink the given node
            node.Previous = newNode;
            // Relink the previous node
            newNode.Previous.Next = newNode;

            // Increment count of the polygon
            Count++;

            return newNode;
        }

        /// <summary>
        /// Remove the given node from the polygon
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <exception cref="InvalidOperationException">The vertex given belongs to a different polygon.</exception>
        public void ClipVertex(VertexNode<T> node)
        {
            if (node == null || node.Owner != this) throw new InvalidOperationException("Given node does not belong to the Polygon!");

            // Remove the vertex
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;

            // Decrement count
            Count--;

            if (Count == 0)
            {
                // Count is 0, thus there should be no head
                Head = null;
            }
            else if (node == Head)
            {
                // If it was the head removed, set new head
                Head = Head.Next;
            }
        }

        /// <summary>
        /// Remove all vertices with the given index from the Polygon.
        /// </summary>
        /// <param name="indexToRemove">The index of the vertices to remove.</param>
        public void RemoveVerticesWithIndex(int indexToRemove)
        {
            if (Head == null) return;

            uint originalCount = Count;

            VertexNode<T> now = Head;
            for (int i = 0; i < originalCount; i++)
            {
                if (now.Data.Index == indexToRemove)
                {
                    ClipVertex(now);
                }
                now = now.Next;
            }
        }

        /// <summary>
        /// Get the list of indices of this Polygon.
        /// </summary>
        /// <returns>List of indices</returns>
        public List<int> ToList()
        {
            List<int> toReturn = new List<int>();
            if (Head == null) return toReturn;

            var now = Head;
            do
            {
                toReturn.Add(now.Data.Index);
                now = now.Next;
            } while (now != Head);

            return toReturn;
        }

        /// <summary>
        /// Get the list of Vector2 of this Polygon.
        /// </summary>
        /// <returns>List of Vector2</returns>
        public List<Vector2> ToVerticesList()
        {
            List<Vector2> toReturn = new List<Vector2>();
            if (Head == null || Vertices == null) return toReturn;

            var now = Head;
            do
            {
                toReturn.Add(Vertices[now.Data.Index]);
                now = now.Next;
            } while (now != Head);

            return toReturn;
        }
    }

    /// <summary>
    /// Class representing a single vertex of a polygon
    /// </summary>
    public class PolygonVertex
    {
        /// <summary>
        /// The index that this vertex refers to
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Copy data from the given vertex to this vertex
        /// </summary>
        /// <param name="vertex">The vertex to copy data from</param>
        public virtual void CopyData(PolygonVertex vertex)
        {
            
        }

        /// <summary>
        /// Construct a PolygonVertex with the given index.
        /// </summary>
        /// <param name="index"></param>
        public PolygonVertex(int index)
        {
            Index = index;
        }
    }

    /// <summary>
    /// Node to hold a vertex
    /// </summary>
    /// <typeparam name="T">The vertex that holds the data</typeparam>
    public class VertexNode<T> where T : PolygonVertex
    {
        /// <summary>
        /// The vertex data this VertexNode is holding onto.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Reference to the Next VertexNode
        /// </summary>
        public VertexNode<T> Next { get; set; }

        /// <summary>
        /// Reference to the Previous VertexNode
        /// </summary>
        public VertexNode<T> Previous { get; set; }

        /// <summary>
        /// The Polygon that the VertexNode belongs to
        /// </summary>
        public Polygon<T> Owner { get; }

        /// <summary>
        /// Contruct a VertexNode with the given reference of the owner and the Vertex data
        /// </summary>
        /// <param name="owner">The Polygon that the VertexNode will be part of</param>
        /// <param name="data">The Vertex data that the VertexNode will be holding onto</param>
        public VertexNode(Polygon<T> owner, T data)
        {
            Owner = owner;
            Next = this;
            Previous = this;
            Data = data;
        }
}
}
