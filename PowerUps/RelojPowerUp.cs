using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class RelojPowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            foreach (Pj otherPj in server.world.Pjs.Values)
            {
                if (otherPj.ID != pj.ID)
                {
                    server.AddBuff((int)BuffTypes.RelojBuff, otherPj);
                }
            }
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.LightSlateGray;
    }
}
