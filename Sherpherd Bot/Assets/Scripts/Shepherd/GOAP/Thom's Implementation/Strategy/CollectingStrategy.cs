using Assets.Scripts.Shepherd.GOAP;
using Data_control;
using Sheep;
using UnityEngine;

namespace GOAPTHOM
{
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

        public void Start()
        {
            GetClosestWool();
        }

        public void Update(float deltaTime)
        {
            Debug.Log("Collecting state");
            
            var direction = (closestWool.transform.position - shepherd.transform.position).normalized;
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);

            if (Vector3.Distance(shepherd.transform.position, closestWool.position) < shepherd.HandRadius)
            {
                shepherd.CollectWool(closestWool);
                GetClosestWool();
            }
        }

        private void GetClosestWool()
        {
            Vector3 shepherdPos = shepherd.transform.position;
            var Wools = Physics.OverlapSphere(shepherdPos, shepherd.SenseRadius , LayerManager.WoolLayer);

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
                }
            }
        }
    }

    public class SellingStrategy : IActionStrategy
    {
        private Shepherd shepherd;
        private Barn barn;

        public SellingStrategy(Shepherd shepherd, Barn barn)
        {
            this.shepherd = shepherd;
            this.barn = barn;
        }

        public bool CanPerform => shepherd.WoolAmount > 0;
        public bool Complete => shepherd.WoolAmount == 0;

        public void Update(float deltaTime)
        {
            Vector2 shepherdPos = new Vector2(shepherd.transform.position.x, shepherd.transform.position.z);
            Vector2 barnPos = new Vector2(barn.transform.position.x, barn.transform.position.z);
            if (Vector2.Distance(shepherdPos, barnPos) < shepherd.HandRadius)
            {
                barn.SellFur(shepherd);
            }
            var direction = (barn.transform.position - shepherd.transform.position).normalized.With(y:0);
            
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);
        }
    }

}