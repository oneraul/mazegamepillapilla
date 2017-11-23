using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class RandomTeleportPowerUp : IPowerUp
    {
        public static Texture2D Icon;
        public static Random rng = new Random();

        public void Action(Pj pj, Server server)
        {
            pj.SpawnInAnEmptyPosition(server.world.maze);
            server.TeleportPj(pj);
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Orange;
    }
}
