using System;
using SimpleInjector;

namespace NitroxModel.Core
{
    public static class NitroxServiceLocator
    {
        public static Container SimpleInjectorContainer { get; private set; }

        public static void InstallContainer(ISimpleInjectorContainerBuilder simpleInjectorContainer)
        {
            if (SimpleInjectorContainer != null)
            {
                throw new InvalidOperationException("A ServiceInjector Container can only be installed once during the life of the application.");
            }

            SimpleInjectorContainer = simpleInjectorContainer.BuildContainer();
        }

        public static T LocateService<T>()
            where T : class
        {
            if (SimpleInjectorContainer == null)
            {
                throw new InvalidOperationException(
                    "Cannot locate services until a SimpleInjector Container has been installed.");
            }

            return SimpleInjectorContainer.GetInstance<T>();
        }
    }
}
