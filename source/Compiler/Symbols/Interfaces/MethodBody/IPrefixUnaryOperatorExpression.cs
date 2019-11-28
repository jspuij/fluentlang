﻿using FluentLang.Compiler.Symbols.Visitor;

namespace FluentLang.Compiler.Symbols.Interfaces.MethodBody
{
	public interface IPrefixUnaryOperatorExpression : IExpression
	{
		T IVisitableSymbol.Visit<T>(ISymbolVisitor<T> visitor)
			=> visitor.Visit(this);
		Operator Operator { get; }
		IExpression Expression { get; }
	}
}