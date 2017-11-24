using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class RelojBuff : DurationBuff
    {
        private static readonly float DURATION = 5f;
        private static readonly float VELOCITY_DEBUFF_AMOUNT = 70;

        private Pj pj;

        public RelojBuff(Pj pj) : base(DURATION)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.v -= VELOCITY_DEBUFF_AMOUNT;
        }

        public override void End()
        {
            pj.v += VELOCITY_DEBUFF_AMOUNT;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.LightSlateGray);
        }

        public override bool ShouldBeRemovedWhenPjGoesImmune() => true;
    }
}
