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
            int mode = 0;
            int k = 3;
            int iterations = 1;
            string path = "";
            bool testMode = false;
            for (int i = 0; i < args.Count(); ++i)
            {
                switch (args[i])
                {
                    case "-f"://file
                        i++;
                        if (i < args.Count()) path = args[i];
                        break;
                    case "-r"://random
                        i++;
                        mode = 1;
                        try{if (i < args.Count()) iterations = int.Parse(args[i]);} catch{}
                        break;
                    case "-a"://random
                        mode = 2;
                        break;
                    case "-t"://test mode, podawany jest wynik, z wyniku tworzony jest input i z tego znowu wynik.
                        i++;
                        testMode = true;
                        try { if (i < args.Count()) k = int.Parse(args[i]); } catch { }
                        break;
                }
            }
            //PROGRAM
            try
            {
                List<string> input = loadFile(path);
                try
                {
                    if (testMode)
                    {
                        string result = input[0];
                        var l = new List<string>();
                        if (result.Length <= k || k <= 0) throw new Exception("Tryb testowy, długość słowa:" + result.Length+" , parametr k:"+k+" , k musi być mniejsze niż słowo.");
                        for (int i = 0; i < result.Length-k+1; ++i)
                        {
                            string sub = result.Substring(i, k);
                            l.Add(sub);
                        }
                        input = shuffle(l);
                        saveToFile(input, "testInput.txt");
                    }
                    List<string> output = new List<string>();
                    switch (mode)
                    {
                        case 0://single
                            output.Add(DNAhybridization.getChain(input));
                            break;
                        case 1://random
                            output = DNAhybridization.getRandomChains(input, iterations);
                            break;
                        case 2://all results
                            output = DNAhybridization.getChains(input);
                            break;
                    }
                       
                    Console.WriteLine("\nUzyskane wyniki " + output.Count + ":");
                    foreach (var e in output)
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

        static List<string> shuffle(List<string> alpha)
        {
            Random rnd = new Random();
            for (int i = 0; i < alpha.Count-1; i++)
            {
                string temp = alpha[i];
                int randomIndex = rnd.Next(i, alpha.Count);
                alpha[i] = alpha[randomIndex];
                alpha[randomIndex] = temp;
            }
            return alpha;
        }

        static void saveToFile(List<string> list, string name)
        {
            using (TextWriter tw = new StreamWriter(name))
            {
                foreach (string s in list)
                    tw.WriteLine(s);
            }
        }
    }
}
