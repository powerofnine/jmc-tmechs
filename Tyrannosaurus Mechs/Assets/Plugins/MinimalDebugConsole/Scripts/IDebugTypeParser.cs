namespace fuj1n.MinimalDebugConsole
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public interface IDebugTypeParser
    {
        [Pure]
        Type GetTarget();

        void AppendArguments(List<Type> arguments);
        object GetValue(Queue<object> arguments);
    }
}