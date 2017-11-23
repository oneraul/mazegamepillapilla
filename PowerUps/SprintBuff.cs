using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class SprintBuff : DurationBuff
    {
        private static float duration = 2.5f;
        private static float velocityBuffAmount = 300;
        public static Texture2D texture;

        private Pj pj;

        public SprintBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.v += velocityBuffAmount;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Crimson);
        }

        public override void End()
        {
            pj.v -= velocityBuffAmount;
        }

        public override bool ShouldBeRemovedWhenPjGoesImmune() => false;
    }
}
