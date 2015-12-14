using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public class CharParserContext
    {
        private string text;
        private ObjectIDGenerator idgen;
        private ImmutableSortedDictionary<PosLenId, object> memoTable;

        public CharParserContext(string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            this.text = text;
            this.idgen = new ObjectIDGenerator();
            this.memoTable = ImmutableSortedDictionary<PosLenId, object>.Empty;
        }

        public string Text { get { return text; } }

        public ParseResult<V> TryParseAt<V>(ICharParser<V> parser, int pos, int len)
        {
            long parserId = idgen.GetId(parser);
            PosLenId posLenId = new PosLenId(pos, len, parserId);

            if (memoTable.ContainsKey(posLenId))
            {
                return (ParseResult<V>)memoTable[posLenId];
            }
            else
            {
                ParseResult<V> r = parser.TryParse(this, pos, len);
                memoTable = memoTable.SetItem(posLenId, r);
                return r;
            }
        }

        public static ParseResult<V> TryParse<V>(ICharParser<V> parser, string text)
        {
            CharParserContext cpc = new CharParserContext(text);
            return cpc.TryParseAt(parser, 0, text.Length);
        }
    }
}
