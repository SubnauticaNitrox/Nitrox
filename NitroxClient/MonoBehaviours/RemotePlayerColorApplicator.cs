using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class RemotePlayerColorApplicator : MonoBehaviour
    {
        private IEnumerable<IColorSwapManager> colorSwapManagers;
        private RemotePlayer remotePlayer;
        private ColorSwapAsyncOperation swapOperation;

        public void Awake()
        {
            colorSwapManagers = NitroxServiceLocator.LocateService<IEnumerable<IColorSwapManager>>();
        }

        public void Update()
        {
            if (remotePlayer == null)
            {
                return;
            }

            if (swapOperation != null)
            {
                return;
            }

            swapOperation = new ColorSwapAsyncOperation(remotePlayer, colorSwapManagers);
            StartCoroutine(ApplyPlayerColor());
        }

        public void AttachRemotePlayer(RemotePlayer remotePlayer)
        {
            if (this.remotePlayer != null)
            {
                throw new InvalidOperationException("An applicator instance may only be used once.");
            }

            this.remotePlayer = remotePlayer;
        }

        private IEnumerator ApplyPlayerColor()
        {
            swapOperation.BeginColorSwap();

            while (!swapOperation.IsColorSwapComplete())
            {
                yield return new WaitForSeconds(0.1f);
            }

            swapOperation.ApplySwappedColors();

            Destroy(this);
        }
    }
}
