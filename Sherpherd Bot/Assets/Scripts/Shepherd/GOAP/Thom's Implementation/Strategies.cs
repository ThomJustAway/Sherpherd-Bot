using Assets.Scripts.Shepherd.GOAP;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAPTHOM
{
    public interface IActionStrategy
    {
        bool CanPerform { get; }
        bool Complete { get; }

        void Start()
        {
            // noop
        }

        void Update(float deltaTime)
        {
            // noop
        }

        void Stop()
        {
            // noop
        }
    }

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

    public class SearchStrategy : IActionStrategy
    {
        private Shepherd shepherd;
        public bool CanPerform => !Complete;

        public bool Complete => !shepherd.GrassPosition.IsUnityNull() &&
            !shepherd.WaterPosition.IsUnityNull();

        public SearchStrategy(Shepherd shepherd)
        {
            this.shepherd = shepherd;
        }

        public void Start()
        {

        }

        public void Update()
        {

        }
    }
}