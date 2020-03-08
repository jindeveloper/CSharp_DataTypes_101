using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace CSharp_DataTypes_101
{

    public struct TypeInformation
    {
        public string Alias_Name { get; set; }
        public string DotNetType { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public TypeInformationCategory TypeInformationCategory { get; set; }

        public override string ToString()
        {
            return string.Format("Name Or Alias: {0}{1}.NET Type: {2}{3}Min Value : {4}{5}Max Value: {6}{7}",
                                    this.Alias_Name,
                                    Environment.NewLine,
                                    this.DotNetType,
                                    Environment.NewLine,
                                    this.MinValue,
                                    Environment.NewLine,
                                    this.MaxValue,
                                    Environment.NewLine);
        }

        public static string GetOutput(List<TypeInformation> typeInformations)
        {
            StringBuilder output = new StringBuilder();

            foreach (var item in typeInformations)
            {
                output.AppendLine(item.ToString());
            }

            return output.ToString();
        }
    }

    [Flags]
    public enum TypeInformationCategory : byte
    {
        IntegerPrimitiveTypes,
        FloatingPrimitiveTypes,
        CharacterPrimitiveTypes,
        BooleanPrimitiveTypes
    }

    public static class StringOutputHelper
    {
        public static string WriteString(string[] values, char firstChar)
        {
            StringBuilder output = new StringBuilder(values.Length);

            foreach (var value in values)
            {
                output.AppendLine($"{firstChar}{value}");
            }

            return output.ToString();
        }
    }

    public class UnitTest_Csharp_DataTypes
    {
        private readonly ITestOutputHelper _output;

        public UnitTest_Csharp_DataTypes(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void UnitTest_GetAll_PrimitiveTypes()
        {
            string assemblyFullName = "System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";

            Assembly assembly = Assembly.Load(assemblyFullName);

            IEnumerable<TypeInfo> primitiveTypes =
                assembly.DefinedTypes.Where(definedType => definedType.IsPrimitive && definedType != typeof(IntPtr) && definedType != typeof(UIntPtr));

            int totalPrimitiveTypes = primitiveTypes.Count();

            string[] dataPrimitiveTypes = new string[totalPrimitiveTypes];

            using (var provider = new CSharpCodeProvider())
            {
                dataPrimitiveTypes = primitiveTypes.Select(x => provider.GetTypeOutput(new CodeTypeReference(x))).ToArray();
            }
            this._output.WriteLine(StringOutputHelper.WriteString(dataPrimitiveTypes, '•'));

            Assert.True(totalPrimitiveTypes == 12);
        }

        [Fact]
        public void UnitTest_GetAll_PrimitiveTypes_With_Information()
        {
            string assemblyFullName = "System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";

            Assembly assembly = Assembly.Load(assemblyFullName);

            IEnumerable<TypeInfo> primitiveTypes =
                assembly.DefinedTypes.Where(definedType => definedType.IsPrimitive && definedType != typeof(IntPtr) && definedType != typeof(UIntPtr));

            var attr = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static;

            var typeInformation = new List<TypeInformation> { };

            using (var provider = new CSharpCodeProvider())
            {
                foreach (var primitiveType in primitiveTypes)
                {
                    switch (primitiveType.FullName)
                    {
                        case "System.SByte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UInt64":
                            typeInformation.Add(new TypeInformation
                            {
                                Alias_Name = provider.GetTypeOutput(new CodeTypeReference(primitiveType)),
                                DotNetType = primitiveType.FullName,
                                MinValue = (primitiveType.GetField("MinValue", attr)).GetValue(primitiveType).ToString(),
                                MaxValue = (primitiveType.GetField("MaxValue", attr)).GetValue(primitiveType).ToString(),
                                TypeInformationCategory = TypeInformationCategory.IntegerPrimitiveTypes
                            });
                            break;
                        case "System.Single":
                        case "System.Double":
                        case "System.Decimal":
                            typeInformation.Add(new TypeInformation
                            {
                                Alias_Name = provider.GetTypeOutput(new CodeTypeReference(primitiveType)),
                                DotNetType = primitiveType.FullName,
                                MinValue = (primitiveType.GetField("MinValue", attr)).GetValue(primitiveType).ToString(),
                                MaxValue = (primitiveType.GetField("MaxValue", attr)).GetValue(primitiveType).ToString(),
                                TypeInformationCategory = TypeInformationCategory.FloatingPrimitiveTypes
                            });
                            break;
                        case "System.Boolean":
                            typeInformation.Add(new TypeInformation
                            {
                                Alias_Name = provider.GetTypeOutput(new CodeTypeReference(primitiveType)),
                                DotNetType = primitiveType.FullName,
                                MinValue = (primitiveType.GetField("FalseString", attr)).GetValue(primitiveType).ToString(),
                                MaxValue = (primitiveType.GetField("TrueString", attr)).GetValue(primitiveType).ToString(),
                                TypeInformationCategory = TypeInformationCategory.FloatingPrimitiveTypes
                            });
                            break;
                        case "System.Char":
                            typeInformation.Add(new TypeInformation
                            {
                                Alias_Name = provider.GetTypeOutput(new CodeTypeReference(primitiveType)),
                                DotNetType = primitiveType.FullName,
                                MinValue = (primitiveType.GetField("MinValue", attr)).GetValue(primitiveType).ToString(),
                                MaxValue = (primitiveType.GetField("MaxValue", attr)).GetValue(primitiveType).ToString(),
                                TypeInformationCategory = TypeInformationCategory.CharacterPrimitiveTypes
                            });
                            break;
                        default:
                            break;
                    }
                }
            }

            Assert.True(primitiveTypes.Count() == typeInformation.Count());
            this._output.WriteLine(TypeInformation.GetOutput(typeInformation));
        }
       
        [Fact]
        public void UnitTest_Get_BuiltIn_ReferenceType_Base_Of_All_Classes()
        {
            string assemblyFullName = "System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";

            Assembly assembly = Assembly.Load(assemblyFullName);

            TypeInfo baseOfAllClasses = assembly.DefinedTypes.FirstOrDefault(x => x.BaseType == null && x.IsClass && x.IsAnsiClass);

            Assert.NotNull(baseOfAllClasses);

            Assert.IsType<object>(Activator.CreateInstance(baseOfAllClasses));
        }
    }
}
