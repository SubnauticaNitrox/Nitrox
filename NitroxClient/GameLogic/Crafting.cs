using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Crafting
    {
        internal static Crafting Instance; 
        private System.Collections.Generic.Dictionary<string, GameLogic.CraftingUsageInfo> fabricatorsInUseByRemotePlayers = new System.Collections.Generic.Dictionary<string, GameLogic.CraftingUsageInfo>();
        private readonly System.Timers.Timer timer = new System.Timers.Timer(5000);
        private readonly int remoteSignalTimeOut = 120; //in seconds. 2 Minutes, because if someone crafts multiple items, no new starttime is sent and we don't want to interrupt remote player.
        private Constructable _lastHoveredConstructable = null;
        private readonly IPacketSender packetSender;
        
        public Crafting(IPacketSender packetSender)
        {
            Instance = this;
            this.packetSender = packetSender;
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerEvent);
            timer.Start();
        }

        //Applies to Fabricator and Workbench
        public void GhostCrafter_Post_OnHandHover(GameObject gameObject, GUIHand hand)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Post_OnHandHover");
#endif
            //if Crafter is in use by remote Player > Display
            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            bool _inUseByOther = false;
            string _playerName = string.Empty;
            lock (fabricatorsInUseByRemotePlayers)
            {
                _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                if (_inUseByOther)
                {
                    _playerName = fabricatorsInUseByRemotePlayers[_crafterGuid].PlayerName;
                }
            }
            if (_inUseByOther)
            {
                string _baseString = "In use by";
                //TODO: Translate _baseString

                //
                string _displayText = _baseString + " " + _playerName;
                HandReticle.main.SetInteractText(_displayText, string.Empty);
                HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
            }
        }

        //Applies to Fabricator and Workbench
        public bool GhostCrafter_Pre_OnHandClick(GameObject gameObject, GUIHand hand)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Pre_OnHandClick");
#endif
            //if Crafter is in use by remote Player > suppress OnHandClick
            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            bool _inUseByOther = false;
            lock (fabricatorsInUseByRemotePlayers)
            {
                _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
            }
            if (_inUseByOther)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Applies to Fabricator and Workbench
        public bool GhostCrafter_Pre_OnOpenedChanged(GameObject gameObject, bool opened)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Pre_OnOpenedChanged - Opened: " + opened);
#endif
            if (opened)
            {
                //Check if Event is triggered by self or remote Action
                string _crafterGuid = GuidHelper.GetGuid(gameObject);
                bool _inUseByOther = false;
                lock (fabricatorsInUseByRemotePlayers)
                {
                    _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                }
                //Notify remote Player for use by local
                if (!_inUseByOther)
                {
                    CrafterStartUse _packetStartUse = new CrafterStartUse(NitroxModel.Core.NitroxServiceLocator.LocateService<LocalPlayer>().PlayerName, _crafterGuid);
                    packetSender.Send(_packetStartUse);
                }
            }
            else
            {
                //Check if Event is triggered by self or remote Action
                string _crafterGuid = GuidHelper.GetGuid(gameObject);
                bool _inUseByOther = false;
                lock (fabricatorsInUseByRemotePlayers)
                {
                    _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                }
                //If not in use by other notify others that we are done with crafting
                if (!_inUseByOther)
                {
                    CrafterEndUse _packetEndUse = new CrafterEndUse(_crafterGuid);
                    packetSender.Send(_packetEndUse);
                }
            }
            return true;
        }

        internal void GhostCrafter_Remote_OnStartUse(string crafterGuid, string playerName)
        {
#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Remote_OnStartUse");
#endif
            lock (fabricatorsInUseByRemotePlayers)
            {
                if (!fabricatorsInUseByRemotePlayers.ContainsKey(crafterGuid))
                {
                    fabricatorsInUseByRemotePlayers.Add(crafterGuid, new CraftingUsageInfo() { PlayerName = playerName, StartTime = DateTime.Now });
                }
                else
                {
                    fabricatorsInUseByRemotePlayers[crafterGuid].StartTime = System.DateTime.Now;
                }
            }
        }

        internal void GhostCrafter_Remote_OnEndUse(string crafterGuid)
        {

#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("GhostCrafter_Remote_OnEndUse");
#endif
            lock (fabricatorsInUseByRemotePlayers)
            {
                if (fabricatorsInUseByRemotePlayers.ContainsKey(crafterGuid))
                {
                    fabricatorsInUseByRemotePlayers.Remove(crafterGuid);
                }
            }
        }

        public bool CrafterLogic_Pre_OnTryPickupSingle(GameObject gameObject, TechType techTyp)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("CrafterLogic_Pre_OnTryPickupSingle");
#endif

            bool _result = true;
            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            lock (fabricatorsInUseByRemotePlayers)
            {
                if (fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid))
                {
                    _result = false;
                }
            }
            return _result;
        }

        public void CrafterLogic_Post_OnTryPickupSingle(GameObject gameObject, bool success, TechType techType)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("CrafterLogic_Post_OnTryPickupSingle");
