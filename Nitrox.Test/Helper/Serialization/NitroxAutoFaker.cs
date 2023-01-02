using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AutoBogus;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Test.Helper.Serialization;

public class NitroxAutoFaker<TType, TBinder> : AutoFaker<TType> where TType : class
                                                                where TBinder : NitroxAutoBinderBase
{
    public NitroxAutoFaker(Dictionary<Type, Type[]> subtypesByBaseType, TBinder binder) : base(binder)
    {
        Configure(config =>
        {
            config.WithBinder(binder)
                  .WithRecursiveDepth(1)
                  .WithTreeDepth(3)
                  .WithOverride(new NitroxTechTypeOverride())
                  .WithOverride(new ClassIdOverride());

            ConfigureAbstractRepeatCount(config, subtypesByBaseType, 1);
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

    private class ClassIdOverride : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            // ReSharper disable once StringLiteralTypo
            return context.GenerateType == typeof(string) && context.GenerateName.ToLower().Equals("classid");
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            context.Instance = context.Faker.Random.Guid().ToString();
        }
    }

    private void ConfigureAbstractRepeatCount(IAutoGenerateConfigBuilder builder, Dictionary<Type, Type[]> subtypesByBaseType, int defaultRepeatCount)
    {
        Dictionary<Type, int> repeatCountByType = new();
        HashSet<Type> visitedTypes = new();

        void TrySaveAbstractMemberCountFromType(Type type, out int containsAbstractMember)
        {
            containsAbstractMember = 0;

            if (visitedTypes.Contains(type))
            {
                return;
            }

            visitedTypes.Add(type);

            if (repeatCountByType.TryGetValue(type, out int cachedRepeatCount))
            {
                containsAbstractMember = cachedRepeatCount;
                return;
            }

            if (subtypesByBaseType.TryGetValue(type, out Type[] subtypes))
            {
                containsAbstractMember = Math.Max(containsAbstractMember, subtypes.Length);
            }

            foreach (KeyValuePair<string, MemberInfo> member in binder.GetMembers(type))
            {
                TrySaveAbstractMemberCountFromType(member.Value.GetMemberType(), out int newContainsAbstractMember);
                containsAbstractMember = Math.Max(containsAbstractMember, newContainsAbstractMember);
            }

            bool isCollection = typeof(ICollection).IsAssignableFrom(type);
            bool isDictionary = typeof(IDictionary).IsAssignableFrom(type);

            if (!isCollection && !isDictionary)
            {
                return;
            }

            if (isCollection)
            {
                Type collectionType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                TrySaveAbstractMemberCountFromType(collectionType, out int newContainsAbstractMember);
                containsAbstractMember = Math.Max(containsAbstractMember, newContainsAbstractMember);
            }
            else // Is Dictionary
            {
                TrySaveAbstractMemberCountFromType(type.GetGenericArguments()[0], out int newContainsAbstractMemberKey);
                containsAbstractMember = Math.Max(containsAbstractMember, newContainsAbstractMemberKey);

                TrySaveAbstractMemberCountFromType(type.GetGenericArguments()[1], out int newContainsAbstractMemberValue);
                containsAbstractMember = Math.Max(containsAbstractMember, newContainsAbstractMemberValue);
            }

            if (containsAbstractMember != 0)
            {
                repeatCountByType[type] = containsAbstractMember;
                containsAbstractMember = 0;
            }
        }

        TrySaveAbstractMemberCountFromType(typeof(TType), out int _);

        builder.WithRepeatCount(x => repeatCountByType.TryGetValue(x.GenerateType, out int repeatCount) ? repeatCount : defaultRepeatCount);
    }
}
