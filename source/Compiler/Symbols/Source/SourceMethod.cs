﻿using FluentLang.Compiler.Diagnostics;
using FluentLang.Compiler.Symbols.Interfaces;
using System;
using System.Collections.Immutable;
using System.Linq;
using static FluentLang.Compiler.Generated.FluentLangParser;

namespace FluentLang.Compiler.Symbols.Source
{
	internal class SourceMethod : IMethod
	{
		private readonly Method_declarationContext _context;
		private readonly SourceSymbolContext _sourceSymbolContext;
		private SourceSymbolContext? _childSourceSymbolContext;

		private readonly Lazy<IType> _returnType;
		private readonly Lazy<ImmutableArray<IParameter>> _parameters;
		private readonly Lazy<ImmutableArray<IInterface>> _localInterfaces;
		private readonly Lazy<ImmutableArray<IMethod>> _localMethods;

		private readonly DiagnosticBag _diagnostics;
		private readonly Lazy<ImmutableArray<Diagnostic>> _allDiagnostics;

		public SourceMethod(
			Method_declarationContext context,
			SourceSymbolContext sourceSymbolContext,
			DiagnosticBag diagnostics)
		{
			_context = context;
			_sourceSymbolContext = sourceSymbolContext;
			_diagnostics = diagnostics.CreateChildBag(this);
			var @namespace = _sourceSymbolContext.Scope is null ? _sourceSymbolContext.NameSpace : null;
			FullyQualifiedName = new QualifiedName(context.method_signature().UPPERCASE_IDENTIFIER().Symbol.Text, @namespace);

			_returnType = new Lazy<IType>(BindReturnType);
			_parameters = new Lazy<ImmutableArray<IParameter>>(BindParameters);
			_localInterfaces = new Lazy<ImmutableArray<IInterface>>(BindLocalInterfaces);
			_localMethods = new Lazy<ImmutableArray<IMethod>>(BindLocalMethods);

			_allDiagnostics = new Lazy<ImmutableArray<Diagnostic>>(() =>
			{
				_diagnostics.EnsureAllDiagnosticsCollectedForSymbol();
				return _diagnostics.ToImmutableArray();
			});
		}

		private IType BindReturnType()
		{
			return _context.method_signature().BindReturnType(_sourceSymbolContext, _diagnostics);
		}

		private ImmutableArray<IParameter> BindParameters()
		{
			return
				_context
				.method_signature()
				.BindParameters(_sourceSymbolContext, _diagnostics);
		}

		private ImmutableArray<IInterface> BindLocalInterfaces()
		{
			var interfaceDeclarations =
				_context
				.method_body()
				.interface_declaration();

			foreach (var duplicateGroup in interfaceDeclarations
				.GroupBy(x => x.UPPERCASE_IDENTIFIER().Symbol.Text)
				.Where(x => x.Count() > 1))
			{
				foreach(var duplicate in duplicateGroup)
				{
					_diagnostics.Add(new Diagnostic(
							new Location(duplicate.UPPERCASE_IDENTIFIER()),
							ErrorCode.DuplicateInterfaceDeclaration,
							ImmutableArray.Create<object?>(duplicateGroup.Key)));
				}
			}

			return
				interfaceDeclarations
				.Select(x => new SourceInterface(
					x.anonymous_interface_declaration(),
					_childSourceSymbolContext ??= _sourceSymbolContext.WithScope(this),
					fullyQualifiedName: new QualifiedName(x.UPPERCASE_IDENTIFIER().Symbol.Text),
					_diagnostics))
				.ToImmutableArray<IInterface>();
		}

		private ImmutableArray<IMethod> BindLocalMethods()
		{
			var methodDeclarations =
				_context
				.method_body()
				.method_declaration();

			foreach (var duplicateGroup in methodDeclarations
				.GroupBy(x => x.method_signature().UPPERCASE_IDENTIFIER().Symbol.Text)
				.Where(x => x.Count() > 1))
			{
				foreach (var duplicate in duplicateGroup)
				{
					_diagnostics.Add(new Diagnostic(
							new Location(duplicate.method_signature().UPPERCASE_IDENTIFIER()),
							ErrorCode.DuplicateMethodDeclaration,
							ImmutableArray.Create<object?>(duplicateGroup.Key)));
				}
			}

			return
				methodDeclarations
				.Select(x => new SourceMethod(
					x,
					_childSourceSymbolContext ??= _sourceSymbolContext.WithScope(this),
					_diagnostics))
				.ToImmutableArray<IMethod>();
		}

		public QualifiedName FullyQualifiedName { get; }

		public IType ReturnType => _returnType.Value;

		public ImmutableArray<IParameter> Parameters => _parameters.Value;

		public ImmutableArray<IInterface> LocalInterfaces => _localInterfaces.Value;

		public ImmutableArray<IMethod> LocalMethods => _localMethods.Value;

		public IMethod? DeclaringMethod => _sourceSymbolContext.Scope;

		public ImmutableArray<Diagnostic> AllDiagnostics => _allDiagnostics.Value;

		void ISymbol.EnsureAllLocalDiagnosticsCollected()
		{
			// Touch all lazy fields to force binding;

			_ = _returnType.Value;
			_ = _parameters.Value;
			_ = _localInterfaces.Value;
			_ = _localMethods.Value;
		}
	}
}

