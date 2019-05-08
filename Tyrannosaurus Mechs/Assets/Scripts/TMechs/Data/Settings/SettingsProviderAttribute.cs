using System;
using JetBrains.Annotations;

namespace TMechs.Data.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
    public class SettingsProviderAttribute : Attribute
    {
    }
}