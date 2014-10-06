﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
'-----------------------------------------------------------------------------
' Contains the definition of the BlockContext
'-----------------------------------------------------------------------------

Namespace Microsoft.CodeAnalysis.VisualBasic.Syntax.InternalSyntax

    Friend NotInheritable Class EventBlockContext
        Inherits DeclarationContext

        Friend Sub New(statement As StatementSyntax, prevContext As BlockContext)
            MyBase.New(SyntaxKind.EventBlock, statement, prevContext)

        End Sub

        Friend Overrides Function ProcessSyntax(node As VBSyntaxNode) As BlockContext

            Select Case node.Kind
                Case SyntaxKind.AddHandlerAccessorStatement
                    Return New MethodBlockContext(SyntaxKind.AddHandlerBlock, DirectCast(node, StatementSyntax), Me)

                Case SyntaxKind.RemoveHandlerAccessorStatement
                    Return New MethodBlockContext(SyntaxKind.RemoveHandlerBlock, DirectCast(node, StatementSyntax), Me)

                Case SyntaxKind.RaiseEventAccessorStatement
                    Return New MethodBlockContext(SyntaxKind.RaiseEventBlock, DirectCast(node, StatementSyntax), Me)

                Case SyntaxKind.AddHandlerBlock,
                    SyntaxKind.RemoveHandlerBlock,
                    SyntaxKind.RaiseEventBlock
                    ' Handle any block created by this context
                    Add(node)

                Case Else
                    ' TODO - Dev10 sometimes reports ERRID_EventMemberSyntax.  This occurs only
                    ' when the statement is a malformed declaration. Ordinary statements produce ERRID_InvInsideEndsEvent. 
                    ' Is it worth preserving this behavior?

                    'End the current block and add the block to the context above
                    Dim context = EndBlock(Nothing)
                    'Let the outer context process this statement
                    Return context.ProcessSyntax(Parser.ReportSyntaxError(node, ERRID.ERR_InvInsideEndsEvent))
            End Select

            Return Me
        End Function

        Friend Overrides Function TryLinkSyntax(node As VBSyntaxNode, ByRef newContext As BlockContext) As LinkResult
            newContext = Nothing

            If KindEndsBlock(node.Kind) Then
                Return UseSyntax(node, newContext)
            End If

            Select Case node.Kind

                Case _
                    SyntaxKind.AddHandlerAccessorStatement,
                    SyntaxKind.RemoveHandlerAccessorStatement,
                    SyntaxKind.RaiseEventAccessorStatement
                    Return UseSyntax(node, newContext)

                Case _
                    SyntaxKind.AddHandlerBlock,
                    SyntaxKind.RemoveHandlerBlock,
                    SyntaxKind.RaiseEventBlock
                    Return UseSyntax(node, newContext, DirectCast(node, AccessorBlockSyntax).End.IsMissing)

                Case Else
                    Return TryUseStatement(node, newContext)
            End Select
        End Function

        Friend Overrides Function CreateBlockSyntax(statement As StatementSyntax) As VBSyntaxNode

            Dim beginEvent As EventStatementSyntax = Nothing
            Dim endEvent As EndBlockStatementSyntax = DirectCast(statement, EndBlockStatementSyntax)
            GetBeginEndStatements(beginEvent, endEvent)

            Dim result = SyntaxFactory.EventBlock(beginEvent, Body(Of AccessorBlockSyntax)(), endEvent)

            FreeStatements()

            Return result
        End Function

    End Class

End Namespace