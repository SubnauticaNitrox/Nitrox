using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nitrox.Model.Extensions;

public static class ObjectExtensions
{
    public static string GetStateAsTextForComparison(this object? obj)
    {
        if (obj == null)
        {
            return "<null>";
        }
        ObjTextBuilderContext context = new();
        context.AddObject(obj);
        return context.ToString();
    }

    private record ObjTextBuilderContext
    {
        private string Spacing => new(' ', Indent * 2);
        private int Indent { get; set; } = 1;
        private StringBuilder Builder { get; } = new();

        public override string ToString() => Builder.ToString();

        public void AddObject(object o)
        {
            Type objType = o.GetType();
            Builder.AppendLine($"OBJ: {objType.FullName}");
            AddFields(o, objType);
            AddProperties(o, objType);
            AddUnityComponents(o, objType);
        }

        private void AddFields(object o, Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => f.FieldType.Name).ThenBy(f => f.Name).ToArray();
            if (fields.Length > 0)
            {
                Builder.AppendLine($"{Spacing}Fields ({fields.Length}):");
                Indent++;
                foreach (FieldInfo field in fields)
                {
                    AddKeyValue(field.Name, field.GetValue(o), field.FieldType);
                }
                Indent--;
            }
        }

        private void AddProperties(object o, Type type)
        {
            PropertyInfo[] properties = type.GetProperties().OrderBy(p => p.PropertyType.Name).ThenBy(p => p.Name).ToArray();
            if (properties.Length > 0)
            {
                Builder.AppendLine($"{Spacing}Properties ({properties.Length}):");
                Indent++;
                foreach (PropertyInfo property in properties)
                {
                    AddKeyValue(property.Name, property.CanRead ? property.GetValue(o) : "<no getter>", property.PropertyType);
                }
                Indent--;
            }
        }

        private void AddKeyValue(string key, object? value, Type valueType)
        {
            Builder.Append(Spacing)
                   .Append(key);
            if (valueType.IsArray)
            {
                Builder.Append(" (").Append((value as object[])?.Length ?? 0).Append(")");
            }
            Builder.Append(" [")
                   .Append(valueType.FullName)
                   .Append("]: ");
            Builder.Append(GetAsTextWithIndentedNewLines(value))
                   .AppendLine();
        }

        private void AddUnityComponents(object o, Type type)
        {
            if (type.FullName != "UnityEngine.GameObject")
            {
                return;
            }
            Type? unityBaseComponentType = Type.GetType("UnityEngine.Component, UnityEngine.CoreModule");
            if (unityBaseComponentType == null)
            {
                return;
            }
            MethodInfo? getComponents = type.GetMethod("GetComponents", [typeof(Type)]);
            if (getComponents == null)
            {
                throw new Exception($"Could not find GetComponents method on {type.FullName}");
            }
            object[] components = getComponents.Invoke(o, [unityBaseComponentType]) as object[];
            if (components == null || components.Length < 1)
            {
                return;
            }

            Builder.Append(Spacing)
                   .AppendLine("Unity Components:");
            Indent++;
            foreach (object component in components)
            {
                Builder.Append(Spacing)
                       .AppendLine(component.GetType().FullName);
                Indent++;
                Type? componentType = component.GetType();
                AddFields(component, componentType);
                AddProperties(component, componentType);
                Indent--;
            }
            Indent--;
        }

        private string GetAsTextWithIndentedNewLines(object? o)
        {
            if (o == null)
            {
                return "<null>";
            }
            if (o is not string str)
            {
                str = o switch
                {
                    IEnumerable<string> strings => string.Join("\n", strings),
                    _ => o.ToString()
                };
            }
            if (string.IsNullOrWhiteSpace(str))
            {
                return "<empty>";
            }

            if (str.Contains("\n"))
            {
                str = str.Replace("\r\n", "\n");
                Indent++;
                str = $"{Environment.NewLine}{Spacing}{str.Replace("\n", $"{Environment.NewLine}{Spacing}").Trim()}";
                Indent--;
            }
            return str;
        }
    }
}
