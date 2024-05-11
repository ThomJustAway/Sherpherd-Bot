namespace GOAPTHOM
{
    /// <summary>
    /// idle strategy for the shepherd to chill and relax :)
    /// </summary>
    public class IdleStrategy : IActionStrategy
    {
        public bool CanPerform => true; // Agent can always Idle
        public bool Complete { get; private set; }

        readonly CountdownTimer timer;

        //will just wait for timer to complete once the timer is up.
        //it finish it's idle strategy. 
        public IdleStrategy(float duration)
        {
            timer = new CountdownTimer(duration);
            timer.OnTimerStart += () => Complete = false;
            timer.OnTimerStop += () => Complete = true;
        }

        public void Start() => timer.Start();
        public void Update(float deltaTime) => timer.Tick(deltaTime);
    }
}