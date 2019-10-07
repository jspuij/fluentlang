﻿using FluentLang.Compiler.Diagnostics;
using FluentLang.Compiler.Symbols.Interfaces;
using FluentLang.Compiler.Symbols.Interfaces.MethodBody;
using System.Collections.Immutable;
using System.Linq;

namespace FluentLang.Compiler.Symbols.ErrorSymbols
{
	internal class ErrorMethod : IMethod, IErrorSymbol
	{
		public static ErrorMethod Instance { get; } = new ErrorMethod(new QualifiedName("<Error Method>"), 0);

		public ErrorMethod(QualifiedName name, int numParameters)
		{
			FullyQualifiedName = name;
			Parameters = Enumerable.Repeat<IParameter>(ErrorParameter.Instance, numParameters).ToImmutableArray();
		}

		public QualifiedName FullyQualifiedName { get; }

		public IType ReturnType => ErrorType.Instance;

		public ImmutableArray<IParameter> Parameters { get; }

		public ImmutableArray<IInterface> LocalInterfaces => ImmutableArray<IInterface>.Empty;

		public ImmutableArray<IMethod> LocalMethods => ImmutableArray<IMethod>.Empty;

		public IMethod? DeclaringMethod => null;

		public ImmutableArray<Diagnostic> AllDiagnostics => ImmutableArray<Diagnostic>.Empty;

		public ImmutableArray<IStatement> Statements => ImmutableArray<IStatement>.Empty;

		void ISymbol.EnsureAllLocalDiagnosticsCollected()
		{
		}
	}
}