using NitroxModel.DataStructures.Unity;

namespace NitroxModel_Subnautica.Helper
{
    public static class VehicleHelper
    {
        public static NitroxVector3[] GetDefaultColours(TechType techType)
        {
            switch (techType)
            {
                case TechType.Seamoth:
                    return new[] {
                        new NitroxVector3(0f, 0f, 1f),
                        new NitroxVector3(0f, 0f, 0f),
                        new NitroxVector3(0f, 0f, 1f),
                        new NitroxVector3(0.577f, 0.447f, 0.604f),
                        new NitroxVector3(0.114f, 0.729f, 0.965f)
                    };
                case TechType.Exosuit:
                    return new[] {
                        new NitroxVector3(0f, 0f, 1f),            //_Color
                        new NitroxVector3(0f, 0f, 0f),            //_Tint
                        new NitroxVector3(0f, 0f, 1f),            //_Color
                        new NitroxVector3(0.577f, 0.447f, 0.604f),//_Color2
                        new NitroxVector3(0.114f, 0.729f, 0.965f) //_Color3
                    };

                case TechType.Cyclops:
                    return new[]
                    {
                        new NitroxVector3(1f, 0f, 1f),
                        new NitroxVector3(0.6f, 0.4f, 0.4f),
                        new NitroxVector3(0.1f, 0.8f, 1f),
                        new NitroxVector3(0f, 0f, 0f)
                    };

                default:
                    return GetPrimalDefaultColours();
            }
        }

        public static NitroxVector3[] GetPrimalDefaultColours()
        {
            return new[]
            {
                new NitroxVector3(0f, 0f, 1f)
            };
        }

        public static bool IsVehicle(TechType techtype)
        {
            switch (techtype)
            {
                case TechType.Seamoth:
                case TechType.Exosuit:
                case TechType.Cyclops:
                case TechType.RocketBase:
                    return true;

                default:
                    return false;
            }
        }
    }
}
