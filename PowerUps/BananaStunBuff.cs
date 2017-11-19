using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class BananaStunBuff : DurationBuff
    {
        private static float duration = 1;

        private Pj pj;

        public BananaStunBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.Stunned = true;
        }

        public override void End()
        {
            pj.Stunned = false;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Gold);
        }
    }
}
