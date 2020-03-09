﻿namespace FluentLang.Compiler.Diagnostics
{
	public enum ErrorCode
	{
		SyntaxError,
		DuplicateInterfaceDeclaration,
		DuplicateMethodDeclaration,
		TypeNotFound,
		CannotReferenceSelfAsAdditiveInterface,
		InvalidParseTree,
		MethodNotFound,
		AmbigiousInterfaceReference,
		AmbigiousMethodReference,
		MismatchedTypes,
		CanOnlyPatchInterface,
		CannotMixInNonInterface,
		CannotPatchInMethodWithoutParameters,
		ResultantTypeOfObjectPatchingExpressionIsNotSubtypeOfFirstParameterOfPatchedInMethod,
		InvalidArgument,
		InvalidIntegerLiteral,
		IntegerLiteralOutOfRange,
		InvalidCharLiteral,
		InvalidRealLiteral,
		NonBooleanCondition,
		NoBestType,
		InvalidLocalReference,
		MemberNotFound,
		LastStatementMustBeReturnStatement,
		OnlyLastStatementCanBeReturnStatement,
		ReturnTypeDoesNotMatch,
		MethodMustContainAtLeastOneStatement,
		HidesLocal,
		InvalidEscapeSequence,
		UseOfMethodWhichCapturesUnassignedLocals,
		ParametersShareNames,
		InvalidMetadataAssembly,
		CannotUseUnexportedInterfaceFromExportedMember,
		MultipleVersionsOfSameAssembly,
		TooManyOptionsInUnion,
		CannotMatchOnNonUnion,
		MatchNotExhaustive,
		CanOnlyCombineInterfaces,
		TypeParametersShareNames,
		CannotConstrainToPrimitive,
		WrongNumberOfTypeArguments,
		TypeArgumentDoesntMatchConstraints,
	}
}
