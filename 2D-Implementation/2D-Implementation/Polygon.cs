using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace PSDSystem
{
    public class Polygon<T> where T : PolygonVertex
    {
        public VertexNode<T>? Head { get; private set; }

        public uint Size { get; private set; }

        public List<Vector2>? Vertices { get; set; }

        public Polygon()
        {
            Head = null;
            Size = 0;
            Vertices = null;
        }

        public VertexNode<T> InsertVertexAtBack(int center)
        {
            T? data = (T?)Activator.CreateInstance(typeof(T), center);

            if (data == null) throw new Exception();

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
            Size++;

            return newNode;
        }

        public List<int> ToList()
        {
            List<int> toReturn = new List<int>();
            if (Head == null) return toReturn;

            var now = Head;
            do
            {
                toReturn.Add(now.Data.Center);
                now = now.Next;
            } while (now != Head);

            return toReturn;
        }
    }

    public class PolygonVertex
    {
        public int Center { get; }

        public PolygonVertex(int center)
        {
            Center = center;
        }
    }

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
