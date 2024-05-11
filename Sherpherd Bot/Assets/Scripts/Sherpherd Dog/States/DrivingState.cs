using Data_control;
using PGGE.Patterns;
using Sheep;
using System;
using System.Linq;
using UnityEngine;

namespace sherpherdDog
{
    /// <summary>
    /// The state that will make flock move to the intent target position
    /// </summary>
    public class DrivingState : ShepherDogState
    {
        //reference the entire flock to know the flock.CG
        SheepFlock flock;

        public DrivingState(FSM fsm, ShepherdDog dog , SheepFlock flock) : base(fsm, dog)
        {
            mId = (int)DogState.Driving;
            this.flock = flock;
        }

        public override void Enter()
        {
            CheckCanCollect();
        }

        /// <summary>
        /// check if there is any misalign sheep. if there is, then try to 
        /// align them back to the flock.cg
        /// </summary>
        void CheckCanCollect()
        {
            //if there are any misalign sheep, make sure to go back to collecting to realign them back
            if (flock.Sheeps.Any(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > dog.GatheringRadius))
            {
                mFsm.SetCurrentState((int)DogState.Collecting);
                return;
            }
        }

        /// <summary>
        /// Check if the flock have manage to reach the target. if it is then make sure
        /// that the dog goes back to listening state.
        /// </summary>
        void CheckReachTarget()
        {
            //if the CG is within the target point
            if(Vector3.Distance(flock.CG, dog.TargetPoint) < dog.TargetRadius)
            {
                mFsm.SetCurrentState((int)DogState.ListeningState);
            }
        }

        public override void Update()
        {
            CheckCanCollect();
            CheckReachTarget();
            
            var targetPoint = ((flock.CG - dog.TargetPoint).normalized * dog.DrivingOffset + flock.CG)
                .With(y: flock.YOffset);
            //find the targetpoint which is behind the flock.cg
            Debug.DrawLine(transform.position, targetPoint, Color.blue);
            var targetDirection = (targetPoint - dog.transform.position).normalized;

            //get the resultant vector and move the dog to align it with the target direction.
            var resultantVector = targetDirection * dog.WeightOfTarget + AvoidOtherSheepsRule() * dog.WeightOfAvoidanceOfOtherSheep;
            //get the target point to arrive
            transform.position += resultantVector.normalized * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        /// <summary>
        /// A rule to avoid scaring the flock if the position of the dog
        /// is unfavorable. (example. the target position requires the
        /// dog to move towards the flock which can break up the cohesion of the
        /// sheep flock. This rule is to make sure that the dog dont ruin
        /// that cohesion so that it makes pushing the sheeps to the intended 
        /// target much easier.
        /// </summary>
        /// <returns></returns>
        private Vector3 AvoidOtherSheepsRule()
        {
            Vector3 result = Vector3.zero;
            var sheeps = Physics.OverlapSphere(transform.position, dog.SenseRadius, LayerManager.SheepLayer)
                .ToArray();
            //find all the sheeps in the area and omit those are near the sheep chosen .

            if (sheeps.Length == 0) return result;

            foreach (var sheep in sheeps)
            {
                var direction = (transform.position - sheep.transform.position);
                //will try to avoid the sheeps that are close to the dog.
                result += direction.normalized * InvSqrt(direction.magnitude * dog.CollectingAvoidanceSheepValue);
            }

            return result;
        }
        private float InvSqrt(float number)
        {
            const float threehalfs = 1.5F;

            float x2 = number * 0.5F;
            float y = number;

            // evil floating point bit level hacking
            uint i = BitConverter.ToUInt32(BitConverter.GetBytes(y), 0);

            // value is pre-assumed
            i = 0x5f3759df - (i >> 1);
            y = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);

            // 1st iteration
            y = y * (threehalfs - (x2 * y * y));

            // 2nd iteration, this can be removed
            // y = y * ( threehalfs - ( x2 * y * y ) );

            return y;
        }

    }
}