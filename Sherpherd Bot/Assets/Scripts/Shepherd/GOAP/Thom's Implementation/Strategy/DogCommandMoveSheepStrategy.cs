using Assets.Scripts.Shepherd.GOAP;
using Sheep;
using sherpherdDog;
using System;
using System.Collections;
using UnityEngine;

namespace GOAPTHOM
{
    /// <summary>
    /// for commanding the dog to make it such that it will move the flock to the a 
    /// certain area.
    /// </summary>
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
        //will do nothing until the dog manage to bring the flock to the target position
        public void Start()
        {
            Debug.Log("try command dog");
            dog.ChaseSheeps(positionEval());
            //cache position
            targetPosition = positionEval();
        }

        //wait for the dog to complete the strategy
    }
}