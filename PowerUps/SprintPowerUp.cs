using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class SprintPowerUp : IPowerUp
    {
        public static Texture2D pixel;
        public static Texture2D Icon;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new SprintBuff(pj));
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Crimson;
    }
}
