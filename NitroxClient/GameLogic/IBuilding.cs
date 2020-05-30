using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.GameLogic
{
    public interface IBuilding
    {
        bool InitialSyncActive { set; }

        void ConstructNewBasePiece(BasePiece basePiece);
        void ChangeConstructAmount(NitroxId id, float constructionAmount);
        void FinishConstruction(NitroxId id);
        void DeconstructBasePiece(NitroxId id);
        void FinishDeconstruction(NitroxId id);
    }
}
