using System;
using Syrinj.Attributes;

public class FindObjectOfTypeAttribute : UnityConvenienceAttribute
{
    public readonly Type ComponentType;

    public FindObjectOfTypeAttribute()
    {
        
    }

    public FindObjectOfTypeAttribute(Type componentType)
    {
        ComponentType = componentType;
    }
}