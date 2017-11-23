using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class ImmuneBuff : DurationBuff
    {
        private static float duration = 1;

        private Pj pj;

        public ImmuneBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.SetImmune(true);
        }

        public override void End()
        {
            pj.SetImmune(false);
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.PaleVioletRed);
        }

        public override bool ShouldBeRemovedWhenPjGoesImmune() => false;
    }
}
