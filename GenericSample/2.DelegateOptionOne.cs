// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

using System.Diagnostics;
using System.Reflection;

namespace GenericSample;

public class DelegateOptionOne
{
    /*
     * Keep in mind, that in C#, generics are *sorta* compile time, *sorta* runtime.
     * At runtime, the JIT will create multiple implementations of a generic method/type
     *
     * All reference types will share one implementation, and each value type will
     * get its own implementation.
     *
     * Additionally, during this process, the JIT will see typeof(T) as a "compile time constant",
     * which allows it to omit code that will never succeed, based on the type parameter.
     *
     * We can leverage this process to make a more performant implementation.
     *
     * Unfortunately, with this implementation, we're taking the reflection hit each time, which is
     * significantly worse than the boxing in DelegateOptionOne.
     */
    
    static DelegateOptionOne()
    {
        /*
         * Get the MethodInfo for the "open" generic method (i.e., AddRenderable<T>, when I don't know what T is)
         * We'll use it each time in the "Add" method.
         * 
         * Jon Skeet has an excellent StackOverflow answer that goes into the difference between
         * open/closed, bounded/unbounded, etc. (in the context of C# generics)
         *      https://stackoverflow.com/a/1735060
         */
        addRenderableOpenGenericMethod = typeof(DelegateOptionOne).GetMethod(
            nameof(AddRenderable), 
            BindingFlags.NonPublic // If you use static methods, ensure you add BindingFlags.Static.
        )!; // 👈 Null-forgiving operator.  This shouldn't ever fail.
        
        // But if 👆 does fail, we'll at least catch it during development 👇
        Debug.Assert(addRenderableOpenGenericMethod is not null);
    }
    private static readonly MethodInfo addRenderableOpenGenericMethod;
    
    public void Add<T>(T component) where T : struct, IComponent
    {
        if (typeof(T).IsAssignableTo(typeof(IRenderable)))
        {
            var closedGenericMethod = addRenderableOpenGenericMethod.MakeGenericMethod(typeof(T));
            closedGenericMethod.Invoke(null, new object?[] { component });
        }
        AddCore(component);
    }
    private void AddCore<T>(T component)
        where T : struct, IComponent
    {
        throw new NotImplementedException("Perform tasks that pertain to *ALL* components");
    }
    private void AddRenderable<T>(T component) 
        where T : struct, IComponent, IRenderable
    {
        throw new NotImplementedException("Perform tasks that pertain to *ONLY* Renderable components");
    }
}