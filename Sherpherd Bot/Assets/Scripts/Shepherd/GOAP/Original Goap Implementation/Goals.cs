using System.Collections.Generic;

namespace OriginalGOAP
{

    //the goal are build upon belief which needs to be satisfied.
    public class AgentGoal
    {
        public string Name { get; }
        public float Priority { get; private set; }
        //what effect is needed to accomplish its goal
        public HashSet<AgentBelief> DesiredEffects { get; } = new();

        AgentGoal(string name)
        {
            Name = name;
        }

        public class Builder
        {
            readonly AgentGoal goal;

            public Builder(string name)
            {
                goal = new AgentGoal(name);
            }

            public Builder WithPriority(float priority)
            {
                goal.Priority = priority;
                return this;
            }

            public Builder WithDesiredEffect(AgentBelief effect)
            {
                goal.DesiredEffects.Add(effect);
                return this;
            }

            public AgentGoal Build()
            {
                return goal;
            }
        }
    }
}