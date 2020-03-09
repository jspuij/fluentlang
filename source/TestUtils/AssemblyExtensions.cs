﻿using FluentLang.Compiler.Compilation;
using FluentLang.Compiler.Diagnostics;
using FluentLang.Compiler.Helpers;
using FluentLang.Compiler.Symbols;
using FluentLang.Compiler.Symbols.Interfaces;
using FluentLang.Compiler.Symbols.Metadata;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Diagnostic = FluentLang.Compiler.Diagnostics.Diagnostic;

namespace FluentLang.TestUtils
{
	public static class AssemblyExtensions
	{
		public static IAssembly VerifyDiagnostics(this IAssembly assembly, params Diagnostic[] expectedDiagnostics)
		{
			var actualDiagnostics = assembly.AllDiagnostics;

			Assert.True(expectedDiagnostics.Length == actualDiagnostics.Length, ErrorMessage());

			Assert.True(
				expectedDiagnostics
					.Zip(
						actualDiagnostics,
						(x, y) =>
							x.ErrorCode == y.ErrorCode
							&& x.Location.GetText().Equals(
								y.Location.GetText(),
								StringComparison.Ordinal))
					.All(x => x),
				ErrorMessage());

			return assembly;

			string ErrorMessage()
			{
				return
					$@"
Expected: 
{string.Join(",\n", expectedDiagnostics.Select(DiagnosticToString))}

Actual:
{string.Join(",\n", actualDiagnostics.Select(DiagnosticToString))}
";
				static string DiagnosticToString(Diagnostic diagnostic)
					=> $"new Diagnostic(new Location(new TextToken(@\"{diagnostic.Location.GetText().ToString()}\")), {nameof(ErrorCode)}.{diagnostic.ErrorCode})";
			}
		}

		public static IAssembly VerifyEmit(
			this IAssembly assembly,
			string? expectedCSharp = null,
			object? expectedResult = null,
			Action<IAssembly, Assembly, ImmutableArray<byte>>? testEmittedAssembly = null)
		{
			var compilationResult = assembly.CompileAssembly(
				out var assemblyBytes,
				out var csharpBytes,
				out _);

			if (!compilationResult.AssemblyDiagnostics.IsEmpty)
				throw new InvalidOperationException("cannot emit assembly with errors");

			if (expectedCSharp is { })
			{
				var actual = Encoding.Default.GetString(csharpBytes.UnsafeAsArray());
				Assert.True(
					expectedCSharp == actual,
					"expected:\n" + expectedCSharp + "\n\nactual:\n" + actual);
			}

			if (compilationResult.Status != CompilationResultStatus.Succeeded)
			{
				Assert.False(true,
					"compiling and emitting csharp failed with diagnostics:\n"
					+ string.Join('\n', compilationResult.RoslynDiagnostics)
					+ "\n\ncsharp code was:\n\n"
					+ Encoding.Default.GetString(csharpBytes.UnsafeAsArray()));
			}

			var assemblyLoadContext = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);
			// verify assembly is valid
			var emittedAssembly = assemblyLoadContext.LoadFromStream(assemblyBytes.ToStream());

			if (expectedResult is { })
			{
				Assert.NotNull(emittedAssembly.EntryPoint);
				Assert.Equal(expectedResult, emittedAssembly.EntryPoint!.Invoke(null, null));
			}

			VerifyMetadata(assembly, emittedAssembly, assemblyBytes);
			testEmittedAssembly?.Invoke(assembly, emittedAssembly, assemblyBytes);

			assemblyLoadContext.Unload();

			return assembly;
		}

