using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TMechs.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class FriendlyNameAttribute : Attribute
    {
        public readonly string name;

        public FriendlyNameAttribute(string name) => this.name = name;

        public static IEnumerable<string> GetNames<T>(bool requireFriendlyName = true, Dictionary<int, T> map = null) where T : Enum
        {
            T[] items = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            List<string> values = new List<string>();

            foreach (T item in items)
            {
                MemberInfo info = item.GetType().GetMember(item.ToString()).SingleOrDefault();
                if (info == null)
                    continue;

                FriendlyNameAttribute fn = (FriendlyNameAttribute) info.GetCustomAttributes(typeof(FriendlyNameAttribute)).SingleOrDefault();

                string name;

                if (fn == null)
                {
                    if (requireFriendlyName)
                        continue;

                    name = info.Name;
                }
                else
                    name = fn.name;

                if (map != null)
                    map.Add(values.Count, item);
                values.Add(name);
            }


            return values;
        }
    }
}