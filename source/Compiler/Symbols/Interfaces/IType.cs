﻿using System.Collections.Generic;

namespace FluentLang.Compiler.Symbols.Interfaces
{
	public interface IType
	{
		public sealed bool IsEquivalentTo(IType other) => IsEquivalentTo(other, null);

		internal bool IsEquivalentTo(IType other, Stack<(IType, IType)>? dependantEqualities);

		public bool IsSubtypeOf(IType other);
	}
}
