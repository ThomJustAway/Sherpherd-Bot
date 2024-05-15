using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace BehaviourTreeImplementation
{
    //store the node and leafs
    public class BehaviourTree
    {
        //the starting node should be the 
        List<IEvaluator> mainBranch;
        //can switch the process of the behaviour tree.
        private SelectionProcess process;
        public delegate void SelectionProcessDelegate();
        public SelectionProcessDelegate Update;
        public enum SelectionProcess
        {
            Sequence,
            Selector
        }

        public BehaviourTree()
        {
            this.mainBranch = new List<IEvaluator>();
        }

        public void AddBranch(IEvaluator branch)
        {
            mainBranch.Add(branch);
        }

        public void AddBranch(List<IEvaluator> branch)
        {
            mainBranch = branch;
        }

        public void SetUpSelection(SelectionProcess process = SelectionProcess.Selector)
        {
            this.process = process;
            if(process == SelectionProcess.Sequence)
            {
                Update = Sequence;
            }
            else
            {
                Update = Selection;
            }
        }
       

        private void Sequence()
        {
            for (int i = 0; i < mainBranch.Count; i++)
            {
                Debug.Log("behaviour tree -->");
                var status = mainBranch[i].Evaluate();
                if (status == Status.Success)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
        }

        private void Selection()
        {
            for (int i = 0; i < mainBranch.Count; i++)
            {
                var status = mainBranch[i].Evaluate();
                if (status == Status.Failed)
                {
                    continue;
                }
                else
                {
                    return;
                }
            }
        }
    }
}