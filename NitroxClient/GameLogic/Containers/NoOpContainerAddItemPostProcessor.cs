using System;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.Containers
{
    class NoOpContainerAddItemPostProcessor: ContainerAddItemPostProcessor
    {
        public override Type[] ApplicableComponents { get; } = new Type[0];

        public override void process(GameObject item, ItemData itemData)
        {
            // No-Op!
        }

    }
}