		private static void VerifyMetadata(
			IAssembly assembly,
			Assembly emittedAssembly,
			ImmutableArray<byte> bytes)
		{
			var metadataAssembly = new MetadataAssembly(
				emittedAssembly,
				bytes,
				assembly
					.ReferencedAssembliesAndSelf
					.Where(x => x.Name != assembly.Name && x.Version != assembly.Version)
					.ToImmutableArray());
			metadataAssembly.VerifyDiagnostics();

			Assert.Equal(assembly.Name, metadataAssembly.Name);
			Assert.Equal(assembly.Version, metadataAssembly.Version);
			Assert.Equal(
				assembly.ReferencedAssembliesAndSelf.Where(x => x.Name != assembly.Name),
				metadataAssembly.ReferencedAssembliesAndSelf.Where(x => x.Name != metadataAssembly.Name));

			var exportedMethods =
				assembly
				.Methods
				.Where(x => x.IsExported)
				.ToDictionary(x => x.FullyQualifiedName);
			var metadataMethods = metadataAssembly.Methods;
			Assert.Equal(exportedMethods.Count, metadataMethods.Length);

			foreach (var metadataMethod in metadataMethods)
			{
				Assert.True(exportedMethods.TryGetValue(
					metadataMethod.FullyQualifiedName,
					out var exportedMethod));
				Assert.Equal(metadataMethod.ReturnType, exportedMethod!.ReturnType, MetadataTypeEqualityComparer.Instance);
				Assert.Equal(exportedMethod.Parameters.Length, metadataMethod.Parameters.Length);
				foreach (var (exportedParam, metadataParam) in
					exportedMethod.Parameters.Zip(metadataMethod.Parameters))
				{
					Assert.Equal(exportedParam.Name, metadataParam.Name);
					Assert.Equal(metadataParam.Type, exportedParam.Type, MetadataTypeEqualityComparer.Instance);
				}

				Assert.Equal(exportedMethod.RequiredMethodKeys.Length, metadataMethod.RequiredMethodKeys.Length);
			}

			var exportedInterfaces =
				assembly
				.Interfaces
				.Where(x => x.IsExported)
				.ToDictionary(x => x.FullyQualifiedName);
			var metadataInterfaces = metadataAssembly.Interfaces;
			Assert.Equal(exportedInterfaces.Count, metadataInterfaces.Length);

			foreach (var metadataInterface in metadataInterfaces)
			{
				Assert.True(exportedInterfaces.TryGetValue(
					metadataInterface.FullyQualifiedName,
					out var exportedInterface));
				Assert.Equal(metadataInterface, exportedInterface!, MetadataTypeEqualityComparer.Instance);
			}
		}
	}

	public class MetadataTypeEqualityComparer : IEqualityComparer<IType>
	{
		private MetadataTypeEqualityComparer() { }
		public static MetadataTypeEqualityComparer Instance = new MetadataTypeEqualityComparer();
		public bool Equals(IType? x, IType? y)
		{
			if (x is IInterface ix && y is IInterface iy)
				return Equals(ix, iy);
			if (x is IUnion ux && y is IUnion uy)
				return Equals(ux, uy);
			if (x is ITypeParameter tx && y is ITypeParameter ty)
				return tx.Name == ty.Name;
			if (x is Primitive px && y is Primitive py)
				return px.Equals(py);
			if (x is null && y is null)
				return true;
			return false;
		}


		private bool Equals(IInterface a, IInterface b)
		{
			if (a.Methods.Length != b.Methods.Length)
				return false;
			foreach(var (am, bm) in a.Methods.Zip(b.Methods))
			{
				if (!Equals(am, bm))
					return false;
			}
			return true;

		}

		private bool Equals(IUnion a, IUnion b)
		{
			if (a.Options.Length != b.Options.Length)
				return false;
			foreach (var (ao, bo) in a.Options.Zip(b.Options))
			{
				if (!Equals(ao, bo))
					return false;
			}
			return true;
		}

		private bool Equals(IInterfaceMethod a, IInterfaceMethod b)
		{
			if (a.Name != b.Name)
				return false;
			if (!Equals(a.ReturnType, b.ReturnType))
				return false;
			if (a.Parameters.Length != b.Parameters.Length)
				return false;
			foreach (var (ap, bp) in a.Parameters.Zip(b.Parameters))
			{
				if (ap.Name != bp.Name)
					return false;

				if (!Equals(ap.Type, bp.Type))
					return false;
			}
			return true;
		}

		public int GetHashCode([DisallowNull] IType obj)
		{
			throw new NotImplementedException();
		}
	}
}
