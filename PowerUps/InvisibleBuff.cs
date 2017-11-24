namespace MazeGamePillaPilla.PowerUps
{
    class InvisibleBuff : DurationBuff
    {
        private static readonly float DURATION = 2;
        private Pj pj;

        public InvisibleBuff(Pj pj) : base(DURATION)
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

        public override bool ShouldBeRemovedWhenPjGoesImmune() => false;
    }
}
