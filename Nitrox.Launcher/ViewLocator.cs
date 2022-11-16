using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher;

public class AppViewLocator : IViewLocator
{
    private static readonly Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
    private static readonly Dictionary<Type, Func<IRoutableView>> viewModelToViewMapFactoryCache = new();

    /// <summary>
    ///     <inheritdoc cref="IViewLocator.ResolveView{T}" />
    /// </summary>
    /// <remarks>
    ///     TODO: Use source generator to fill in the mapping of ViewModel -> View (removing requirement of reflection).
    /// </remarks>
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel == null)
        {
            throw new ArgumentNullException($"Argument {nameof(viewModel)} must not be null.");
        }
        Type viewModelType = viewModel.GetType();

        // If view factory is cached, return the view via this creator.
        IRoutableView view;
        if (viewModelToViewMapFactoryCache.TryGetValue(viewModelType, out Func<IRoutableView>? viewFactory))
        {
            view = viewFactory();
            view.DataContext = viewModel;
            return view;
        }

        // Create a new view factory for the given ViewModel type. 
        Type targetBaseViewType = typeof(RoutableViewBase<>).MakeGenericType(viewModelType);
        Type viewThatImplementsBaseType = assemblyTypes.First(t => targetBaseViewType.IsAssignableFrom(t));
        viewFactory = () => (IRoutableView)Activator.CreateInstance(viewThatImplementsBaseType)!;
        view = viewFactory();
        if (view == null)
        {
            throw new Exception($"Unable to create instance of view {targetBaseViewType}");
        }
        view.DataContext = viewModel;
        viewModelToViewMapFactoryCache.Add(viewModelType, viewFactory);
        return view;
    }
}
