using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Crafting
    {
        private readonly IPacketSender packetSender;
        

        public Crafting(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
            NitroxServiceLocator.LocateService<SimulationOwnership>().OnLockStatusChanged += new SimulationOwnership.LockStatusChangedRemote(Crafter_Remote_LockStatusChanged);
        }


        //applies to Fabricator and Workbench
        public void GhostCrafter_Post_OnHandHover(GameObject gameObject, GUIHand hand)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnHandHover");
#endif

            //if Crafter is in use by remote Player > display to local Player

            string _crafterGuid = GuidHelper.GetGuid(gameObject);//, false);
            ushort _remotePlayerId;

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(_crafterGuid, out _remotePlayerId))
            {
                RemotePlayer _remotePlayer;
                if (NitroxServiceLocator.LocateService<PlayerManager>().TryFind(_remotePlayerId, out _remotePlayer))
                {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnHandHover - remote player usage");
#endif

                    string _baseString = NitroxServiceLocator.LocateService<TranslationManager>().GetTranslation("txtCrafterInUseBy {0}");
                    string _displayText = string.Format(_baseString, _remotePlayer.PlayerName);

#if TRACE && GAMEEVENTCRAFTING
                    NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnHandHover - remote player usage: " + _displayText);
#endif

                    HandReticle.main.SetInteractText(_displayText, string.Empty);
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                }
                else
                {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnHandHover - remote player not found");
#endif

                    //exclusive lock is active, but remote Player can not be found
                    //possible causes:
                    //- disconnect of remote Player did not free the lock locally
                    throw new System.Exception("GhostCrafter_Post_OnHandHover: " + "No remote Player could be found for ID: " + _remotePlayerId);
                }
            }
        }

        //applies to Fabricator and Workbench
        public bool GhostCrafter_Pre_OnHandClick(GhostCrafter __instance, GameObject gameObject, GUIHand hand, ref bool ____opened)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Pre_OnHandClick");
#endif

            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            ushort _remotePlayerId;

            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(_crafterGuid, out _remotePlayerId))
            {
                //if Crafter is in use by remote Player > deny local interaction
                return false;
            }
            else
            {
                //if not, trigger OpenState if it is a Fabricator and closed
                Constructable _constructable = gameObject.RequireComponentInChildren<Constructable>(true);
                if(!____opened && __instance is Fabricator && _constructable.constructed)
                {
                    MethodInfo hasEnoughPower = typeof(GhostCrafter).GetMethod("HasEnoughPower", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (((bool)hasEnoughPower.Invoke(__instance, new object[] { })))
                    {
                        MethodInfo onOpenedChange = typeof(Fabricator).GetMethod("OnOpenedChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                        onOpenedChange.Invoke(__instance, new object[] { true });
                    }
                }
                return true;
            }
        }

        //applies to Fabricator and Workbench
        public bool GhostCrafter_Pre_OnOpenedChanged(GameObject gameObject, bool opened)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Pre_OnOpenedChanged - Opened: " + opened);
#endif

            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            ushort _remotePlayerId;

            //check if event is triggered by local or remote Player
            if (!NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(_crafterGuid, out _remotePlayerId))
            {
                if (opened)
                {
                    Constructable _constructable = gameObject.RequireComponentInChildren<Constructable>(true);
                    if (_constructable.constructed)
                    {
                        //lock the Crafter for exclusive usage for local Player
                        NitroxServiceLocator.LocateService<SimulationOwnership>().RequestSimulationLock(_crafterGuid, NitroxModel.DataStructures.SimulationLockType.EXCLUSIVE, null);
                    }
                }
                else
                {
                    //release the Crafter after finish of local Player usage
                    if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByLocalPlayer(_crafterGuid))
                    {
                        NitroxServiceLocator.LocateService<SimulationOwnership>().ReleaseSimulationLock(_crafterGuid);
                    }
                }
            }
            
            return true;
        }


        public bool CrafterLogic_Pre_TryPickupSingle(GameObject gameObject, TechType techTyp)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("CrafterLogic_Pre_OnTryPickupSingle");
