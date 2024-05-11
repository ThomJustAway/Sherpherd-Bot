using Assets.Scripts.Shepherd.GOAP;
using Sheep;
using System.Linq;
using UnityEngine;

namespace GOAPTHOM
{
    /// <summary>
    /// shearing strategy is when the shepherd would shear nearby 
    /// sheeps until all the sheeps are sheared off completely.
    /// </summary>
    public class ShearingStrategy : IActionStrategy
    {
        readonly SheepFlock flock;
        readonly Shepherd shepherd;
        SheepBehaviour chosenSheepToShear;

        public ShearingStrategy(SheepFlock flock, Shepherd shepherd)
        {
            this.flock = flock;
            this.shepherd = shepherd;
        }

        public bool CanPerform => !Complete;
        //make sure that all the Wools are sheared
        public bool Complete => flock.Sheeps.All(sheep => sheep.Wool == 0);
        //the strategy is complete once all the sheep are completely sheared off.
        public void Start()
        {
            //get a near sheep.
            GetClosestSheep();
        }

        public void Update(float deltaTime)
        {
            Debug.Log("sheering state");
            if(Vector3.Distance(shepherd.transform.position, chosenSheepToShear.transform.position) < shepherd.HandRadius)
            {
                //if the shepherd can interact with the sheep, shear the sheep and re-evaluate the next clostest sheep.
                chosenSheepToShear.ShearWool();
                GetClosestSheep();
            }
            var direction = (chosenSheepToShear.transform.position - shepherd.transform.position ).normalized;
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);
        }

        private void GetClosestSheep()
        {
            var sheeps = flock.Sheeps
                .Where(sheep => sheep.Wool > 0)
                .ToArray();

            if (sheeps.Length == 0) return;
            //get all teh sheeps that still have wool
            Vector3 shepherdPos = shepherd.transform.position;
            var closestDistance = Vector3.Distance(shepherdPos, sheeps[0].transform.position);
            chosenSheepToShear = sheeps[0];
            for( int i = 1; i < sheeps.Length; i++ )
            {
                float dis = Vector3.Distance(shepherdPos, sheeps[i].transform.position);
                if (dis < closestDistance)
                {
                    chosenSheepToShear = sheeps[i];
                    closestDistance = dis;
                    //filter them all out to get the closest sheep.
                }
            }
        }
    }

}