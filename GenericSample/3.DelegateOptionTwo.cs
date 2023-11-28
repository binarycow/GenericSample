// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace GenericSample;

public class DelegateOptionTwo
{
    /*
     * This option will take what we did in DelegateOptionTwo, and cache a delegate.
     * This means we will take the reflection hit *one* time.  And then any future invocation of our "Add" method will be
     * very quick.
     *
     * The delegate we're creating comes from MethodInfo.CreateDelegate
     */
    
    private static readonly ConcurrentDictionary<Type, Delegate> cachedDelegates = new();
    
    public void Add<T>(T component) where T : struct, IComponent
    {
        // Use "GetOrAdd" to lazily instantiate the cached delegate
        var @delegate = cachedDelegates.GetOrAdd(typeof(T), static _ => CreateDelegate<T>()) as Action<DelegateOptionTwo, T>;
        
        // 👆 shouldn't ever fail.  But if it does, we'll catch it during development.
        Debug.Assert(@delegate is not null);
        @delegate(this, component);
    }
    
    /*
     * Some important notes about the CreateDelegate method:
     * 1. We use a type parameter, so we can ensure we create the exact type of delegate that we want to use.
     * 2. It is invoked only *ONCE* for a given type parameter.
     *    - (Because we store the result in a ConcurrentDictionary).
     *    - This means that we don't have to be as concerned with performance within this method
     * 3. This method is static, so we don't capture an instance of our owning type (DelegateOptionTwo, in this case).
     * 4. The returned delegate accepts an instance of our owning type (DelegateOptionTwo) so it can call instance methods.
     * 5. We're actually invoking a *STATIC* method, AddRenderableStatic<T>
     *    - This will simply cally the *INSTANCE* method, AddRenderable<T>
     *    - We do this so the delegate signatures match.
     */
    private static Action<DelegateOptionTwo, T> CreateDelegate<T>()
        where T : struct, IComponent
    {
        if (typeof(T).IsAssignableTo(typeof(IRenderable)))
        {
            // If you want, you can cache this, just like we did in DelegateOptionTwo.  But since we're going to be in this
            // method only once per type parameter, it's less of a concern.
            var openGenericMethod = typeof(DelegateOptionTwo).GetMethod(nameof(AddRenderableStatic), BindingFlags.NonPublic | BindingFlags.Static);
            // Again, 👆 should never fail, but let's check anyway (development only tho)
            Debug.Assert(openGenericMethod is not null);
            
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(typeof(T));
            
            return closedGenericMethod.CreateDelegate<Action<DelegateOptionTwo, T>>();
        }
        else
        {
            /*
             * Since we have a generic constraint on this method (that also satisfies the constraints in AddCore<T>,
             * there's no problem just creating a simple delegate via a lambda.
             */
            return (instance, component) => instance.AddCore(component);
        }
    }
    
    private void AddCore<T>(T component)
        where T : struct, IComponent
    {
        throw new NotImplementedException("Perform tasks that pertain to *ALL* components");
    }
    private void AddRenderable<T>(T component) 
        where T : struct, IComponent, IRenderable
    {
        /*
         * To make the generation of the delegate easier, we're not going to call AddRenderable<T>, and then *ALSO* call
         * AddCore<T>, like in our other implementations.
         *
         * We'll just have AddRenderable<T> call AddCore<T>.
         */
        AddCore(component);
        throw new NotImplementedException("Perform tasks that pertain to *ONLY* Renderable components");
    }
    private static void AddRenderableStatic<T>(DelegateOptionTwo instance, T component)
        where T : struct, IComponent, IRenderable
    {
        instance.AddRenderable(component);
    }
}