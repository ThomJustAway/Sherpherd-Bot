using Assets.Scripts.Shepherd.GOAP;
using Data_control;
using Sheep;
using UnityEngine;

namespace GOAPTHOM
{
    /// <summary>
    /// For collecting wool
    /// </summary>
    public class CollectingStrategy : IActionStrategy
    {
        readonly Shepherd shepherd;
        Transform closestWool;

        public CollectingStrategy(Shepherd shepherd)
        {
            this.shepherd = shepherd;
        }

        public bool CanPerform => !Complete;
        //make sure that all the Wools are sheered
        public bool Complete => !Physics.CheckSphere(shepherd.transform.position, 
            shepherd.SenseRadius, 
            LayerManager.WoolLayer);
        //the strategy is completed if it sense that there is no more wool around the shepherd.

        public void Start()
        {
            GetClosestWool();
        }

        public void Update(float deltaTime)
        {
            //try to go close to the shepherd.
            var direction = (closestWool.transform.position - shepherd.transform.position).normalized;
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);

            if (Vector3.Distance(shepherd.transform.position, closestWool.position) < shepherd.HandRadius)
            {//if the shepherd can interact with the wool, then collect the wool.
                shepherd.CollectWool(closestWool);
                GetClosestWool();
            }
        }
        /// <summary>
        /// get the closest wool avaliable for the shepherd.
        /// </summary>
        private void GetClosestWool()
        {
            Vector3 shepherdPos = shepherd.transform.position;
            var Wools = Physics.OverlapSphere(shepherdPos, shepherd.SenseRadius , LayerManager.WoolLayer);
            //do ray casting to find all the wool nearby
            if (Wools.Length == 0) return;

            var closestDistance = Vector3.Distance(shepherdPos, Wools[0].transform.position);
            closestWool = Wools[0].transform;
            for (int i = 1; i < Wools.Length; i++)
            {
                float dis = Vector3.Distance(shepherdPos, Wools[i].transform.position);
                if (dis < closestDistance)
                {
                    closestWool = Wools[i].transform;
                    closestDistance = dis;
                    //find the closest wool from the shepherd.
                }
            }
        }
    }

}