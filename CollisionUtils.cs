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

        private static Random rng = new Random();


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
            // http://playtechs.blogspot.cz/2007/03/raytracing-on-grid.html

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


        public static bool AabbMapIntersectionTest(this IIntersectable item, Cell[,] maze)
        {
            foreach (Cell cell in item.GetSurroundingCells(maze))
            {
                if (item.AabbAabbIntersectionTest(cell))
                {
                    if (item.SatIntersectionTest(cell))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Cell[] GetSurroundingCells(this IIntersectable item, Cell[,] maze)
        {
            Rectangle aabb = item.GetAABB();
            List<Cell> cells = new List<Cell>();

            int currentCellX = aabb.Center.X / Tile.Size;
            int currentCellY = aabb.Center.Y / Tile.Size;

            int smallX = currentCellX - 1;
            int largeX = currentCellX + 1;

            int smallY = currentCellY - 1;
            int largeY = currentCellY + 1;

            for (int y = smallY; y <= largeY; y++)
            {
                for (int x = smallX; x <= largeX; x++)
                {
                    Cell cell = maze[y, x];
                    if (cell.Tile.Id != 0)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells.ToArray();
        }


        public static void SpawnInAnEmptyPosition(this ISpawnable itemToSpawn, Cell[,] maze)
        {
            IIntersectable intersectable = (IIntersectable)itemToSpawn;
            do
            {
                itemToSpawn.SetPosition(
                    rng.Next(2 * Tile.Size, (maze.GetLength(1) - 1) * Tile.Size),
                    rng.Next(2 * Tile.Size, (maze.GetLength(0) - 1) * Tile.Size));

            } while (intersectable.AabbMapIntersectionTest(maze));
        }
    }
}
