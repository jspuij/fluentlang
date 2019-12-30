﻿using FluentLang.Compiler.Symbols.Interfaces;
using FluentLang.Compiler.Symbols.Source;
using FluentLang.flc;
using FluentLang.flc.DependencyLoading;
using FluentLang.flc.ProjectSystem;
using FluentLang.TestUtils;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Version = FluentLang.Compiler.Symbols.Interfaces.Version;

namespace FluentLang.Compiler.Tests.Unit.DependencyLoading
{
	public class DependencyLoaderTests : TestBase
	{
		public DependencyLoaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public void ReturnsDirectDependenciesOfProject()
		{
			var name = "a";
			var version = new Version(1, 2, 3);
			CreateAssembly(
				"",
				name,
				version)
				.VerifyDiagnostics()
				.VerifyEmit(_testOutputHelper, testEmittedAssembly: (original, assembly) =>
			  {
				  var assemblyLoadContext = new AssemblyLoadContext(null, true);
				  var assemblyLoader = new MockAssemblyLoader(assembly);
				  var dependencyLoader = new DependencyLoader(
					  ImmutableArray.Create<IAssemblyLoader>(assemblyLoader),
					  new DependencyAttributeReader(),
					  GetLogger<DependencyLoader>());
				  var dependencies = dependencyLoader.LoadDependenciesAsync(
					  new ProjectInfo(
						  "p",
						  ImmutableArray.Create(""),
						  references: ImmutableArray.Create(new Reference(
							  Reference.ReferenceType.Assembly,
							  name,
							  version.ToString()))),
					  assemblyLoadContext,
					  Array.Empty<IAssembly>()).Result;
				  var dependency = dependencies.Single();
				  Assert.Equal(original.Name, dependency.Name);
				  Assert.Equal(original.Version, dependency.Version);
			  });
		}

		[Fact]
		public void TakesFromSecondAssemblyLoaderIfNotInFirst()
		{
			var name = "a";
			var version = new Version(1, 2, 3);
			CreateAssembly(
				"",
				name,
				version)
				.VerifyDiagnostics()
				.VerifyEmit(_testOutputHelper, testEmittedAssembly: (original, assembly) =>
				{
					var assemblyLoadContext = new AssemblyLoadContext(null, true);
					var assemblyLoader1 = new MockAssemblyLoader();
					var assemblyLoader2 = new MockAssemblyLoader(assembly);
					var dependencyLoader = new DependencyLoader(
						ImmutableArray.Create<IAssemblyLoader>(assemblyLoader1, assemblyLoader2),
						new DependencyAttributeReader(),
						GetLogger<DependencyLoader>());
					var dependencies = dependencyLoader.LoadDependenciesAsync(
						new ProjectInfo(
							"p",
							ImmutableArray.Create(""),
							references: ImmutableArray.Create(new Reference(
								Reference.ReferenceType.Assembly,
								name,
								version.ToString()))),
						assemblyLoadContext,
						Array.Empty<IAssembly>()).Result;
					var dependency = dependencies.Single();
					Assert.Equal(original.Name, dependency.Name);
					Assert.Equal(original.Version, dependency.Version);
				});
		}

		[Fact]
		public void ThrowsIfNotInAnyAssemblyLoader()
		{
			var name = "a";
			var version = new Version(1, 2, 3);
			CreateAssembly(
				"",
				name,
				version)
				.VerifyDiagnostics()
				.VerifyEmit(_testOutputHelper, testEmittedAssembly: (original, assembly) =>
				{
					var assemblyLoadContext = new AssemblyLoadContext(null, true);
					var assemblyLoader = new MockAssemblyLoader();
					var dependencyLoader = new DependencyLoader(
						ImmutableArray.Create<IAssemblyLoader>(assemblyLoader),
						new DependencyAttributeReader(),
						GetLogger<DependencyLoader>());
					Assert.Throws<FlcException>(() => dependencyLoader.LoadDependenciesAsync(
						new ProjectInfo(
							"p",
							ImmutableArray.Create(""),
							references: ImmutableArray.Create(new Reference(
								Reference.ReferenceType.Assembly,
								name,
								version.ToString()))),
						assemblyLoadContext,
						Array.Empty<IAssembly>()).Result);
				});
		}

		[Fact]
		public void CanLoadDependeciesWithSubdependencies()
		{
			CreateAssembly(
				"",
				"sub",
				new Version(1, 2, 3))
				.VerifyDiagnostics()
				.VerifyEmit(_testOutputHelper, testEmittedAssembly: (_, subDependency) =>
				{
					var name = "a";
					var version = new Version(1, 2, 3);
					CreateAssembly(
						"",
						name,
						version)
						.VerifyDiagnostics()
						.VerifyEmit(_testOutputHelper, testEmittedAssembly: (original, assembly) =>
						{
							var assemblyLoadContext = new AssemblyLoadContext(null, true);
							var assemblyLoader = new MockAssemblyLoader(subDependency, assembly);
							var dependencyLoader = new DependencyLoader(
								ImmutableArray.Create<IAssemblyLoader>(assemblyLoader),
								new DependencyAttributeReader(),
								GetLogger<DependencyLoader>());
							var dependencies = dependencyLoader.LoadDependenciesAsync(
								new ProjectInfo(
									"p",
									ImmutableArray.Create(""),
									references: ImmutableArray.Create(new Reference(
										Reference.ReferenceType.Assembly,
										name,
										version.ToString()))),
								assemblyLoadContext,
								Array.Empty<IAssembly>()).Result;
							var dependency = dependencies.Single();
							Assert.Equal(original.Name, dependency.Name);
							Assert.Equal(original.Version, dependency.Version);
						});
				});
		}

		[Fact]
		public void ThrowsIfSubDependencyNotInAnyAssemblyLoader()
		{
			var subDependency = CreateAssembly("");
			var name = "a";
			var version = new Version(1, 2, 3);
			CreateAssembly(
				"",
				name,
				version,
				subDependency)
				.VerifyDiagnostics()
				.VerifyEmit(_testOutputHelper, testEmittedAssembly: (original, assembly) =>
				{
					var assemblyLoadContext = new AssemblyLoadContext(null, true);
					var assemblyLoader = new MockAssemblyLoader(assembly);
					var dependencyLoader = new DependencyLoader(
						ImmutableArray.Create<IAssemblyLoader>(assemblyLoader),
						new DependencyAttributeReader(),
						GetLogger<DependencyLoader>());
					Assert.Throws<FlcException>(() => dependencyLoader.LoadDependenciesAsync(
						new ProjectInfo(
							"p",
							ImmutableArray.Create(""),
							references: ImmutableArray.Create(new Reference(
								Reference.ReferenceType.Assembly,
								name,
								version.ToString()))),
						assemblyLoadContext,
						Array.Empty<IAssembly>()).Result);
				});
		}

		public class MockAssemblyLoader : IAssemblyLoader
		{
			private readonly ImmutableArray<Assembly> _assemblies;

			public MockAssemblyLoader(params Assembly[] assemblies)
			{
				_assemblies = assemblies.ToImmutableArray();
			}
			public ValueTask<Assembly?> TryLoadAssemblyAsync(AssemblyLoadContext assemblyLoadContext, Dependency dependency, CancellationToken cancellationToken = default)
			{
				return new ValueTask<Assembly?>(_assemblies.FirstOrDefault(
					x => x.GetName().Name == $"{dependency.Name}${dependency.Version}"));
			}
		}
	}
}
