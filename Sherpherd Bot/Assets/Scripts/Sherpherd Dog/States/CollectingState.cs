using Data_control;
using PGGE.Patterns;
using Sheep;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace sherpherdDog
{
    //needs to do the following, 
    public class CollectingState : ShepherDogState
    {
        //DrivingOffset is a state where the shepherd dog would collect wandering
        //sheeps to group them up to the global center of the herd.
        private SheepBehaviour sheepChosen;
        private SheepFlock flock;
        private Vector3 sheepChosenPosition { get { return sheepChosen?.transform.position ?? Vector3.zero; } }
        Vector3 targetPoint;
        public CollectingState(FSM fsm, ShepherdDog dog , SheepFlock flock) : base(fsm, dog)
        {
            mId = (int)DogState.Collecting;
            this.flock = flock;
        }
        public override void Enter()
        {
            CheckCanDrive();
            //else find the sheep closest and target them
        }

        //check condition first
        private void CheckCanDrive()
        {
            if(flock.Sheeps.All(sheep => Vector3.Distance(sheep.transform.position, flock.CG) <= dog.GatheringRadius))
            {//if all the sheep is within cohesion radius from the point of CG. the dog
             //should be able to drive the sheeps.
                mFsm.SetCurrentState((int)DogState.Driving);
                return;
            }
            //else that means that there is a sheep that is outside and find the nearest sheep to push
            //the sheep back to the flock.
            SearchClosestSheep();
        }

        /// <summary>
        /// Find the closest sheep that is out of the flock.
        /// </summary>
        private void SearchClosestSheep()
        {
            //find all the sheep that are outside of the flock.cg 
            var targetSheeps = flock.Sheeps
                .Where(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > dog.GatheringRadius)
                .ToArray();

            SheepBehaviour sheepChosen = targetSheeps[0];
            Vector3 dogPosition = dog.transform.position;
            float closestDistance = Vector3.Distance(sheepChosen.transform.position, dogPosition);

            //From the sheep that are outside, find the closest sheep
            for(int i = 1; i < targetSheeps.Length; i++)
            {//skip the first sheep and keep on moving through the array
                float distance = Vector3.Distance(targetSheeps[i].transform.position, dogPosition);
                if (distance < closestDistance)
                {
                    //set new sheep and distance
                    sheepChosen = targetSheeps[i];
                    closestDistance = distance;
                }
            }

            this.sheepChosen = sheepChosen;
        }

        public override void Update()
        {
            CheckReachTarget();
            if (Vector3.Distance(sheepChosenPosition , flock.CG) < flock.CohesionRadius)
            {
                //if under cohesion radius, check if there are any other sheep that needs to be collected
                CheckCanDrive();
            }
            //Find the position that allows the chosen sheep to be push to the flock.
            targetPoint = (((sheepChosenPosition - flock.CG).normalized * dog.CollectionOffset) + sheepChosenPosition)
                .With(y: flock.YOffset);

            Vector3 targetMovement = (targetPoint - dog.transform.position).normalized;

            
            targetMovement *= dog.WeightOfTarget;
            var avoidScaringSheepDirection = AvoidScaringSheepRule() * dog.WeightOfAvoidanceFromChosenSheep;
            //add the weights from the target and to avoid the scaring of sheep to get resultant vector.
            Vector3 resultantMovement = targetMovement +
                avoidScaringSheepDirection;

            Debug.DrawRay(transform.position, targetMovement, Color.yellow);
            Debug.DrawRay(transform.position, avoidScaringSheepDirection, Color.red);
            Debug.DrawLine(transform.position, targetPoint, Color.white);

            transform.position +=  resultantMovement.normalized * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetMovement);
        }
        /// <summary>
        /// Rule to avoid scaring sheeps that are within the flock.cg.
        /// This prevent the hardwork of the shepherd to go to waste.
        /// It would also try to move behind the sheep so that it would 
        /// not have to chase for the sheep.
        /// </summary>
        /// <returns>A vector that avoid the sheeps in the flock.cg</returns>
        private Vector3 AvoidScaringSheepRule()
        {
            //will need to check the angle
            var targetDirectionToCG = flock.CG - targetPoint;
            var directionCurrently = sheepChosenPosition - transform.position;
            //if the angle are closely align to the targetPoint then ignore this rule and scare the sheep.
            bool isAngleCorrect = Vector3.Angle(targetDirectionToCG, directionCurrently) < dog.AngleOfAvoidance;
            bool isNearTheSheep = Vector3.Distance(sheepChosenPosition, transform.position) > flock.EscapeRadius;

            if (isNearTheSheep || isAngleCorrect)
                return Vector3.zero;

            //else try to avoid scaring the sheep and the neighbour around it.
            Vector3 result = Vector3.zero;
            var sheepsToAvoid = Physics.OverlapSphere(sheepChosenPosition, flock.CohesionRadius, LayerManager.SheepLayer);
            foreach(var sheep in sheepsToAvoid)
            {
                var direction = (transform.position - sheep.transform.position);
                //will try to avoid the sheeps that are good.
                result += direction.normalized * InvSqrt(direction.magnitude * dog.CollectingAvoidanceSheepValue);
            }

            return result;
        }

        private void CheckReachTarget()
        {
            //if the CG is within the target point
            if (Vector3.Distance(flock.CG, dog.TargetPoint) < dog.TargetRadius)
            {
                mFsm.SetCurrentState((int)DogState.ListeningState);
            }
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