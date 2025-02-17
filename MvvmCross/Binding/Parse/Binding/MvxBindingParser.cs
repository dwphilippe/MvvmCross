﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvvmCross.Base;
using MvvmCross.Exceptions;

namespace MvvmCross.Binding.Parse.Binding
{
    public abstract class MvxBindingParser
        : MvxParser, IMvxBindingParser
    {
        protected abstract MvxSerializableBindingDescription ParseBindingDescription();

        public bool TryParseBindingDescription(string text, out MvxSerializableBindingDescription requestedDescription)
        {
            try
            {
                Reset(text);
                requestedDescription = ParseBindingDescription();
                return true;
            }
            catch (Exception exception)
            {
                MvxBindingLog.Error("Problem parsing binding {0}", exception.ToLongString());
                requestedDescription = null;
                return false;
            }
        }

        public bool TryParseBindingSpecification(string text, out MvxSerializableBindingSpecification requestedBindings)
        {
            try
            {
                Reset(text);

                var toReturn = new MvxSerializableBindingSpecification();
                while (!IsComplete)
                {
                    SkipWhitespaceAndDescriptionSeparators();
                    var result = ParseTargetPropertyNameAndDescription();
                    toReturn[result.Key] = result.Value;
                    SkipWhitespaceAndDescriptionSeparators();
                }

                requestedBindings = toReturn;
                return true;
            }
            catch (Exception exception)
            {
                MvxBindingLog.Error("Problem parsing binding {0}", exception.ToLongString());
                requestedBindings = null;
                return false;
            }
        }

        protected KeyValuePair<string, MvxSerializableBindingDescription> ParseTargetPropertyNameAndDescription()
        {
            var targetPropertyName = ReadTargetPropertyName();
            SkipWhitespace();
            var description = ParseBindingDescription();
            return new KeyValuePair<string, MvxSerializableBindingDescription>(targetPropertyName, description);
        }

        protected void ParseEquals(string block)
        {
            if (IsComplete)
                throw new MvxException("Cannot terminate binding expression during option {0} in {1}",
                                       block,
                                       FullText);
            if (CurrentChar != '=')
                throw new MvxException("Must follow binding option {0} with an '=' in {1}",
                                       block,
                                       FullText);

            MoveNext();
            if (IsComplete)
                throw new MvxException("Cannot terminate binding expression during option {0} in {1}",
                                       block,
                                       FullText);
        }

        protected MvxBindingMode ReadBindingMode()
        {
            return (MvxBindingMode)ReadEnumerationValue(typeof(MvxBindingMode));
        }

        protected string ReadTextUntilNonQuotedOccurrenceOfAnyOf(params char[] terminationCharacters)
        {
            var terminationLookup = terminationCharacters.ToDictionary(c => c, c => true);
            SkipWhitespace();
            var toReturn = new StringBuilder();

            while (!IsComplete)
            {
                var currentChar = CurrentChar;
                if (currentChar == '\'' || currentChar == '\"')
                {
                    var subText = ReadQuotedString();
                    toReturn.Append(currentChar);
                    toReturn.Append(subText);
                    toReturn.Append(currentChar);
                    continue;
                }

                if (terminationLookup.ContainsKey(currentChar))
                {
                    break;
                }

                toReturn.Append(currentChar);
                MoveNext();
            }

            return toReturn.ToString();
        }

        protected string ReadTargetPropertyName()
        {
            return ReadValidCSharpName();
        }

        protected void SkipWhitespaceAndOptionSeparators()
        {
            SkipWhitespaceAndCharacters(',');
        }

        protected void SkipWhitespaceAndDescriptionSeparators()
        {
            SkipWhitespaceAndCharacters(';');
        }
    }
}
