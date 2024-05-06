using Assets.Scripts.Shepherd.GOAP;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAPTHOM
{
    public class SearchStrategy : IActionStrategy
    {
        private Shepherd shepherd;
        private Vector3 nextDestination;
        public bool CanPerform => !Complete;
        private Transform transfromReference;
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
            nextDestination = shepherd.transform.position + (UnityEngine.Random.insideUnitSphere * shepherd.WanderingRadius).With(y: 0);
        }

    }
}