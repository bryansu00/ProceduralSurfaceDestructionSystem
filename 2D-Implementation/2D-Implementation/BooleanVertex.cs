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

        public BooleanVertex(int index) : base(index)
        {
            IsOutside = false;
            Cross = null;
            Processed = false;
        }
    }

    public interface IHasBooleanVertexProperties<T> where T : PolygonVertex
    {
        public bool IsOutside { get; set; }

        public VertexNode<T>? Cross { get; set; }

        public bool Processed { get; set; }
    }
}
