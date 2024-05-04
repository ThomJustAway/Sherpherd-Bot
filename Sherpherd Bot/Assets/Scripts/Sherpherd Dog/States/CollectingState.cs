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
    public class CollectingState : SherpherDogState
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
            {//if all teh sheep are within cohesion radius from the point of CG. the dog should be able to push it.
                mFsm.SetCurrentState((int)DogState.Driving);
                return;
            }
            SearchClosestSheep();
        }

        private void SearchClosestSheep()
        {
            var targetSheeps = flock.Sheeps
                .Where(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > dog.GatheringRadius)
                .ToArray();
            //get all the sheeps that are out of place
            SheepBehaviour sheepChosen = targetSheeps[0];
            Vector3 dogPosition = dog.transform.position;
            float closestDistance = Vector3.Distance(sheepChosen.transform.position, dogPosition);

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
            //problem: the sheep would also move in too close to the target point,
            //as a result, it would move back. this cause it to move back and forth until the 
            //chosen sheep starts to move
            if (Vector3.Distance(sheepChosenPosition , flock.CG) < flock.CohesionRadius)
            {
                //if under cohesion radius, check if there are any other sheep that needs to be collected
                CheckCanDrive();
            }
            //Find the position that allows the chosen sheep to be push to the flock.
            targetPoint = (((sheepChosenPosition - flock.CG).normalized * dog.CollectionOffset) + sheepChosenPosition)
                .With(y: flock.YOffset);

            
            
            Vector3 targetMovement = (targetPoint - dog.transform.position).normalized;

            //Vector3 resultantMovement = targetMovement * dog.WeightOfTarget +
            //    //AvoidOtherSheepsRule() * dog.WeightOfAvoidanceOfOtherSheep +
            //    AvoidScaringSheepRule() * dog.WeightOfAvoidanceFromChosenSheep;
            //add rules to avoid walls, other sheeps and not scare the other sheep

            //for debugging
            targetMovement *= dog.WeightOfTarget;
            var avoidScaringSheepDirection = AvoidScaringSheepRule() * dog.WeightOfAvoidanceFromChosenSheep;
            //var avoidOtherSheepDirection = AvoidOtherSheepsRule() * dog.WeightOfAvoidanceOfOtherSheep;
            Vector3 resultantMovement = targetMovement +
                avoidScaringSheepDirection;/* +*/
                //avoidOtherSheepDirection;


            Debug.DrawRay(transform.position, targetMovement, Color.yellow);
            Debug.DrawRay(transform.position, avoidScaringSheepDirection, Color.red);
            Debug.DrawRay(transform.position, avoidOtherSheepDirection, Color.black);
            Debug.DrawLine(transform.position, targetPoint, Color.white);
            //Vector3 resultantMovement = targetMovement * dog.WeightOfTarget +
            //    //AvoidOtherSheepsRule() * dog.WeightOfAvoidanceOfOtherSheep +
            //    AvoidScaringSheepRule() * dog.WeightOfAvoidanceFromChosenSheep;


            transform.position +=  resultantMovement.normalized * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetMovement);
        }

        //private Vector3 AvoidOtherSheepsRule()
        //{
        //    Vector3 result = Vector3.zero;
        //    var sheeps = Physics.OverlapSphere(transform.position, dog.SenseRadius, LayerManager.SheepLayer)
        //        .Where(sheep => Vector3.Distance(sheep.transform.position, sheepChosen.transform.position) > flock.CohesionRadius)
        //        .ToArray();
        //    //find all the sheeps in the area and omit those are near the sheep chosen .
            
        //    if(sheeps.Length == 0) return result;

        //    foreach(var sheep in sheeps) 
        //    {
        //        var direction = (transform.position - sheep.transform.position);
        //        //will try to avoid the sheeps that are good.
        //        result += direction.normalized * InvSqrt(direction.magnitude * dog.CollectingAvoidanceSheepValue);
        //    }

        //    return result;
        //}

        private Vector3 AvoidScaringSheepRule()
        {
            //will need to check the angle
            var targetDirectionToCG = flock.CG - targetPoint;
            var directionCurrently = sheepChosenPosition - transform.position;
            //if the angle are closely align then ignore this rule and scare the sheep.
            bool isAngleCorrect = Vector3.Angle(targetDirectionToCG, directionCurrently) < dog.AngleOfAvoidance;
            bool isNearTheSheep = Vector3.Distance(sheepChosenPosition, transform.position) > flock.EscapeRadius;

            Debug.Log($"Is angle good? {isAngleCorrect}");

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

    //todo add driving to the dog
    public class DrivingState : SherpherDogState
    {
        //Gathering is when the dog would push all the 
        //herd to the specified location
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

        void CheckCanCollect()
        {
            //if there are any misalign sheep, make sure to go back to collecting to realign them back
            if (flock.Sheeps.Any(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > dog.GatheringRadius))
            {
                mFsm.SetCurrentState((int)DogState.Collecting);
                return;
            }
        }

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
            //find target point to push the flock to target position
            //problem here
            var targetPoint = ((flock.CG - dog.TargetPoint).normalized * dog.DrivingOffset + flock.CG)
                .With(y: flock.YOffset);
            Debug.DrawLine(transform.position, targetPoint, Color.blue);
            var targetDirection = (targetPoint - dog.transform.position).normalized;
            //get the target point to arrive
            transform.position += targetDirection * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

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
                //will try to avoid the sheeps that are good.
                result += direction.normalized * InvSqrt(direction.magnitude * dog.CollectingAvoidanceSheepValue);
            }

            return result;
        }

    }

    public class ListeningState : SherpherDogState
    {
        public ListeningState(FSM fsm, ShepherdDog dog) : base(fsm, dog)
        {
            mId = (int)DogState.ListeningState;
        }

        //Todo add update behaviour here!
    }
}