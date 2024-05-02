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
        private Vector3 nextDestination;
        public bool CanPerform => !Complete;

        public bool Complete => !shepherd.GrassPosition.IsUnityNull() &&
            !shepherd.WaterPosition.IsUnityNull();

        public SearchStrategy(Shepherd shepherd)
        {
            this.shepherd = shepherd;
        }

        public void Start()
        {
            FindNewPosition();
        }
        public void Update()
        {
            if(Vector3.Distance(shepherd.transform.position, nextDestination) < 1f)
            {
                FindNewPosition();
            }

            var direction = (shepherd.transform.position - nextDestination).normalized;
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation, 
                Time.deltaTime * shepherd.RotationSpeed);


        }

        private void FindNewPosition()
        {
            nextDestination = shepherd.transform.position + (Random.insideUnitSphere * shepherd.WanderingRadius).With(y: 0);
        }

    }
}