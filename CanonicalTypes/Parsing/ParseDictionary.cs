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
        public static ICharParser<TDict> BuildDictionaryParser<TKey, TValue, TDict>
        (
            ICharParser<TKey> keyParser,
            ICharParser<TValue> valueParser,
            TDict empty,
            Func<TDict, TKey, TValue, TDict> addItem
        )
        {
            ICharParser<Tuple<TKey, TValue>> kvp = CharParserBuilder.ParseConvert
            (
                CharParserBuilder.ParseSequence
                (
                    new ICharParser<object>[]
                    {
                        keyParser.ResultToObject(),
                        Token("=>"),
                        valueParser.ResultToObject(),
                    }
                    .ToImmutableList()
                ),
                objs => new Tuple<TKey, TValue>((TKey)objs[0], (TValue)objs[2]),
                null
            );

            var dict = CharParserBuilder.ParseSequence
            (
                new ICharParser<object>[]
                {
                    Token("{"),
                    CharParserBuilder.ParseOptRep
                    (
                        CharParserBuilder.ParseConvert
                        (
                            CharParserBuilder.ParseSequence
                            (
                                new ICharParser<object>[]
                                {
                                    kvp.ResultToObject(),
                                    Token(",")
                                }
                                .ToImmutableList()
                            ),
                            lst => (Tuple<TKey, TValue>)lst[0],
                            null
                        ),
                        true,
                        true
                    )
                    .ResultToObject(),
                    CharParserBuilder.ParseOptRep
                    (
                        kvp,
                        true,
                        false
                    )
                    .ResultToObject(),
                    Token("}"),
                }
                .ToImmutableList()
            );

            return CharParserBuilder.ParseConvert
            (
                dict,
                objs =>
                {
                    ImmutableList<Tuple<TKey, TValue>> l1 = (ImmutableList<Tuple<TKey, TValue>>)objs[1];
                    ImmutableList<Tuple<TKey, TValue>> l2 = (ImmutableList<Tuple<TKey, TValue>>)objs[2];

                    TDict v = empty;
                    foreach (Tuple<TKey, TValue> kvp0 in l1.Concat(l2))
                    {
                        v = addItem(v, kvp0.Item1, kvp0.Item2);
                    }
                    return v;
                },
                null
            );
        }
    }
}
