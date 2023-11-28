// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace GenericSample;

public class DelegateOptionThree
{
    /*
     * This is basically the same as DelegateOptionThree.  The difference is how we create the delegate.
     *
     * In DelegateOptionThree, we used MethodInfo.CreateDelegate.
     * Here in DelegateOptionThree, we'll use expression trees.
     *
     * Expression trees are more flexible than MethodInfo.CreateDelegate.
     *  - MethodInfo.CreateDelegate returns a delegate that does one thing, and one thing only - calls that method.
     *  - Expression trees let you do all sorts of stuff, with the downside that its verbose and complicated.
     */
    
    private static readonly ConcurrentDictionary<Type, Delegate> cachedDelegates = new();
    
    public void Add<T>(T component) where T : struct, IComponent
    {
        var @delegate = cachedDelegates.GetOrAdd(typeof(T), static _ => CreateDelegate<T>()) as Action<DelegateOptionThree, T>;
        Debug.Assert(@delegate is not null);
        @delegate(this, component);
    }
    
    /*
     * Some important notes about the CreateDelegate method:
     * 1. We use a type parameter, so we can ensure we create the exact type of delegate that we want to use.
     * 2. It is invoked only *ONCE* for a given type parameter.
     *    - (Because we store the result in a ConcurrentDictionary).
     *    - This means that we don't have to be as concerned with performance within this method
     * 3. This method is static, so we don't capture an instance of our owning type (DelegateOptionThree, in this case).
     * 4. The returned delegate accepts an instance of our owning type (DelegateOptionThree) so it can call instance methods.
     */
    private static Action<DelegateOptionThree, T> CreateDelegate<T>()
        where T : struct, IComponent
    {
        if (typeof(T).IsAssignableTo(typeof(IRenderable)))
        {
            // For this example, we'll move this code into a separate method, since
            // expression trees can be quite... verbose.
            return CreateRenderableDelegate<T>();
        }
        /*
         * Since we have a generic constraint on this method (that also satisfies the constraints in AddCore<T>,
         * there's no problem just creating a simple delegate via a lambda.
         */
        return (instance, component) => instance.AddCore(component);
    }
    private static Action<DelegateOptionThree, T> CreateRenderableDelegate<T>() 
        where T : struct, IComponent
    {
        // Get the MethodInfo for the two methods we need to call
        var addRenderableMethod = CreateClosedGenericMethodInfo(nameof(AddRenderable));
        var addCoreMethod = CreateClosedGenericMethodInfo(nameof(AddCore));
        
        // Declare the parameters to the delegate
        var instanceParameter = Expression.Parameter(typeof(DelegateOptionThree), name: "instance");
        var componentParameter = Expression.Parameter(typeof(T), name: "component");

        // Create an expression that represents calling the "AddCore" method
        //      instance.AddCore(component);
        var callAddCore = Expression.Call(instanceParameter, addCoreMethod, componentParameter);
        // Create an expression that represents calling the "AddRenderable" method
        //      instance.AddRenderable(component);
        var callAddRenderable = Expression.Call(instanceParameter, addRenderableMethod, componentParameter);

        // The body of the lambda is our two call expressions. 
        //      {
        //          instance.AddCore(component);
        //          instance.AddRenderable(component);
        //      }
        var lambdaBody = Expression.Block(
            callAddCore,
            callAddRenderable
        );
        
        // Now the lambda itself
        //      (DelegateOptionThree instance, T component) =>
        //      {
        //          instance.AddCore(component);
        //          instance.AddRenderable(component);
        //      }
        
        var lambda = Expression.Lambda<Action<DelegateOptionThree, T>>(
            lambdaBody,
            instanceParameter,
            componentParameter
        );
        
        // Compile the expression into an actual delegate
        return lambda.Compile();
            
        /*
         * Since we need to do this for two methods now, move this code into a local function for reusability.
         */
        static MethodInfo CreateClosedGenericMethodInfo(string methodName)
        {
            // We still need to get this method info.
            var openGenericMethod = typeof(DelegateOptionThree).GetMethod(
                methodName, 
                BindingFlags.NonPublic // If you use static methods, ensure you add BindingFlags.Static.
            );
            Debug.Assert(openGenericMethod is not null);
            return openGenericMethod.MakeGenericMethod(typeof(T));
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
        // Note, that unlike DelegateOptionThree, we don't need to call AddCore from in here. 
        // This is because we're handling it in the expression tree.
        throw new NotImplementedException("Perform tasks that pertain to *ONLY* Renderable components");
    }
}