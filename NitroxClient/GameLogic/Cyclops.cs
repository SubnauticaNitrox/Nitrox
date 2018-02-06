using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly IPacketSender packetSender;

        /// <summary>
        /// We need to use a different random generator for each Cyclops. One Cyclops could trigger an event, whereas another can trigger a
        /// different event. If the clients recorded them at the same time, it would likely de-sync the generator.
        /// Synced random numbers are not actually necessary, but it does improve quality of gameplay. If we can't consistently generate
        /// damage points and fires at synced spots, laggy clients would see the points jump around the ship when the client receives the "correct" positions
        /// of damage/fires and has to remove/add points to match the server.
        /// </summary>
        private static readonly Dictionary<string, CyclopsRandomNumberHolder> randomNumberGenerators = new Dictionary<string, CyclopsRandomNumberHolder>();

        public Cyclops(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ToggleInternalLight(string guid, bool isOn)
        {
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(guid, isOn);
            packetSender.Send(packet);
        }

        public void ToggleFloodLights(string guid, bool isOn)
        {
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(guid, isOn);
            packetSender.Send(packet);
        }

        public void BeginSilentRunning(string guid)
        {
            CyclopsBeginSilentRunning packet = new CyclopsBeginSilentRunning(guid);
            packetSender.Send(packet);
        }

        public void ChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(guid, mode);
            packetSender.Send(packet);
        }

        public void ToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(guid, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void ActivateHorn(string guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(guid);
            packetSender.Send(packet);
        }

        public void ActivateShield(string guid)
        {
            CyclopsActivateShield packet = new CyclopsActivateShield(guid);
            packetSender.Send(packet);
        }

        /// <summary>
        /// Get the next "random" number.
        /// </summary>
        public int GetNextRandomNumber(SubRoot subRoot, int min, int max)
        {
            string subRootGuid = GuidHelper.GetGuid(subRoot.gameObject);

            CyclopsRandomNumberHolder rngHolder;
            int serverSeed = Multiplayer.Logic.ServerTime.GetSeedFromCurrentServerTime();
            if (!randomNumberGenerators.TryGetValue(subRootGuid, out rngHolder))
            {
                rngHolder = new CyclopsRandomNumberHolder() { Seed = serverSeed, RandomGenerator = new System.Random(serverSeed) };
                randomNumberGenerators.Add(subRootGuid, rngHolder);
            }

            if (rngHolder.Seed != serverSeed)
            {
                rngHolder.Seed = serverSeed;
                rngHolder.RandomGenerator = new System.Random(serverSeed);
            }

            return rngHolder.RandomGenerator.Next(min, max);
        }

        /// <summary>
        /// <para>
        /// At first I thought "Oh look, they made it easy for me to figure out where the fire is by referencing the <see cref="CyclopsRooms"/> enum!"
        /// Nope, it's just a reference to a list of <see cref="Transform"/> that are randomly chosen and activated at <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
        /// , which then creates/activates a <see cref="Fire"/> located at the selected <see cref="Transform"/>. 
        /// </para>
        /// <para>
        /// The only saving grace is that <see cref="Fire"/> does not spawn other <see cref="Fire"/>s when it "grows", like I feared it would. 
        /// Instead, it scales the transform of the fire up as its <see cref="Fire.livemixin.health"/> increases. The added fire effects are the responsibility of 
        /// <see cref="Fire.fireFX"/>, not the <see cref="Fire"/> itself.
        /// </para>
        /// <para>
        /// I am entirely convinced every single aspect of the Cyclops has a random number generator attached to it.
        /// </para>
        /// </summary>
        public void FireCreated(SubFire subFire, DamageInfo damageInfo, CyclopsRooms room)
        {
            // The good news: We know where the new fire is. The bad news: We don't know which fire it is. The good news: We know the indexes are identical, making
            // guid syncing unnecessary.
            //CyclopsFireDamage packet = new CyclopsFireDamage(subRootGuid, subFire.subRoot.damageManager.subLiveMixin.health, subFire.GetAllFires().ToArray());
            //packetSender.Send(packet);

            Log.Debug("SubFire.OnTakeDamage called Cyclops. FireCreated() for room " + room.ToString());
        }

        /// <summary>
        /// After the damage is calculated and all of the <see cref="CyclopsDamagePoint"/>s and <see cref="Fire"/>s have been set, it's packaged and sent out to the server.
        /// </summary>
        public void OnTakeDamage(SubRoot subRoot, DamageInfo info)
        {
            string subGuid = GuidHelper.GetGuid(subRoot.gameObject);

            SerializableDamageInfo damageInfo = null;

            if (info != null)
            {
                // Source of the damage. Used if the damage done to the Cyclops was not calculated on other clients. Currently it's just used to figure out what sounds and
                // visual effects should be used.
                SerializableDamageInfo serializedDamageInfo = new SerializableDamageInfo()
                {
                    OriginalDamage = info.originalDamage,
                    Damage = info.damage,
                    Position = info.position,
                    Type = info.type,
                    DealerGuid = info.dealer != null ? GuidHelper.GetGuid(info.dealer) : string.Empty
                };
            }

            int[] damagePointIndexes = GetActiveDamagePointIndexes(subRoot).ToArray();
            SerializableRoomFire[] firePointIndexes = GetActiveRoomFireIndexes(subRoot.GetComponent<SubFire>()).ToArray();

            CyclopsDamage packet = new CyclopsDamage(subGuid, subRoot.GetComponent<LiveMixin>().health, subRoot.damageManager.subLiveMixin.health, subRoot.GetComponent<SubFire>().liveMixin.health, damagePointIndexes, firePointIndexes, damageInfo);
            packetSender.Send(packet);
        }

        /// <summary>
        /// Spawn a new Cyclops. <paramref name="subRoot"/> will not have its position or rotation declared upon spawning. You must pull those values from
        /// elsewhere.
        /// </summary>
        public void SpawnNew(GameObject subRoot, Vector3 position, Quaternion rotation)
        {
            string guid = GuidHelper.GetGuid(subRoot.gameObject);
            VehicleMovement packet = new VehicleMovement(packetSender.PlayerId, position, Vector3.zero, rotation, Vector3.zero, TechType.Cyclops, guid, 0, 0, false);
            packetSender.Send(packet);
        }

        public void ExternalDamagePointRepaired(SubRoot subRoot)
        {
            OnTakeDamage(subRoot, null);
        }

        /// <summary>
        /// Get all of the index locations of <see cref="CyclopsDamagePoint"/>s in <see cref="CyclopsExternalDamageManager.damagePoints"/> 
        /// with <see cref="GameObject.activeSelf"/> set as true.
        /// </summary>
        public IEnumerable<int> GetActiveDamagePointIndexes(SubRoot subRoot)
        {
            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i].gameObject.activeSelf)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Get all of the index locations of all the fires on the <see cref="SubRoot"/>. <see cref="SubFire.RoomFire.spawnNodes"/> contains
        /// a static list of all possible fire nodes.
        /// </summary>
        public IEnumerable<SerializableRoomFire> GetActiveRoomFireIndexes(SubFire subFire)
        {
            string subRootGuid = GuidHelper.GetGuid(subFire.subRoot.gameObject);
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in roomFires)
            {
                // There be fires here.
                if (roomFire.Value.fireValue > 0)
                {
                    List<SerializableFireNode> activeNodes = new List<SerializableFireNode>();
                    for (int i = 0; i < roomFire.Value.spawnNodes.Length; i++)
                    {
                        // Is this a fire? Copied from SubFire.CreateFire(SubFire.RoomFire startInRoom)
                        if (roomFire.Value.spawnNodes[i].childCount > 0)
                        {
                            activeNodes.Add(new SerializableFireNode()
                            {
                                NodeIndex = i,
                                FireCount = roomFire.Value.spawnNodes[i].childCount
                            });
                        }
                    }

                    SerializableRoomFire newRoomFire = new SerializableRoomFire()
                    {
                        Room = roomFire.Key,
                        ActiveRoomFireNodes = activeNodes.ToArray()
                    };

                    yield return newRoomFire;
                }
            }
        }

        /// <summary>
        /// Send a notice to the server to update the damage points on a Cyclops.
        /// 
        /// This method is a stand-in until we agree on how we're going to synchronize the random number generator on the server and clients. Right now when a point is
        /// generated, it's not done by determining what attacked the Cyclops because the logic for that is 
        /// </summary>
        private void UpdateExternalDamagePoints(SubRoot subRoot, string caller)
        {
            /*
            string guid = GuidHelper.GetGuid(subRoot.gameObject);
            Validate.NotNull(subRoot.damageManager, "Cyclops Guid: " + guid + " has a null 'CyclopsExternalDamageManager'!");

            // We need all of the active damage point guids to fully sync the damage between computers. SubRoot.damageManager.CreatePoint() is going to create
            // its own random points of damage on all clients as the Cyclops is damaged. When the "winning" damage point list is chosen by the server and redistributed,
            // we will need a list of all of the points that should be active, otherwise each client will have a damage points randomly located.
            List<int> damagePointIndexesList = new List<int>();
            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i].gameObject.activeSelf)
                {
                    damagePointIndexesList.Add(i);
                }
            }

            Log.Debug("UpdateExternalDamagePoints called by '" + caller + "' with active indexes: " + string.Join(", ", damagePointIndexesList.Select(x => x.ToString()).ToArray()));

            // We include the health because the "host" computer that called for an update on the damage points is now the one who dictates the overall
            // state of the Cyclops. Damage points are the only way to repair a Cyclops, so reporting the health is only meant to satisfy the logic checks
            // the client does to make sure there are the correct number of damage points for the Cyclop's current health. If there's too many or too few, it will 
            // remove/add damage points. This made for an incredibly hard to track and irritating bug. It has to be the LiveMixin located in the DamageManager. The
            // LiveMixin on SubRoot will not work.
            CyclopsDamage packet = new CyclopsDamage(guid, subRoot.damageManager.subLiveMixin.health, damagePointIndexesList.ToArray(), GetActiveRoomFireIndexes(subRoot.GetComponent<SubFire>()).ToArray());
            packetSender.Send(packet);
            */
        }

        private struct CyclopsRandomNumberHolder
        {
            public int Seed { get; set; }
            public System.Random RandomGenerator { get; set; }
        }
    }
}
