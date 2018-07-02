using System;
using UnityEngine;
using System.Collections.Generic;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class PDASaveData
    {

        [ProtoMember(1)]
        public PDADataPartial PDADataPartial { get; set; }
        [ProtoMember(2)]
        public PDADataComplete PDADataComplete { get; set; }
        [ProtoMember(3)]
        public PDADataEnciclopedia PDADataEnciclopedia { get; set; }
        [ProtoMember(4)]
        public PDADataknownTech PDADataknownTech { get; set; }

        public PDASaveData()
        {

        }
    }

    [Serializable]
    [ProtoContract]
    public class PDADataPartial
    {
        public PDADataPartial()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        private List<PDA_Entry> pdaEntryPartial = new List<PDA_Entry>();

        [ProtoMember(1)]
        public List<PDA_Entry> Serializable
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

        public void Add(PDA_Entry data)
        {
            lock (pdaEntryPartial)
            {
                pdaEntryPartial.Add(data);
            }
        }

        public bool Contain(TechType data, out PDA_Entry itenout)
        {
            lock (pdaEntryPartial)
            {
                foreach (PDA_Entry item in pdaEntryPartial)
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

        public void Delete(PDA_Entry data)
        {
            lock (pdaEntryPartial)
            {
                pdaEntryPartial.Remove(data);
            }
        }
    }
    [Serializable]
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
        public List<TechType> Serializable
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
    [Serializable]
    [ProtoContract]
    public class PDADataEnciclopedia
    {
        public PDADataEnciclopedia()
        {
            // For serialization purposes
        }

        [ProtoIgnore]
        public List<string> PDAEncyclopediaEntry = new List<string>();

        [ProtoMember(1)]
        public List<string> Serializable
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
    [Serializable]
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
        public List<TechType> Serializable
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

