using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGamePillaPilla.PowerUps
{
    abstract class Buff
    {
        public bool ToBeRemoved { get; protected set; }

        public virtual void Activate() { }
        public virtual void Update(float dt) { }
        public virtual void End() { }
        public virtual void Draw(SpriteBatch spritebatch, Matrix cameraMatrix) { }
        public abstract bool ShouldBeRemovedWhenPjGoesImmune();
    }

    public enum BuffTypes
    {
        SprintBuff,
        TraverseWallsBuff,
        BananaStunBuff,
        InvisibleBuff,
        ImmuneBuff,
        RelojBuff,
    }
}
