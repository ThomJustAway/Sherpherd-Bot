﻿using System.Collections;

namespace GOAPTHOM
{
    public interface IActionStrategy
    {
        bool CanPerform { get; }
        bool Complete { get; }

        void Start()
        {
            // noop
        }

        void Update(float deltaTime)
        {
            // noop
        }

        void Stop()
        {
            // noop
        }
    }
}