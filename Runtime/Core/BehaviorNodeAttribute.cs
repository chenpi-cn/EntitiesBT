using System;
using System.Reflection;

namespace EntitiesBT.Core
{
    public enum BehaviorNodeType
    {
        Composite,
        Decorate,
        Action,
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BehaviorNodeAttribute : Attribute
    {
        public Guid Guid { get; } 
        public int Id { get; }
        public BehaviorNodeType Type { get; }
        public string ResetFunc { get; }
        public string TickFunc { get; }
        public string AccessTypesFunc { get; }
        
        public BehaviorNodeAttribute(string guid, BehaviorNodeType type = BehaviorNodeType.Action, string tickFunc = "Tick", string resetFunc = "Reset", string accessTypesFunc = "AccessTypes")
        {
            Type = type;
            Guid = Guid.Parse(guid);
            Id = Guid.GetHashCode();
            TickFunc = tickFunc;
            ResetFunc = resetFunc;
            AccessTypesFunc = accessTypesFunc;
        }
    }

    public static class BehaviorNodeAttributeExtensions
    {
        public static BehaviorNodeAttribute GetBehaviorNodeAttribute(this Type type)
        {
            return (BehaviorNodeAttribute) type.GetCustomAttribute(typeof(BehaviorNodeAttribute));
        }
    }
}
