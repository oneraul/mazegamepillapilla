namespace MazeGamePillaPilla.PowerUps
{
    class InvisibleBuff : DurationBuff
    {
        private static float duration = 2;
        private Pj pj;

        public InvisibleBuff(Pj pj) : base(duration)
        {
            this.pj = pj;
            this.Activate();
        }

        public override void Activate()
        {
            pj.Invisible = true;
        }

        public override void End()
        {
            pj.Invisible = false;
        }
    }
}
