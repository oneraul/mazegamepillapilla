using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class ImmunePowerUp : IPowerUp
    {
        public static Texture2D Icon;

        public void Action(Pj pj, Server server)
        {
            List<int> buffsToBeRemoved = new List<int>();
            foreach (KeyValuePair<int, Buff> kvp in pj.Buffs)
            {
                if (kvp.Value.ShouldBeRemovedWhenPjGoesImmune())
                {
                    buffsToBeRemoved.Add(kvp.Key);
                }
            }
            foreach (int buffId in buffsToBeRemoved)
            {
                server.RemoveBuff(pj, buffId);
            }

            server.AddBuff((int)BuffTypes.ImmuneBuff, pj);
        }

        public Texture2D GetIcon() => Icon;
        public Color GetColor() => Color.PaleVioletRed;
        public PjAnimationMachine.Animations GetAnimation() => PjAnimationMachine.Animations.Test;
    }
}
