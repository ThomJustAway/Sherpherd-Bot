using Assets.Scripts.Shepherd.GOAP;
using System;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace GOAPTHOM
{
    /// <summary>
    /// The search strategy is where the shepherd will aimlessly 
    /// find a position until a given evaluator is satisfied.
    /// </summary>
    public class SearchStrategy : IActionStrategy
    {
        private Shepherd shepherd;
        private Vector3 nextDestination;
        public bool CanPerform => !Complete;
        //change the search completion here!!!!!!!
        private Func<bool> evaluator;
        public bool Complete => evaluator();

        public SearchStrategy(Shepherd shepherd , Func<bool> evaluator)
        {
            this.shepherd = shepherd;
            this.evaluator = evaluator;
        }

        public void Start()
        {
            //get a new position for the shepherd to explore
            FindNewPosition();
        }

        public void Update(float deltaTime)
        {
            if(Vector3.Distance(shepherd.transform.position, nextDestination) < 0.1f)
            {//if it reach the position, evaluate a new position.
                FindNewPosition();
            }

            //else move to the destination until it reaches it.
            var direction = (nextDestination - shepherd.transform.position ).normalized;
            ///move the shepherd and rotate the shepherd
            var targetRotation = Quaternion.LookRotation(direction);
            Debug.DrawLine(shepherd.transform.position, nextDestination, Color.yellow);
            shepherd.transform.position += direction * shepherd.MovingSpeed * deltaTime;
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation, 
                Time.deltaTime * shepherd.RotationSpeed);
        }

        private void FindNewPosition()
        {
            nextDestination = shepherd.transform.position + 
                (UnityEngine.Random.insideUnitSphere * shepherd.WanderingRadius).With(y: 0);
        }

    }
}