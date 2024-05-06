using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAPTHOM
{
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

            public Builder(Goal goalEnum)
            {
                string name = goalEnum.ToString();
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

            public Builder WithDesiredEffect(Beliefs belief, Dictionary<string, AgentBelief> dict)
            {
                string name = belief.ToString();
                goal.DesiredEffects.Add(dict[name]);
                return this;
            }

            public AgentGoal Build()
            {
                return goal;
            }
        }
    }
}