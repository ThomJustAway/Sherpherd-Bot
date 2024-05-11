using System;

namespace GOAPTHOM
{
    /// <summary>
    /// The wait strategy is the shepherd waiting patiently until the 
    /// a condition is completed. used for waiting for the sheep to 
    /// eat and drink finish.
    /// </summary>
    public class WaitTillStrategy : IActionStrategy
    {
        private readonly Func<bool> eval;

        public WaitTillStrategy(Func<bool> eval)
        {
            this.eval = eval;
        }

        public bool CanPerform => !Complete;

        public bool Complete => eval();

        
    }
}