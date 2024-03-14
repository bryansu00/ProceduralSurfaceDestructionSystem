#nullable enable
using System.Collections.Generic;

namespace PSDSystem
{
    /// <summary>
    /// Represents the current shape of the surface after modifications
    /// </summary>
    /// <typeparam name="T">A vertex class that represent each vertex of a polygon.</typeparam>
    public class SurfaceShape<T> where T : PolygonVertex
    {
        /// <summary>
        /// Each PolygonGroup in this variable represents an outer polygon and
        /// its groups of inner polygons
        /// </summary>
        public List<PolygonGroup<T>> Polygons { get; }

        public SurfaceShape()
        {
            Polygons = new List<PolygonGroup<T>>();
        }

        public SurfaceShape(List<PolygonGroup<T>> polygons)
        {
            Polygons = polygons;
        }

        /// <summary>
        /// Add a polygon as an outer polygon
        /// </summary>
        /// <param name="polygon">The outer polygon</param>
        public void AddOuterPolygon(Polygon<T> polygon)
        {
            PolygonGroup<T> pair = new PolygonGroup<T>(polygon);

            Polygons.Add(pair);
        }

        /// <summary>
        /// Add a outer polygon and its list of inner polygons
        /// as part of the SurfaceShape
        /// </summary>
        /// <param name="outerPolygon">The outer polygon</param>
        /// <param name="innerPolygons">The list of inner polygons</param>
        public void AddPair(Polygon<T> outerPolygon, List<Polygon<T>> innerPolygons)
        {
            PolygonGroup<T> pair = new PolygonGroup<T>(outerPolygon, innerPolygons);

            Polygons.Add(pair);
        }

        /// <summary>
        /// Remove a group of polygons from the SurfaceShape
        /// </summary>
        /// <param name="group"></param>
        public void RemoveGroup(PolygonGroup<T> group)
        {
            Polygons.Remove(group);
        }
    }

    /// <summary>
    /// Represent a group of polygons on a surface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PolygonGroup<T> where T : PolygonVertex
    {
        public Polygon<T> OuterPolygon { get; }

        public List<Polygon<T>> InnerPolygons { get; set; }

        public PolygonGroup(Polygon<T>? outerPolygon, List<Polygon<T>>? innerPolygons = null)
        {
            if (outerPolygon != null) OuterPolygon = outerPolygon;
            else OuterPolygon = new Polygon<T>();

            if (innerPolygons != null) InnerPolygons = innerPolygons;
            else InnerPolygons = new List<Polygon<T>>();
        }
    }
}
