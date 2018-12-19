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
            List<string> dup = new List<string>();  //zawiera duplikaty
            var inputTable = validator(input, out dup);
            var splitTable = split(inputTable);
            string letters = getLetters(splitTable);
            graph = new Graph(letters, splitTable, inputTable, dup);
            var path = graph.getEulerPath();
            string output = pathToString(path);
            return output;
        }

        //To jest optymalizacja, która wykorzystuje już raz zbudowany graf, liczy wtedy na nim ścieżkę od nowa, ta opcja ma sens w trybie losowym.
        static string getChainAgain()
        {
            if (graph == null) throw new Exception("Błąd, nigdy nie wywołano getChain();");
            var path = graph.getEulerPath();
            string output = pathToString(path);
            return output;
        }

        //Zwraca wszystkie rozwiązania
        public static List<string> getChains(List<string> input)
        {
            HashSet<string> hs = new HashSet<string>();
            for (int i = 0; ; ++i)
            {
                string output = "";
                if (i == 0)
                    output = getChain(input);
                else
                    output = getChainAgain();
                hs.Add(output);
                if (graph.noMoreSolutions) break;
            }
            return hs.ToList();
        }

        //Zwraca wszystkie rozwiązania
        public static List<string> getRandomChains(List<string> input, int iterations)
        {
            HashSet<string> hs = new HashSet<string>();
            for (int i = 0; i < iterations; ++i)
            {
                string output = "";
                if (i == 0)
                    output = getChain(input, true);
                else
                    output = getChainAgain();
                hs.Add(output);
            }
            return hs.ToList();
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
        private static HashSet<string> validator(List<string> input, out List<string> dup)
        {
            HashSet<string> hs = new HashSet<string>();
            dup = new List<string>();
            int k = 0;
            foreach (string element in input)
            {
                if (k==0) k = element.Length;
                if (element.Length == 0) continue;
                if (k != element.Length) throw new Exception("Wszystkie słowa muszą mieć identyczną długość "+k+"!="+ element.Length + ", możliwe że gdzieś jest spacja.");
                string uppercase = element.ToUpper();
                if (hs.Contains(uppercase)) dup.Add(uppercase);
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
            public Graph g;
            public string key = "";
            List<CharNode> edgesList = new List<CharNode>();
            List<CharNode> virtualEdgesList = null;
            public int inCount; //liczba krawędzi przychodzących
            public int outCount;    //liczba krawędzi wychodzących

            private struct CharNode
            {
                public char key;
                public Node value;
                public CharNode(char c, Node n)
                {
                    key = c;
                    value = n;
                }
            }

            //dodaje krawędź/ścieżke
            public void addEdge(char keyChar, Node newNode)
            {
                if (edgesList == null) return;
                edgesList.Add(new CharNode(keyChar, newNode));
                outCount++;
                newNode.inCount++;
            }

            //potrzebne do niszczenia za sobą ścieżek przy przechodzeniu przez graf
            public void buildVirtualEdges()
            {
                virtualEdgesList = new List<CharNode>();
                foreach (var p in edgesList)
                {
                    var tmp = new CharNode(p.key, p.value);
                    virtualEdgesList.Add(tmp);
                }
            }

            //rekurencyjne przechodzenie grafu i zawalanie za sobą ścieżek
            public List<string> solvePath(List<string> tmp = null)
            {
                for (;;)
                {
                    if (virtualEdgesList.Count > 0)
                    {
                        var pair = virtualEdgesList.First();//wywala wyjatek gdy węzeł jest liściem
                        int choice = 0;
                        if (randomPath) { 
                            // TRYB LOSOWEGO WYBORU SCIEZKI
                            choice = rnd.Next(virtualEdgesList.Count()); 
                        }
                        else
                        {
                            //TRYB STRATEGICZNEGO WYBORU SCIEZKI
                            if (virtualEdgesList.Count > 1 && g.lastChoice != null)
                            {
                                int ind = g.localChoices;
                                choice = g.lastChoice[ind];
                                if (choice == -1) choice = virtualEdgesList.Count - 1;
                                bool change = g.changeLock;
                                for (int i = ind + 1; i < g.totalChoices; ++i) if (g.lastChoice[i] != -1) { change = false; break; }
                                if (change)
                                {
                                    choice++;
                                    g.changeLock = false;
                                    g.lastChoice[ind] = choice;
                                    if (choice >= virtualEdgesList.Count - 1)
                                    {
                                        choice = virtualEdgesList.Count - 1;//chyba niepotrzebne
                                        g.lastChoice[ind] = -1;
                                    }
                                    for (int i = ind + 1; i < g.totalChoices; ++i) g.lastChoice[i] = 0;
                                }
                            }
                            if (virtualEdgesList.Count > 1 && g.lastChoice == null) g.totalChoices++;
                            if (virtualEdgesList.Count > 1) g.localChoices++;
                        }
                        
                        pair = virtualEdgesList.ElementAt(choice);
                        virtualEdgesList.Remove(pair);
                        tmp = pair.value.solvePath(tmp);
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
            public List<int> lastChoice = null;
            public int totalChoices = 0;
            public int localChoices = 0;
            public bool changeLock;
            public bool noMoreSolutions = false;

            List<Node> nodes = new List<Node>();
            string letters = "";    //zawiera litery "ACGT"
            public Graph(string letters, HashSet<string> stringNodes, HashSet<string> input, List<string> dup)
            {
                this.letters = letters;
                foreach (var n in stringNodes)
                {
                    Node n2 = new Node();
                    n2.key = n;
                    n2.g = this;
                    nodes.Add(n2);
                }
                buildEdges(input, dup);
            }

            //uzupełnia węzłom ścieżki
            void buildEdges(HashSet<string> input, List<string> dup)
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
                //obsługa duplikatów
                foreach (var d in dup)
                {
                    string start = d.Substring(0, d.Length - 1);
                    string end = d.Substring(1);
                    char c = d[d.Length - 1];
                    Node n1 = null;
                    Node n2 = null;
                    foreach (var n in nodes)
                    {
                        if (n.key == start) n1 = n;
                        if (n.key == end) n2 = n;
                    }
                    if (n1 != null && n2 != null) {
                        n1.addEdge(c, n2);
                    }
                }
                //zrobić sprawdzanie błędów negatywnych, sprawdzanie rekurencyjne i przewidywanie nowych ścieżek
                //zrobić sprawdzanie błędów pozytywnych
                //zrobić sprawdzanie czy graf jest spójny
                //zrobić sprawdzanie liści
            }

            public List<string> getEulerPath()
            {
                changeLock = true;
                localChoices = 0;
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
                    updateFlags();
                    return output;
                } else if (maxCount == 0 && minCount == 0)
                {
                    //graf eulerowski
                    start = nodes.First();
                    var output = start.solvePath();
                    output.Reverse();
                    updateFlags();
                    return output;
                } else
                {
                    //zabezpieczenie przed dziwnymi grafami
                    //nie ma jeszcze sprawdzania spójności grafu !!!!!!!!!!!!!!!
                    return null;
                }
            }
            void updateFlags()
            {
                if (lastChoice != null)
                {
                    bool endProgram = true;
                    foreach (var element in lastChoice)
                    {
                        if (element != -1) { endProgram = false; break; }
                    }
                    noMoreSolutions = endProgram;
                }
                if (lastChoice == null)
                {
                    lastChoice = new List<int>();
                    for (int i = 0; i < totalChoices; ++i)
                    {
                        lastChoice.Add(0);
                    }
                }
            }
        }
    }
}
