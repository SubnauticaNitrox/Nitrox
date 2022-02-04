using System;
using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxModel.Helper;

namespace NitroxTest.Model
{
    [TestClass]
    public class ReflectTest
    {
        [TestMethod]
        public void Method()
        {
            // Get static method.
            MethodInfo staticMethod = Reflect.Method(() => AbusedClass.StaticMethodReturnsInt());
            staticMethod.Should().NotBeNull();
            staticMethod.ReturnType.Should().Be<int>();
            staticMethod.Name.Should().BeEquivalentTo(nameof(AbusedClass.StaticMethodReturnsInt));
            staticMethod.Invoke(null, Array.Empty<object>());
            // Extra check for method with parameters, just to be safe.
            staticMethod = Reflect.Method(() => AbusedClass.StaticMethodHasParams("", null));
            staticMethod.Should().NotBeNull();
            staticMethod.ReturnType.Should().Be<string>();
            staticMethod.Name.Should().BeEquivalentTo(nameof(AbusedClass.StaticMethodHasParams));
            staticMethod.GetParameters().Should().OnlyHaveUniqueItems();
            staticMethod.GetParameters()[0].Name.Should().BeEquivalentTo("myValue");
            staticMethod.GetParameters()[0].ParameterType.Should().Be<string>();
            staticMethod.GetParameters()[1].ParameterType.Should().Be<Process>();
            staticMethod.Invoke(null, new[] { "hello, reflection", (object)null }).Should().BeEquivalentTo("hello, reflection");

            // Get instance method.
            MethodInfo instanceMethod = Reflect.Method((AbusedClass t) => t.Method());
            instanceMethod.Should().NotBeNull();
            instanceMethod.ReturnType.Should().Be<int>();
            instanceMethod.Name.Should().BeEquivalentTo(nameof(AbusedClass.Method));
        }

        [TestMethod]
        public void Field()
        {
            // Get static field.
            FieldInfo staticField = Reflect.Field(() => AbusedClass.StaticField);
            staticField.Name.Should().BeEquivalentTo(nameof(AbusedClass.StaticField));
            staticField.FieldType.Should().Be<int>();
            // Get instance field.
            FieldInfo instanceField = Reflect.Field((AbusedClass t) => t.InstanceField);
            instanceField.Name.Should().BeEquivalentTo(nameof(AbusedClass.InstanceField));
            instanceField.FieldType.Should().Be<int>();
        }

        [TestMethod]
        public void Property()
        {
            // Get static property.
            PropertyInfo staticProperty = Reflect.Property(() => AbusedClass.StaticProperty);
            staticProperty.Name.Should().BeEquivalentTo(nameof(AbusedClass.StaticProperty));
            staticProperty.PropertyType.Should().Be<int>();
            // Get instance property.
            PropertyInfo instanceProperty = Reflect.Property((AbusedClass t) => t.InstanceProperty);
            instanceProperty.Name.Should().BeEquivalentTo(nameof(AbusedClass.InstanceProperty));
            instanceProperty.PropertyType.Should().Be<int>();
        }

        [TestMethod]
        public void Constructor()
        {
            ConstructorInfo method = Reflect.Constructor(() => new KeyBindingManager());
            method.DeclaringType.Should().Be<KeyBindingManager>();
        }
        
        private class AbusedClass
        {
            public static readonly int StaticReadOnlyField = 1;
            public static int StaticField = 2;
            public int InstanceField = 3;
            public static int StaticProperty { get; set; } = 4;
            public int InstanceProperty { get; set; } = 5;

            public static int StaticMethodReturnsInt()
            {
                return 2;
            }

            public static string StaticMethodHasParams(string myValue, Process process)
            {
                return myValue;
            }

            public int Method()
            {
                return 1;
            }
        }
    }
}