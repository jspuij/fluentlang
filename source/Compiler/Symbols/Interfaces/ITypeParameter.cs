﻿using FluentLang.Compiler.Symbols.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLang.Compiler.Symbols.Interfaces
{
	public interface ITypeParameter : IType, IEquatable<ITypeParameter>
	{
		[return: MaybeNull]
		T IVisitableSymbol.Visit<T>(ISymbolVisitor<T> visitor)
			=> visitor.Visit(this);

		public string Name { get; }
		public IType? ConstrainedTo { get; }

		bool IType.IsEquivalentTo(IType other, Stack<(IType, IType)>? dependantEqualities)
		{
			return ReferenceEquals(this, other);
		}

		bool IType.IsSubtypeOf(IType other)
		{
			return IsEquivalentTo(other) || (ConstrainedTo?.IsSubtypeOf(other) ?? false);
		}

		IType IType.Substitute(IReadOnlyDictionary<ITypeParameter, IType> substitutions)
		{
			return substitutions.TryGetValue(this, out var substitution)
				? substitution
				: this;
		}

		bool IEquatable<ITypeParameter>.Equals(ITypeParameter other) => ReferenceEquals(this, other);
	}
}

