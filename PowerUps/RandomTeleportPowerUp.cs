using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class RandomTeleportPowerUp : IPowerUp
    {
        public const float ANIMATION_DURATION = 0.15f;
        public static Texture2D Icon;
        public static Random rng = new Random();

        public void Action(Pj pj, Server server)
        {
            ScheduleManager.Schedule(ANIMATION_DURATION, () =>
            {
                pj.SpawnInAnEmptyPosition(server.world.maze);
                server.TeleportPj(pj);
            });
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Orange;
        public PjAnimationMachine.Animations GetAnimation() => PjAnimationMachine.Animations.Teleporting;
    }
}
