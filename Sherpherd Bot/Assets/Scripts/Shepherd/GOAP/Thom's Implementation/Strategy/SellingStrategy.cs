using Assets.Scripts.Shepherd.GOAP;
using UnityEngine;

namespace GOAPTHOM
{
    /// <summary>
    /// Strategy to sell wool once the shepherd have collect wool from the sheeps.
    /// </summary>
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
        //the Action is complete once the shepherd has no more wool

        public void Update(float deltaTime)
        {
            Vector2 shepherdPos = new Vector2(shepherd.transform.position.x, shepherd.transform.position.z);
            Vector2 barnPos = new Vector2(barn.transform.position.x, barn.transform.position.z);
            if (Vector2.Distance(shepherdPos, barnPos) < shepherd.HandRadius)
            {
                barn.SellFur(shepherd);
            }//if the shepherd is near to the barn. then sell the wool.

            //else move the shepherd.
            var direction = (barn.transform.position - shepherd.transform.position).normalized.With(y:0);
            
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);
        }
    }

}