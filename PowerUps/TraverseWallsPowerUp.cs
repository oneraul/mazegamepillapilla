using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class TraverseWallsPowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj)
        {
            pj.Buffs.Add(new TraverseWallsBuff(pj));
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.Aqua;
    }
}
