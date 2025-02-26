﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class AttributeTests_Nullable : CSharpTestBase
    {
        // An empty project should not require System.Attribute.
        [Fact]
        public void EmptyProject_MissingAttribute()
        {
            var source = "";
            var comp = CreateEmptyCompilation(source, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion).WithLocation(1, 1)
                );
        }

        [Fact]
        public void ExplicitAttributeFromSource()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(byte a) { }
        public NullableAttribute(byte[] b) { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ExplicitAttributeFromMetadata()
        {
            var source0 =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(byte a) { }
        public NullableAttribute(byte[] b) { }
    }
}";
            var comp0 = CreateCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), references: new[] { ref0 }, parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ExplicitAttribute_MissingSingleByteConstructor()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(byte[] b) { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (5,34): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         public NullableAttribute(byte[] b) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "byte[] b").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(5, 34),
                // (10,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 19),
                // (10,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?[] y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 30));
        }

        [Fact]
        public void ExplicitAttribute_MissingConstructor()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute() { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (10,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 19),
                // (10,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?[] y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 30));
        }

        [Fact]
        public void ExplicitAttribute_MissingBothNeededConstructors()
        {
            var source =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(string[] b) { }
    }
}
class C
{
    static void F(object? x, object?[] y) { }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (5,34): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         public NullableAttribute(string[] b) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "string[] b").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(5, 34),
                // (10,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 19),
                // (10,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     static void F(object? x, object?[] y) { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?[] y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(10, 30));
        }

        [Fact]
        public void NullableAttribute_MissingByte()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public class Attribute
    {
    }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (3,11): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' context.
                //     object? F() => null;
                Diagnostic(ErrorCode.WRN_MissingNonNullTypesContextForAnnotation, "?").WithLocation(3, 11),
                // error CS0518: Predefined type 'System.Byte' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Byte").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Int32' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "").WithArguments("System.Int32").WithLocation(1, 1)
                );
        }

        [Fact]
        public void NullableAttribute_MissingAttribute()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Byte { }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (3,11): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' context.
                //     object? F() => null;
                Diagnostic(ErrorCode.WRN_MissingNonNullTypesContextForAnnotation, "?").WithLocation(3, 11),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1),
                // error CS0518: Predefined type 'System.Attribute' is not defined or imported
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound).WithArguments("System.Attribute").WithLocation(1, 1));
        }

        [Fact]
        public void NullableAttribute_StaticAttributeConstructorOnly()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct Byte { }
    public class Attribute
    {
        static Attribute() { }
        public Attribute(object o) { }
    }
}";
            var comp0 = CreateEmptyCompilation(source0, parseOptions: TestOptions.Regular7);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics(
                // (3,11): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' context.
                //     object? F() => null;
                Diagnostic(ErrorCode.WRN_MissingNonNullTypesContextForAnnotation, "?").WithLocation(3, 11),
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1),
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1),
                // error CS1729: 'Attribute' does not contain a constructor that takes 0 arguments
                Diagnostic(ErrorCode.ERR_BadCtorArgCount).WithArguments("System.Attribute", "0").WithLocation(1, 1));
        }

        [Fact]
        public void EmitAttribute_NoNullable()
        {
            var source =
@"public class C
{
    public object F = new object();
}";
            // C# 7.0: No NullableAttribute.
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular7);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNoNullableAttribute(field.GetAttributes());
                AssertNoNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
            // C# 8.0: NullableAttribute not included if no ? annotation.
            comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNoNullableAttribute(field.GetAttributes());
                AssertNoNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
        }

        [Fact]
        public void EmitAttribute_LocalFunctionConstraints()
        {
            var source = @"
class C
{
#nullable enable
    void M1()
    {
        local(new C());
        void local<T>(T t) where T : C?
        {
        }
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                Assert.NotNull(assembly.GetTypeByMetadataName("System.Runtime.CompilerServices.NullableAttribute"));
            });
        }

        [Fact]
        public void EmitAttribute_LocalFunctionConstraints_Nested()
        {
            var source = @"
interface I<T> { }
class C
{
#nullable enable
    void M1()
    {
        void local<T>(T t) where T : I<C?>
        {
        }
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                Assert.NotNull(assembly.GetTypeByMetadataName("System.Runtime.CompilerServices.NullableAttribute"));
            });
        }

        [Fact]
        public void EmitAttribute_LocalFunctionConstraints_NoAnnotation()
        {
            var source = @"
class C
{
#nullable enable
    void M1()
    {
        local(new C());
        void local<T>(T t) where T : C
        {
        }
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                Assert.NotNull(assembly.GetTypeByMetadataName("System.Runtime.CompilerServices.NullableAttribute"));
            });
        }

        [Fact]
        public void EmitAttribute_Module()
        {
            var source =
@"public class C
{
    public object? F = new object();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var assembly = module.ContainingAssembly;
                var type = assembly.GetTypeByMetadataName("C");
                var field = (FieldSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(field.GetAttributes());
                AssertNoNullableAttribute(module.GetAttributes());
                AssertAttributes(assembly.GetAttributes(),
                    "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                    "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                    "System.Diagnostics.DebuggableAttribute");
            });
        }

        [Fact]
        public void EmitAttribute_NetModule()
        {
            var source =
@"public class C
{
    public object? F = new object();
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,20): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public object? F = new object();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "F").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 20));
        }

        [Fact]
        public void EmitAttribute_NetModuleNoDeclarations()
        {
            var source = "";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            CompileAndVerify(comp, verify: Verification.Skipped, symbolValidator: module =>
            {
                AssertAttributes(module.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_BaseClass()
        {
            var source =
@"public class A<T>
{
}
public class B1 : A<object>
{
}
public class B2 : A<object?>
{
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("A`1");
                AssertNoNullableAttribute(type.GetAttributes());
                type = module.ContainingAssembly.GetTypeByMetadataName("B1");
                AssertNoNullableAttribute(type.BaseType().GetAttributes());
                AssertNullableAttribute(type.GetAttributes());
                type = module.ContainingAssembly.GetTypeByMetadataName("B2");
                AssertNoNullableAttribute(type.BaseType().GetAttributes());
                AssertNullableAttribute(type.GetAttributes());
            });
            var source2 =
@"class C
{
    static void F(A<object> x, A<object?> y)
    {
    }
    static void G(B1 x, B2 y)
    {
        F(x, x);
        F(y, y);
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (8,14): warning CS8620: Argument of type 'B1' cannot be used as an input of type 'A<object?>' for parameter 'y' in 'void C.F(A<object> x, A<object?> y)' due to differences in the nullability of reference types.
                //         F(x, x);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "x").WithArguments("B1", "A<object?>", "y", "void C.F(A<object> x, A<object?> y)").WithLocation(8, 14),
                // (9,11): warning CS8620: Argument of type 'B2' cannot be used as an input of type 'A<object>' for parameter 'x' in 'void C.F(A<object> x, A<object?> y)' due to differences in the nullability of reference types.
                //         F(y, y);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "y").WithArguments("B2", "A<object>", "x", "void C.F(A<object> x, A<object?> y)").WithLocation(9, 11));
        }

        [Fact]
        public void EmitAttribute_Interface_01()
        {
            var source =
@"public interface I<T>
{
}
public class A : I<object>
{
}
#nullable disable
public class AOblivious : I<object> { }
#nullable enable
public class B : I<object?>
{
}
";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                typeDef = GetTypeDefinitionByName(reader, "B");
                interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
            });
            var source2 =
@"class C
{
    static void F(I<object> x, I<object?> y) { }
#nullable disable
    static void FOblivious(I<object> x) { }
#nullable enable
    static void G(A x, B y, AOblivious z)
    {
        F(x, x);
        F(y, y);
        F(z, z);
        FOblivious(x);
        FOblivious(y);
        FOblivious(z);
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (9,14): warning CS8620: Argument of type 'A' cannot be used as an input of type 'I<object?>' for parameter 'y' in 'void C.F(I<object> x, I<object?> y)' due to differences in the nullability of reference types.
                //         F(x, x);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "x").WithArguments("A", "I<object?>", "y", "void C.F(I<object> x, I<object?> y)").WithLocation(9, 14),
                // (10,11): warning CS8620: Argument of type 'B' cannot be used as an input of type 'I<object>' for parameter 'x' in 'void C.F(I<object> x, I<object?> y)' due to differences in the nullability of reference types.
                //         F(y, y);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "y").WithArguments("B", "I<object>", "x", "void C.F(I<object> x, I<object?> y)").WithLocation(10, 11));
        }

        [Fact]
        public void EmitAttribute_Interface_02()
        {
            var source =
@"public interface I<T>
{
}
public class A : I<(object X, object Y)>
{
}
public class B : I<(object X, object? Y)>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(),
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])");
                typeDef = GetTypeDefinitionByName(reader, "B");
                interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                AssertAttributes(reader, interfaceImpl.GetCustomAttributes(),
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
            });

            var source2 =
@"class C
{
    static void F(I<(object, object)> a, I<(object, object?)> b)
    {
    }
    static void G(A a, B b)
    {
        F(a, a);
        F(b, b);
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (9,11): warning CS8620: Argument of type 'B' cannot be used as an input of type 'I<(object, object)>' for parameter 'a' in 'void C.F(I<(object, object)> a, I<(object, object?)> b)' due to differences in the nullability of reference types.
                //         F(b, b);
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInArgument, "b").WithArguments("B", "I<(object, object)>", "a", "void C.F(I<(object, object)> a, I<(object, object?)> b)").WithLocation(9, 11));

            var type = comp2.GetMember<NamedTypeSymbol>("A");
            Assert.Equal("I<(System.Object X, System.Object Y)>", type.Interfaces()[0].ToTestDisplayString());
            type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal("I<(System.Object X, System.Object? Y)>", type.Interfaces()[0].ToTestDisplayString());
        }

        [Fact]
        public void EmitAttribute_Constraint_Nullable()
        {
            var source =
@"public class A
{
}
public class C<T> where T : A?
{
}
public class D<T> where T : A
{
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "C`1");
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte)");
                typeDef = GetTypeDefinitionByName(reader, "D`1");
                typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte)");
            });

            var source2 =
