using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;


namespace NitroxClient.GameLogic.Containers
{
    public abstract class ContainerAddItemPostProcessor
    {

        private static readonly NoOpContainerAddItemPostProcessor noOpProcessor = new NoOpContainerAddItemPostProcessor();
        private static readonly IEnumerable<ContainerAddItemPostProcessor> processors;

        public abstract Type[] ApplicableComponents { get; }

        static ContainerAddItemPostProcessor()
        {
            processors = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(t => typeof(ContainerAddItemPostProcessor).IsAssignableFrom(t) &&
                                            t.IsClass && !t.IsAbstract && t != typeof(NoOpContainerAddItemPostProcessor)
                                    )
                                .Select(Activator.CreateInstance)
                                .Cast<ContainerAddItemPostProcessor>();
        }

        public abstract void process(GameObject item, ItemData itemData);

        public static ContainerAddItemPostProcessor From(GameObject item)
        {
            foreach (ContainerAddItemPostProcessor processor in processors)
            {
                foreach (Type type in processor.ApplicableComponents)
                {
                    if (item.GetComponent(type))
                    {
                        Log.Info("Found custom ContainerAddItemPostProcessor for " + type);
                        return processor;
                    }
                }
            }

            return noOpProcessor;
        }
    }
}
