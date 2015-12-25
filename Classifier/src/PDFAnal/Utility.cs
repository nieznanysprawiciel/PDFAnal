using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LAIR.ResourceAPIs.WordNet;

namespace PDFAnal
{
    public static class Utility
    {
        public static void Log(string log)
        {
            Debug.WriteLine(log);
        }

        public static string Words( SynSet synSet )
        {
            StringBuilder sb = new StringBuilder("{");
            foreach (string word in synSet.Words)
            {
                sb.Append(word + " ");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
