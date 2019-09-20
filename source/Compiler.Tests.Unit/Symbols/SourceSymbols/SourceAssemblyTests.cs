﻿using FluentLang.Compiler.Symbols;
using FluentLang.Compiler.Symbols.Interfaces;
using FluentLang.Compiler.Symbols.Source;
using FluentLang.Compiler.Tests.Unit.TestHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Xunit;
using Version = FluentLang.Compiler.Symbols.Interfaces.Version;

namespace FluentLang.Compiler.Tests.Unit.Symbols
{
	public class SourceAssemblyTests : TestBase
	{
		[Fact]
		public void IgnoresDocumentsWithIrrecoverableSyntaxErrors()
		{
			var assembly = CreateAssembly(@"
interface I { M() : () bool; }");

			Assert.Empty(assembly.Interfaces);

			assembly = CreateAssembly(new string[] {
				"interface I1 { M() : () bool; }",
				"interface I2 { M() : bool; }",
			});

			var @interface = Assert.Single(assembly.Interfaces);
			Assert.Equal("I2", @interface.FullyQualifiedName!.ToString());
		}

		[Fact]
		public void ContainsOnlyHighestVersionsOfTreeOfReferencedAssemblies()
		{
			var assembly1 = new DummyAssembly { };
			var assembly2 = new DummyAssembly { Version = new Version("0.1.1"), ReferencedAssemblies = ImmutableArray.Create<IAssembly>(assembly1) };
			var assembly3 = new DummyAssembly
			{
				Name = QualifiedName("DifferentAssembly"),
				Version = new Version("0.2.3"),
				ReferencedAssemblies = ImmutableArray.Create<IAssembly>(assembly1)
			};

			var assembly = (IAssembly)new SourceAssembly(
				QualifiedName("TestAssembly"),
				new Version("1.0.0"),
				ImmutableArray.Create<IAssembly>(assembly2, assembly3),
				ImmutableArray<IDocument>.Empty);

			Assert.Equal(new[] { assembly1, assembly3, assembly }, assembly.ReferencedAssemblies);
		}

		[Fact]
		public void ContainsAllInterfacesInAssembly()
		{
			var assembly = CreateAssembly(@"
interface I1 { M() : bool; }
interface I2 { M() : int; }");

			Assert.Equal(2, assembly.Interfaces.Length);
			Assert.Equal(new[] { "I1", "I2" }, assembly.Interfaces.Select(x => x.FullyQualifiedName!.ToString()).OrderBy(x => x));
		}

		[Fact]
		public void ContainsAllMethodsInAssembly()
		{
			var assembly = CreateAssembly(@"
M1() : int {}
M2() : int {}");

			Assert.Equal(2, assembly.Methods.Length);
			Assert.Equal(new[] { "M1", "M2" }, assembly.Methods.Select(x => x.FullyQualifiedName.ToString()).OrderBy(x => x));
		}

		[Fact]
		public void CanAccessInterfaceInAssemblyByName()
		{
			var assembly = CreateAssembly(@"
interface I1 { M() : bool; }
namespace A.B.C
{
	interface I2 { M() : int; }
	namespace D
	{
		interface I3 { M() : string; }
	}
}");

			Assert.True(assembly.TryGetInterface(QualifiedName("I1"), out var i1));
			Assert.Equal(i1!.FullyQualifiedName, QualifiedName("I1"));

			Assert.True(assembly.TryGetInterface(QualifiedName("A.B.C.I2"), out var i2));
			Assert.Equal(i2!.FullyQualifiedName, QualifiedName("A.B.C.I2"));

			Assert.True(assembly.TryGetInterface(QualifiedName("A.B.C.D.I3"), out var i3));
			Assert.Equal(i3!.FullyQualifiedName, QualifiedName("A.B.C.D.I3"));
		}

#if TryGetMethodDefined
		[Fact]
		public void CanAccessMethodInAssemblyByName()
		{
			var assembly = CreateAssembly(@"
M1() : int {}
namespace A.B.C
{
	M2() : int {}
	namespace D
	{
		M3() : int {}
	}
}");

			Assert.True(assembly.TryGetMethod(QualifiedName("M1"), out var m1));
			Assert.Equal(m1!.FullyQualifiedName, QualifiedName("M1"));

			Assert.True(assembly.TryGetMethod(QualifiedName("A.B.C.M2"), out var m2));
			Assert.Equal(m2!.FullyQualifiedName, QualifiedName("A.B.C.M2"));

			Assert.True(assembly.TryGetMethod(QualifiedName("A.B.C.D.M3"), out var m3));
			Assert.Equal(m3!.FullyQualifiedName, QualifiedName("A.B.C.D.M3"));
		}
#endif

		private class DummyAssembly : IAssembly
		{
			public QualifiedName Name { get; set; } = QualifiedName("Assembly");

			public Version Version { get; set; } = new Version("1.0.0");

			public ImmutableArray<IAssembly> ReferencedAssemblies { get; set; } = ImmutableArray<IAssembly>.Empty;

			public ImmutableArray<IInterface> Interfaces { get; set; } = ImmutableArray<IInterface>.Empty;

			public ImmutableArray<IMethod> Methods { get; set; } = ImmutableArray<IMethod>.Empty;

			public bool TryGetInterface(QualifiedName fullyQualifiedName, [NotNullWhen(true)] out IInterface? @interface)
			{
				@interface = Interfaces.FirstOrDefault(x => x.FullyQualifiedName == fullyQualifiedName);
				return @interface != null;
			}
		}
	}
}