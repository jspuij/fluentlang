﻿using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree.Xpath;
using FluentLang.Compiler.Generated;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Xunit;

namespace FluentLang.Compiler.Tests.Unit.Generated
{
	public class ParserTests
	{
		[Fact]
		public void GeneratesTreeForDemo()
		{
			using var reader = new StringReader(Example.DEMO);

			var input = new AntlrInputStream(reader);
			var lexer = new FluentLangLexer(input);
			var tokenStream = new CommonTokenStream(lexer);
			var parser = new FluentLangParser(tokenStream);
			var errorListener = new ErrorHandler();
			parser.ErrorListeners.Add(errorListener);
			var compilationUnit = parser.compilation_unit();
			Assert.Equal(Array.Empty<RecognitionException>(), errorListener.Errors);
			Assert.Equal("(compilation_unit (namespace_member_declarations (namespace_member_declaration (namespace_declaration namespace Math { (namespace_member_declaration (interface_declaration interface Counter (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature Count ( ) (type_declaration : (type (base_type int)))) ;) (interface_member_declaration (method_signature Increment ( ) (type_declaration : (type Counter))) ;) (interface_member_declaration (method_signature Save ( ) (type_declaration : (type Counter))) ;) (interface_member_declaration (method_signature Restore ( ) (type_declaration : (type Counter))) ;) })))) (namespace_member_declaration (method_declaration (method_signature Increment ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type Counter))) (method_body { (method_statement (declaration_statement let count = (expression (expression (expression this) . Count (invocation ( ))) (operator +) (expression (literal 1))) ;)) (method_statement (return_statement return (expression (expression this) + (object_patch (fully_qualified_method (fully_qualified_name Count)))) ;)) (method_declaration (method_signature Count ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression count) ;)) })) }))) (namespace_member_declaration (method_declaration (method_signature CreateCounter ( ) (type_declaration : (type Counter))) (method_body { (method_statement (return_statement return (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name Count))) + (object_patch (fully_qualified_method (fully_qualified_name Increment))) + (object_patch (fully_qualified_method (fully_qualified_name Save))) + (object_patch (fully_qualified_method (fully_qualified_name Restore))) + (object_patch (fully_qualified_method (fully_qualified_name StoredValue)))) ;)) (method_declaration (method_signature Count ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 0)) ;)) })) (method_declaration (method_signature Save ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type Counter))) (method_body { (method_statement (declaration_statement let current = (expression (expression this) . Count (invocation ( ))) ;)) (method_statement (return_statement return (expression (expression this) + (object_patch (fully_qualified_method (fully_qualified_name StoredValue)))) ;)) (method_declaration (method_signature StoredValue ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression current) ;)) })) })) (method_declaration (method_signature Restore ( (parameters (parameter this (type_declaration : (type SavableCounter)))) ) (type_declaration : (type Counter))) (method_body { (method_statement (declaration_statement let stored = (expression (expression this) . StoredValue (invocation ( ))) ;)) (method_statement (return_statement return (expression (expression this) + (object_patch (fully_qualified_method (fully_qualified_name Count)))) ;)) (method_declaration (method_signature Count ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression stored) ;)) })) })) (method_declaration (method_signature StoredValue ( (parameters (parameter this (type_declaration : (type Counter)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 0)) ;)) })) (method_statement (interface_declaration interface SavableCounter (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature StoredValue ( ) (type_declaration : (type (base_type int)))) ;) }) + (simple_anonymous_interface_declaration Counter)))) }))) })) (namespace_member_declaration (namespace_declaration namespace Disambiguation { (namespace_member_declaration (method_declaration (method_signature M ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (return_statement return (expression (empty_interface { })) ;)) }))) (namespace_member_declaration (method_declaration (method_signature M ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature P ( ) (type_declaration : (type (base_type int)))) ;) })))))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (return_statement return (expression (empty_interface { })) ;)) }))) (namespace_member_declaration (method_declaration (method_signature M ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 0)) ;)) }))) (namespace_member_declaration (method_declaration (method_signature SomeMethod ( ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (return_statement return (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name M) ( (fully_qualified_method_parameters (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) + (object_patch (fully_qualified_method (fully_qualified_name M) ( (fully_qualified_method_parameters (type (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature P ( ) (type_declaration : (type (base_type int)))) ;) })))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) + (object_patch (fully_qualified_method (fully_qualified_name M) ( (fully_qualified_method_parameters (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))) ) (type_declaration : (type (base_type int)))))) ;)) }))) (namespace_member_declaration (method_declaration (method_signature M1 ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 0)) ;)) }))) (namespace_member_declaration (namespace_declaration namespace Inner { (namespace_member_declaration (method_declaration (method_signature M1 ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 1)) ;)) }))) (namespace_member_declaration (method_declaration (method_signature SomeMethod ( (parameters (parameter chooseInner (type_declaration : (type (base_type bool))))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (return_statement return (expression if ( (expression chooseInner) ) (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name M1)))) else (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name Disambiguation . M1))))) ;)) }))) })) (namespace_member_declaration (method_declaration (method_signature SomeMethod ( (parameters (parameter chooseInner (type_declaration : (type (base_type bool))))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (return_statement return (expression if ( (expression chooseInner) ) (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name Inner . M1)))) else (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name M1))))) ;)) }))) })) (namespace_member_declaration (namespace_declaration namespace Exporting { (namespace_member_declaration (interface_declaration export interface Adder (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature Add ( (parameters (parameter a (type_declaration : (type (base_type int)))) , (parameter b (type_declaration : (type (base_type int))))) ) (type_declaration : (type (base_type int)))) ;) })))) (namespace_member_declaration (method_declaration export (method_signature Sum3 ( (parameters (parameter a (type_declaration : (type (base_type int)))) , (parameter b (type_declaration : (type (base_type int)))) , (parameter c (type_declaration : (type (base_type int)))) , (parameter adder (type_declaration : (type Adder)))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (declaration_statement let intermediate = (expression (expression adder) . Add (invocation ( (arguments (expression a) , (expression b)) ))) ;)) (method_statement (return_statement return (expression (expression adder) . Add (invocation ( (arguments (expression intermediate) , (expression c)) ))) ;)) }))) })) (namespace_member_declaration (namespace_declaration namespace Mixins { (namespace_member_declaration (interface_declaration interface PrintValue (anonymous_interface_declaration (simple_anonymous_interface_declaration { (interface_member_declaration (method_signature Value ( ) (type_declaration : (type (base_type int)))) ;) (interface_member_declaration (method_signature PrintValue ( ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) ;) })))) (namespace_member_declaration (method_declaration (method_signature GetPrintValue ( ) (type_declaration : (type PrintValue))) (method_body { (method_statement (return_statement return (expression (expression (empty_interface { })) + (object_patch (fully_qualified_method (fully_qualified_name Value))) + (object_patch (fully_qualified_method (fully_qualified_name PrintValue)))) ;)) (method_declaration (method_signature Value ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 42)) ;)) })) (method_declaration (method_signature PrintValue ( (parameters (parameter this (type_declaration : (type PrintValue)))) ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (declaration_statement let _ = (expression (expression (fully_qualified_name System . Console . GetConsole) (invocation ( ))) . WriteLine (invocation ( (arguments (expression (expression this) . Value (invocation ( )))) ))) ;)) (method_statement (return_statement return (expression (empty_interface { })) ;)) })) }))) (namespace_member_declaration (method_declaration (method_signature PrintMixedInValue ( ) (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { })))))) (method_body { (method_statement (declaration_statement let originalPrintValue = (expression (fully_qualified_name GetPrintValue) (invocation ( ))) ;)) (method_statement (declaration_statement let mixedinPrintValue = (expression (expression (empty_interface { })) + (object_patch mixin (expression originalPrintValue))) ;)) (method_statement (declaration_statement let _ = (expression (expression (fully_qualified_name System . Console . GetConsole) (invocation ( ))) . WriteLine (invocation ( (arguments (expression (expression originalPrintValue) + (object_patch (fully_qualified_method (fully_qualified_name Value))))) ))) ;)) (method_statement (declaration_statement let _ = (expression (expression (fully_qualified_name System . Console . GetConsole) (invocation ( ))) . WriteLine (invocation ( (arguments (expression (expression mixedinPrintValue) + (object_patch (fully_qualified_method (fully_qualified_name Value))))) ))) ;)) (method_statement (declaration_statement let _ = (expression (expression ( (expression (expression originalPrintValue) + (object_patch (fully_qualified_method (fully_qualified_name Value)))) )) . PrintValue (invocation ( ))) ;)) (method_statement (declaration_statement let _ = (expression (expression ( (expression (expression mixedinPrintValue) + (object_patch (fully_qualified_method (fully_qualified_name Value)))) )) . PrintValue (invocation ( ))) ;)) (method_statement (return_statement return (expression (empty_interface { })) ;)) (method_declaration (method_signature Value ( (parameters (parameter this (type_declaration : (type (anonymous_interface_declaration (simple_anonymous_interface_declaration (empty_interface { }))))))) ) (type_declaration : (type (base_type int)))) (method_body { (method_statement (return_statement return (expression (literal 73)) ;)) })) }))) }))) <EOF>)", 
				compilationUnit.ToStringTree(parser));
		}

		public class ErrorHandler : BaseErrorListener
		{
			public List<RecognitionException> Errors { get; } = new List<RecognitionException>();
			public override void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
			{
				base.ReportAmbiguity(recognizer, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
			}

			public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
			{
				base.ReportAttemptingFullContext(recognizer, dfa, startIndex, stopIndex, conflictingAlts, conflictState);
			}

			public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
			{
				base.ReportContextSensitivity(recognizer, dfa, startIndex, stopIndex, prediction, acceptState);
			}

			public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
			{
				Errors.Add(e);
			}
		}
	}
}
