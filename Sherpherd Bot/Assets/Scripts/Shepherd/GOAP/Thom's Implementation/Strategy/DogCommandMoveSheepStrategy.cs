using Assets.Scripts.Shepherd.GOAP;
using Sheep;
using sherpherdDog;
using System;
using System.Collections;
using UnityEngine;

namespace GOAPTHOM
{
    public class DogCommandMoveSheepStrategy : IActionStrategy
    {
        private SheepFlock flock;
        private Func<Vector3> positionEval;
        private ShepherdDog dog;
        private Vector3 targetPosition;
        public DogCommandMoveSheepStrategy(SheepFlock flock, 
            Func<Vector3> positionEval,
            ShepherdDog dog)
        {
            this.flock = flock;
            this.dog = dog;
            this.positionEval = positionEval;
        }

        public bool CanPerform => !Complete;
        //if flock cg is at the target position.
        public bool Complete => Vector3.Distance(flock.CG , targetPosition) < dog.TargetRadius ;

        public void Enter()
        {
            dog.ChaseSheeps(positionEval());
            //cache position
            targetPosition = positionEval();
        }

        //wait for the dog to complete the strategy
    }
}