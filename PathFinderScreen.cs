using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class PathfinderScreen : IScreen
    {
        private Cell[,] maze;
        private Texture2D pixel;
        private Dictionary<string, Pj> Pjs;
        private List<AiPj> aiPjs;
        private Cell CurrentCell;
        private PathfindingNode[] nodes;
        private PathfindingNodeHorizontal[,] horizontalNodes;
        private PathfindingNodeVertical[,] verticalNodes;
        private Matrix cameraMatrix;

        public void Draw(SpriteBatch spritebatch)
        {
            List<IDrawable> SortDrawables()
            {
                List<IDrawable> drawables = new List<IDrawable>();

                List<Pj> pjsToInsert = new List<Pj>();
                pjsToInsert.AddRange(Pjs.Values);
                pjsToInsert.Sort((a, b) => b.y.CompareTo(a.y));

                int mazeW = maze.GetLength(1);
                int mazeH = maze.GetLength(0);
                for (int y = 0; y < mazeH; y++)
                {
                    for (int i = pjsToInsert.Count - 1; i >= 0; i--)
                    {
                        Pj pj = pjsToInsert[i];

                        Cell leftCell = maze[y, (int)((pj.x - pj.hw) / Tile.Size)];
                        Cell rightCell = maze[y, (int)((pj.x + pj.hw) / Tile.Size)];

                        if (pj.y < leftCell.GetSortY() && pj.y < rightCell.GetSortY())
                        {
                            drawables.Add(pj);
                            pjsToInsert.RemoveAt(i);
                        }
                    }

                    for (int x = 0; x < mazeW; x++)
                    {
                        drawables.Add(maze[y, x]);
                    }
                }

                return drawables;
            }


            spritebatch.GraphicsDevice.Clear(new Color(77f / 255, 174f / 255, 183f / 255));

            spritebatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
            spritebatch.Draw(pixel, new Rectangle(0, 0, maze.GetLength(1) * Tile.Size, maze.GetLength(0) * Tile.Size), new Color(182f / 255, 186f / 255, 159f / 255));

            foreach (AiPj pj in aiPjs) Pathfinder.DrawPath(spritebatch, pixel, pj.path);

            foreach (IDrawable drawable in SortDrawables())
            {
                drawable.Draw(spritebatch, cameraMatrix);
            }

            /*foreach (PathfindingNode node in nodes)
            {
                node.DrawNavigationPosition(spritebatch, pixel);
            }*/

            spritebatch.End();
        }

        public void Enter()
        {
            CurrentCell = maze[5, 3];
            horizontalNodes = new PathfindingNodeHorizontal[maze.GetLength(0), maze.GetLength(1)];
            verticalNodes = new PathfindingNodeVertical[maze.GetLength(0), maze.GetLength(1)];

            // init horizontal nodes
            for (int i = 2; i < horizontalNodes.GetLength(0)-1; i++)
            {
                for(int j = 2; j < horizontalNodes.GetLength(1)-2; j++)
                {
                    Tile topTile = maze[i-1, j].Tile;
                    Tile bottomTile = maze[i, j].Tile;
                    horizontalNodes[i, j] = new PathfindingNodeHorizontal(i, j, topTile, bottomTile);
                    if (horizontalNodes[i, j].ToRemove) horizontalNodes[i, j] = null;
                }
            }

            // init vertical nodes
            for (int i = 1; i < verticalNodes.GetLength(0) - 2; i++)
            {
                for (int j = 2; j < verticalNodes.GetLength(1) - 1; j++)
                {
                    Tile leftTile = maze[i, j-1].Tile;
                    Tile rightTile = maze[i, j].Tile;
                    verticalNodes[i, j] = new PathfindingNodeVertical(i, j, leftTile, rightTile);
                    if (verticalNodes[i, j].ToRemove) verticalNodes[i, j] = null;
                }
            }

            // connect the nodes
            List<PathfindingNode> tmp = new List<PathfindingNode>();
            for (int i = 0; i < verticalNodes.GetLength(0); i++)
            {
                for (int j = 0; j < verticalNodes.GetLength(1); j++)
                {
                    PathfindingNodeHorizontal horizontalNode = horizontalNodes[i, j];
                    if(horizontalNode != null)
                    {
                        tmp.Clear();
                        tmp.AddIfNotNull(horizontalNodes[i - 1, j]);
                        tmp.AddIfNotNull(horizontalNodes[i + 1, j]);
                        tmp.AddIfNotNull(verticalNodes[i, j]);
                        tmp.AddIfNotNull(verticalNodes[i, j + 1]);
                        tmp.AddIfNotNull(verticalNodes[i - 1, j]);
                        tmp.AddIfNotNull(verticalNodes[i - 1, j + 1]);
                        horizontalNode.OpenNodes = tmp.ToArray();
                    }

                    PathfindingNodeVertical verticalNode = verticalNodes[i, j];
                    if(verticalNode != null)
                    {
                        tmp.Clear();
                        tmp.AddIfNotNull(verticalNodes[i, j - 1]);
                        tmp.AddIfNotNull(verticalNodes[i, j + 1]);
                        tmp.AddIfNotNull(horizontalNodes[i, j]);
                        tmp.AddIfNotNull(horizontalNodes[i, j - 1]);
                        tmp.AddIfNotNull(horizontalNodes[i + 1, j]);
                        tmp.AddIfNotNull(horizontalNodes[i + 1, j - 1]);
                        verticalNode.OpenNodes = tmp.ToArray();
                    }
                }
            }

            // store them all in a single array
            tmp.Clear();
            /*
            for (int i = 0; i < verticalNodes.GetLength(0); i++)
            {
                for (int j = 0; j < verticalNodes.GetLength(1); j++)
                {
                    tmp.AddIfNotNull(horizontalNodes[i, j]);
                    tmp.AddIfNotNull(verticalNodes[i, j]);
                }
            }
            */
            foreach (PathfindingNode node in horizontalNodes) { tmp.AddIfNotNull(node); }
            foreach (PathfindingNode node in verticalNodes) { tmp.AddIfNotNull(node); }
            nodes = tmp.ToArray();
        }

        public void Exit()
        {
        }

        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            cameraMatrix = Matrix.CreateTranslation(400 - Tile.Size * 11, Tile.Size, 0);

            Tile.Init(GraphicsDevice);

            // load maze
            int MapId = 0;
            maze = Cell.ParseData(Content, MapData.GetMap(MapId));

            // load graphic stuff
            pixel = Content.Load<Texture2D>("pixel");
            Pj.ColliderTexture = Content.Load<Texture2D>("pj");
            Pj.IdleTexture = Content.Load<Texture2D>("pj_idle");
            Pj.RunningTexture = Content.Load<Texture2D>("pj_running");
            Pj.PaletteTexture = Content.Load<Texture2D>("pj_palette");
            Pj.effect = Content.Load<Effect>("pj_shader");
            Pj.effect.Parameters["u_lut"].SetValue(Pj.PaletteTexture);


            this.Pjs = new Dictionary<string, Pj>();
            this.aiPjs = new List<AiPj>();

            //Pjs.Add("Player", new TestPj("Player", PlayerControllerIndex.Keyboard, 18.5f * Tile.Size, 2.5f * Tile.Size, 1));
            AiPj aipj = new AiPj("AI", Tile.Size * 2.5f, Tile.Size * 2.5f, 2);
            Pjs.Add("AI", aipj);
            aiPjs.Add(aipj);
        }

        public void Update(float dt)
        {
            foreach (Pj pj in Pjs.Values) pj.Update(dt, maze);

            foreach (AiPj pj in aiPjs)
            {
                if (pj.NeedsToRecalculatePath)
                {
                    pj.NeedsToRecalculatePath = false;
                    Pj player = Pjs["Player"];
                    Vector2 to = new Vector2(player.x, player.y);
                    Vector2 from = new Vector2(pj.x, pj.y);
                    pj.path = Pathfinder.CalculatePath(nodes, horizontalNodes, verticalNodes, from, to, maze);
                }
            }
        }
    }
}
