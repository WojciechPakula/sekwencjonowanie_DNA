using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sekwencjonowanie_DNA
{
    class DNAhybridization
    {
        public static Boolean randomPath = false;   //wybiera losowo ścieżki w grafie
        private static Graph graph = null;
        static Random rnd = new Random();

        //Zwraca słowo na podstawie wszystkich jego podsłów
        public static string getChain(List<string> input, Boolean randomMode = false)
        {
            randomPath = randomMode;
            var inputTable = validator(input);
            var splitTable = split(inputTable);
            string letters = getLetters(splitTable);
            graph = new Graph(letters, splitTable, inputTable);
            var path = graph.getEulerPath();
            string output = pathToString(path);
            return output;
        }

        //To jest optymalizacja, która wykorzystuje już raz zbudowany graf, liczy wtedy na nim ścieżkę od nowa, ta opcja ma sens w trybie losowym.
        public static string getChainAgain()
        {
            if (graph == null) throw new Exception("Błąd, nigdy nie wywołano getChain();");
            var path = graph.getEulerPath();
            string output = pathToString(path);
            return output;
        }

        //zamienia listę węzłów na tekst wynikowy
        public static string pathToString(List<string> input) {
            string output = null;
            foreach (var n in input)
            {
                if (output == null)
                {
                    output = n;
                } else
                {
                    output += n.Last();
                }
            }
            return output;
        }

        //zwraca wszystkie uzyte rodzaje liter (zwykle "ACGT")
        public static string getLetters(HashSet<string> input)
        {
            HashSet<char> hs = new HashSet<char>();
            foreach (string element in input)
            {
                foreach (char letter in element)
                {
                    hs.Add(letter);
                }
            }
            var array = hs.ToArray<char>();
            return new string(array);
        }

        //usuwa powtórzenia oraz zamienia małe litery na duże
        private static HashSet<string> validator(List<string> input)
        {
            HashSet<string> hs = new HashSet<string>();
            int k = 0;
            foreach (string element in input)
            {
                if (k==0) k = element.Length;
                if (k != element.Length) throw new Exception("Wszystkie słowa muszą mieć identyczną długość "+k+"!="+ element.Length);
                string uppercase = element.ToUpper();
                hs.Add(uppercase);
            }
            return hs;
        }

        //tworzy przedrostki i przyrostki jako jeden zbiór wierzchołków przyszłego grafu
        private static HashSet<string> split(HashSet<string> input)
        {
            HashSet<string> hs = new HashSet<string>();
            //tu moze byc blad w wyznaczaniu wezlow (ale raczej nie ma)
            foreach (string element in input)
            {
                string start = element.Substring(0, element.Length - 1);
                string end = element.Substring(1);
                hs.Add(start);
                hs.Add(end);
            }
            return hs;
        }

        private class Node
        {
            public string key = "";
            Dictionary<char, Node> edges = new Dictionary<char, Node>();
            Dictionary<char, Node> virtualEdges = null;
            public int inCount; //liczba krawędzi przychodzących
            public int outCount;    //liczba krawędzi wychodzących

            //dodaje krawędź/ścieżke
            public void addEdge(char keyChar, Node newNode)
            {
                if (edges == null) return;
                edges[keyChar] = newNode;
                outCount++;
                newNode.inCount++;
            }

            //potrzebne do niszczenia za sobą ścieżek przy przechodzeniu przez graf
            public void buildVirtualEdges()
            {
                virtualEdges = new Dictionary<char, Node>();
                foreach (var p in edges)
                {
                    virtualEdges[p.Key] = p.Value;
                }
            }

            //rekurencyjne przechodzenie grafu i zawalanie za sobą ścieżek
            public List<string> solvePath(List<string> tmp = null)
            {
                for (;;)
                {
                    if (virtualEdges.Count > 0)
                    {
                        var pair = virtualEdges.First();//wywala wyjatek gdy węzeł jest liściem
                        if (randomPath)
                        {
                            int r = rnd.Next(virtualEdges.Count());
                            pair = virtualEdges.ElementAt(r);
                        }
                        virtualEdges.Remove(pair.Key);
                        tmp = pair.Value.solvePath(tmp);
                    } else
                    {
                        //nie ma więcej krawędzi
                        if (tmp == null) tmp = new List<string>();
                        tmp.Add(key);
                        return tmp;
                    }
                }
            }
        }
        private class Graph
        {
            List<Node> nodes = new List<Node>();
            string letters = "";    //zawiera litery "ACGT"
            public Graph(string letters, HashSet<string> stringNodes, HashSet<string> input)
            {
                this.letters = letters;
                foreach (var n in stringNodes)
                {
                    Node n2 = new Node();
                    n2.key = n;
                    nodes.Add(n2);
                }
                buildEdges(input);
            }

            //uzupełnia węzłom ścieżki
            void buildEdges(HashSet<string> input)
            {
                foreach (var n in nodes)
                {
                    string k = n.key;
                    foreach (char c in letters)
                    {
                        string f = k.Substring(1)+c;
                        foreach (var n2 in nodes)
                        {
                            if (n2.key == f)
                            {
                                string full = n.key + c;
                                var exists = input.Contains(full);//może być nieoptymalnie
                                if (exists)
                                {
                                    n.addEdge(c, n2);
                                    break;
                                }
                            }
                        }
                    }
                }
                //zrobić sprawdzanie błędów negatywnych, sprawdzanie rekurencyjne i przewidywanie nowych ścieżek
                //zrobić sprawdzanie błędów pozytywnych
                //zrobić sprawdzanie czy graf jest spójny
                //zrobić sprawdzanie liści
            }

            public List<string> getEulerPath()
            {
                int maxCount = 0;   //ilosc węzłów które mają więcej wyjśc niż wejść
                int minCount = 0;   //ilość węzłów które mają więcej wejść niż wyjść
                Node start = null;
                Node end = null;    //nie użyte
                foreach (var n in nodes)
                {
                    n.buildVirtualEdges();
                    if (n.outCount - n.inCount > 0)
                    {
                        start = n;
                        maxCount++;
                    }
                    if (n.outCount - n.inCount < 0)
                    {
                        end = n;
                        minCount++;
                    }
                }
                if (maxCount == 1 && minCount == 1)
                {
                    //graf półeulerowski
                    var output = start.solvePath();
                    output.Reverse();
                    return output;
                } else if (maxCount == 0 && minCount == 0)
                {
                    //graf eulerowski
                    start = nodes.First();
                    /*if (randomPath)
                    {
                        int r = rnd.Next(nodes.Count());
                        start = nodes.ElementAt(r);
                    }*/
                    var output = start.solvePath();
                    output.Reverse();
                    return output;
                } else
                {
                    //zabezpieczenie przed dziwnymi grafami
                    //nie ma jeszcze sprawdzania spójności grafu !!!!!!!!!!!!!!!
                    return null;
                }
            }
        }
    }
}
