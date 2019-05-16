using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugConsole
{
    public class ColorParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Color);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(byte));
            arguments.Add(typeof(byte));
            arguments.Add(typeof(byte));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Color((byte)arguments.Dequeue() / 255F, (byte)arguments.Dequeue() / 255F, (byte)arguments.Dequeue() / 255F);
        }
    }

    public class Vector2Parser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Vector2);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Vector2((float) arguments.Dequeue(), (float) arguments.Dequeue());
        }
    }
    
    public class Vector3Parser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Vector3);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Vector3((float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue());
        }
    }
    
    public class Vector4Parser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Vector4);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Vector4((float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue());
        }
    }
    
    public class RectParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Rect);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Rect((float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue());
        }
    }

    public class BoundsParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Bounds);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Bounds(
                    new Vector3((float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue()),
                    new Vector3((float) arguments.Dequeue(), (float) arguments.Dequeue(), (float) arguments.Dequeue())
            );
        }
    }
    
    public class QuaternionParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Quaternion);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
            arguments.Add(typeof(float));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Quaternion((float)arguments.Dequeue(), (float)arguments.Dequeue(), (float)arguments.Dequeue(), (float)arguments.Dequeue());
        }
    }
    
    public class Vector2IntParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Vector2Int);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Vector2Int((int) arguments.Dequeue(), (int) arguments.Dequeue());
        }
    }
    
    public class Vector3IntParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(Vector3Int);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Vector3((int) arguments.Dequeue(), (int) arguments.Dequeue(), (int) arguments.Dequeue());
        }
    }
    
    public class RectIntParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(RectInt);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new RectInt((int) arguments.Dequeue(), (int) arguments.Dequeue(), (int) arguments.Dequeue(), (int) arguments.Dequeue());
        }
    }

    public class BoundsIntParser : IDebugTypeParser
    {
        public Type GetTarget()
            => typeof(BoundsInt);

        public void AppendArguments(List<Type> arguments)
        {
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
            arguments.Add(typeof(int));
        }

        public object GetValue(Queue<object> arguments)
        {
            return new Bounds(
                    new Vector3((int) arguments.Dequeue(), (int) arguments.Dequeue(), (int) arguments.Dequeue()),
                    new Vector3((int) arguments.Dequeue(), (int) arguments.Dequeue(), (int) arguments.Dequeue())
            );
        }
    }
}