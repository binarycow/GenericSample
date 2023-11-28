// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
namespace GenericSample;



public class Problem
{
    public void Add<T>(T component) where T : struct, IComponent
    {
        if (component is IRenderable)
        {
            // 🚨 ERROR!!! 🚨
            // The type 'T' must be convertible to 'GenericSample.IRenderable' in order to use it as parameter
            // 'T' in the generic method 'void GenericSample.Problem.AddRenderable<T>(T)'
            AddRenderable(component); 
        }
        AddCore(component);
    }
    private void AddCore<T>(T component)
        where T : struct, IComponent
    {
        throw new NotImplementedException("Perform tasks that pertain to *ALL* components");
    }
    private void AddRenderable<T>(T component) where T : struct, IComponent, IRenderable
    {
        throw new NotImplementedException("Perform tasks that pertain to *ONLY* Renderable components");
    }  
}
