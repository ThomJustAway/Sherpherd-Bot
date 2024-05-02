using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GOAPTHOM
{
    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
    }

    public class GoapPlanner : IGoapPlanner
    {
        public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
        {
            // Order goals by priority, descending
            List<AgentGoal> orderedGoals = goals
                .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
                .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)
                .ToList();
            //sort the goals that has the most priority.

            // Try to solve each goal in order
            foreach (var goal in orderedGoals)
            {
                Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

                // If we can find a path to the goal, return the plan
                if (FindPath(goalNode, agent.actions))
                {
                    // If the goalNode has no leaves and no action to perform try a different goal
                    if (goalNode.IsLeafDead) continue;

                    Stack<AgentAction> actionStack = new Stack<AgentAction>();
                    while (goalNode.Leaves.Count > 0)
                    {
                        var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                        goalNode = cheapestLeaf;
                        //go through the leaf and add them into the goal node
                        actionStack.Push(cheapestLeaf.Action);
                    }

                    return new ActionPlan(goal, actionStack, goalNode.Cost);
                }
            }

            Debug.LogWarning("No plan found");
            return null;
        }

        // TODO: Consider a more powerful search algorithm like A* or D*
        bool FindPath(Node parent, HashSet<AgentAction> actions)
        {
            // Order actions by cost, ascending this is so that the 
            var orderedActions = actions.OrderBy(a => a.Cost);
            //find the action that is of the easiest to execute.

            foreach (var action in orderedActions)
            {
                //see what conditions needs to be satisfied for action to take place
                var requiredEffects = parent.RequiredEffects;

                // Remove any effects that evaluate to true, there is no action to take
                requiredEffects.RemoveWhere(b => b.Evaluate());

                // If there are no required effects to fulfill, we have a plan
                if (requiredEffects.Count == 0)
                {
                    return true;
                }

                //if the action effect can satisfy the required effects
                if (action.Effects.Any(requiredEffects.Contains))
                {
                    //create a clone of the required effect and make sure to add conditions is needed to be satisfied
                    var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                    newRequiredEffects.ExceptWith(action.Effects);
                    newRequiredEffects.UnionWith(action.Preconditions);
                    //make sure to change the require effect so as to search it down the tree.

                    var newAvailableActions = new HashSet<AgentAction>(actions);
                    newAvailableActions.Remove(action);
                    //renew the new actions that will be used for the next search
                    var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                    // Explore the new node recursively
                    if (FindPath(newNode, newAvailableActions))
                    {
                        parent.Leaves.Add(newNode);
                        //have a record of the effect that were left for the node.
                        newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                    }

                    // If all effects at this depth have been satisfied, return true
                    if (newRequiredEffects.Count == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class Node
    {
        public Node Parent { get; }
        public AgentAction Action { get; }
        public HashSet<AgentBelief> RequiredEffects { get; }
        public List<Node> Leaves { get; }
        public float Cost { get; }

        public bool IsLeafDead => Leaves.Count == 0 && Action == null;

        public Node(Node parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
        {
            Parent = parent;
            Action = action;
            //creates a new hashset for safety
            RequiredEffects = new HashSet<AgentBelief>(effects);
            Leaves = new List<Node>();
            Cost = cost;
        }
    }

    public class ActionPlan
    {
        public AgentGoal AgentGoal { get; }
        public Stack<AgentAction> Actions { get; }
        public float TotalCost { get; set; }

        public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost)
        {
            AgentGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }
}