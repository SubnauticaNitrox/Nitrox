using System;
using UnityEngine;
using System.Collections.Generic;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
<<<<<<< HEAD
<<<<<<< HEAD
    [Serializable]
    [ProtoContract]
    public class PDAStateData
    {
=======
    [ProtoContract]
    public class PDAStateData
    {

>>>>>>> c7606c2... Changes Requested
=======
    [Serializable]
    [ProtoContract]
    public class PDAStateData
    {
>>>>>>> 5ccaa74... Server Request the object are Serializable
        [ProtoMember(1)]
        public PDADataPartial PDADataPartial { get; set; }
        [ProtoMember(2)]
        public PDADataComplete PDADataComplete { get; set; }
        [ProtoMember(3)]
        public PDADataEncyclopedia PDADataEnciclopedia { get; set; }
        [ProtoMember(4)]
        public PDADataknownTech PDADataknownTech { get; set; }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> c7606c2... Changes Requested
=======
    [Serializable]
>>>>>>> 5ccaa74... Server Request the object are Serializable
    [ProtoContract]
    public class PDADataPartial
    {
        public PDADataPartial()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        private List<PDAEntry> pdaEntryPartial = new List<PDAEntry>();

        [ProtoMember(1)]
        public List<PDAEntry> GetList
        {
            get
            {
                lock (pdaEntryPartial)
                {
                    return pdaEntryPartial;
                }
            }
            set { pdaEntryPartial = value; }
        }

        public void Add(PDAEntry data)
        {
            lock (pdaEntryPartial)
            {
                pdaEntryPartial.Add(data);
            }
        }

        public bool Contain(TechType data, out PDAEntry itenout)
        {
            lock (pdaEntryPartial)
            {
                foreach (PDAEntry item in pdaEntryPartial)
                {
                    if (item.TechType == data)
                    {
                        itenout = item;
                        return true;
                    }
                }
            }

            itenout = null;
            return false;
        }

        public void Delete(PDAEntry data)
        {
            lock (pdaEntryPartial)
            {
                pdaEntryPartial.Remove(data);
            }
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> c7606c2... Changes Requested
=======
    [Serializable]
>>>>>>> 5ccaa74... Server Request the object are Serializable
    [ProtoContract]
    public class PDADataComplete
    {
        public PDADataComplete()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        private List<TechType> pdaEntryComplete = new List<TechType>();

        [ProtoMember(1)]
        public List<TechType> GetList
        {
            get
            {
                lock (pdaEntryComplete)
                {
                    return pdaEntryComplete;
                }
            }
            set { pdaEntryComplete = value; }
        }

        public void Add(TechType data)
        {
            lock (pdaEntryComplete)
            {
                pdaEntryComplete.Add(data);
            }
        }
        public void Delete(TechType data)
        {
            lock (pdaEntryComplete)
            {
                pdaEntryComplete.Remove(data);
            }
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> c7606c2... Changes Requested
=======
    [Serializable]
>>>>>>> 5ccaa74... Server Request the object are Serializable
    [ProtoContract]
    public class PDADataEncyclopedia
    {
        public PDADataEncyclopedia()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        public List<string> PDAEncyclopediaEntry = new List<string>();

        [ProtoMember(1)]
        public List<string> GetList
        {
            get
            {
                lock (PDAEncyclopediaEntry)
                {
                    return PDAEncyclopediaEntry;
                }
            }
            set { PDAEncyclopediaEntry = value; }
        }

        public void Add(string key)
        {
            lock (PDAEncyclopediaEntry)
            {
                PDAEncyclopediaEntry.Add(key);
            }
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    [Serializable]
=======
>>>>>>> c7606c2... Changes Requested
=======
    [Serializable]
>>>>>>> 5ccaa74... Server Request the object are Serializable
    [ProtoContract]
    public class PDADataknownTech
    {
        public PDADataknownTech()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        private List<TechType> knownTech = new List<TechType>();

        [ProtoMember(1)]
        public List<TechType> GetList
        {
            get
            {
                lock (knownTech)
                {
                    return knownTech;
                }
            }
            set { knownTech = value; }
        }

        public void Add(TechType key)
        {
            lock (knownTech)
            {
                knownTech.Add(key);
            }
        }
    }
}

