using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DebugConsole
{
    public interface IDebugTypeParser
    {
        [Pure]
        Type GetTarget();
        void AppendArguments(List<Type> arguments);
        object GetValue(Queue<object> arguments);
    }
}