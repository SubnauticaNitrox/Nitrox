using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public class CellChanges
    {
        public List<AbsoluteEntityCell> Added { get; set; }
        public List<AbsoluteEntityCell> Removed { get; set; }

        public void Add(AbsoluteEntityCell absoluteEntityCell)
        {
            Added.Add(absoluteEntityCell);
        }

        public void Remove(AbsoluteEntityCell absoluteEntityCell)
        {
            Removed.Add(absoluteEntityCell);
        }

        public CellChanges (IEnumerable<AbsoluteEntityCell> added, IEnumerable<AbsoluteEntityCell> removed)
        {
            Added = new List<AbsoluteEntityCell>(added);
            Removed = new List<AbsoluteEntityCell>(removed);
        }

        public CellChanges()
        {
            Added = new List<AbsoluteEntityCell>();
            Removed = new List<AbsoluteEntityCell>();
        }
    }
}
