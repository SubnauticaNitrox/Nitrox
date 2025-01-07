using System;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class SeaTreaderMetadataProcessor : EntityMetadataProcessor<SeaTreaderMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, SeaTreaderMetadata metadata)
    {
        if (gameObject.TryGetComponent(out SeaTreader seaTreader))
        {
            if (!seaTreader.isInitialized)
            {
                seaTreader.InitializeOnce();
            }

            seaTreader.reverseDirection = metadata.ReverseDirection;

            float grazingTimeLeft = Math.Max(0, metadata.GrazingEndTime - DayNightCycle.main.timePassedAsFloat);

            seaTreader.grazing = grazingTimeLeft > 0;
            seaTreader.grazingTimeLeft = grazingTimeLeft;

            seaTreader.leashPosition = metadata.LeashPosition.ToUnity();
            seaTreader.leashPosition.y = gameObject.transform.position.y;
            seaTreader.isInitialized = true;
            seaTreader.InitializeAgain();
        }
        else
        {
            Log.Error($"Could not find {nameof(SeaTreader)} on {gameObject.name}");
        }
    }
}
