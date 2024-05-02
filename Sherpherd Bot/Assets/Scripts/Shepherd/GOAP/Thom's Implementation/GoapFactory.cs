using DependencyInjection;
using OriginalGOAP;
using System.Collections;
using UnityEngine;
using UnityServiceLocator;

namespace GOAPTHOM
{
    public class GoapFactory : MonoBehaviour , IDependencyProvider
    {
        void Awake()
        {
            ServiceLocator.Global.Register(this);
        }

        [Provide] public GoapFactory ProvideFactory() => this;

        public IGoapPlanner CreatePlanner()
        {
            return new GoapPlanner();
        }
    }
}