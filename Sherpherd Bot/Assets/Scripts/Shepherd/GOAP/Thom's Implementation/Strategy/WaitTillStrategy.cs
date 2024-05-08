using System;

namespace GOAPTHOM
{
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