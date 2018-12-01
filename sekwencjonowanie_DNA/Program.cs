using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sekwencjonowanie_DNA
{
    class Program
    {
        //Uruchomić program z argumentami "-f input.txt" gdzie input.txt zawiera słowa takiej samej długości rozdzielone znakiem nowej linii.
        static void Main(string[] args)
        {
            //PARAMETRY
            bool randomMode = false;
            string path = "";
            for (int i = 0; i < args.Count(); ++i)
            {
                switch (args[i])
                {
                    case "-f"://file
                        i++;
                        if (i < args.Count()) path = args[i];
                        break;
                    case "-r"://random
                        randomMode = true;
                        break;
                }
            }
            //PROGRAM
            try
            {
                List<string> input = loadFile(path);
                try
                {
                    HashSet<string> hs = new HashSet<string>();
                    for (int i = 0; i < 200; ++i)
                    {
                        string output = "";
                        if (i == 0)
                            output = DNAhybridization.getChain(input, randomMode);
                        else
                            output = DNAhybridization.getChainAgain();
                        hs.Add(output);
                        Console.WriteLine(output);
                    }
                    Console.WriteLine("\nUzyskane wyniki " +hs.Count+":");
                    foreach (var e in hs)
                    {
                        Console.WriteLine(e);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            } catch
            {
                Console.WriteLine("Brak pliku wejściowego");
            }
            Console.WriteLine("\nReadKey();");
            Console.ReadKey();
        }

        static List<string> loadFile(string path)
        {
            List<string> l = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                string line = sr.ReadToEnd();
                Console.WriteLine(line);
                line = line.Replace("\r", string.Empty);
                var s = line.Split('\n');
                l = s.ToList();
            }
            return l;
        }
    }
}
