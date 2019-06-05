using System;

namespace TMechs.Types
{
    public class StringVDictionary<V> : SerializedDictionary<string, V>
    {
        private readonly Random random = new Random();
        
        public override string GetNextKey()
        {
            return $"{RandChar()}{RandChar()}{RandChar()}{RandChar()}{RandChar()}";
        }

        private char RandChar()
        {
            return (char)random.Next(97, 123);
        }
    }
}