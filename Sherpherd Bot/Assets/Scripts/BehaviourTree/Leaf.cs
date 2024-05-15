using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeImplementation
{

    public class Leaf : IEvaluator
    {
        public string Name { get; set; }
        public IAction actions;

        public Leaf(string name, IAction actions)
        {
            this.actions = actions;
            Name = name;
        }


        public Status Evaluate()
        {
            Debug.Log($"At leaf {Name}");

            return actions.Action();
        }
    }

}