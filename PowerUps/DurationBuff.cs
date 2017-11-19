namespace MazeGamePillaPilla.PowerUps
{
    abstract class DurationBuff : Buff
    {
        public float Duration { get; private set; }
        protected float Timer { get; private set; }

        protected DurationBuff(float duration)
        {
            Duration = duration;
            Timer = 0;
        }

        public override void Update(float dt)
        {
            Timer += dt;
            if (Timer >= Duration)
            {
                ToBeRemoved = true;
            }
        }
    }
}
