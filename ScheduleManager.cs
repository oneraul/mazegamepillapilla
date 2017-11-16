using System;
using System.Collections.Generic;

namespace MazeGamePillaPilla
{
    static class ScheduleManager
    {
        private static List<float> timers = new List<float>();
        private static List<Action> actions = new List<Action>();

        public static void Schedule(float time, Action action)
        {
            timers.Add(time);
            actions.Add(action);
        }

        public static void ScheduleInLoop(float timer, Action action)
        {
            LoopAction loop = new LoopAction(timer, action);
            Schedule(timer, loop.Handle());
        }

        public static void Update(float dt)
        {
            for (int i = timers.Count-1; i >= 0; i--)
            {
                timers[i] -= dt;
                if (timers[i] <= 0)
                {
                    actions[i]();
                    timers.RemoveAt(i);
                    actions.RemoveAt(i);
                }
            }
        }

        public static void Clear()
        {
            timers.Clear();
            actions.Clear();
        }


        struct LoopAction
        {
            float time;
            Action action;

            public LoopAction(float time, Action action)
            {
                this.time = time;
                this.action = action;
            }

            public Action Handle()
            {
                LoopAction self = this;
                return () =>
                {
                    self.action();
                    ScheduleManager.Schedule(self.time, self.Handle());
                };
            }
        }
    }
}
