using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla
{
    class GameScreen : IScreen
    {
        protected Matrix cameraMatrix;
        internal Cell[,] maze;
        internal Texture2D pixel;

        internal Dictionary<string, Pj> Pjs;
        internal List<LocalPj> LocalPlayers;

        private Client client;

        internal GameScreen(Client client)
        {
            this.client = client;
            Pjs = new Dictionary<string, Pj>();
            LocalPlayers = new List<LocalPj>();
        }


        public void Initialize(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            System.Diagnostics.Debug.WriteLine("Enters GameScreen");
            cameraMatrix = Matrix.CreateTranslation(400 - Tile.Size * 11, Tile.Size, 0);
        }

        public void Enter()
        {
            client.CharacterUpdated += OnCharacterUpdated;
        }

        public void Exit()
        {
            client.CharacterUpdated -= OnCharacterUpdated;
        }


        public void OnCharacterUpdated(object source, GameplayUpdateEventArgs args)
        {
            Pjs[args.Packet.CharacterID].ProcessServerUpdate(args.Packet, maze);
        }


        public void Update(float dt)
        {
            client.GameplayUpdate(dt);
            foreach (LocalPj pj in LocalPlayers) pj.ProcessInput(dt, client, maze);
            foreach (Pj pj in Pjs.Values) pj.Update(dt, maze);
        }


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

            foreach (IDrawable drawable in SortDrawables())
            {
                drawable.Draw(spritebatch, cameraMatrix);
            }

            spritebatch.End();
        }
    }

    class ServerGameScreen : GameScreen, IScreen
    {
        private Server server;

        internal ServerGameScreen(Client client, Server server) : base(client)
        {
            this.server = server;
        }

        public new void Enter()
        {
            Input.Pressed += OnBackPressed;
            base.Enter();
        }

        public new void Exit()
        {
            Input.Pressed -= OnBackPressed;
            base.Exit();
        }

        public new void Update(float dt)
        {
            server.GameplayUpdate(dt);
            base.Update(dt);
        }

        public new void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cameraMatrix);
            spriteBatch.DrawString(Button.Font, "SERVER", new Vector2(20, 20), Color.Red);
            spriteBatch.End();
        }

        protected void OnBackPressed(object source, InputStateEventArgs args)
        {
            if ((args.PlayerIndex == PlayerControllerIndex.Keyboard || args.PlayerIndex == PlayerControllerIndex.One)
            && args.Button == InputKeys.Back)
            {
                server.GoToScoresScreen();
            }
        }
    }
}
