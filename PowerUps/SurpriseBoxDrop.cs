using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class SurpriseBoxDrop : Drop
    {
        private static Random rng = new Random();

        public static Texture2D modelTexture;
        private static int radius = 10;

        private AnimationFrame model;
        private float rotation;

        public SurpriseBoxDrop(int x, int y) : base(x, y, radius, (pj, server) =>
        {
            int roll = rng.Next(4);
            roll = 3;
            switch (roll)
            {
                case 0: server.AddPowerUp((int)PowerUpTypes.SprintPowerUp, pj); break;
                case 1: server.AddPowerUp((int)PowerUpTypes.TraverseWallsPowerUp, pj); break;
                case 2: server.AddPowerUp((int)PowerUpTypes.BananaPowerUp, pj); break;
                case 3: server.AddPowerUp((int)PowerUpTypes.InvisiblePowerUp, pj); break;
            }
        })
        {
            int layers = 16;
            int side = 16;
            model = new AnimationFrame(layers);
            for (int i = 0; i < layers; i++)
            {
                model.Rectangles[i] = new Rectangle(i * layers, 0, side, side);
            }
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintPowerUp.pixel, GetAABB(), Color.GreenYellow);
            model.Draw(modelTexture, spritebatch, GetAABB().Center.X, GetAABB().Center.Y, rotation, 1, 8, 8);
        }

        public override void Update(float dt)
        {
            rotation = (float)((rotation + dt) % (Math.PI * 2));
        }

        public static Point SpawnInAnEmptyPosition(Cell[,] maze)
        {
            int x = rng.Next(2 * Tile.Size, (maze.GetLength(1) - 1) * Tile.Size);
            int y = rng.Next(2 * Tile.Size, (maze.GetLength(0) - 1) * Tile.Size);
            SurpriseBoxDrop box = new SurpriseBoxDrop(x, y);

            bool isFree(SurpriseBoxDrop drop)
            {
                foreach (Vector2 vertex in drop.GetVertices())
                {
                    int currentCellX = (int)(vertex.X / Tile.Size);
                    int currentCellY = (int)(vertex.Y / Tile.Size);

                    if (currentCellX >= 0 && currentCellX < maze.GetLength(1)
                    && currentCellY >= 0 && currentCellY < maze.GetLength(0))
                    {
                        Cell cell = maze[currentCellY, currentCellX];
                        if (drop.AabbAabbIntersectionTest(cell))
                        {
                            if (drop.SatIntersectionTest(cell))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            int iteraciones = 0;
            while (!isFree(box))
            {
                iteraciones++;
                x = rng.Next(2 * Tile.Size, (maze.GetLength(1) - 1) * Tile.Size);
                y = rng.Next(2 * Tile.Size, (maze.GetLength(0) - 1) * Tile.Size);
                box.SetPosition(x, y);
            }

            return new Point(x, y);
        }
    }
}
