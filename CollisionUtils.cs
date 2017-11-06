using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MazeGamePillaPilla
{
    static class CollisionUtils
    {
        public struct Range
        {
            public float min;
            public float max;

            public Range(float min, float max)
            {
                this.min = min;
                this.max = max;
            }
        }


        private static Range Get1dProjectionOntoAxis(this IIntersectable intersectable, Vector2 axis)
        {
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            foreach (Vector2 vertex in intersectable.GetVertices())
            {
                float projection = Vector2.Dot(vertex, axis);
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }

            return new Range(min, max);
        }


        private static bool IsThere1dProjectionOverlap(Range A, Range B)
        {
            return !(A.min > B.max || A.max < B.min);
        }


        // Should be called after IsThere1dProjectionOverlap
        private static float Get1dProjectionOverlap(Range A, Range B)
        {
            return A.min < B.min ? A.max - B.min : A.min - B.max;
        }


        public static bool AabbAabbIntersectionTest(this IIntersectable A, IIntersectable B)
        {
            Rectangle a = A.GetAABB();
            Rectangle b = B.GetAABB();
            return !(a.Right < b.Left || a.Left > b.Right || a.Bottom < b.Top || a.Top > b.Bottom);
        }


        public static bool SatIntersectionTest(this IIntersectable A, IIntersectable B)
        {
            if (A.GetSatProjectionAxes().Length == 0 || B.GetSatProjectionAxes().Length == 0) return false;

            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(A.GetSatProjectionAxes());
            axes.AddRange(B.GetSatProjectionAxes());

            foreach (Vector2 axis in axes)
            {
                Range projectionA = A.Get1dProjectionOntoAxis(axis);
                Range projectionB = B.Get1dProjectionOntoAxis(axis);

                if (!IsThere1dProjectionOverlap(projectionA, projectionB))
                {
                    return false;
                }
            }

            return true;
        }


        public static Vector2? SatIntersectionTestGetMtv(this IIntersectable A, IIntersectable B)
        {
            if (A.GetSatProjectionAxes().Length == 0 || B.GetSatProjectionAxes().Length == 0) return null;

            List<Vector2> axes = new List<Vector2>();
            axes.AddRange(A.GetSatProjectionAxes());
            axes.AddRange(B.GetSatProjectionAxes());
            axes = new HashSet<Vector2>(axes).ToList(); // remove duplicated axes

            float minOverlap = float.PositiveInfinity;
            Vector2? minOverlapAxis = null;

            foreach (Vector2 axis in axes)
            {
                Range projectionA = A.Get1dProjectionOntoAxis(axis);
                Range projectionB = B.Get1dProjectionOntoAxis(axis);

                if (!IsThere1dProjectionOverlap(projectionA, projectionB))
                {
                    return null;
                }

                float overlap = Get1dProjectionOverlap(projectionA, projectionB);

                if (Math.Abs(overlap) < Math.Abs(minOverlap))
                {
                    minOverlap = overlap;
                    minOverlapAxis = axis;
                }
            }

            if (minOverlapAxis == null) return null;

            return minOverlapAxis * minOverlap;
        }

        public static bool RayMapIntersectionTest(VisibilityRay ray, Cell[,] maze)
        {
            // TODO
            // Optimize testing only against the possible instersection cells instead of the whole maze.
            // Use the Bresenham Line-Drawing Algorithm to find de cells to check.

            foreach (Cell cell in maze)
            {
                if (ray.AabbAabbIntersectionTest(cell))
                {
                    if (ray.SatIntersectionTest(cell))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
