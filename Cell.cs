using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class Cell : IDrawable, IIntersectable
    {
        private readonly static Random rand = new Random();

        internal static Cell[,] ParseData(ContentManager Content, int[,] mapData)
        {
            int mazeH = mapData.GetLength(0);
            int mazeW = mapData.GetLength(1);

            Cell[,] maze = new Cell[mazeH, mazeW];

            for(int y = 0; y < mazeH; y++)
            {
                for(int x = 0; x < mazeW; x++)
                {
                    maze[y, x] = new Cell(mapData[y, x], x, y, Content);
                }
            }

            return maze;
        }


        public Tile Tile { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        private Texture2D texture;
        private Vector2 position;
        private Color color;

        private Vector2[] vertices;
        private Vector2[] projectionAxes;


        Cell(int tile, int x, int y, ContentManager Content)
        {
            this.Tile = Tile.Tileset[tile];

            this.X = x * Tile.Size;
            this.Y = y * Tile.Size;

            position = new Vector2(this.X, this.Y-Tile.Size);

            float r, g, b;
            if (tile == 1)
            {
                r = (float)rand.NextDouble();
                g = (float)rand.NextDouble();
                b = (float)rand.NextDouble();
            }
            else
            {
                r = g = b = 0;
            }
            color = new Color(r, g, b);

            texture = Tile.Texture;

            // ---------------------------------------

            Edge[] GetEdges()
            {
                if (Tile.Id == 0) return new Edge[0];

                Edge[] edges = new Edge[4];

                Tile ti = Tile;
                Edge[] ed = Tile.Edges;

                for (int i = 0; i < this.Tile.Edges.Length; i++)
                {
                    edges[i] = new Edge(this.Tile.Edges[i], X, Y);
                };

                return edges;
            }
            Vector2[] ComputeVertices()
            {
                List<Vector2> verts = new List<Vector2>();

                foreach (Edge edge in GetEdges())
                {
                    verts.Add(edge.A);
                    verts.Add(edge.B);
                }

                return verts.ToArray();
            }
            Vector2[] ComputeProjectionAxes()
            {
                List<Vector2> axes = new List<Vector2>();

                bool vertical = false;
                bool horizontal = false;

                foreach (Edge edge in GetEdges())
                {
                    Vector2 tmp = edge.B - edge.A;
                    tmp.Normalize();
                    Vector2 axis = new Vector2(-tmp.Y, tmp.X);

                    if ((axis.X == -1 || axis.X == 1) && axis.Y == 0)
                    {
                        if (!horizontal)
                        {
                            horizontal = true;
                            axes.Add(new Vector2(1, 0));
                        }
                    }
                    else if (axis.X == 0 && (axis.Y == 1 || axis.Y == -1))
                    {
                        if (!vertical)
                        {
                            vertical = true;
                            axes.Add(new Vector2(0, 1));
                        }
                    }
                    else
                    {
                        if (!axes.Contains(axis))
                            axes.Add(axis);
                    }
                }

                return axes.ToArray();
            }

            vertices = ComputeVertices();
            projectionAxes = ComputeProjectionAxes();
        }

        public void Draw(SpriteBatch batch, Matrix cameraMatrix)
        {
            batch.Draw(texture, position, Color.White);
        }

        public float GetSortY()
        {
            return Y + Tile.SortY;
        }

        public Vector2[] GetVertices()
        {
            return vertices;
        }

        public Vector2[] GetSatProjectionAxes()
        {
            return projectionAxes;
        }

        public Rectangle GetAABB()
        {
            if (this.Tile.Id == 0) return new Rectangle(-99999, -99999, 0, 0);
            return new Rectangle(X, Y, Tile.Size, Tile.Size);
        }
    }
}
