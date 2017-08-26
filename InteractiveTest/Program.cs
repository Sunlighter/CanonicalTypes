using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunlighter.CanonicalTypes;

namespace InteractiveTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.Write("> ");
                    string str = Console.ReadLine();
                    Datum d;
                    if (Datum.TryParse(str, out d))
                    {
                        Console.WriteLine("* " + d.ToString());
                        if (d is SymbolDatum && ((SymbolDatum)d).IsInterned && ((SymbolDatum)d).Name == "exit")
                            break;
                    }
                    else
                    {
                        Console.WriteLine("Parse error");
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc);
            }
        }
    }
}