#endif

            if (success)
            {
                string _crafterGuid = GuidHelper.GetGuid(gameObject);
                FabricatorItemPickup _fabricatorItemPickup = new FabricatorItemPickup(_crafterGuid, techType.Model());
                packetSender.Send(_fabricatorItemPickup);             
            }
        }

        public bool Fabricator_Pre_OnCraftingEnd(GameObject gameObject)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Fabricator_Pre_OnCraftingEnd");
#endif

            bool _result = true;
            string _crafterGuid = GuidHelper.GetGuid(gameObject);
            lock (fabricatorsInUseByRemotePlayers)
            {
                if (fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid))
                {
                    _result = false;
                }
            }
            if (_result)
            {
                FabricatorEndCrafting _fabricatorEnd = new FabricatorEndCrafting(_crafterGuid);
                packetSender.Send(_fabricatorEnd);
            }
            return _result;
        }

        internal void Fabricator_Remote_OnCraftingEnd(string fabricatorGuid)
        {
#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Fabricator_Remote_OnCraftingEnd");
#endif

            bool _inUseByOther = false;
            lock (fabricatorsInUseByRemotePlayers)
            {
                _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(fabricatorGuid);
            }
            if (_inUseByOther)
            {
                GameObject gameObject = GuidHelper.RequireObjectFrom(fabricatorGuid);
                CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);
                crafterLogic.Reset();
            }
        }

        public void Fabricator_Post_OnCraftingBegin(GameObject crafter, TechType techType, float duration)
        {
#if TRACE && GAMEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Fabricator_Post_OnCraftingBegin");
#endif

            string _crafterGuid = GuidHelper.GetGuid(crafter);
            FabricatorBeginCrafting _fabricatorBeginCrafting = new FabricatorBeginCrafting(_crafterGuid, techType.Model(), duration);
            packetSender.Send(_fabricatorBeginCrafting);
        }

        internal void Fabricator_Remote_OnCraftingBegin(string fabricatorGuid, NitroxModel.DataStructures.TechType techType, float duration)
        {
#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Fabricator_Remote_OnCraftingBegin");
#endif
            GameObject gameObject = GuidHelper.RequireObjectFrom(fabricatorGuid);
            Fabricator fabricator = gameObject.RequireComponentInChildren<Fabricator>(true);
            FieldInfo logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
            if (logic != null)
            {
                CrafterLogic crafterLogic = (CrafterLogic)logic.GetValue(fabricator);
                crafterLogic.Craft(techType.Enum(), duration);
            }
        }

        internal void Fabricator_Remote_ItemPickup(string fabricatorGuid, NitroxModel.DataStructures.TechType techType)
        {
#if TRACE && REMOTEEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Fabricator_Remote_ItemPickup");
#endif

            bool _inUseByOther = false;
            lock (fabricatorsInUseByRemotePlayers)
            {
                _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(fabricatorGuid);
            }
            if(_inUseByOther)
            {
                GameObject gameObject = GuidHelper.RequireObjectFrom(fabricatorGuid);
                CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);

                if (crafterLogic.numCrafted > 0)
                {
                    crafterLogic.numCrafted--;

                    if (crafterLogic.numCrafted == 0)
                    {
                        crafterLogic.Reset();
                    }
                }
            }
        }

        //Applies to Fabricator and Workbench
        public bool BuilderTool_Pre_OnHover(GameObject gameObject, Constructable constructable)
        {
#if TRACE && GAMEEVENTBUILDING
            NitroxModel.Logger.Log.Debug("BuilderTool_Pre_OnHover");
#endif
            if (constructable.techType == TechType.Fabricator || constructable.techType == TechType.Workbench)
            {
                string _crafterGuid = GuidHelper.GetGuid(constructable.gameObject);
                bool _inUseByOther = false;
                string _playerName = string.Empty;
                lock (fabricatorsInUseByRemotePlayers)
                {
                    _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                    if (_inUseByOther)
                    {
                        _playerName = fabricatorsInUseByRemotePlayers[_crafterGuid].PlayerName;
                    }
                }
                if (_inUseByOther && _crafterGuid != string.Empty)
                {
                    return false;
                }
            }
            return true;
        }

        //Applies to Fabricator and Workbench
        public void BuilderTool_Post_OnHover(GameObject gameObject, Constructable constructable)
        {
#if TRACE && GAMEEVENTBUILDING
            NitroxModel.Logger.Log.Debug("BuilderTool_Post_OnHover");
#endif
            if (constructable.techType == TechType.Fabricator || constructable.techType == TechType.Workbench)
            {
                //if Crafter is in use by remote Player > Display
                _lastHoveredConstructable = constructable;
                string _crafterGuid = GuidHelper.GetGuid(constructable.gameObject);
                bool _inUseByOther = false;
                string _playerName = string.Empty;

                lock (fabricatorsInUseByRemotePlayers)
                {
                    _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                    if (_inUseByOther)
                    {
                        _playerName = fabricatorsInUseByRemotePlayers[_crafterGuid].PlayerName;
                    }
                }

                if (_inUseByOther && _crafterGuid != string.Empty)
                {
                    string _baseString = "In use by";
                    //TODO: Translate _baseString

                    //
                    string _displayText = _baseString + " " + _playerName;
                    HandReticle.main.SetInteractText(_displayText, string.Empty);
                    HandReticle.main.SetInteractInfo(_displayText, string.Empty); //must be used, otherwise it is not displayed
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                }
            }
            else
            {
                _lastHoveredConstructable = null;
            }
        }

        public bool BuilderTool_Pre_HandleInput(GameObject gameObject)
        {
#if TRACE && GAMEEVENTBUILDING
            NitroxModel.Logger.Log.Debug("BuilderTool_Pre_HandleInput");
#endif
            if (_lastHoveredConstructable != null)
            {
                //if Crafter is in use by remote Player, supress deconstruction
                string _crafterGuid = GuidHelper.GetGuid(_lastHoveredConstructable.gameObject);
                bool _inUseByOther = false;
                string _playerName = string.Empty;

                lock (fabricatorsInUseByRemotePlayers)
                {
                    _inUseByOther = fabricatorsInUseByRemotePlayers.ContainsKey(_crafterGuid);
                    if (_inUseByOther)
                    {
                        _playerName = fabricatorsInUseByRemotePlayers[_crafterGuid].PlayerName;
                    }
                }

                if (_inUseByOther && _crafterGuid != string.Empty)
                {
                    return false;
                }

                return true;
            }
            else
            { return true; }
        }
        
        //If Crafters are locked and something unplanned happend (wrong packet-order, etc.) > check if remoteSignalTimeOut is elapsed and reset Crafter
        private void OnTimerEvent(object sender, System.Timers.ElapsedEventArgs args)
        {
#if TRACE && TIMEREEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("Crafting - Checkintervall RemoteSignalTimeOut");
#endif
            System.Collections.Generic.List<string> _craftersToRemove = new System.Collections.Generic.List<string>();

            lock (fabricatorsInUseByRemotePlayers)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, GameLogic.CraftingUsageInfo> _fabricatorTimeInfo in fabricatorsInUseByRemotePlayers)
                {
                    if ((System.DateTime.Now - _fabricatorTimeInfo.Value.StartTime).TotalSeconds > remoteSignalTimeOut)
                    {
                        _craftersToRemove.Add(_fabricatorTimeInfo.Key);
                    }
                }
            }

            foreach (string _fabricatorGuid in _craftersToRemove)
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(_fabricatorGuid);
                CrafterLogic _crafterLogic = _gameObject.GetComponentInChildren<CrafterLogic>(true);
                _crafterLogic.Reset();
            }

            //remove the guids after the .Reset so the Crafter.OnOpenedChange doesn't send ClosedByMe Notifys to other Players
            lock (fabricatorsInUseByRemotePlayers)
            {
                foreach (string _fabricatorGuid in _craftersToRemove)
                {
                    fabricatorsInUseByRemotePlayers.Remove(_fabricatorGuid);
                }
            }
        }

        internal void ReleaseFabricatorsUsedBy(string playerName)
        {
#if TRACE && DISCONNECTEVENTCRAFTING
            NitroxModel.Logger.Log.Debug("ReleaseFabricatorsUsedBy");
#endif
            System.Collections.Generic.List<string> _craftersToRemove = new System.Collections.Generic.List<string>();

            lock (fabricatorsInUseByRemotePlayers)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, GameLogic.CraftingUsageInfo> _fabricatorTimeInfo in fabricatorsInUseByRemotePlayers)
                {
                    if (_fabricatorTimeInfo.Value.PlayerName == playerName)
                    {
                        _craftersToRemove.Add(_fabricatorTimeInfo.Key);
                    }
                }
            }

            foreach (string _fabricatorGuid in _craftersToRemove)
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(_fabricatorGuid);
                CrafterLogic _crafterLogic = _gameObject.GetComponentInChildren<CrafterLogic>(true);
                _crafterLogic.Reset();
            }

            //remove the guids after the .Reset so the Crafter.OnOpenedChange doesn't send ClosedByMe Notifys to other Players
            lock (fabricatorsInUseByRemotePlayers)
            {
                foreach (string _fabricatorGuid in _craftersToRemove)
                {
                    fabricatorsInUseByRemotePlayers.Remove(_fabricatorGuid);
                }
            }
        }

    }

    internal class CraftingUsageInfo
    {
        public string PlayerName { get; set; }
        public System.DateTime StartTime { get; set; }
    }
}
