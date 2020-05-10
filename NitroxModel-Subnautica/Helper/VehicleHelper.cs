using UnityEngine;

namespace NitroxModel_Subnautica.Helper
{
    public static class VehicleHelper
    {
        public static Vector3[] GetDefaultColours(TechType techType)
        {
            switch (techType)
            {
                case TechType.Seamoth:
                case TechType.Exosuit:
                    return new Vector3[] {
                        new Vector3(0f, 0f, 1f),            //_Color
                        new Vector3(0f, 0f, 0f),            //_Tint
                        new Vector3(0f, 0f, 1f),            //_Color
                        new Vector3(0.577f, 0.447f, 0.604f),//_Color2
                        new Vector3(0.114f, 0.729f, 0.965f) //_Color3
                    };

                case TechType.Cyclops:
                    return new Vector3[]
                    {
                        new Vector3(1f, 0f, 1f),
                        new Vector3(0.6f, 0.4f, 0.4f),
                        new Vector3(0.1f, 0.8f, 1f),
                        new Vector3(0f, 0f, 0f)
                    };

                default:
                    return GetPrimalDefaultColours();
            }
        }

        public static Vector3[] GetPrimalDefaultColours()
        {
            return new Vector3[]
            {
                new Vector3(0f, 0f, 1f)
            };
        }

        public static bool IsVehicle(TechType techtype)
        {
            switch (techtype)
            {
                case TechType.Seamoth:
                case TechType.Exosuit:
                case TechType.Cyclops:
                    return true;

                default:
                    return false;
            }
        }
    }
}
