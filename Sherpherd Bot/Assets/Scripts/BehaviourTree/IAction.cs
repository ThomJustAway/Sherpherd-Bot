using Assets.Scripts.Shepherd.GOAP;
using Data_control;
using Sheep;
using System;
using System.Linq;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    public interface IAction
    {
        Status Action();
        public void Reset()
        {
            //noop
        }
    }

    public class MoveTo : IAction
    {
        private Func<bool> completionCondition;
        private Shepherd shepherd;
        Func<Vector3> location;

        public MoveTo(Func<bool> completionCondition, Shepherd shepherd, Func<Vector3> location)
        {
            this.completionCondition = completionCondition;
            this.shepherd = shepherd;
            this.location = location;
        }

        public Status Action()
        {
            if (completionCondition())
            {
                return Status.Success;
            }
            var direction = (location() - shepherd.transform.position).normalized;
            shepherd.Move(direction);
            return Status.Running;
        }
    }

    public class Search : IAction
    {
        private Func<bool> completionCondition;
        private Shepherd shepherd;
        private Vector3 nextDestination;

        public Search(Func<bool> completionCondition, Shepherd shepherd)
        {
            this.completionCondition = completionCondition;
            this.shepherd = shepherd;
        }

        public Status Action()
        {
            if (completionCondition()) return Status.Success;
            if(Vector3.Distance(shepherd.transform.position, nextDestination) < 0.1f)
            {
                FindNewPosition();
            }
            else
            {
                shepherd.Move((nextDestination - shepherd.transform.position).normalized);
            }
            return Status.Running;
        }

        private void FindNewPosition()
        {
            nextDestination = shepherd.transform.position +
                (UnityEngine.Random.insideUnitSphere * shepherd.WanderingRadius).With(y: 0);
        }
    }


    public class Condition : IAction
    {
        private Func<bool> completionCondition;
        private Status failResult;
        public Condition(Func<bool> completionCondition)
        {
            this.completionCondition = completionCondition;
            failResult = Status.Failed;
        }

        public Condition(Func<bool> completionCondition, Status failResult)
        {
            this.completionCondition = completionCondition;
            this.failResult = failResult;
        }

        public Status Action()
        {
            if(completionCondition()) { return Status.Success; }
            return failResult;
        }
    }

    public class CustomFunc : IAction
    {
        private Func<Status> customFunc;

        public CustomFunc(Func<Status> customFunc)
        {
            this.customFunc = customFunc;
        }

        public Status Action()
        {
            return customFunc();
        }
    }

    public class WaitFor : IAction
    {
        private float elapseTime;
        private float waitTime;

        public WaitFor(float waitTime)
        {
            this.elapseTime = 0;
            this.waitTime = waitTime;
        }

        public Status Action()
        {
            if(elapseTime >= waitTime)
            {
                elapseTime = 0;
                return Status.Success;
            }
            elapseTime += waitTime;
            return Status.Running;
        }
    }
    public class ShearingSheeps : IAction
    {
        private SheepFlock flock;
        private Shepherd shepherd;
        private SheepBehaviour chosenSheep;

        public ShearingSheeps(SheepFlock flock, Shepherd shepherd)
        {
            this.flock = flock;
            this.shepherd = shepherd;
        }

        public Status Action()
        {
            if(chosenSheep == null) { GetClosestSheep(); }
            if(Vector3.Distance(shepherd.transform.position, chosenSheep.transform.position) < shepherd.HandRadius)
            {
                chosenSheep.ShearWool();
                if(flock.flockWool == 0)
                {
                    chosenSheep = null;
                    return Status.Success;
                }
                GetClosestSheep() ;
            }
            var direction = (chosenSheep.transform.position - shepherd.transform.position).normalized;
            shepherd.Move(direction);

            return Status.Running;
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
            chosenSheep = sheeps[0];
            for (int i = 1; i < sheeps.Length; i++)
            {
                float dis = Vector3.Distance(shepherdPos, sheeps[i].transform.position);
                if (dis < closestDistance)
                {
                    chosenSheep = sheeps[i];
                    closestDistance = dis;
                    //filter them all out to get the closest sheep.
                }
            }
        }
    }

    public class CollectingWool : IAction
    {
        Transform closestWool;
        Shepherd shepherd;

        public CollectingWool(Shepherd shepherd)
        {
            this.shepherd = shepherd;
        }

        public Status Action()
        {
            if (!Physics.CheckSphere(shepherd.transform.position,
            shepherd.SenseRadius,
            LayerManager.WoolLayer)) return Status.Success;
            if(closestWool == null)
            {
                GetClosestWool();
            }
            if (Vector3.Distance(shepherd.transform.position, closestWool.position) < shepherd.HandRadius)
            {
                //if the shepherd can interact with the wool, then collect the wool.
                shepherd.CollectWool(closestWool);
                GetClosestWool();
            }

            var direction = (closestWool.transform.position - shepherd.transform.position).normalized;
            shepherd.Move(direction);

            return Status.Running;
        }

        private void GetClosestWool()
        {
            Vector3 shepherdPos = shepherd.transform.position;
            var Wools = Physics.OverlapSphere(shepherdPos, shepherd.SenseRadius, LayerManager.WoolLayer);
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