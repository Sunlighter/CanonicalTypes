using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public static partial class Parser
    {
        public static ICharParser<TSet> BuildSetParser<TItem, TSet>
        (
            ICharParser<TItem> itemParser,
            TSet empty,
            Func<TSet, TItem, TSet> addItem
        )
        {
            var set = CharParserBuilder.ParseSequence
            (
                new ICharParser<object>[]
                {
                    Token("#s{"),
                    CharParserBuilder.ParseOptRep
                    (
                        itemParser, true, true

                    ).ResultToObject(),
                    Token("}")
                }
                .ToImmutableList()
            );

            return CharParserBuilder.ParseConvert
            (
                set,
                objs =>
                {
                    ImmutableList<TItem> items = (ImmutableList<TItem>)objs[1];

                    TSet s = empty;
                    foreach (TItem item in items)
                    {
                        s = addItem(s, item);
                    }

                    return s;
                },
                null
            );
        }
    }
}
