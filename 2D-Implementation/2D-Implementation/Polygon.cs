﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace PSDSystem
{
    /// <summary>
    /// Class representing a 2D Polygon
    /// </summary>
    /// <typeparam name="T">A vertex class that represent each vertex of a polygon.</typeparam>
    public class Polygon<T> where T : PolygonVertex
    {
        /// <summary>
        /// The head node of a vertex of the polygon
        /// </summary>
        public VertexNode<T>? Head { get; private set; }

        /// <summary>
        /// The number of vertex in the polygon
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// The list of vertices the polygon is referring to
        /// </summary>
        public List<Vector2>? Vertices { get; set; }

        public Polygon()
        {
            Head = null;
            Count = 0;
            Vertices = null;
        }

        /// <summary>
        /// Inserts a vertex with the given index to the polygon
        /// </summary>
        /// <param name="index">The index that the vertex will refer to</param>
        /// <returns>The vertex inserted at the back</returns>
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

        public VertexNode<T> InsertVertexAfter(VertexNode<T> node, int index)
        {
            if (node == null || node.Owner != this) throw new InvalidOperationException("Given node does not belong to the Polygon!");

            T? data = (T?)Activator.CreateInstance(typeof(T), index);
            if (data == null) throw new Exception("Unable to create an instance of a Vertex!");

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
        /// Convert the polygon to list of indices referring to this.Vertices
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
        public T Data { get; }

        public VertexNode<T> Next { get; set; }

        public VertexNode<T> Previous { get; set; }

        public Polygon<T> Owner { get; }

        public VertexNode(Polygon<T> owner, T data)
        {
            Owner = owner;
            Next = this;
            Previous = this;
            Data = data;
        }
}
}
