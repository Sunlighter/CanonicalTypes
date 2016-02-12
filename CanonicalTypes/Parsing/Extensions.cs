using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalTypes.Parsing
{
    public static class Extensions
    {
        public static long GetId(this ObjectIDGenerator idgen, object obj)
        {
            bool firstTime;
            return idgen.GetId(obj, out firstTime);
        }

        public static string Quoted(this string str)
        {
            if (str == null)
            {
                return "null";
            }
            else
            {
                StringBuilder sb = new StringBuilder("\"");
                foreach (char ch in str)
                {
                    if (ch == '\\') sb.Append("\\\\");
                    else if (ch == '\"') sb.Append("\\\"");
                    else if (ch >= ' ' && ch <= '~')
                    {
                        sb.Append(ch);
                    }
                    else if (ch == '\a') sb.Append("\\a");
                    else if (ch == '\b') sb.Append("\\b");
                    else if (ch == '\t') sb.Append("\\t");
                    else if (ch == '\n') sb.Append("\\n");
                    else if (ch == '\v') sb.Append("\\v");
                    else if (ch == '\f') sb.Append("\\f");
                    else if (ch == '\r') sb.Append("\\r");
                    else if ((int)ch < 256)
                    {
                        sb.AppendFormat("\\x{0:x2}", (int)ch);
                    }
                    else
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)ch);
                    }
                }
                sb.Append("\"");
                return sb.ToString();
            }
        }

        public static string SymbolQuoted(this string str)
        {
            if (str == null)
            {
                return "null";
            }
            else
            {
                StringBuilder sb = new StringBuilder("|");
                foreach (char ch in str)
                {
                    if (ch == '\\') sb.Append("\\\\");
                    else if (ch == '|') sb.Append("\\|");
                    else if (ch >= ' ' && ch <= '~')
                    {
                        sb.Append(ch);
                    }
                    else if (ch == '\a') sb.Append("\\a");
                    else if (ch == '\b') sb.Append("\\b");
                    else if (ch == '\t') sb.Append("\\t");
                    else if (ch == '\n') sb.Append("\\n");
                    else if (ch == '\v') sb.Append("\\v");
                    else if (ch == '\f') sb.Append("\\f");
                    else if (ch == '\r') sb.Append("\\r");
                    else if ((int)ch < 256)
                    {
                        sb.AppendFormat("\\x{0:x2}", (int)ch);
                    }
                    else
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)ch);
                    }
                }
                sb.Append("|");
                return sb.ToString();
            }
        }
    }
}
