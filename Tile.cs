using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class Tile
    {
        internal readonly static int Size = 32;
        public static Tile[] Tileset { get; private set; }

        public int Id { get; protected set; }
        public float SortY { get; protected set; }
        public Edge[] Edges { get; protected set; } // clock-wise order: top, right, bottom, left
        public Texture2D Texture { get; protected set; }


        public static void Init(GraphicsDevice GraphicsDevice)
        {
            InitData();
            InitTextures(GraphicsDevice);
        }
        

        private void Draw(GraphicsDevice GraphicsDevice, BasicEffect effect)
        {
            if (Id == 0) return;

            const int height = 1;//20;
            for (int h = 0; h < height; h++)
            {
                Color color = (h == height-1) ? new Color(153f / 255, 186f / 255, 0) : new Color(118f / 255, 166f / 255, 19f / 255);
                VertexPositionColor[] verts;
                int primitivesCount = 2;
                verts = new VertexPositionColor[4] {
                    new VertexPositionColor(new Vector3(Edges[0].A.X, Edges[0].A.Y + Size - h, 0), color),
                    new VertexPositionColor(new Vector3(Edges[1].A.X, Edges[1].A.Y + Size - h, 0), color),
                    new VertexPositionColor(new Vector3(Edges[3].A.X, Edges[3].A.Y + Size - h, 0), color),
                    new VertexPositionColor(new Vector3(Edges[2].A.X, Edges[2].A.Y + Size - h, 0), color),
                };
                
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, verts, 0, primitivesCount);
                }
            }
        }


        private static void InitTextures(GraphicsDevice GraphicsDevice)
        {
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            RenderTarget2D canvas = new RenderTarget2D(GraphicsDevice, Size, Size*2);

            GraphicsDevice.SetRenderTarget(canvas);
            GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None, MultiSampleAntiAlias = false };

            effect.World = Matrix.Identity;
            effect.View = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, Tile.Size, Tile.Size*2, 0, 0.1f, 100f);
            effect.VertexColorEnabled = true;

            for (int i = 0; i < Tileset.Length; i++)
            {
                GraphicsDevice.Clear(Color.Transparent);

                Tileset[i].Draw(GraphicsDevice, effect);

                Color[] textureData = new Color[canvas.Width * canvas.Height];
                canvas.GetData(textureData);
                Tileset[i].Texture = new Texture2D(GraphicsDevice, canvas.Width, canvas.Height);
                Tileset[i].Texture.SetData(textureData);
            }

            GraphicsDevice.SetRenderTarget(null);
            effect.Dispose();
            canvas.Dispose();
        }


        private static void InitData()
        {
            Tileset = new Tile[26];
            for (int i = 0; i < Tileset.Length; i++)
            {
                Tileset[i] = new Tile() { Id = i, SortY = Size };
            }

            int s = Size;
            int h = Size / 2;

            Tileset[1].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, 0)
            };

            Tileset[2].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, s),
                new Edge(0, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[3].SortY = 0;
            Tileset[3].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   0, s),
                new Edge(s, 0,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[4].SortY = 0;
            Tileset[4].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, 0),
                new Edge(s, s,   0, 0),
            };

            Tileset[5].Edges = new Edge[4]
            {
                new Edge(0, s,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   s, 0),
            };

            Tileset[6].Edges = new Edge[4]
            {
                new Edge(0, h,   s, h),
                new Edge(s, h,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, h),
            };

            Tileset[7].SortY = h;
            Tileset[7].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, h),
                new Edge(s, h,   0, h),
                new Edge(0, h,   0, 0),
            };

            Tileset[8].Edges = new Edge[4]
            {
                new Edge(0, 0,   h, 0),
                new Edge(h, 0,   h, s),
                new Edge(h, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[9].Edges = new Edge[4]
            {
                new Edge(h, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   h, s),
                new Edge(h, s,   h, 0),
            };

            Tileset[10].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, h),
                new Edge(s, h,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[11].Edges = new Edge[4]
            {
                new Edge(0, h,   s, s),
                new Edge(0, h,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, h),
            };

            Tileset[12].SortY = 0;
            Tileset[12].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   0, h),
                new Edge(0, h,   0, 0),
                new Edge(0, h,   0, 0),
            };

            Tileset[13].SortY = 0;
            Tileset[13].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, h),
                new Edge(s, h,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[14].SortY = 0;
            Tileset[14].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, h),
                new Edge(0, h,   0, 0),
            };

            Tileset[15].SortY = 0;
            Tileset[15].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, h),
                new Edge(s, 0,   s, h),
                new Edge(s, h,   0, 0),
            };

            Tileset[16].Edges = new Edge[4]
            {
                new Edge(s, h,   s, s),
                new Edge(s, h,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   s, h),
            };

            Tileset[17].Edges = new Edge[4]
            {
                new Edge(0, h,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, h),
            };

            Tileset[18].Edges = new Edge[4]
            {
                new Edge(0, 0,   h, s),
                new Edge(0, 0,   h, s),
                new Edge(h, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[19].Edges = new Edge[4]
            {
                new Edge(0, 0,   h, 0),
                new Edge(h, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[20].SortY = 0;
            Tileset[20].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   h, s),
                new Edge(h, s,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[21].SortY = 0;
            Tileset[21].Edges = new Edge[4]
            {
                new Edge(0, 0,   h, 0),
                new Edge(h, 0,   0, s),
                new Edge(h, 0,   0, s),
                new Edge(0, s,   0, 0),
            };

            Tileset[22].SortY = 0;
            Tileset[22].Edges = new Edge[4]
            {
                new Edge(h, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   h, 0),
            };

            Tileset[23].SortY = 0;
            Tileset[23].Edges = new Edge[4]
            {
                new Edge(0, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   h, s),
                new Edge(h, s,   0, 0),
            };

            Tileset[24].Edges = new Edge[4]
            {
                new Edge(h, 0,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   0, s),
                new Edge(0, s,   h, 0),
            };

            Tileset[25].Edges = new Edge[4]
            {
                new Edge(h, s,   s, 0),
                new Edge(s, 0,   s, s),
                new Edge(s, s,   h, s),
                new Edge(h, s,   s, 0),
            };
        }
    }
}
