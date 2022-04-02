using Syrinj.Attributes;

public class InjectAttribute : UnityInjectorAttribute
{
    public readonly string Tag;

    public InjectAttribute()
    {
        
    }

    public InjectAttribute(string tag)
    {
        this.Tag = tag;
    }
}
