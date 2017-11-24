using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    interface IPowerUp
    {
        void Action(Pj pj, Server server);
        Texture2D GetIcon();
        Color GetColor();

    }

    public enum PowerUpTypes
    {
        SprintPowerUp,
        TraverseWallsPowerUp,
        BananaPowerUp,
        InvisiblePowerUp,
        TintaPowerUp,
        ImmunePowerUp,
        RandomTeleportPowerUp,
        RelojPowerUp
    }
}
