using GOAPTHOM;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    //public class NodeFactory
    //{
    //    private Dictionary<string, IEvaluator> nodes;
    //    private CompositeNode compositeNode;
    //    public NodeFactory()
    //    {
    //        nodes = new Dictionary<string, IEvaluator>();
    //    }

    //    public NodeFactory CreateSelectorNode(string name)
    //    {
    //        if(compositeNode != null)
    //        {
    //            throw new System.Exception("Node factory cant work on multiple instance of nodes");
    //        }
    //        compositeNode = new SelectorNode();
    //        compositeNode.Name = name;
    //        return this;
    //    }

    //    public NodeFactory CreateSequenceNode(string name)
    //    {
    //        if (compositeNode != null)
    //        {
    //            throw new System.Exception("Node factory cant work on multiple instance of nodes");
    //        }
    //        compositeNode = new SequenceNode();
    //        compositeNode.Name = name;
    //        return this;
    //    }

    //    public NodeFactory CreateSelectorNode(LeafName name)
    //    {
    //        if (compositeNode != null)
    //        {
    //            throw new System.Exception("Node factory cant work on multiple instance of nodes");
    //        }
    //        compositeNode = new SelectorNode();
    //        compositeNode.Name = name.ToString();
    //        return this;
    //    }

    //    public NodeFactory CreateSequenceNode(LeafName name)
    //    {
    //        if (compositeNode != null)
    //        {
    //            throw new System.Exception("Node factory cant work on multiple instance of nodes");
    //        }
    //        compositeNode = new SequenceNode();
    //        compositeNode.Name = name.ToString();
    //        return this;
    //    }

    //    public NodeFactory AddNodesComposite(IEvaluator node)
    //    {
    //        if (compositeNode == null) throw new System.Exception("Node factory cant work on without composite node");

    //        compositeNode.AddChild(node);
    //        return this;
    //    }
    //    public NodeFactory AddNodesComposite(List<IEvaluator> nodes)
    //    {
    //        if (compositeNode == null) throw new System.Exception("Node factory cant work on without composite node");

    //        compositeNode.AddChild(nodes);
    //        return this;
    //    }
    //    public void BuildCompositeNode()
    //    {
    //        if (compositeNode == null)
    //        {
    //            throw new System.Exception("Node factory cant build without a process node");
    //        }
    //        nodes.Add(compositeNode.Name, compositeNode);
    //        compositeNode = null; //reset the pointer
    //    }

    //    public Dictionary<string, IEvaluator> Create()
    //    {
    //        return nodes;   
    //    }
    //}

    public class SequenceNode : CompositeNode
    {
        public SequenceNode(string name) : base(name)
        {
        }

        public override Status Evaluate()
        {
            Debug.Log($"At {Name} node");
            //starts from left to right
            if (children == null || children?.Count == 0)
            {//check if can start the operation
                Debug.LogError($"Please create a children for {Name}");
                return Status.Failed;
            }

            for(; currentChild < children.Count; currentChild++)
            {
                Status selectedStatus = children[currentChild].Evaluate();
                if (selectedStatus == Status.Success)
                {
                    if(currentChild == children.Count - 1)
                    {
                        currentChild = 0;
                        return Status.Success;
                    }
                    continue;
                }
                else
                {
                    return selectedStatus;
                }
            }
            //if something wrong
            Debug.LogWarning($"Something wrong with node {Name}");
            return Status.Failed;
        }
    }

    public class SelectorNode : CompositeNode
    {
        public SelectorNode(string name) : base(name)
        {
        }

        public override Status Evaluate()
        {
            //start from left to right to decide which state is good
            if (children == null || children?.Count == 0)
            {//check if can start the operation
                Debug.LogError($"Please create a children for {Name}");
                return Status.Failed;
            }
            for(; currentChild < children.Count; currentChild++)
            {
                var state = children[currentChild].Evaluate();
                if (state == Status.Failed)
                {
                    if( currentChild == children.Count - 1) { return Status.Failed; }
                }
                else
                {
                    return state;
                }
            }

            Debug.LogWarning($"Something wrong with node {Name}");
            return Status.Failed;
        }

    }

    public abstract class CompositeNode : IEvaluator
    {
        protected int currentChild;
        protected List<IEvaluator> children;

        protected CompositeNode(string name)
        {
            Name = name;
            this.children = new();
        }

        public string Name { get ; set ; }

        public abstract Status Evaluate();

        public void AddChild(IEvaluator child)
        {
            children.Add(child);
        }

        public void AddChild(List<IEvaluator> childList)
        {
            children = childList;
        }

        public void AddName(string name)
        {
            Name = name;
        }
    }

    public interface IEvaluator
    {
        public string Name { get; set; }

        public Status Evaluate();
    }    
    
    public enum Status
    {
        Failed,
        Running,
        Success
    }
}