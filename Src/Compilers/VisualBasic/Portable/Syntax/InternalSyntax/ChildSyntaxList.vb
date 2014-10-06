﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Namespace Microsoft.CodeAnalysis.VisualBasic.Syntax.InternalSyntax

    Partial Friend Structure ChildSyntaxList
        Private ReadOnly _node As VBSyntaxNode

        Friend Sub New(node As VBSyntaxNode)
            _node = node
        End Sub

        Public Function GetEnumerator() As Enumerator
            Return New Enumerator(_node)
        End Function
    End Structure
End Namespace
