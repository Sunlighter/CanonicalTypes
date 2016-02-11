using CanonicalTypes.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CanonicalTypes
{
#if false
    public class MutableBoxReferenceCollector : IDatumVisitor<Datum>
    {
        // TODO: find the mutable boxes but also find out which ones need numbering.
    }
#endif

    public class DatumToStringVisitor : IDatumVisitor<string>
    {
        DictionaryDatum boxes;

        public DatumToStringVisitor()
        {
            boxes = DictionaryDatum.Empty;
        }

        public string VisitNull(NullDatum d)
        {
            return "#nil";
        }

        public string VisitBoolean(BooleanDatum d)
        {
            return d.Value ? "#t" : "#f";
        }

        private static Lazy<ImmutableDictionary<char, string>> namedCharacters = new Lazy<ImmutableDictionary<char, string>>(BuildNamedCharacters, LazyThreadSafetyMode.ExecutionAndPublication);

        private static ImmutableDictionary<char, string> BuildNamedCharacters()
        {
            return ImmutableDictionary<char, string>.Empty
                .Add((char)0, "nul")
                .Add('\a', "bel")
                .Add('\b', "backspace")
                .Add('\t', "tab")
                .Add('\n', "newline")
                .Add('\v', "vt")
                .Add('\f', "page")
                .Add('\r', "return")
                .Add(' ', "space");
        }

        public static ImmutableDictionary<char, string> NamedCharacters => namedCharacters.Value;

        public string VisitChar(CharDatum d)
        {
            if (NamedCharacters.ContainsKey(d.Value))
            {
                return "#\\" + NamedCharacters[d.Value]; 
            }
            else if (char.IsLetter(d.Value) || char.IsNumber(d.Value) || char.IsPunctuation(d.Value) || char.IsSymbol(d.Value))
            {
                return "#\\" + d.Value;
            }
            else if (Regex.IsMatch("" + d.Value, "\\p{M}"))
            {
                return "#\\" + d.Value;
            }
            else
            {
                int i = (int)d.Value;
                return $"#\\{i:X4}";
            }
        }

        


        public string VisitString(StringDatum d)
        {
            return d.Value.Quoted();
        }

        public string VisitInt(IntDatum d)
        {
            return d.Value.ToString();
        }

        public string VisitFloat(FloatDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitByteArray(ByteArrayDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitSymbol(SymbolDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitList(ListDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitSet(SetDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitDictionary(DictionaryDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitMutableBox(MutableBoxDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitRational(RationalDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitGuid(GuidDatum d)
        {
            throw new NotImplementedException();
        }
    }
}
