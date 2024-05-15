using DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace GOAPTHOM
{
    //the GOAP implementation was inspired by Git-amend. U can take a look at his implementation
    //https://www.youtube.com/watch?v=T_sBYgP7_2k&pp=ygUXZ29hcCBpbXBsZW1lbnRhdGlvbiBnaXQ%3D
    public class GoapAgent 
    {
        AgentGoal lastGoal; 
        public AgentGoal currentGoal;
        public ActionPlan actionPlan;
        public AgentAction currentAction;

        public Dictionary<string, AgentBelief> beliefs;
        public HashSet<AgentAction> actions;
        public HashSet<AgentGoal> goals;

        [Inject] GoapFactory gFactory;
        IGoapPlanner gPlanner;

        //introduce a seam to change the update function

        public delegate void UpdateFunction();

        public UpdateFunction updateFunction;
        //call the update function in any class
        //able to implement your own update function for extra customizability

        public void CreatePlanner()
        {
            gPlanner = new GoapPlanner();
        }

        public void SetUpdate(UpdateFunction update = null)
        {
            if(update != null)
            {
                updateFunction = update;
            }
            else
            {
                updateFunction = NormalUpdate;
            }
        }

        //void Start()
        //{
        //    SetupTimers();
        //    SetupBeliefs();
        //    SetupActions();
        //    SetupGoals();
        //}

        public void SetupBeliefs(Dictionary<string,AgentBelief> beliefs)
        {
            this.beliefs = beliefs;
            //BeliefFactory factory = new BeliefFactory(this, beliefs);

            //factory.AddBelief("Nothing", () => false);

            //factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
            //factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
            //factory.AddBelief("AgentHealthLow", () => health < 30);
            //factory.AddBelief("AgentIsHealthy", () => health >= 50);
            //factory.AddBelief("AgentStaminaLow", () => stamina < 10);
            //factory.AddBelief("AgentIsRested", () => stamina >= 50);

            //factory.AddLocationBelief("AgentAtDoorOne", 3f, doorOnePosition);
            //factory.AddLocationBelief("AgentAtDoorTwo", 3f, doorTwoPosition);
            //factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);
            //factory.AddLocationBelief("AgentAtFoodShack", 3f, foodShack);

            //factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
            //factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
            //factory.AddBelief("AttackingPlayer", () => false); // Player can always be attacked, this will never become true
        }

        public void SetupActions(HashSet<AgentAction> actions)
        {
            this.actions = actions;
            //actions = new HashSet<AgentAction>();

            //actions.Add(new AgentAction.Builder("Relax")
            //    .WithStrategy(new IdleStrategy(5))
            //    .AddEffect(beliefs["Nothing"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("Wander Around")
            //    .WithStrategy(new SearchStrategy(navMeshAgent, 10))
            //    .AddEffect(beliefs["AgentMoving"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("MoveToEatingPosition")
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => foodShack.position))
            //    .AddEffect(beliefs["AgentAtFoodShack"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("Eat")
            //    .WithStrategy(new IdleStrategy(5))  // Later replace with a Command
            //    .AddPrecondition(beliefs["AgentAtFoodShack"])
            //    .AddEffect(beliefs["AgentIsHealthy"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("MoveToDoorOne")
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => doorOnePosition.position))
            //    .AddEffect(beliefs["AgentAtDoorOne"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("MoveToDoorTwo")
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => doorTwoPosition.position))
            //    .AddEffect(beliefs["AgentAtDoorTwo"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("MoveFromDoorOneToRestArea")
            //    .WithCost(2)
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            //    .AddPrecondition(beliefs["AgentAtDoorOne"])
            //    .AddEffect(beliefs["AgentAtRestingPosition"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("MoveFromDoorTwoRestArea")
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            //    .AddPrecondition(beliefs["AgentAtDoorTwo"])
            //    .AddEffect(beliefs["AgentAtRestingPosition"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("Rest")
            //    .WithStrategy(new IdleStrategy(5))
            //    .AddPrecondition(beliefs["AgentAtRestingPosition"])
            //    .AddEffect(beliefs["AgentIsRested"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("ChasePlayer")
            //    .WithStrategy(new MoveStrategy(navMeshAgent, () => beliefs["PlayerInChaseRange"].Location))
            //    .AddPrecondition(beliefs["PlayerInChaseRange"])
            //    .AddEffect(beliefs["PlayerInAttackRange"])
            //    .BuildCompositeNode());

            //actions.Add(new AgentAction.Builder("AttackPlayer")
            //    .WithStrategy(new AttackStrategy(animations))
            //    .AddPrecondition(beliefs["PlayerInAttackRange"])
            //    .AddEffect(beliefs["AttackingPlayer"])
            //    .BuildCompositeNode());
        }

        public void SetupGoals(HashSet<AgentGoal> goals)
        {
            this.goals = goals;
            //goals = new HashSet<AgentGoal>();

            //goals.Add(new AgentGoal.Builder("Chill Out")
            //    .WithPriority(1)
            //    .WithDesiredEffect(beliefs["Nothing"])
            //    .BuildCompositeNode());

            //goals.Add(new AgentGoal.Builder("Wander")
            //    .WithPriority(1)
            //    .WithDesiredEffect(beliefs["AgentMoving"])
            //    .BuildCompositeNode());

            //goals.Add(new AgentGoal.Builder("KeepHealthUp")
            //    .WithPriority(2)
            //    .WithDesiredEffect(beliefs["AgentIsHealthy"])
            //    .BuildCompositeNode());

            //goals.Add(new AgentGoal.Builder("KeepStaminaUp")
            //    .WithPriority(2)
            //    .WithDesiredEffect(beliefs["AgentIsRested"])
            //    .BuildCompositeNode());

            //goals.Add(new AgentGoal.Builder("SeekAndDestroy")
            //    .WithPriority(3)
            //    .WithDesiredEffect(beliefs["AttackingPlayer"])
            //    .BuildCompositeNode());
        }

        #region normal update
        void NormalUpdate()
        {
            // NormalUpdate the plan and current Action if there is one
            if (currentAction == null)
            {
                Debug.Log("Calculating any potential new plan");
                CalculatePlan();

                if (actionPlan != null && actionPlan.Actions.Count > 0)
                {

                    currentGoal = actionPlan.AgentGoal;
                    Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                    currentAction = actionPlan.Actions.Pop();
                    Debug.Log($"Popped action: {currentAction.Name}");
                    // Verify all precondition effects are true

                    if (currentAction.Preconditions.All(b => b.Evaluate()))
                    {
                        currentAction.Start();
                    }
                    else
                    {
                        Debug.Log("Preconditions not met, clearing current action and goal");
                        currentAction = null;
                        currentGoal = null;
                    }
                }
            }

            // If we have a current Action, execute it
            if (actionPlan != null && currentAction != null)
            {
                currentAction.Update(Time.deltaTime);

                if (currentAction.Complete)
                {
                    Debug.Log($"{currentAction.Name} complete");
                    currentAction.Stop();
                    currentAction = null;

                    if (actionPlan.Actions.Count == 0)
                    {
                        Debug.Log("Plan complete");
                        lastGoal = currentGoal;
                        currentGoal = null;
                    }
                }
            }
        }

        void CalculatePlan()
        {
            var priorityLevel = currentGoal?.Priority ?? 0;

            HashSet<AgentGoal> goalsToCheck = goals;

            // If we have a current goal, we only want to check goals with higher priority
            if (currentGoal != null)
            {
                Debug.Log("Current goal exists, checking goals with higher priority");
                goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
            }

            var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
            if (potentialPlan != null)
            {
                actionPlan = potentialPlan;
            }
        }
        #endregion

    }
}