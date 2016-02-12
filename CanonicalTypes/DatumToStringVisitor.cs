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
    public class MutableBoxReferenceCollector : IDatumVisitorWithState<MutableBoxReferenceCollector.State>
    {
        private static Lazy<MutableBoxReferenceCollector> instance = new Lazy<MutableBoxReferenceCollector>(() => new MutableBoxReferenceCollector(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static MutableBoxReferenceCollector Instance => instance.Value;

        private MutableBoxReferenceCollector()
        {

        }

        public class State
        {
            public State(SetDatum boxesSeen, DictionaryDatum boxesReferenced, int nextBoxNumber)
            {
                BoxesSeen = boxesSeen;
                BoxesReferenced = boxesReferenced;
                NextBoxNumber = nextBoxNumber;
            }

            public SetDatum BoxesSeen { get; }
            public DictionaryDatum BoxesReferenced { get; }
            public int NextBoxNumber { get; }

            public static State Empty = new State(SetDatum.Empty, DictionaryDatum.Empty, 1);
        }

        public State VisitNull(State state, NullDatum d) => state;

        public State VisitBoolean(State state, BooleanDatum d) => state;

        public State VisitChar(State state, CharDatum d) => state;

        public State VisitString(State state, StringDatum d) => state;

        public State VisitInt(State state, IntDatum d) => state;

        public State VisitFloat(State state, FloatDatum d) => state;

        public State VisitByteArray(State state, ByteArrayDatum d) => state;

        public State VisitSymbol(State state, SymbolDatum d) => state;

        public State VisitList(State state, ListDatum d)
        {
            State s = state;
            foreach(Datum listItem in d)
            {
                s = listItem.Visit(this, s);
            }
            return s;
        }

        public State VisitSet(State state, SetDatum d)
        {
            State s = state;
            foreach (Datum listItem in d)
            {
                s = listItem.Visit(this, s);
            }
            return s;
        }

        public State VisitDictionary(State state, DictionaryDatum d)
        {
            State s = state;
            foreach(KeyValuePair<Datum, Datum> pair in d)
            {
                s = pair.Key.Visit(this, s);
                s = pair.Value.Visit(this, s);
            }
            return s;
        }

        public State VisitMutableBox(State state, MutableBoxDatum d)
        {
            if (state.BoxesSeen.Contains(d))
            {
                State s2 = new State
                (
                    state.BoxesSeen,
                    state.BoxesReferenced.Add(d, new IntDatum(state.NextBoxNumber)),
                    state.NextBoxNumber + 1
                );

                return s2;
            }
            else
            {
                State s2 = new State
                (
                    state.BoxesSeen.Add(d),
                    state.BoxesReferenced,
                    state.NextBoxNumber
                );

                State s3 = d.Content.Visit<State>(this, s2);

                return s3;
            }
        }

        public State VisitRational(State state, RationalDatum d) => state;

        public State VisitGuid(State state, GuidDatum d) => state;
    }

    public class DatumToStringVisitor : IDatumVisitor<string>
    {
        MutableBoxReferenceCollector.State boxReferences;
        SetDatum boxesStarted;

        public DatumToStringVisitor(MutableBoxReferenceCollector.State boxReferences)
        {
            this.boxReferences = boxReferences;
            this.boxesStarted = SetDatum.Empty;
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
                return $"#\\x{i:X4}";
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
            return d.Value.ToString("G");
        }

        public string VisitByteArray(ByteArrayDatum d)
        {
            return "#y(" + string.Join("", d.Bytes.Select(b => b.ToString("X2"))) + ")";
        }

        public string VisitSymbol(SymbolDatum d)
        {
            throw new NotImplementedException();
        }

        public string VisitList(ListDatum d)
        {
            return "(" + string.Join(" ", d.Values.Select(i => i.Visit(this))) + ")";
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
            if (boxReferences.BoxesReferenced.ContainsKey(d))
            {
                if (boxesStarted.Contains(d))
                {
                    return "#b[" + boxReferences.BoxesReferenced[d].Visit(this) + "]";
                }
                else
                {
                    boxesStarted = boxesStarted.Add(d);
                    return "#b[" + boxReferences.BoxesReferenced[d].Visit(this) + "]=" + d.Content.Visit(this);
                }
                
            }
            else
            {
                return "#b=" + d.Content.Visit(this);
            }
        }

        public string VisitRational(RationalDatum d)
        {
            return d.Value.ToString();
        }

        public string VisitGuid(GuidDatum d)
        {
            return "#g" + d.Value.ToString("B");
        }
    }
}
