﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Formatting
{
    internal abstract partial class AbstractTriviaDataFactory
    {
        protected class FormattedWhitespace : TriviaData
        {
            private readonly string newString;

            public FormattedWhitespace(OptionSet optionSet, int lineBreaks, int indentation, string language) :
                base(optionSet, language)
            {
                this.LineBreaks = Math.Max(0, lineBreaks);
                this.Spaces = Math.Max(0, indentation);

                this.newString = CreateString();
            }

            private string CreateString()
            {
                if (this.SecondTokenIsFirstTokenOnLine)
                {
                    var builder = StringBuilderPool.Allocate();
                    for (int i = 0; i < this.LineBreaks; i++)
                    {
                        builder.AppendLine();
                    }

                    builder.Append(this.Spaces.CreateIndentationString(this.OptionSet.GetOption(FormattingOptions.UseTabs, this.Language), this.OptionSet.GetOption(FormattingOptions.TabSize, this.Language)));
                    return StringBuilderPool.ReturnAndFree(builder);
                }

                // space case. always use space
                return new string(' ', this.Spaces);
            }

            public override bool TreatAsElastic
            {
                get
                {
                    return false;
                }
            }

            public override bool IsWhitespaceOnlyTrivia
            {
                get
                {
                    return true;
                }
            }

            public override bool ContainsChanges
            {
                get
                {
                    return true;
                }
            }

            public override IEnumerable<TextChange> GetTextChanges(TextSpan textSpan)
            {
                return SpecializedCollections.SingletonEnumerable<TextChange>(new TextChange(textSpan, this.newString));
            }

            public override TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules)
            {
                throw new NotImplementedException();
            }

            public override TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override void Format(
                FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = TokenPairIndexNotNeeded)
            {
                throw new NotImplementedException();
            }
        }
    }
}
