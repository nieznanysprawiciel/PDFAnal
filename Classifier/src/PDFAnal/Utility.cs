using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using LAIR.ResourceAPIs.WordNet;

namespace PDFAnal
{
    public static class Utility
    {
        public static void Log(string log)
        {
            //  console
            Debug.WriteLine(log);

            //  file
            System.IO.StreamWriter sw = System.IO.File.AppendText("log.txt");
            try
            {
                sw.WriteLine(log);
            }
            finally
            {
                sw.Close();
            }
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
