using Assets.Scripts.Shepherd.GOAP;
using System;
using UnityEngine;

namespace GOAPTHOM
{
    public class MoveStrategy : IActionStrategy
    {
        readonly Func<Vector3> destination;
        readonly Shepherd shepherd;
        public bool CanPerform => !Complete;
        public bool Complete => Vector3.Distance(destination(), shepherd.transform.position) < 1f;

        public MoveStrategy(Shepherd shepherd ,Func<Vector3> destination)
        {
            this.shepherd = shepherd;
            this.destination = destination;
        }

        public void Update(float deltaTime)
        {
            var direction = (destination() - shepherd.transform.position ).normalized;
            ///move the shepherd and rotate the shepherd
            shepherd.transform.position += direction * shepherd.MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            shepherd.transform.rotation = Quaternion.Slerp(shepherd.transform.rotation, targetRotation,
                Time.deltaTime * shepherd.RotationSpeed);
        }
    }
}