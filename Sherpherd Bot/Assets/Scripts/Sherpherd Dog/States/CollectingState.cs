using PGGE.Patterns;
using Sheep;
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
            if(flock.Sheeps.All(sheep => Vector3.Distance(sheep.transform.position, flock.CG) <= flock.CohesionRadius))
            {//if all teh sheep are within cohesion radius from the point of CG. the dog should be able to push it.
                mFsm.SetCurrentState((int)DogState.Driving);
                return;
            }
            SearchClosestSheep();
        }

        private void SearchClosestSheep()
        {
            var targetSheeps = flock.Sheeps
                .Where(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > flock.CohesionRadius)
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
            if (Vector3.Distance(sheepChosenPosition , flock.CG) < flock.CohesionRadius)
            {
                //if under cohesion radius, check if there are any other sheep that needs to be collected
                CheckCanDrive();
            }
            //Find the position that allows the chosen sheep to be push to the flock.
            var targetPoint = (((sheepChosenPosition - flock.CG).normalized * dog.DrivingOffset) + sheepChosenPosition)
                .With(y: flock.YOffset);

            //move the dog to the position
            var targetDirection = (targetPoint - dog.transform.position).normalized;
            //get the target point to arrive
            transform.position += targetDirection * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }
    }
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
            if (flock.Sheeps.Any(sheep => Vector3.Distance(sheep.transform.position, flock.CG) > flock.CohesionRadius))
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
            var targetPoint = ((flock.CG - transform.position).normalized * dog.DrivingOffset + transform.position)
                .With(y: flock.YOffset);
            Debug.DrawLine(transform.position, targetPoint, Color.blue);
            var targetDirection = (targetPoint - dog.transform.position).normalized;
            //get the target point to arrive
            transform.position += targetDirection * dog.MaxSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(targetDirection);
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