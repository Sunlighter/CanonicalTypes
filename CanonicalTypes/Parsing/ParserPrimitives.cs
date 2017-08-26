using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sunlighter.CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        private class CharParserEmptyString : ICharParser<string>
        {
            public CharParserEmptyString()
            {

            }

            public ParseResult<string> TryParse(CharParserContext context, int off, int len)
            {
                if (Utility.IsValidRange(context.Text, off, len))
                {
                    return new ParseSuccess<string>(off, 0, string.Empty);
                }
                else
                {
                    return new ParseFailure<string>(off, "Invalid range");
                }
            }
        }

        private class CharParserExact : ICharParser<string>
        {
            private readonly string target;
            private readonly StringComparison style;

            public CharParserExact(string target, StringComparison style)
            {
                this.target = target;
                this.style = style;
            }

            public ParseResult<string> TryParse(CharParserContext context, int off, int len)
            {
                if (len < target.Length)
                {
                    return new ParseFailure<string>(off, "Expected \"" + target + "\", found EOF");
                }
                else
                {
                    string text = context.Text;

                    if (Utility.IsValidRange(text, off, len))
                    {
                        string inputRegion = text.Substring(off, target.Length);
                        if (string.Compare(inputRegion, target, style) == 0)
                        {
                            return new ParseSuccess<string>(off, target.Length, inputRegion);
                        }
                        else
                        {
                            return new ParseFailure<string>(off, "Expected \"" + target + "\"");
                        }
                    }
                    else
                    {
                        return new ParseFailure<string>(off, "Invalid range");
                    }
                }
            }
        }

        public static ICharParser<string> ParseExact(string target, StringComparison style)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (target.Length == 0) return new CharParserEmptyString();

            return new CharParserExact(target, style);
        }

        private class CharParserEof : ICharParser<Nothing>
        {
            public CharParserEof()
            {

            }

            public ParseResult<Nothing> TryParse(CharParserContext context, int pos, int len)
            {
                if (Utility.IsValidRange(context.Text, pos, len))
                {
                    if (len == 0)
                    {
                        return new ParseSuccess<Nothing>(pos, 0, Nothing.Value);
                    }
                    else
                    {
                        return new ParseFailure<Nothing>(pos, "EOF expected");
                    }
                }
                else
                {
                    return new ParseFailure<Nothing>(pos, "Invalid range");
                }
            }
        }

        private static Lazy<CharParserEof> charParserEof = new Lazy<CharParserEof>(() => new CharParserEof(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ICharParser<Nothing> ParseEOF => charParserEof.Value;

        private class CharParserTryConvert<TIn, TOut> : ICharParser<TOut>
        {
            private ICharParser<TIn> subParser;
            private Func<TIn, Option<TOut>> conversionFunc;
            private string failureMessage;

            public CharParserTryConvert(ICharParser<TIn> subParser, Func<TIn, Option<TOut>> conversionFunc, string failureMessage)
            {
                this.subParser = subParser;
                this.conversionFunc = conversionFunc;
                this.failureMessage = failureMessage;
            }

            public ParseResult<TOut> TryParse(CharParserContext context, int off, int len)
            {
                ParseResult<TIn> pr1 = context.TryParseAt(subParser, off, len);

                return pr1.Visit<ParseResult<TOut>>
                (
                    success =>
                    {
                        Option<TOut> conversionResult = conversionFunc(success.Value);

                        if (conversionResult.HasValue)
                        {
                            return new ParseSuccess<TOut>(success.Position, success.Length, conversionResult.Value);
                        }
                        else
                        {
                            return new ParseFailure<TOut>(success.Position, failureMessage);
                        }
                    },
                    failure =>
                    {
                        return new ParseFailure<TOut>(failure.Errors);
                    }
                );
            }
        }

        public static ICharParser<TOut> ParseTryConvert<TIn, TOut>(ICharParser<TIn> subParser, Func<TIn, Option<TOut>> tryConversionFunc, string failureMessage)
        {
            return new CharParserTryConvert<TIn, TOut>(subParser, tryConversionFunc, failureMessage);
        }

        private class CharParserConvert<TIn, TOut> : ICharParser<TOut>
        {
            private ICharParser<TIn> subParser;
            private Func<TIn, TOut> conversionFunc;
            private string failureMessage;

            public CharParserConvert(ICharParser<TIn> subParser, Func<TIn, TOut> conversionFunc, string failureMessage)
            {
                this.subParser = subParser;
                this.conversionFunc = conversionFunc;
                this.failureMessage = failureMessage;
            }

            public ParseResult<TOut> TryParse(CharParserContext context, int off, int len)
            {
                ParseResult<TIn> pr1 = context.TryParseAt(subParser, off, len);

                return pr1.Visit<ParseResult<TOut>>
                (
                    success =>
                    {
                        try
                        {
                            TOut conversionResult = conversionFunc(success.Value);
                            return new ParseSuccess<TOut>(success.Position, success.Length, conversionResult);
                        }
                        catch(Exception exc)
                        {
                            return new ParseFailure<TOut>(success.Position, failureMessage ?? exc.ToString());
                        }
                    },
                    failure =>
                    {
                        return new ParseFailure<TOut>(failure.Errors);
                    }
                );
            }
        }

        public static ICharParser<TOut> ParseConvert<TIn, TOut>(ICharParser<TIn> subParser, Func<TIn, TOut> conversionFunc, string failureMessage)
        {
            return new CharParserConvert<TIn, TOut>(subParser, conversionFunc, failureMessage);
        }

        private class CharParserSequence<T> : ICharParser<ImmutableList<T>>
        {
            private ImmutableList<ICharParser<T>> subParsers;

            public CharParserSequence(ImmutableList<ICharParser<T>> subParsers)
            {
                this.subParsers = subParsers;
            }

            public ParseResult<ImmutableList<T>> TryParse(CharParserContext context, int off, int len)
            {
                int pos = off;
                ImmutableList<T> results = ImmutableList<T>.Empty;
                foreach (ICharParser<T> subParser in subParsers)
                {
                    ParseResult<T> subResult = context.TryParseAt(subParser, pos, len - (pos - off));

                    if (subResult is ParseSuccess<T>)
                    {
                        var subSuccess = (ParseSuccess<T>)subResult;
                        pos += subSuccess.Length;
                        results = results.Add(subSuccess.Value);
                    }
                    else
                    {
                        var subFailure = (ParseFailure<T>)subResult;
                        return new ParseFailure<ImmutableList<T>>(subFailure.Errors);
                    }
                    
                }

                return new ParseSuccess<ImmutableList<T>>(off, pos - off, results);
            }
        }

        public static ICharParser<ImmutableList<T>> ParseSequence<T>(ImmutableList<ICharParser<T>> subParsers)
        {
            return new CharParserSequence<T>(subParsers);
        }

        public static ICharParser<ImmutableList<T>> ParseSequence<T>(params ICharParser<T>[] subParsers)
        {
            return new CharParserSequence<T>(subParsers.ToImmutableList());
        }

        private class CharParserAlternatives<T> : ICharParser<T>
        {
            private ImmutableList<ICharParser<T>> subParsers;

            public CharParserAlternatives(ImmutableList<ICharParser<T>> subParsers)
            {
                if (subParsers == null) throw new ArgumentNullException(nameof(subParsers));

                if (subParsers.Count == 0) throw new ArgumentException($"{nameof(subParsers)} must have non-zero length");
                this.subParsers = subParsers;
            }

            public ParseResult<T> TryParse(CharParserContext context, int off, int len)
            {
                ParseFailure<T> error = null;

                foreach (ICharParser<T> subParser in subParsers)
                {
                    ParseResult<T> pr = context.TryParseAt(subParser, off, len);

                    if (pr is ParseSuccess<T>)
                    {
                        return pr;
                    }
                    else
                    {
                        ParseFailure<T> pf = (ParseFailure<T>)pr;
                        if (error == null)
                        {
                            error = pf;
                        }
                        else
                        {
                            error = new ParseFailure<T>(error.Errors.AddRange(pf.Errors));
                        }
                    }
                }

                System.Diagnostics.Debug.Assert(error != null);

                return error;
            }
        }

        public static ICharParser<T> ParseAlternatives<T>(ImmutableList<ICharParser<T>> subParsers)
        {
            return new CharParserAlternatives<T>(subParsers);
        }

        public static ICharParser<T> ParseAlternatives<T>(params ICharParser<T>[] subParsers)
        {
            return new CharParserAlternatives<T>(subParsers.ToImmutableList());
        }

        private class CharParserFromRegex : ICharParser<Match>
        {
            private Regex regex;
            private string failureMessage;

            public CharParserFromRegex(Regex regex, string failureMessage)
            {
                this.regex = regex;
                this.failureMessage = failureMessage;
            }

            public ParseResult<Match> TryParse(CharParserContext context, int off, int len)
            {
                Match m = regex.Match(context.Text, off, len);

                if (m.Success && m.Index == off)
                {
                    return new ParseSuccess<Match>(m.Index, m.Length, m);
                }
                else
                {
                    return new ParseFailure<Match>(off, failureMessage ?? $"Failed to parse {regex.ToString().Quoted()}");
                }
            }
        }

        public static ICharParser<Match> ParseFromRegex(Regex regex, string failureMessage)
        {
            return new CharParserFromRegex(regex, failureMessage);
        }

        private class CharParserOptRep<T> : ICharParser<ImmutableList<T>>
        {
            private readonly ICharParser<T> subParser;
            private readonly bool optional;
            private readonly bool repeating;

            public CharParserOptRep(ICharParser<T> subParser, bool optional, bool repeating)
            {
                if (subParser == null) throw new ArgumentNullException(nameof(subParser));

                this.subParser = subParser;
                this.optional = optional;
                this.repeating = repeating;

                if (!optional && !repeating)
                {
                    throw new ArgumentException("CharParserOptRep must be optional or repeating or both");
                }
            }

            public ParseResult<ImmutableList<T>> TryParse(CharParserContext context, int off, int len)
            {
                int pos = off;
                ImmutableList<T> results = ImmutableList<T>.Empty;

                if (repeating)
                {
                    ParseResult<T> pr;
                
                    while(true)
                    {
                        pr = context.TryParseAt(subParser, pos, len - (pos - off));
                        if (pr is ParseFailure<T>) break;
                        ParseSuccess<T> ps = (ParseSuccess<T>)pr;
                        pos += ps.Length;
                        results = results.Add(ps.Value);
                    }

                    if (!optional && results.Count == 0)
                    {
                        return new ParseFailure<ImmutableList<T>>(((ParseFailure<T>)pr).Errors);
                    }
                    else
                    {
                        return new ParseSuccess<ImmutableList<T>>(off, pos - off, results);
                    }
                }
                else if (optional)
                {
                    ParseResult<T> pr = context.TryParseAt(subParser, off, len);

                    return pr.Visit<ParseResult<ImmutableList<T>>>
                    (
                        success =>
                        {
                            return new ParseSuccess<ImmutableList<T>>(success.Position, success.Length, ImmutableList<T>.Empty.Add(success.Value));
                        },
                        failure =>
                        {
                            return new ParseSuccess<ImmutableList<T>>(off, 0, ImmutableList<T>.Empty);
                        }
                    );
                }
                else
                {
                    throw new InvalidOperationException("Neither optional nor repeating");
                }
            }
        }

        public static ICharParser<ImmutableList<T>> ParseOptRep<T>(ICharParser<T> subParser, bool optional, bool repeating)
        {
            if (!optional && !repeating)
            {
                return ParseConvert(subParser, result => ImmutableList<T>.Empty.Add(result), null);
            }
            else
            {
                return new CharParserOptRep<T>(subParser, optional, repeating);
            }
        }

        private class CharParserVariable<T> : ICharParser<T>
        {
            public CharParserVariable()
            {
                SubParser = null;
            }

            public ICharParser<T> SubParser { get; set; }

            public ParseResult<T> TryParse(CharParserContext context, int off, int len)
            {
                if (SubParser == null)
                {
                    return new ParseFailure<T>(off, "Reference to parser not initialized");
                }
                else
                {
                    return context.TryParseAt(SubParser, off, len);
                }
            }
        }

        public static ICharParser<T> GetParseVariable<T>()
        {
            return new CharParserVariable<T>();
        }

        public static void SetParseVariable<T>(ICharParser<T> target, ICharParser<T> subParser)
        {
            if (target is CharParserVariable<T>)
            {
                var target2 = (CharParserVariable<T>)target;

                target2.SubParser = subParser;
            }
            else throw new ArgumentException($"{nameof(target)} is not a parse variable");
        }
    }
}
