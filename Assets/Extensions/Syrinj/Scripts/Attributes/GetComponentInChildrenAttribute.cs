using System;
using Syrinj.Attributes;

public class GetComponentInChildrenAttribute : UnityConvenienceAttribute
{
    public readonly Type ComponentType;

    public GetComponentInChildrenAttribute()
    {

    }

    public GetComponentInChildrenAttribute(Type componentType)
    {
        ComponentType = componentType;
    }
}
