using System;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Test.Helper.Serialization;

public class NitroxAutoFaker<TType, TBinder> : AutoFaker<TType>
    where TType : class
    where TBinder : NitroxAutoBinderBase
{
    public NitroxAutoFaker(TBinder binder) : this(new Dictionary<Type, Type[]>(), binder) { }

    public NitroxAutoFaker(Dictionary<Type, Type[]> subtypesByBaseType, TBinder binder)
    {
        Configure(newBinder =>
        {
            newBinder.WithBinder(binder)
                     .WithOverride(new NitroxTechTypeOverride());

            if (subtypesByBaseType.Values.Count != 0)
            {
                int highestAbstractObjectCount = subtypesByBaseType.Values.Max(objects => objects.Length);
                newBinder.WithRepeatCount(Math.Max(2, highestAbstractObjectCount));
            }
        });
    }

    private class NitroxTechTypeOverride : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType == typeof(NitroxTechType);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            context.Instance = new NitroxTechType(context.Faker.PickRandom<TechType>().ToString());
        }
    }
}
