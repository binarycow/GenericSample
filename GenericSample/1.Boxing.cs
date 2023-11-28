// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
namespace GenericSample;

public class Boxing
{
    public void Add<T>(T component) where T : struct, IComponent
    {
        /*
         * 🚨🚩 This method will box the struct!
         * The "declaration" pattern that we use below will declare and assign a variable of type IRenderable.
         * Any time a value type is treated as an interface like this, it is boxed.
         *      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/conversions#1029-boxing-conversions
         * This boxing negates the performance benefits you may get by constraining T to a struct.
         */
        if (component is IRenderable renderable)
        {
            AddRenderable(component, renderable); 
        }
        AddCore(component);
    }
    private void AddCore<T>(T component)
        where T : struct, IComponent
    {
        throw new NotImplementedException("Perform tasks that pertain to *ALL* components");
    }
    
    private void AddRenderable<TComponent, TRenderable>(
        TComponent component,
        TRenderable renderable
    )
        where TComponent : struct, IComponent
        where TRenderable : IRenderable
    {
        throw new NotImplementedException("Perform tasks that pertain to *ONLY* Renderable components");
    }
}