#endif

            //allow TryPickupSingle if we have exclusive lock on the crafter
            //if not, prevent execution >> fixing Git Issue #291

            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            return NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByLocalPlayer(_crafterGuid);
            
        }

        public void CrafterLogic_Post_TryPickupSingle(GameObject gameObject, bool success, TechType techType)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("CrafterLogic_Post_OnTryPickupSingle");
#endif

            //TryPickupSingle returns also true in cases of prefab or techtype could not be found. 
            //no need to check if really an item has been crafted by local Player, because it is only used 
            //to stop the animation on the remote side. The real new item pickup is handled on ItemContainerAdd. 

            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            if (success && NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByLocalPlayer(_crafterGuid))
            {
                packetSender.Send(new CrafterItemPickup(_crafterGuid));
            }
        }

        public void GhostCrafter_Post_OnCraftingBegin(GameObject crafter, TechType techType, float duration)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnCraftingBegin");
#endif

            string _crafterGuid = GuidHelper.GetGuid(crafter);
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByLocalPlayer(_crafterGuid))
            {
                packetSender.Send(new CrafterBeginCrafting(_crafterGuid, techType.Model(), duration));
            }
        }

        public bool GhostCrafter_Pre_OnCraftingEnd(GameObject gameObject)
        {

#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Pre_OnCraftingEnd");
#endif

            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByLocalPlayer(_crafterGuid))
            {
                packetSender.Send(new CrafterEndCrafting(_crafterGuid));
                return true;
            }
            else
            {
                //suppress the PickupLogic if the event was raised by a remote Player
                return false;
            }

        }


        internal void GhostCrafter_Remote_CraftingEnd(string crafterGuid)
        {

#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Remote_CraftingEnd");
#endif

            ushort _remotePlayerId;
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(crafterGuid, out _remotePlayerId))
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(crafterGuid);
                CrafterLogic _crafterLogic = _gameObject.RequireComponentInChildren<CrafterLogic>(true);
                _crafterLogic.Reset();
            }
        }

        internal void GhostCrafter_Remote_CraftingBegin(string crafterGuid, NitroxModel.DataStructures.TechType techType, float duration)
        {

#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Remote_CraftingBegin");
#endif

            ushort _remotePlayerId;
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(crafterGuid, out _remotePlayerId))
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(crafterGuid);
                GhostCrafter _crafter = _gameObject.RequireComponentInChildren<GhostCrafter>(true);
                FieldInfo _logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_logic != null)
                {
                    CrafterLogic _crafterLogic = (CrafterLogic)_logic.GetValue(_crafter);
                    _crafterLogic.Craft(techType.Enum(), duration);
                }
            }
        }

        internal void CrafterLogic_Remote_ItemPickup(string crafterGuid)
        {

#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("CrafterLogic_Remote_ItemPickup");
#endif

            ushort _remotePlayerId;
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(crafterGuid, out _remotePlayerId))
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(crafterGuid);
                CrafterLogic _crafterLogic = _gameObject.RequireComponentInChildren<CrafterLogic>(true);

                if (_crafterLogic.numCrafted > 0)
                {
                    _crafterLogic.numCrafted--;

                    if (_crafterLogic.numCrafted == 0)
                    {
                        _crafterLogic.Reset();
                    }
                }
            }
        }

        private void Crafter_Remote_LockStatusChanged(string guid, bool locked)
        {
            Optional<GameObject> _opgameObject = GuidHelper.GetObjectFrom(guid);
            if (!_opgameObject.IsEmpty())
            {
                GameObject _gameObject = _opgameObject.Get();
                GhostCrafter _crafter = _gameObject.GetComponentInChildren<GhostCrafter>(true);
                Constructable _constructable = _gameObject.GetComponentInChildren<Constructable>(true);
                if (_crafter != null && _constructable != null)
                {
                    if (_crafter is Fabricator && _constructable.constructed)
                    {
                        MethodInfo onOpenedChange = typeof(Fabricator).GetMethod("OnOpenedChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (locked)
                        {
                            onOpenedChange.Invoke(_crafter, new object[] { true });
                        }
                        else
                        {
                            onOpenedChange.Invoke(_crafter, new object[] { false });
                        }
                    }
                }
            }
        }

        
    }
}
