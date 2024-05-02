using System;
using System.Collections.Generic;
using UnityEngine;

//this factory is to generate a lot of belief easily.
namespace OriginalGOAP
{
    public class BeliefFactory {
    readonly GoapAgent agent;
    //contains the dictionary reference of the agent belief tied to a string key.
    readonly Dictionary<string, AgentBelief> beliefs;
    
    public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs) {
        this.agent = agent;
        this.beliefs = beliefs; 
    }
    
    //add belief with a condition
    public void AddBelief(string key, Func<bool> condition) {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(condition)
            .Build());
    }
    
    public void AddSensorBelief(string key, Sensor sensor) {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(() => sensor.IsTargetInRange)
            .WithLocation(() => sensor.TargetPosition)
            .Build());
    }
    
    public void AddLocationBelief(string key, float distance, Transform locationCondition) {
        AddLocationBelief(key, distance, locationCondition.position);
    }
    
    public void AddLocationBelief(string key, float distance, Vector3 locationCondition) {
        beliefs.Add(key, new AgentBelief.Builder(key)
            .WithCondition(() => InRangeOf(locationCondition, distance))
            .WithLocation(() => locationCondition)
            .Build());
    }
    
    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;
}

public class AgentBelief {
    public string Name { get; }
    
    Func<bool> condition = () => false;
    Func<Vector3> observedLocation = () => Vector3.zero;
    
    //this is to evaluate the location and the condition of the belief.
    public Vector3 Location => observedLocation();
    public bool Evaluate() => condition();
    
    AgentBelief(string name) {
        Name = name;
    }
    

    public class Builder {
        readonly AgentBelief belief;
        
        public Builder(string name) {
            belief = new AgentBelief(name);
        }
        //give the condition to satisfy the belief
        public Builder WithCondition(Func<bool> condition) {
            belief.condition = condition;
            return this;
        }
        //give a observed location to the 
        public Builder WithLocation(Func<Vector3> observedLocation) {
            belief.observedLocation = observedLocation;
            return this;
        }
        
        public AgentBelief Build() {
            return belief;
        }
    }
}

}