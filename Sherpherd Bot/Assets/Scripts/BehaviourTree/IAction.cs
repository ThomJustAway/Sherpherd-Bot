using Assets.Scripts.Player;
using Assets.Scripts.Shepherd.GOAP;
using Data_control;
using Sheep;
using System;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    //what kind of actions the leaf can take.
    public interface IAction
    {
        Status Action();
        public void Reset()
        {
            //noop
        }
    }
    /// <summary>
    /// Simple class that allows the shepherd to move to a 
    /// certain location. When execture the leaf node. it will
    /// try to move to a destine location until it reaches it.
    /// </summary>
    public class MoveTo : IAction
    {
        //what kind of condition to check in order to stop running this leaf node.
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
            //wil move the shepherd to destined location.
            var direction = (location() - shepherd.transform.position).normalized;
            shepherd.Move(direction);
            return Status.Running;
        }
    }

    /// <summary>
    /// This function just allow the shepherd to wonder around
    /// until it complete a certain condition.
    /// </summary>
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
            //if it complete the completeion condition it will return success
            if (completionCondition()) return Status.Success;
            if(Vector3.Distance(shepherd.transform.position, nextDestination) < 0.1f)
            {//if it is close to the destine location, it will plan for the new position
                FindNewPosition();
            }
            else
            {
                //else it will continue to move to the location plan
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
    /// <summary>
    /// This make the leaf node check for condition
    /// will return success if it manage to condition satify
    /// else will return a fail result (could be running or failed).
    /// </summary>
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
    /// <summary>
    /// custom function if the feature
    /// is quite simple and doesn't need a class.
    /// </summary>
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
    /// <summary>
    /// A simple leaf node that just return success until a certain
    /// period of time has pass.
    /// </summary>
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
    /// <summary>
    /// A special action to allow the shepherd shear all the sheeps
    /// </summary>
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
            //will find the closest sheep
            if(chosenSheep == null) { GetClosestSheep(); }
            if(Vector3.Distance(shepherd.transform.position, chosenSheep.transform.position) < shepherd.HandRadius)
            {
                //if the close to the sheep, then it would shear it.
                chosenSheep.ShearWool();
                if(flock.flockWool == 0)
                {
                    chosenSheep = null;
                    return Status.Success;
                }
                GetClosestSheep() ;
            }
            //move to sheep.
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
            //get all the sheeps that still have wool
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
    /// <summary>
    /// A special action to allow the shepherd to collect all the wool
    /// </summary>
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
            //will move to the wool location
            var direction = (closestWool.transform.position - shepherd.transform.position).normalized;
            shepherd.Move(direction);

            return Status.Running;
        }

        private void GetClosestWool()
        {
            //this function allow the shepherd to get the closest wool.
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

    /// <summary>
    /// A sepcial action for the shepherd when interacting with the player
    /// </summary>
    public class WaitingForPlayer : IAction
    {
        private CompositeNode parentNode;
        private PlayerInteractions player;
        private ShepherdUI ui;
        private Shepherd shepherd;

        public WaitingForPlayer(PlayerInteractions player, ShepherdUI ui, Shepherd shepherd , CompositeNode parentNode)
        {
            this.player = player;
            this.ui = ui;
            this.shepherd = shepherd;
            this.parentNode = parentNode;
        }

        public Status Action()
        {

            if(Vector3.Distance(shepherd.transform.position, player.transform.position) > shepherd.InteractionRange)
            {
                //if the player leave the shepherd, then it cant run this node anymore
                ui.SetTextBox();
                parentNode.Reset();
                return Status.Failed;
            }
            //else just be running running until the player does something

            if (shepherd.HaveRecieveFood)
            {
                //set the trigger back to original state
                shepherd.HaveRecieveFood = false;
                Debug.Log("have recieve food");
                //will show the message for 5 second before closing.
                ui.SetTemporaryMessage(5f, "Wow! Thank you for the food!");
            }

            return Status.Running;
        }
    }
}