using System;

namespace GOAPTHOM
{
    public class IdleStrategy : IActionStrategy
    {
        public bool CanPerform => true; // Agent can always Idle
        public bool Complete { get; private set; }

        readonly CountdownTimer timer;

        public IdleStrategy(float duration)
        {
            timer = new CountdownTimer(duration);
            timer.OnTimerStart += () => Complete = false;
            timer.OnTimerStop += () => Complete = true;
        }

        public void Start() => timer.Start();
        public void Update(float deltaTime) => timer.Tick(deltaTime);
    }

    public class WaitTillStrategy : IActionStrategy
    {
        private readonly Func<bool> eval;

        public WaitTillStrategy(Func<bool> eval)
        {
            this.eval = eval;
        }

        public bool CanPerform => !Complete;

        public bool Complete => eval();

        
    }
}