﻿using FluentLang.Compiler.Symbols.Visitor;
using System.Diagnostics.CodeAnalysis;

namespace FluentLang.Compiler.Symbols.Interfaces.MethodBody
{
	public interface IReturnStatement : IStatement
	{
		[return: MaybeNull]
		T IVisitableSymbol.Visit<T>(ISymbolVisitor<T> visitor)
			=> visitor.Visit(this);
		IExpression Expression { get; }
	}
}