@"class B : A { }
class Program
{
    static void Main()
    {
        new C<A?>();
        new C<A>();
        new C<B?>();
        new C<B>();
        new D<A?>(); // warning
        new D<A>();
        new D<B?>(); // warning
        new D<B>();
    }
}";
            var comp2 = CreateCompilation(new[] { source, source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp2.VerifyEmitDiagnostics(
                // (10,15): warning CS8627: The type 'A?' cannot be used as type parameter 'T' in the generic type or method 'D<T>'. Nullability of type argument 'A?' doesn't match constraint type 'A'.
                //         new D<A?>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A?").WithArguments("D<T>", "A", "T", "A?").WithLocation(10, 15),
                // (12,15): warning CS8627: The type 'B?' cannot be used as type parameter 'T' in the generic type or method 'D<T>'. Nullability of type argument 'B?' doesn't match constraint type 'A'.
                //         new D<B?>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "B?").WithArguments("D<T>", "A", "T", "B?").WithLocation(12, 15));

            comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyEmitDiagnostics(
                // (10,15): warning CS8627: The type 'A?' cannot be used as type parameter 'T' in the generic type or method 'D<T>'. Nullability of type argument 'A?' doesn't match constraint type 'A'.
                //         new D<A?>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A?").WithArguments("D<T>", "A", "T", "A?").WithLocation(10, 15),
                // (12,15): warning CS8627: The type 'B?' cannot be used as type parameter 'T' in the generic type or method 'D<T>'. Nullability of type argument 'B?' doesn't match constraint type 'A'.
                //         new D<B?>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "B?").WithArguments("D<T>", "A", "T", "B?").WithLocation(12, 15));

            var type = comp2.GetMember<NamedTypeSymbol>("C");
            Assert.Equal("A?", type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
            type = comp2.GetMember<NamedTypeSymbol>("D");
            Assert.Equal("A!", type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
        }

        // https://github.com/dotnet/roslyn/issues/29976: Test with [NonNullTypes].
        [Fact]
        public void EmitAttribute_Constraint_Oblivious()
        {
            var source =
@"public class A<T>
{
}
public class C<T> where T : A<object>
{
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular7);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "C`1");
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes());
            });

            var source2 =
@"class B1 : A<object?> { }
class B2 : A<object> { }
class Program
{
    static void Main()
    {
        new C<A<object?>>();
        new C<A<object>>();
        new C<A<object?>?>();
        new C<A<object>?>();
        new C<B1>();
        new C<B2>();
        new C<B1?>();
        new C<B2?>();
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics();

            var type = comp2.GetMember<NamedTypeSymbol>("C");
            Assert.Equal("A<System.Object>", type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
        }

        [WorkItem(27742, "https://github.com/dotnet/roslyn/issues/27742")]
        [Fact]
        public void EmitAttribute_Constraint_Nested()
        {
            var source =
@"public class A<T>
{
}
public class B<T> where T : A<object?>
{
}
public class C<T> where T : A<object>
{
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "B`1");
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                typeDef = GetTypeDefinitionByName(reader, "C`1");
                typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte)");
            });

            var source2 =
@"class Program
{
    static void Main()
    {
        new B<A<object?>>();
        new B<A<object>>(); // warning
        new C<A<object?>>(); // warning
        new C<A<object>>();
    }
}";
            var comp2 = CreateCompilation(new[] { source, source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            comp2.VerifyEmitDiagnostics(
                // (6,15): warning CS8627: The type 'A<object>' cannot be used as type parameter 'T' in the generic type or method 'B<T>'. Nullability of type argument 'A<object>' doesn't match constraint type 'A<object?>'.
                //         new B<A<object>>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A<object>").WithArguments("B<T>", "A<object?>", "T", "A<object>").WithLocation(6, 15),
                // (7,15): warning CS8627: The type 'A<object?>' cannot be used as type parameter 'T' in the generic type or method 'C<T>'. Nullability of type argument 'A<object?>' doesn't match constraint type 'A<object>'.
                //         new C<A<object?>>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A<object?>").WithArguments("C<T>", "A<object>", "T", "A<object?>").WithLocation(7, 15));

            comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (6,15): warning CS8627: The type 'A<object>' cannot be used as type parameter 'T' in the generic type or method 'B<T>'. Nullability of type argument 'A<object>' doesn't match constraint type 'A<object?>'.
                //         new B<A<object>>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A<object>").WithArguments("B<T>", "A<object?>", "T", "A<object>").WithLocation(6, 15),
                // (7,15): warning CS8627: The type 'A<object?>' cannot be used as type parameter 'T' in the generic type or method 'C<T>'. Nullability of type argument 'A<object?>' doesn't match constraint type 'A<object>'.
                //         new C<A<object?>>(); // warning
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterConstraint, "A<object?>").WithArguments("C<T>", "A<object>", "T", "A<object?>").WithLocation(7, 15));

            var type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal("A<System.Object?>!", type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
            type = comp2.GetMember<NamedTypeSymbol>("C");
            Assert.Equal("A<System.Object!>!", type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
        }

        // https://github.com/dotnet/roslyn/issues/29976: Test `class C<T> where T : class? { }`.
        [Fact]
        public void EmitAttribute_Constraint_TypeParameter()
        {
            var source =
@"public class C<T, U>
    where T : class
    where U : T?
{
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "C`2");
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[1]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                AssertAttributes(reader, constraint.GetCustomAttributes(), "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte)");
            });

            var source2 =
@"class Program
{
    static void Main()
    {
        new C<object?, string?>();
        new C<object?, string>();
        new C<object, string?>();
        new C<object, string>();
    }
}";
            var comp2 = CreateCompilation(new[] { source, source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            var expected = new[] {
                // (5,15): warning CS8634: The type 'object?' cannot be used as type parameter 'T' in the generic type or method 'C<T, U>'. Nullability of type argument 'object?' doesn't match 'class' constraint.
                //         new C<object?, string?>();
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterReferenceTypeConstraint, "object?").WithArguments("C<T, U>", "T", "object?").WithLocation(5, 15),
                // (6,15): warning CS8634: The type 'object?' cannot be used as type parameter 'T' in the generic type or method 'C<T, U>'. Nullability of type argument 'object?' doesn't match 'class' constraint.
                //         new C<object?, string>();
                Diagnostic(ErrorCode.WRN_NullabilityMismatchInTypeParameterReferenceTypeConstraint, "object?").WithArguments("C<T, U>", "T", "object?").WithLocation(6, 15)
            };

            comp2.VerifyEmitDiagnostics(expected);

            comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyEmitDiagnostics(expected);

            var type = comp2.GetMember<NamedTypeSymbol>("C");
            Assert.Equal("T?", type.TypeParameters[1].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString(true));
        }

        [Fact]
        public void EmitAttribute_MethodReturnType()
        {
            var source =
@"public class C
{
    public object? F() => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(method.GetReturnTypeAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_MethodParameters()
        {
            var source =
@"public class C
{
    public void F(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                AssertNullableAttribute(method.Parameters.Single().GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_ConstructorParameters()
        {
            var source =
@"public class C
{
    public C(object?[] c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = type.Constructors.Single();
                AssertNullableAttribute(method.Parameters.Single().GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_PropertyType()
        {
            var source =
@"public class C
{
    public object? P => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var property = (PropertySymbol)type.GetMembers("P").Single();
                AssertNullableAttribute(property.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_PropertyParameters()
        {
            var source =
@"public class C
{
    public object this[object x, object? y] => throw new System.NotImplementedException();
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var property = (PropertySymbol)type.GetMembers("this[]").Single();
                AssertNoNullableAttribute(property.Parameters[0].GetAttributes());
                AssertNullableAttribute(property.Parameters[1].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_OperatorReturnType()
        {
            var source =
@"public class C
{
    public static object? operator+(C a, C b) => null;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("op_Addition").Single();
                AssertNullableAttribute(method.GetReturnTypeAttributes());
                AssertNoNullableAttribute(method.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_OperatorParameters()
        {
            var source =
@"public class C
{
    public static object operator+(C a, object?[] b) => a;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("op_Addition").Single();
                AssertNoNullableAttribute(method.Parameters[0].GetAttributes());
                AssertNullableAttribute(method.Parameters[1].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_DelegateReturnType()
        {
            var source =
@"public delegate object? D();";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var method = module.ContainingAssembly.GetTypeByMetadataName("D").DelegateInvokeMethod;
                AssertNullableAttribute(method.GetReturnTypeAttributes());
                AssertNoNullableAttribute(method.GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_DelegateParameters()
        {
            var source =
@"public delegate void D(object?[] o);";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var method = module.ContainingAssembly.GetTypeByMetadataName("D").DelegateInvokeMethod;
                AssertNullableAttribute(method.Parameters[0].GetAttributes());
            });
        }

        [Fact]
        public void EmitAttribute_LambdaReturnType()
        {
            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            AssertNoNullableAttribute(comp);
        }

        [Fact]
        public void EmitAttribute_LambdaParameters()
        {
            var source =
@"delegate void D<T>(T t);
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G()
    {
        F((object? o) => { });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            AssertNoNullableAttribute(comp);
        }

        // See https://github.com/dotnet/roslyn/issues/28862.
        [Fact]
        public void EmitAttribute_QueryClauseParameters()
        {
            var source0 =
@"public class A
{
    public static object?[] F(object[] x) => x;
}";
            var comp0 = CreateCompilation(source0, parseOptions: TestOptions.Regular8);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"using System.Linq;
class B
{
    static void M(object[] c)
    {
        var z = from x in A.F(c)
            let y = x
            where y != null
            select y;
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, references: new[] { ref0 });
            AssertNoNullableAttribute(comp);
        }

        [Fact]
        public void EmitAttribute_LocalFunctionReturnType()
        {
            var source =
@"class C
{
    static void M()
    {
        object?[] L() => throw new System.NotImplementedException();
        L();
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("C").GetMethod("<M>g__L|0_0");
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                });
        }

        [Fact]
        public void EmitAttribute_LocalFunctionParameters()
        {
            var source =
@"class C
{
    static void M()
    {
        void L(object? x, object y) { }
        L(null, 2);
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("C").GetMethod("<M>g__L|0_0");
                    AssertNullableAttribute(method.Parameters[0].GetAttributes());
                    AssertNoNullableAttribute(method.Parameters[1].GetAttributes());
                });
        }

        [Fact]
        public void EmitAttribute_ExplicitImplementationForwardingMethod()
        {
            var source0 =
@"public class A
{
    public object? F() => null;
}";
            var comp0 = CreateCompilation(source0, parseOptions: TestOptions.Regular8);
            var ref0 = comp0.EmitToImageReference();
            var source =
@"interface I
{
    object? F();
}
class B : A, I
{
}";
            CompileAndVerify(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var method = module.ContainingAssembly.GetTypeByMetadataName("B").GetMethod("I.F");
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertNoNullableAttribute(method.GetAttributes());
                });
        }

        [Fact]
        [WorkItem(30010, "https://github.com/dotnet/roslyn/issues/30010")]
        public void EmitAttribute_Iterator_01()
        {
            var source =
@"using System.Collections.Generic;
class C
{
    static IEnumerable<object?> F()
    {
        yield break;
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var property = module.ContainingAssembly.GetTypeByMetadataName("C").GetTypeMember("<F>d__0").GetProperty("System.Collections.Generic.IEnumerator<System.Object>.Current");
                    AssertNoNullableAttribute(property.GetAttributes());
                    var method = property.GetMethod;
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Diagnostics.DebuggerHiddenAttribute");
                });
        }

        [Fact]
        public void EmitAttribute_Iterator_02()
        {
            var source =
@"using System.Collections.Generic;
class C
{
    static IEnumerable<object?[]> F()
    {
        yield break;
    }
}";
            CompileAndVerify(
                source,
                parseOptions: TestOptions.Regular8,
                options: TestOptions.DebugDll.WithMetadataImportOptions(MetadataImportOptions.All),
                symbolValidator: module =>
                {
                    var property = module.ContainingAssembly.GetTypeByMetadataName("C").GetTypeMember("<F>d__0").GetProperty("System.Collections.Generic.IEnumerator<System.Object[]>.Current");
                    AssertNoNullableAttribute(property.GetAttributes());
                    var method = property.GetMethod;
                    AssertNullableAttribute(method.GetReturnTypeAttributes());
                    AssertAttributes(method.GetAttributes(), "System.Diagnostics.DebuggerHiddenAttribute");
                });
        }

        [Fact]
        public void EmitPrivateMetadata_Types()
        {
            var source =
@"public class Base<T, U> { }
namespace Namespace
{
    public class Public : Base<object, string?> { }
    internal class Internal : Base<object, string?> { }
}
public class PublicTypes
{
    public class Public : Base<object, string?> { }
    internal class Internal : Base<object, string?> { }
    protected class Protected : Base<object, string?> { }
    protected internal class ProtectedInternal : Base<object, string?> { }
    private protected class PrivateProtected : Base<object, string?> { }
    private class Private : Base<object, string?> { }
}
internal class InternalTypes
{
    public class Public : Base<object, string?> { }
    internal class Internal : Base<object, string?> { }
    protected class Protected : Base<object, string?> { }
    protected internal class ProtectedInternal : Base<object, string?> { }
    private protected class PrivateProtected : Base<object, string?> { }
    private class Private : Base<object, string?> { }
}";
            var expectedPublicOnly = @"
Base<T, U>
    [Nullable(2)] T
    [Nullable(2)] U
PublicTypes
    [Nullable({ 0, 1, 2 })] PublicTypes.Public
    [Nullable({ 0, 1, 2 })] PublicTypes.Protected
    [Nullable({ 0, 1, 2 })] PublicTypes.ProtectedInternal
[Nullable({ 0, 1, 2 })] Namespace.Public
";
            var expectedPublicAndInternal = @"
Base<T, U>
    [Nullable(2)] T
    [Nullable(2)] U
PublicTypes
    [Nullable({ 0, 1, 2 })] PublicTypes.Public
    [Nullable({ 0, 1, 2 })] PublicTypes.Internal
    [Nullable({ 0, 1, 2 })] PublicTypes.Protected
    [Nullable({ 0, 1, 2 })] PublicTypes.ProtectedInternal
    [Nullable({ 0, 1, 2 })] PublicTypes.PrivateProtected
InternalTypes
    [Nullable({ 0, 1, 2 })] InternalTypes.Public
    [Nullable({ 0, 1, 2 })] InternalTypes.Internal
    [Nullable({ 0, 1, 2 })] InternalTypes.Protected
    [Nullable({ 0, 1, 2 })] InternalTypes.ProtectedInternal
    [Nullable({ 0, 1, 2 })] InternalTypes.PrivateProtected
[Nullable({ 0, 1, 2 })] Namespace.Public
[Nullable({ 0, 1, 2 })] Namespace.Internal
";
            var expectedAll = @"
Base<T, U>
    [Nullable(2)] T
    [Nullable(2)] U
PublicTypes
    [Nullable({ 0, 1, 2 })] PublicTypes.Public
    [Nullable({ 0, 1, 2 })] PublicTypes.Internal
    [Nullable({ 0, 1, 2 })] PublicTypes.Protected
    [Nullable({ 0, 1, 2 })] PublicTypes.ProtectedInternal
    [Nullable({ 0, 1, 2 })] PublicTypes.PrivateProtected
    [Nullable({ 0, 1, 2 })] PublicTypes.Private
InternalTypes
    [Nullable({ 0, 1, 2 })] InternalTypes.Public
    [Nullable({ 0, 1, 2 })] InternalTypes.Internal
    [Nullable({ 0, 1, 2 })] InternalTypes.Protected
    [Nullable({ 0, 1, 2 })] InternalTypes.ProtectedInternal
    [Nullable({ 0, 1, 2 })] InternalTypes.PrivateProtected
    [Nullable({ 0, 1, 2 })] InternalTypes.Private
[Nullable({ 0, 1, 2 })] Namespace.Public
[Nullable({ 0, 1, 2 })] Namespace.Internal
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Delegates()
        {
            var source =
@"public class Program
{
    protected delegate object ProtectedDelegate(object? arg);
    internal delegate object InternalDelegate(object? arg);
    private delegate object PrivateDelegate(object? arg);
}";
            var expectedPublicOnly = @"
Program
    Program.ProtectedDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
";
            var expectedPublicAndInternal = @"
Program
    Program.ProtectedDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
    Program.InternalDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
";
            var expectedAll = @"
Program
    Program.ProtectedDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
    Program.InternalDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
    Program.PrivateDelegate
        [Nullable(1)] System.Object! Invoke(System.Object? arg)
            [Nullable(2)] System.Object? arg
        System.IAsyncResult BeginInvoke(System.Object? arg, System.AsyncCallback callback, System.Object @object)
            [Nullable(2)] System.Object? arg
        [Nullable(1)] System.Object! EndInvoke(System.IAsyncResult result)
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Events()
        {
            var source =
@"#nullable disable
public delegate void D<T>(T t);
#nullable enable
public class Program
{
    protected event D<object?> ProtectedEvent { add { } remove { } }
    internal event D<object?> InternalEvent { add { } remove { } }
    private event D<object?> PrivateEvent { add { } remove { } }
}";
            var expectedPublicOnly = @"
Program
    [Nullable({ 1, 2 })] event D<System.Object?>! ProtectedEvent
";
            var expectedPublicAndInternal = @"
Program
    [Nullable({ 1, 2 })] event D<System.Object?>! ProtectedEvent
    [Nullable({ 1, 2 })] event D<System.Object?>! InternalEvent
";
            var expectedAll = @"
Program
    [Nullable({ 1, 2 })] event D<System.Object?>! ProtectedEvent
    [Nullable({ 1, 2 })] event D<System.Object?>! InternalEvent
    [Nullable({ 1, 2 })] event D<System.Object?>! PrivateEvent
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Fields()
        {
            var source =
@"public class Program
{
    protected object? ProtectedField;
    internal object? InternalField;
    private object? PrivateField;
}";
            var expectedPublicOnly = @"
Program
    [Nullable(2)] System.Object? ProtectedField
";
            var expectedPublicAndInternal = @"
Program
    [Nullable(2)] System.Object? ProtectedField
    [Nullable(2)] System.Object? InternalField
";
            var expectedAll = @"
Program
    [Nullable(2)] System.Object? ProtectedField
    [Nullable(2)] System.Object? InternalField
    [Nullable(2)] System.Object? PrivateField
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Methods()
        {
            var source =
@"public class Program
{
    protected object? ProtectedMethod(object arg) => null;
    internal object? InternalMethod(object arg) => null;
    private object? PrivateMethod(object arg) => null;
}";
            var expectedPublicOnly = @"
Program
    [Nullable(2)] System.Object? ProtectedMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
";
            var expectedPublicAndInternal = @"
Program
    [Nullable(2)] System.Object? ProtectedMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
    [Nullable(2)] System.Object? InternalMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
";
            var expectedAll = @"
Program
    [Nullable(2)] System.Object? ProtectedMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
    [Nullable(2)] System.Object? InternalMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
    [Nullable(2)] System.Object? PrivateMethod(System.Object! arg)
        [Nullable(1)] System.Object! arg
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Properties()
        {
            var source =
@"public class Program
{
    protected object? ProtectedProperty => null;
    internal object? InternalProperty => null;
    private object? PrivateProperty => null;
}";
            var expectedPublicOnly = @"
Program
    [Nullable(2)] System.Object? ProtectedProperty { get; }
";
            var expectedPublicAndInternal = @"
Program
    [Nullable(2)] System.Object? ProtectedProperty { get; }
    [Nullable(2)] System.Object? InternalProperty { get; }
";
            var expectedAll = @"
Program
    [Nullable(2)] System.Object? ProtectedProperty { get; }
    [Nullable(2)] System.Object? InternalProperty { get; }
    [Nullable(2)] System.Object? PrivateProperty { get; }
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_TypeParameters()
        {
            var source =
@"public class Base { }
public class Program
{
    protected static void ProtectedMethod<T, U>()
        where T : notnull
        where U : class
    {
    }
    internal static void InternalMethod<T, U>()
        where T : notnull
        where U : class
    {
    }
    private static void PrivateMethod<T, U>()
        where T : notnull
        where U : class
    {
    }
}";
            var expectedPublicOnly = @"
Program
    void ProtectedMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
";
            var expectedPublicAndInternal = @"
Program
    void ProtectedMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
    void InternalMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
";
            var expectedAll = @"
Program
    void ProtectedMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
    void InternalMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
    void PrivateMethod<T, U>() where T : notnull where U : class!
        [Nullable(1)] T
        [Nullable(1)] U
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_SynthesizedFields()
        {
            var source =
@"public struct S<T> { }
public class Public
{
    public static void PublicMethod()
    {
        S<object?> s;
        System.Action a = () => { s.ToString(); };
    }
}";
            var expectedPublicOnly = @"
S<T>
    [Nullable(2)] T
";
            var expectedPublicAndInternal = @"
S<T>
    [Nullable(2)] T
";
            var expectedAll = @"
S<T>
    [Nullable(2)] T
Public
    Public.<>c__DisplayClass0_0
        [Nullable({ 0, 2 })] S<System.Object?> s
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_SynthesizedParameters()
        {
            var source =
@"public class Public
{
    private static void PrivateMethod(string x)
    {
        _ = new System.Action<string?>((string y) => { });
    }
}";
            var expectedPublicOnly = @"";
            var expectedPublicAndInternal = @"";
            var expectedAll = @"
Public
    void PrivateMethod(System.String! x)
        [Nullable(1)] System.String! x
    Public.<>c
        [Nullable({ 0, 2 })] System.Action<System.String?> <>9__0_0
        void <PrivateMethod>b__0_0(System.String! y)
            [Nullable(1)] System.String! y
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_AnonymousType()
        {
            var source =
@"public class Program
{
    public static void Main()
    {
        _ = new { A = new object(), B = (string?)null };
    }
}";
            var expectedPublicOnly = @"";
            var expectedPublicAndInternal = @"";
            var expectedAll = @"";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        [Fact]
        public void EmitPrivateMetadata_Iterator()
        {
            var source =
@"using System.Collections.Generic;
public class Program
{
    public static IEnumerable<object?> F()
    {
        yield break;
    }
}";
            var expectedPublicOnly = @"
Program
    [Nullable({ 1, 2 })] System.Collections.Generic.IEnumerable<System.Object?>! F()
";
            var expectedPublicAndInternal = @"
Program
    [Nullable({ 1, 2 })] System.Collections.Generic.IEnumerable<System.Object?>! F()
";
            var expectedAll = @"
Program
    [Nullable({ 1, 2 })] System.Collections.Generic.IEnumerable<System.Object?>! F()
    Program.<F>d__0
        [Nullable(2)] System.Object? <>2__current
";
            EmitPrivateMetadata(source, expectedPublicOnly, expectedPublicAndInternal, expectedAll);
        }

        private void EmitPrivateMetadata(string source, string expectedPublicOnly, string expectedPublicAndInternal, string expectedAll)
        {
            var sourceIVTs =
@"using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo(""Other"")]";

            var options = WithNonNullTypesTrue().WithMetadataImportOptions(MetadataImportOptions.All);
            var parseOptions = TestOptions.Regular8;
            VerifyNullableAttributes(CreateCompilation(source, options: options, parseOptions: parseOptions), expectedAll);
            VerifyNullableAttributes(CreateCompilation(source, options: options, parseOptions: parseOptions.WithFeature("nullablePublicOnly")), expectedPublicOnly);
            VerifyNullableAttributes(CreateCompilation(new[] { source, sourceIVTs }, options: options, parseOptions: parseOptions), expectedAll);
            VerifyNullableAttributes(CreateCompilation(new[] { source, sourceIVTs }, options: options, parseOptions: parseOptions.WithFeature("nullablePublicOnly")), expectedPublicAndInternal);
        }

        /// <summary>
        /// Should only require NullableAttribute constructor if nullable annotations are emitted.
        /// </summary>
        [Fact]
        public void EmitPrivateMetadata_MissingAttributeConstructor()
        {
            var sourceAttribute =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute { }
}";
            var source =
@"#pragma warning disable 0067
#pragma warning disable 0169
#pragma warning disable 8321
public class A
{
    private object? F;
    private static object? M(object arg) => null;
    private object? P => null;
    private object? this[object x, object? y] => null;
    public static void M()
    {
        object? f(object arg) => arg;
        object? l(object arg) { return arg; }
        D<object> d = () => new object();
    }
}
internal delegate T D<T>();
internal interface I<T> { }
internal class B : I<object>
{
    public static object operator!(B b) => b;
    public event D<object?> E;
    private (object, object?) F;
}";
            var options = WithNonNullTypesTrue();
            var parseOptions = TestOptions.Regular8;

            var comp = CreateCompilation(new[] { sourceAttribute, source }, options: options, parseOptions: parseOptions);
            comp.VerifyEmitDiagnostics(
                // (6,21): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? F;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "F").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(6, 21),
                // (7,20): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private static object? M(object arg) => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(7, 20),
                // (7,30): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private static object? M(object arg) => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object arg").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(7, 30),
                // (8,13): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? P => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(8, 13),
                // (9,13): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? this[object x, object? y] => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(9, 13),
                // (9,26): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? this[object x, object? y] => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object x").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(9, 26),
                // (9,36): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? this[object x, object? y] => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object? y").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(9, 36),
                // (12,9): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         object? f(object arg) => arg;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(12, 9),
                // (12,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         object? f(object arg) => arg;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object arg").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(12, 19),
                // (13,9): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         object? l(object arg) { return arg; }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(13, 9),
                // (13,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         object? l(object arg) { return arg; }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object arg").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(13, 19),
                // (14,26): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //         D<object> d = () => new object();
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "=>").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(14, 26),
                // (17,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                // internal delegate T D<T>();
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(17, 19),
                // (17,23): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                // internal delegate T D<T>();
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(17, 23),
                // (18,22): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                // internal interface I<T> { }
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(18, 22),
                // (19,16): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                // internal class B : I<object>
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "B").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(19, 16),
                // (21,19): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     public static object operator!(B b) => b;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(21, 19),
                // (21,36): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     public static object operator!(B b) => b;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "B b").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(21, 36),
                // (22,29): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     public event D<object?> E;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "E").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(22, 29),
                // (23,31): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private (object, object?) F;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "F").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(23, 31));

            comp = CreateCompilation(new[] { sourceAttribute, source }, options: options, parseOptions: parseOptions.WithFeature("nullablePublicOnly"));
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void EmitPrivateMetadata_MissingAttributeConstructor_NullableDisabled()
        {
            var sourceAttribute =
@"namespace System.Runtime.CompilerServices
{
    public sealed class NullableAttribute : Attribute { }
}";
            var source =
@"public class Program
{
    private object? P => null;
}";
            var options = TestOptions.ReleaseDll;
            var parseOptions = TestOptions.Regular8;

            var comp = CreateCompilation(new[] { sourceAttribute, source }, options: options, parseOptions: parseOptions);
            comp.VerifyEmitDiagnostics(
                // (3,13): error CS0656: Missing compiler required member 'System.Runtime.CompilerServices.NullableAttribute..ctor'
                //     private object? P => null;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute", ".ctor").WithLocation(3, 13),
                // (3,19): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' context.
                //     private object? P => null;
                Diagnostic(ErrorCode.WRN_MissingNonNullTypesContextForAnnotation, "?").WithLocation(3, 19));

            comp = CreateCompilation(new[] { sourceAttribute, source }, options: options, parseOptions: parseOptions.WithFeature("nullablePublicOnly"));
            comp.VerifyEmitDiagnostics(
                // (3,19): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' context.
                //     private object? P => null;
                Diagnostic(ErrorCode.WRN_MissingNonNullTypesContextForAnnotation, "?").WithLocation(3, 19));
        }

        [Fact]
        public void UseSiteError_LambdaReturnType()
        {
            var source0 =
@"namespace System
{
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct IntPtr { }
    public class MulticastDelegate { }
}";
            var comp0 = CreateEmptyCompilation(source0);
            var ref0 = comp0.EmitToImageReference();

            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateEmptyCompilation(
                source,
                references: new[] { ref0 },
                parseOptions: TestOptions.Regular8);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ModuleMissingAttribute_BaseClass()
        {
            var source =
@"class A<T>
{
}
class B : A<object?>
{
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (1,9): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // class A<T>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 9),
                // (4,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // class B : A<object?>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "B").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_Interface()
        {
            var source =
@"interface I<T>
{
}
class C : I<(object X, object? Y)>
{
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (1,13): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // interface I<T>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 13),
                // (4,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // class C : I<(object X, object? Y)>
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "C").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_MethodReturnType()
        {
            var source =
@"class C
{
    object? F() => null;
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object? F() => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 5));
        }

        [Fact]
        public void ModuleMissingAttribute_MethodParameters()
        {
            var source =
@"class C
{
    void F(object?[] c) { }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,12): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     void F(object?[] c) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] c").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 12));
        }

        [Fact]
        public void ModuleMissingAttribute_ConstructorParameters()
        {
            var source =
@"class C
{
    C(object?[] c) { }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,7): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     C(object?[] c) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] c").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 7));
        }

        [Fact]
        public void ModuleMissingAttribute_PropertyType()
        {
            var source =
@"class C
{
    object? P => null;
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object? P => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 5));
        }

        [Fact]
        public void ModuleMissingAttribute_PropertyParameters()
        {
            var source =
@"class C
{
    object this[object x, object? y] => throw new System.NotImplementedException();
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object this[object x, object? y] => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 5),
                // (3,17): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object this[object x, object? y] => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object x").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 17),
                // (3,27): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     object this[object x, object? y] => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? y").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 27));
        }

        [Fact]
        public void ModuleMissingAttribute_OperatorReturnType()
        {
            var source =
@"class C
{
    public static object? operator+(C a, C b) => null;
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,19): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object? operator+(C a, C b) => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 19),
                // (3,37): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object? operator+(C a, C b) => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "C a").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 37),
                // (3,42): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object? operator+(C a, C b) => null;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "C b").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 42)
                );
        }

        [Fact]
        public void ModuleMissingAttribute_OperatorParameters()
        {
            var source =
@"class C
{
    public static object operator+(C a, object?[] b) => a;
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (3,19): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object operator+(C a, object?[] b) => a;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 19),
                // (3,36): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object operator+(C a, object?[] b) => a;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "C a").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 36),
                // (3,41): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     public static object operator+(C a, object?[] b) => a;
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] b").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(3, 41));
        }

        [Fact]
        public void ModuleMissingAttribute_DelegateReturnType()
        {
            var source =
@"delegate object? D();";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (1,10): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate object? D();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 10));
        }

        [Fact]
        public void ModuleMissingAttribute_DelegateParameters()
        {
            var source =
@"delegate void D(object?[] o);";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (1,17): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate void D(object?[] o);
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[] o").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 17));
        }

        [Fact]
        public void ModuleMissingAttribute_LambdaReturnType()
        {
            var source =
@"delegate T D<T>();
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G(object o)
    {
        F(() =>
        {
            if (o != new object()) return o;
            return null;
        });
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.ReleaseModule);
            // The lambda signature is emitted without a [Nullable] attribute because
            // the return type is inferred from flow analysis, not from initial binding.
            // As a result, there is no missing attribute warning.
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void ModuleMissingAttribute_LambdaParameters()
        {
            var source =
@"delegate void D<T>(T t);
class C
{
    static void F<T>(D<T> d)
    {
    }
    static void G()
    {
        F((object? o) => { });
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (1,17): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate void D<T>(T t);
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 17),
                // (1,20): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                // delegate void D<T>(T t);
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "T t").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(1, 20),
                // (4,19): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     static void F<T>(D<T> d)
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "T").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 19),
                // (4,22): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //     static void F<T>(D<T> d)
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "D<T> d").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(4, 22),
                // (9,12): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         F((object? o) => { });
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? o").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(9, 12));
        }

        [Fact]
        public void ModuleMissingAttribute_LocalFunctionReturnType()
        {
            var source =
@"class C
{
    static void M()
    {
        object?[] L() => throw new System.NotImplementedException();
        L();
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (5,9): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         object?[] L() => throw new System.NotImplementedException();
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object?[]").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(5, 9));
        }

        [Fact]
        public void ModuleMissingAttribute_LocalFunctionParameters()
        {
            var source =
@"class C
{
    static void M()
    {
        void L(object? x, object y) { }
        L(null, 2);
    }
}";
            var comp = CreateCompilation(new[] { source }, parseOptions: TestOptions.Regular8, options: WithNonNullTypesTrue(TestOptions.ReleaseModule));
            comp.VerifyEmitDiagnostics(
                // (5,16): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         void L(object? x, object y) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object? x").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(5, 16),
                // (5,27): error CS0518: Predefined type 'System.Runtime.CompilerServices.NullableAttribute' is not defined or imported
                //         void L(object? x, object y) { }
                Diagnostic(ErrorCode.ERR_PredefinedTypeNotFound, "object y").WithArguments("System.Runtime.CompilerServices.NullableAttribute").WithLocation(5, 27));
        }

        [Fact]
        public void Tuples()
        {
            var source =
@"public class A
{
    public static ((object?, object) _1, object? _2, object _3, ((object?[], object), object?) _4) Nested;
    public static (object? _1, object _2, object? _3, object _4, object? _5, object _6, object? _7, object _8, object? _9) Long;
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "A");
                var fieldDefs = typeDef.GetFields().Select(f => reader.GetFieldDefinition(f)).ToArray();

                // Nested tuple
                var field = fieldDefs.Single(f => reader.StringComparer.Equals(f.Name, "Nested"));
                var customAttributes = field.GetCustomAttributes();
                AssertAttributes(reader, customAttributes,
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                var customAttribute = reader.GetCustomAttribute(customAttributes.ElementAt(1));
                AssertEx.Equal(ImmutableArray.Create<byte>(0, 0, 2, 0, 2, 0, 0, 0, 0, 2, 0, 2), reader.ReadByteArray(customAttribute.Value));

                // Long tuple
                field = fieldDefs.Single(f => reader.StringComparer.Equals(f.Name, "Long"));
                customAttributes = field.GetCustomAttributes();
                AssertAttributes(reader, customAttributes,
                    "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                    "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                customAttribute = reader.GetCustomAttribute(customAttributes.ElementAt(1));
                AssertEx.Equal(ImmutableArray.Create<byte>(0, 2, 0, 2, 0, 2, 0, 2, 0, 0, 2), reader.ReadByteArray(customAttribute.Value));
            });

            var source2 =
@"class B
{
    static void Main()
    {
        A.Nested._1.Item1.ToString(); // 1
        A.Nested._1.Item2.ToString();
        A.Nested._2.ToString(); // 2
        A.Nested._3.ToString();
        A.Nested._4.Item1.Item1.ToString();
        A.Nested._4.Item1.Item1[0].ToString(); // 3
        A.Nested._4.Item1.Item2.ToString();
        A.Nested._4.Item2.ToString(); // 4
        A.Long._1.ToString(); // 5
        A.Long._2.ToString();
        A.Long._3.ToString(); // 6
        A.Long._4.ToString();
        A.Long._5.ToString(); // 7
        A.Long._6.ToString();
        A.Long._7.ToString(); // 8
        A.Long._8.ToString();
        A.Long._9.ToString(); // 9
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (5,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Nested._1.Item1.ToString(); // 1
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._1.Item1").WithLocation(5, 9),
                // (7,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Nested._2.ToString(); // 2
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._2").WithLocation(7, 9),
                // (10,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Nested._4.Item1.Item1[0].ToString(); // 3
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._4.Item1.Item1[0]").WithLocation(10, 9),
                // (12,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Nested._4.Item2.ToString(); // 4
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Nested._4.Item2").WithLocation(12, 9),
                // (13,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Long._1.ToString(); // 5
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._1").WithLocation(13, 9),
                // (15,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Long._3.ToString(); // 6
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._3").WithLocation(15, 9),
                // (17,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Long._5.ToString(); // 7
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._5").WithLocation(17, 9),
                // (19,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Long._7.ToString(); // 8
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._7").WithLocation(19, 9),
                // (21,9): warning CS8602: Dereference of a possibly null reference.
                //         A.Long._9.ToString(); // 9
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "A.Long._9").WithLocation(21, 9));

            var type = comp2.GetMember<NamedTypeSymbol>("A");
            Assert.Equal(
                "((System.Object?, System.Object) _1, System.Object? _2, System.Object _3, ((System.Object?[], System.Object), System.Object?) _4)",
                type.GetMember<FieldSymbol>("Nested").TypeWithAnnotations.ToTestDisplayString());
            Assert.Equal(
                "(System.Object? _1, System.Object _2, System.Object? _3, System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)",
                type.GetMember<FieldSymbol>("Long").TypeWithAnnotations.ToTestDisplayString());
        }

        // DynamicAttribute and NullableAttribute formats should be aligned.
        [Fact]
        public void TuplesDynamic()
        {
            var source =
@"#pragma warning disable 0067
using System;
public interface I<T> { }
public class A<T> { }
public class B<T> :
    A<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>,
    I<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>
    where T : A<(object? _1, (object _2, object? _3), object _4, object? _5, object _6, object? _7, object _8, object? _9)>
{
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Field;
    public event EventHandler<(dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9)> Event;
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Method(
        (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) arg) => arg;
    public (dynamic? _1, (object _2, dynamic? _3), object _4, dynamic? _5, object _6, dynamic? _7, object _8, dynamic? _9) Property { get; set; }
}";
            var comp = CreateCompilation(new[] { source }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, validator: assembly =>
            {
                var reader = assembly.GetMetadataReader();
                var typeDef = GetTypeDefinitionByName(reader, "B`1");
                // Base type
                checkAttributesNoDynamic(typeDef.GetCustomAttributes(), addOne: 0); // add one for A<T>
                // Interface implementation
                var interfaceImpl = reader.GetInterfaceImplementation(typeDef.GetInterfaceImplementations().Single());
                checkAttributesNoDynamic(interfaceImpl.GetCustomAttributes(), addOne: 0); // add one for I<T>
                // Type parameter constraint type
                var typeParameter = reader.GetGenericParameter(typeDef.GetGenericParameters()[0]);
                var constraint = reader.GetGenericParameterConstraint(typeParameter.GetConstraints()[0]);
                checkAttributesNoDynamic(constraint.GetCustomAttributes(), addOne: 1); // add one for A<T>
                // Field type
                var field = typeDef.GetFields().Select(f => reader.GetFieldDefinition(f)).Single(f => reader.StringComparer.Equals(f.Name, "Field"));
                checkAttributes(field.GetCustomAttributes());
                // Event type
                var @event = typeDef.GetEvents().Select(e => reader.GetEventDefinition(e)).Single(e => reader.StringComparer.Equals(e.Name, "Event"));
                checkAttributes(@event.GetCustomAttributes(), addOne: 1); // add one for EventHandler<T>
                // Method return type and parameter type
                var method = typeDef.GetMethods().Select(m => reader.GetMethodDefinition(m)).Single(m => reader.StringComparer.Equals(m.Name, "Method"));
                var parameters = method.GetParameters().Select(p => reader.GetParameter(p)).ToArray();
                checkAttributes(parameters[0].GetCustomAttributes()); // return type
                checkAttributes(parameters[1].GetCustomAttributes()); // parameter
                // Property type
                var property = typeDef.GetProperties().Select(p => reader.GetPropertyDefinition(p)).Single(p => reader.StringComparer.Equals(p.Name, "Property"));
                checkAttributes(property.GetCustomAttributes());

                void checkAttributes(CustomAttributeHandleCollection customAttributes, byte? addOne = null)
                {
                    AssertAttributes(reader, customAttributes,
                        "MemberReference:Void System.Runtime.CompilerServices.DynamicAttribute..ctor(Boolean[])",
                        "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                        "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                    checkAttribute(reader.GetCustomAttribute(customAttributes.ElementAt(2)), addOne);
                }

                void checkAttributesNoDynamic(CustomAttributeHandleCollection customAttributes, byte? addOne = null)
                {
                    AssertAttributes(reader, customAttributes,
                        "MemberReference:Void System.Runtime.CompilerServices.TupleElementNamesAttribute..ctor(String[])",
                        "MethodDefinition:Void System.Runtime.CompilerServices.NullableAttribute..ctor(Byte[])");
                    checkAttribute(reader.GetCustomAttribute(customAttributes.ElementAt(1)), addOne);
                }

                void checkAttribute(CustomAttribute customAttribute, byte? addOne)
                {
                    var expectedBits = ImmutableArray.Create<byte>(0, 2, 0, 1, 2, 1, 2, 1, 2, 1, 0, 2);
                    if (addOne.HasValue)
                    {
                        expectedBits = ImmutableArray.Create(addOne.GetValueOrDefault()).Concat(expectedBits);
                    }
                    AssertEx.Equal(expectedBits, reader.ReadByteArray(customAttribute.Value));
                }
            });

            var source2 =
@"class C
{
    static void F(B<A<(object?, (object, object?), object, object?, object, object?, object, object?)>> b)
    {
        b.Field._8.ToString();
        b.Field._9.ToString(); // 1
        b.Method(default)._8.ToString();
        b.Method(default)._9.ToString(); // 2
        b.Property._8.ToString();
        b.Property._9.ToString(); // 3
    }
}";
            var comp2 = CreateCompilation(new[] { source2 }, options: WithNonNullTypesTrue(), parseOptions: TestOptions.Regular8, references: new[] { comp.EmitToImageReference() });
            comp2.VerifyDiagnostics(
                // (6,9): warning CS8602: Dereference of a possibly null reference.
                //         b.Field._9.ToString(); // 1
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Field._9").WithLocation(6, 9),
                // (8,9): warning CS8602: Dereference of a possibly null reference.
                //         b.Method(default)._9.ToString(); // 2
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Method(default)._9").WithLocation(8, 9),
                // (10,9): warning CS8602: Dereference of a possibly null reference.
                //         b.Property._9.ToString(); // 3
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "b.Property._9").WithLocation(10, 9));

            var type = comp2.GetMember<NamedTypeSymbol>("B");
            Assert.Equal(
                "A<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.BaseTypeNoUseSiteDiagnostics.ToTestDisplayString());
            Assert.Equal(
                "I<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.Interfaces()[0].ToTestDisplayString());
            Assert.Equal(
                "A<(System.Object? _1, (System.Object _2, System.Object? _3), System.Object _4, System.Object? _5, System.Object _6, System.Object? _7, System.Object _8, System.Object? _9)>",
                type.TypeParameters[0].ConstraintTypesNoUseSiteDiagnostics[0].ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9)",
                type.GetMember<FieldSymbol>("Field").TypeWithAnnotations.ToTestDisplayString());
            Assert.Equal(
                "System.EventHandler<(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9)>",
                type.GetMember<EventSymbol>("Event").TypeWithAnnotations.ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) B<T>.Method((dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) arg)",
                type.GetMember<MethodSymbol>("Method").ToTestDisplayString());
            Assert.Equal(
                "(dynamic? _1, (System.Object _2, dynamic? _3), System.Object _4, dynamic? _5, System.Object _6, dynamic? _7, System.Object _8, dynamic? _9) B<T>.Property { get; set; }",
                type.GetMember<PropertySymbol>("Property").ToTestDisplayString());
        }

        [Fact]
        public void NullableFlags_Field_Exists()
        {
            var source =
@"public class C
{
    public void F(object? c) { }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8);
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                var attributes = method.Parameters.Single().GetAttributes();
                AssertNullableAttribute(attributes);

                var nullable = GetNullableAttribute(attributes);

                var field = nullable.AttributeClass.GetField("NullableFlags");
                Assert.NotNull(field);
                Assert.Equal("System.Byte[]", field.TypeWithAnnotations.ToTestDisplayString());
            });
        }

        [Fact]
        public void NullableFlags_Field_Contains_ConstructorArguments_SingleByteConstructor()
        {
            var source =
@"
#nullable enable
using System;
using System.Linq;
public class C
{
    public void F(object? c) { }

    public static void Main()
    {
        var attribute = typeof(C).GetMethod(""F"").GetParameters()[0].GetCustomAttributes(true).Single(a => a.GetType().Name == ""NullableAttribute"");
        var field = attribute.GetType().GetField(""NullableFlags"");
        byte[] flags = (byte[])field.GetValue(attribute);

        Console.Write($""{{ {string.Join("","", flags)} }}"");
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.DebugExe);
            CompileAndVerify(comp, expectedOutput: "{ 2 }", symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                var attributes = method.Parameters.Single().GetAttributes();
                AssertNullableAttribute(attributes);

                var nullable = GetNullableAttribute(attributes);
                var args = nullable.ConstructorArguments;
                Assert.Single(args);
                Assert.Equal((byte)2, args.First().Value);
            });
        }

        [Fact]
        public void NullableFlags_Field_Contains_ConstructorArguments_ByteArrayConstructor()
        {
            var source =
@"
#nullable enable
using System;
using System.Linq;
public class C
{
    public void F(Action<object?, Action<object, object?>?> c) { }

    public static void Main()
    {
        var attribute = typeof(C).GetMethod(""F"").GetParameters()[0].GetCustomAttributes(true).Single(a => a.GetType().Name == ""NullableAttribute"");
        var field = attribute.GetType().GetField(""NullableFlags"");
        byte[] flags = (byte[])field.GetValue(attribute);

        System.Console.Write($""{{ {string.Join("","", flags)} }}"");
    }
}";
            var comp = CreateCompilation(source, parseOptions: TestOptions.Regular8, options: TestOptions.DebugExe);
            CompileAndVerify(comp, expectedOutput: "{ 1,2,2,1,2 }", symbolValidator: module =>
            {
                var type = module.ContainingAssembly.GetTypeByMetadataName("C");
                var method = (MethodSymbol)type.GetMembers("F").Single();
                var attributes = method.Parameters.Single().GetAttributes();
                AssertNullableAttribute(attributes);

                var nullable = GetNullableAttribute(attributes);
                var args = nullable.ConstructorArguments;
                Assert.Single(args);
                var byteargs = args.First().Values;
                Assert.Equal((byte)1, byteargs[0].Value);
                Assert.Equal((byte)2, byteargs[1].Value);
                Assert.Equal((byte)2, byteargs[2].Value);
                Assert.Equal((byte)1, byteargs[3].Value);
                Assert.Equal((byte)2, byteargs[4].Value);
            });
        }

        private static void AssertNoNullableAttribute(ImmutableArray<CSharpAttributeData> attributes)
        {
            AssertAttributes(attributes);
        }

        private static void AssertNullableAttribute(ImmutableArray<CSharpAttributeData> attributes)
        {
            AssertAttributes(attributes, "System.Runtime.CompilerServices.NullableAttribute");
        }

        private static void AssertAttributes(ImmutableArray<CSharpAttributeData> attributes, params string[] expectedNames)
        {
            var actualNames = attributes.Select(a => a.AttributeClass.ToTestDisplayString()).ToArray();
            AssertEx.SetEqual(actualNames, expectedNames);
        }

        private static void AssertNoNullableAttribute(CSharpCompilation comp)
        {
            string attributeName = "NullableAttribute";
            var image = comp.EmitToArray();
            using (var reader = new PEReader(image))
            {
                var metadataReader = reader.GetMetadataReader();
                var attributes = metadataReader.GetCustomAttributeRows().Select(metadataReader.GetCustomAttributeName).ToArray();
                Assert.False(attributes.Contains(attributeName));
            }
        }

        private static CSharpAttributeData GetNullableAttribute(ImmutableArray<CSharpAttributeData> attributes)
        {
            return attributes.Single(a => a.AttributeClass.ToTestDisplayString() == "System.Runtime.CompilerServices.NullableAttribute");
        }

        private static TypeDefinition GetTypeDefinitionByName(MetadataReader reader, string name)
        {
            return reader.GetTypeDefinition(reader.TypeDefinitions.Single(h => reader.StringComparer.Equals(reader.GetTypeDefinition(h).Name, name)));
        }

        private static void AssertAttributes(MetadataReader reader, CustomAttributeHandleCollection handles, params string[] expectedNames)
        {
            var actualNames = handles.Select(h => reader.Dump(reader.GetCustomAttribute(h).Constructor)).ToArray();
            AssertEx.SetEqual(actualNames, expectedNames);
        }

        private void VerifyNullableAttributes(CSharpCompilation comp, string expected)
        {
            CompileAndVerify(comp, symbolValidator: module =>
            {
                var actual = NullableAttributesVisitor.GetString(module);
                AssertEx.AssertEqualToleratingWhitespaceDifferences(expected, actual);
            });
        }
    }
}
