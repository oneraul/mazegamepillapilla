using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    class TraverseWallsBuff : DurationBuff
    {
        private static readonly float DURATION = 1.5f;
        public static Texture2D texture;

        private Pj pj;

        public TraverseWallsBuff(Pj pj) : base(DURATION)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.CanTraverseWalls = true;
        }

        public override void Draw(SpriteBatch spritebatch, Matrix cameraMatrix)
        {
            spritebatch.Draw(SprintBuff.texture, new Rectangle((int)(pj.x - 16), (int)(pj.y - 16), 32, 32), Color.Aqua);
        }

        public override void End()
        {
            pj.CanTraverseWalls = false;
        }

        public override bool ShouldBeRemovedWhenPjGoesImmune() => false;
    }
}
