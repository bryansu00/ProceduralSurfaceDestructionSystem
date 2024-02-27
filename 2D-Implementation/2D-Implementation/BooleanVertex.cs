using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDSystem
{
    public class BooleanVertex : PolygonVertex, IHasBooleanVertexProperties<BooleanVertex>
    {
        public bool IsOutside { get; set; }

        public VertexNode<BooleanVertex>? Cross { get; set; }

        public bool Processed { get; set; }

        public bool OnEdge { get; set; }

        public bool IsAnAddedVertex { get; set; }

        public BooleanVertex(int index) : base(index)
        {
            IsOutside = true;
            Cross = null;
            Processed = false;
            OnEdge = false;
            IsAnAddedVertex = false;
        }
    }

    public interface IHasBooleanVertexProperties<T> where T : PolygonVertex
    {
        public bool IsOutside { get; set; }

        public VertexNode<T>? Cross { get; set; }

        public bool Processed { get; set; }

        public bool OnEdge { get; set; }

        public bool IsAnAddedVertex { get; set; }
    }
}
