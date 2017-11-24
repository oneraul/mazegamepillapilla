using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class SprintBuff : DurationBuff
    {
        private static readonly float DURATION = 2.5f;
        private static readonly float VELOCITY_BUFF_AMOUNT = 300;
        public static Texture2D texture;

        private Pj pj;

        public SprintBuff(Pj pj) : base(DURATION)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.v += VELOCITY_BUFF_AMOUNT;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Crimson);
        }

        public override void End()
        {
            pj.v -= VELOCITY_BUFF_AMOUNT;
        }

        public override bool ShouldBeRemovedWhenPjGoesImmune() => false;
    }
}
