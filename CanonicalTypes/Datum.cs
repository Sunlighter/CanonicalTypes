using System;
using CanonicalTypes.Parsing;

namespace CanonicalTypes
{
    public abstract class Datum
    {
        public abstract DatumType DatumType { get; }

        public abstract T Visit<T>(IDatumVisitor<T> visitor);

        public static bool TryParse(string str, out Datum d)
        {
            ICharParser<Datum> parser = Parser.ParseConvert
            (
                Parser.ParseSequence
                (
                    Parser.ParseDatumWithBoxes.ResultToObject(),
                    Parser.ParseEOF.ResultToObject()
                ),
                list => (Datum)(list[0]),
                "failure message"
            );

            CharParserContext context = new CharParserContext(str);

            ParseResult<Datum> result = context.TryParseAt(parser, 0, str.Length);

            if (result is ParseSuccess<Datum>)
            {
                ParseSuccess<Datum> success = (ParseSuccess<Datum>)result;
                d = success.Value;
                return true;
            }
            
            d = null;
            return false;
        }
    }
}