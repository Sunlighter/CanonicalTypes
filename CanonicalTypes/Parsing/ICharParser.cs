using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public interface ICharParser<V>
    {
        ParseResult<V> TryParse(CharParserContext context, int pos, int len);
    }
}
