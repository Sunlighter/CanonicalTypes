﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sunlighter.CanonicalTypes.Parsing;
using Sunlighter.CanonicalTypes;
using System.Collections.Immutable;
using System.Numerics;
using System.Linq;

namespace CanonicalTypesTest
{
    [TestClass]
    public class ParseDatumTests
    {
        private ICharParser<Datum> parseNull;
        private ICharParser<Datum> parseDatum;
        private Func<ParseResult<bool>, string> formatBoolResult;
        private Func<ParseResult<BigInteger>, string> formatBigIntegerResult;
        private Func<ParseResult<double>, string> formatDoubleResult;

        public ParseDatumTests()
        {
            parseNull = Parser.ParseNull;
            parseDatum = Parser.ParseDatum;
            formatBoolResult = Utility.GetParseResultStringConverter<bool>(b => b.ToString());
            formatBigIntegerResult = Utility.GetParseResultStringConverter<BigInteger>(b => b.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            formatDoubleResult = Utility.GetParseResultStringConverter<double>(d => d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        [TestMethod]
        public void ParseNull()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, NullDatum.Value),
                    null
                ),
                "#nil"
            );

            Assert.AreEqual("{ success, pos = 0, len = 4, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseSpaceNull()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, NullDatum.Value),
                    null
                ),
                " #nil"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseSpaceTrue()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, BooleanDatum.True),
                    null
                ),
                " #t"
            );

            Assert.AreEqual("{ success, pos = 0, len = 3, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseComplexList()
        {
            Datum complexList = new ListDatum
            (
                new Datum[]
                {
                    BooleanDatum.True,
                    NullDatum.Value,
                    new FloatDatum(-3.75),
                    new ListDatum
                    (
                        new Datum[]
                        {
                            BooleanDatum.True,
                            BooleanDatum.False,
                        }
                        .ToImmutableList()
                    )
                }
                .ToImmutableList()
            );

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, complexList),
                    null
                ),
                " ( #t #nil -3.75\r\n (#t #f) )"
            );

            Assert.AreEqual("{ success, pos = 0, len = 28, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseComplexDictionary()
        {
            Datum complexDictionary = DictionaryDatum.FromEnumerable
            (
                new Tuple<Datum, Datum>[]
                {
                    new Tuple<Datum, Datum>(BooleanDatum.True, BooleanDatum.False),
                    new Tuple<Datum, Datum>(NullDatum.Value, new IntDatum(1000)),
                    new Tuple<Datum, Datum>(new ListDatum(new Datum[] { BooleanDatum.True, new RationalDatum(BigRational.OneHalf) }.ToImmutableList()), new SymbolDatum("blah")),
                }
            );

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, complexDictionary),
                    null
                ),
                " { #t: #f, #nil: 1000, (#t 1/2): blah }"
            );

            Assert.AreEqual("{ success, pos = 0, len = 39, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseBigInteger()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "-2359862495629582635"
            );

            Assert.AreEqual("{ success, pos = 0, len = 20, value = -2359862495629582635 }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButRational()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1/3"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButFloatE()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1e+10"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseNotBigIntegerButFloat()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseBigInteger,
                "1.5"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse integer\" } }", formatBigIntegerResult(result));
        }

        [TestMethod]
        public void ParseDouble()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "6.125"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = 6.125 }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseNotDoubleButRational()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "1/3"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse float (int part)\" } }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseNotDoubleButInteger()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "10000000"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse float (int part)\" } }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseDoubleWithExponent()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseDouble,
                "1.5e+24"
            );

            Assert.AreEqual("{ success, pos = 0, len = 7, value = 1.5E+24 }", formatDoubleResult(result));
        }

        [TestMethod]
        public void ParseQuotedSymbol()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseQuotedSymbol,
                     s => string.Compare(s, "a b", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "|a b|"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseQuotedSymbolWithHexEscape()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseQuotedSymbol,
                     s => string.Compare(s, "a bA", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "|a b\\x41|"
            );

            Assert.AreEqual("{ success, pos = 0, len = 9, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseQuotedSymbolWithUnicodeEscape()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseQuotedSymbol,
                     s => string.Compare(s, "a b•", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "|a b\\U2022|"
            );

            Assert.AreEqual("{ success, pos = 0, len = 11, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseQuotedSymbolWithEscapedBar()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseQuotedSymbol,
                     s => string.Compare(s, "|", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "|\\||"
            );

            Assert.AreEqual("{ success, pos = 0, len = 4, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseString()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseString,
                     s => string.Compare(s, "a b", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "\"a b\""
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseStringWithHexEscape()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseString,
                     s => string.Compare(s, "a bA", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "\"a b\\x41\""
            );

            Assert.AreEqual("{ success, pos = 0, len = 9, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseStringWithUnicodeEscape()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseString,
                     s => string.Compare(s, "a b•", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "\"a b\\U2022\""
            );

            Assert.AreEqual("{ success, pos = 0, len = 11, value = True }", formatBoolResult(result));
        }


        [TestMethod]
        public void ParseStringWithEscapedQuote()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseString,
                     s => string.Compare(s, "\"", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "\"\\\"\""
            );

            Assert.AreEqual("{ success, pos = 0, len = 4, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseStringWithNewlineEscape()
        {
            var result = CharParserContext.TryParse
            (
                 Parser.ParseConvert
                 (
                     Parser.ParseString,
                     s => string.Compare(s, "abcd", StringComparison.InvariantCulture) == 0,
                     "failed to test string"
                 ),
                 "\"ab\\\r\ncd\""
            );

            Assert.AreEqual("{ success, pos = 0, len = 9, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseRational()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseBigRational,
                    r => (r == BigRational.OneHalf),
                    "failed to test big rational"
                ),
                "1/2"
            );
            
            Assert.AreEqual("{ success, pos = 0, len = 3, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseNamedChar()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseNamedChar,
                    r => (r == '\t'),
                    "failed to test named character"
                ),
                "#\\tab"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseNamedCharInvalid()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseNamedChar,
                    r => true,
                    "failed to test invalid named character"
                ),
                "#\\wrong"
            );

            Assert.AreEqual("{ failure, { pos = 0, message = \"Failed to parse named character\" } }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseLiteralChar()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseLiteralChar,
                    r => (r == '$'),
                    "failed to test literal character"
                ),
                "#\\$"
            );

            Assert.AreEqual("{ success, pos = 0, len = 3, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseHexChar()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseHexChar,
                    r => (r == 'a'),
                    "failed to test hex character"
                ),
                "#\\x61"
            );

            Assert.AreEqual("{ success, pos = 0, len = 5, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseCharacterList()
        {
            Datum characterList = new ListDatum
            (
                new Datum[]
                {
                    new CharDatum('a'),
                    new CharDatum('\uFEFF'),
                    new CharDatum('\n'),
                }
                .ToImmutableList()
            );

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, characterList),
                    null
                ),
                "(#\\a #\\xFEFF #\\newline)"
            );

            Assert.AreEqual("{ success, pos = 0, len = 23, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseSet()
        {
            Datum set = SetDatum.Empty.Add(new SymbolDatum("a")).Add(new SymbolDatum("b")).Add(new IntDatum(1000));

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    parseDatum,
                    d => DatumEqualityComparer.Instance.Equals(d, set),
                    null
                ),
                "#s{ a b 1000 }"
            );

            Assert.AreEqual("{ success, pos = 0, len = 14, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseGuid()
        {
            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseGuid,
                    g => g == new Guid("{01234567-89ab-cdef-0123-456789ABCDEF}"),
                    null
                ),
                "#g{01234567-89ab-cdef-0123-456789ABCDEF}"
            );

            Assert.AreEqual("{ success, pos = 0, len = 40, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseByteArray()
        {
            byte[] b = new byte[]
            {
                0xFE, 0x12, 0x3A, 0x4B, 0x79, 0x18, 0x02, 0xA3
            };

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseByteArray,
                    a => a.Length == 8 && Enumerable.Range(0, 8).All(i => a[i] == b[i]),
                    null
                ),
                "#y(FE12 3A4B [1879] 02A3)"
            );

            Assert.AreEqual("{ success, pos = 0, len = 25, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseQuasiQuoteUnquote()
        {
            Datum d = new ListDatum
            (
                ImmutableList<Datum>.Empty
                .Add(new SymbolDatum("quasiquote"))
                .Add
                (
                    new ListDatum
                    (
                        ImmutableList<Datum>.Empty
                        .Add(new SymbolDatum("a"))
                        .Add(new SymbolDatum("b"))
                        .Add
                        (
                            new ListDatum
                            (
                                ImmutableList<Datum>.Empty
                                .Add(new SymbolDatum("unquote"))
                                .Add(new SymbolDatum("x"))
                            )
                        )
                        .Add
                        (
                            new ListDatum
                            (
                                ImmutableList<Datum>.Empty
                                .Add(new SymbolDatum("unquote-splicing"))
                                .Add(new SymbolDatum("y"))
                            )
                        )
                    )
                )
            );

            var result = CharParserContext.TryParse
            (
                Parser.ParseConvert
                (
                    Parser.ParseDatum,
                    a => DatumEqualityComparer.Instance.Equals(a, d),
                    null
                ),
                " `(a b ,x ,@y)"
            );

            Assert.AreEqual("{ success, pos = 0, len = 14, value = True }", formatBoolResult(result));
        }

        [TestMethod]
        public void ParseMutableBoxes()
        {
            var d1 = CharParserContext.TryParse
            (
                Parser.ParseDatumWithBoxes,
                "#b[1]=(1 2 #b[2]=(3 4 #b[3]=(5 6 #b[1])))"
            );

            // unfortunately, you have to use the debugger to see if this is parsed correctly.

            if (d1 is ParseSuccess<Datum>)
            {
                byte[] b0 = ((ParseSuccess<Datum>)d1).Value.SerializeToBytes();

                Datum d3 = b0.DeserializeToDatum();

                byte[] b1 = d3.SerializeToBytes();

                Assert.IsTrue(b0.Length == b1.Length && Enumerable.Range(0, b0.Length).All(i => b0[i] == b1[i]));
            }
            else
            {
                Assert.Fail("Parsing failed");
            }
        }

        [TestMethod]
        public void TestTryParse()
        {
            Datum d1;
            bool success = Datum.TryParse("#f", out d1);

            Assert.IsTrue(success);
            Assert.IsTrue(d1 is BooleanDatum);
        }
    }
}
