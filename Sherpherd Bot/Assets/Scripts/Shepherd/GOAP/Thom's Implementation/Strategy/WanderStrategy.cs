using Assets.Scripts.Shepherd.GOAP;
using sherpherdDog;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAPTHOM
{

    /// <summary>
    /// the wander strategy is a where the shepherd would 
    /// walk around until a certain timer is up.
    /// </summary>
    public class WanderStrategy : IActionStrategy
    {
        readonly Shepherd shepherd;
        CountdownTimer timer;
        public bool CanPerform => !Complete;

        public bool Complete { get; private set; }
        private Vector3 nextDestination;
        public WanderStrategy(Shepherd shepherd , float time)
        {
            this.shepherd = shepherd;
            timer = new CountdownTimer(time);
            timer.OnTimerStart += () => Complete = false;
            timer.OnTimerStop += () => Complete = true;
            //once the timer stop. the startegy is completed.
        }

        public void Start()
        {
            FindNewPosition();
            timer.Start();
        }

        public void Update(float deltaTime)
        {
            timer.Tick(deltaTime);

            if(Vector3.Distance(shepherd.transform.position, nextDestination) < 1f)
            {
                FindNewPosition();
            }//the shepherd will evaluate a new position to walk to once it reach a certain destination.

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