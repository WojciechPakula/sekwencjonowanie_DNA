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
            List<Node> parentsList = null;
            public List<CharNode> virtualEdgesList = null;
            public int inCount; //liczba krawędzi przychodzących
            public int outCount;    //liczba krawędzi wychodzących

            public int getVirtualEdgesCount()
            {
                return virtualEdgesList.Count;
            }

            public List<Node> getParents()
            {
                if (parentsList == null)
                {
                    parentsList = new List<Node>();
                    foreach (var n in g.nodes)
                    {
                        if (n == this) continue;
                        foreach (var e in n.virtualEdgesList)
                        {
                            if (n.getKey().Substring(1) + e.key == key) parentsList.Add(n);
                        }
                    }
                }
                return parentsList;
            }

            public string getKey()
            {
                return key;
            }

            public struct CharNode
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

            //usuwa krawędź/ścieżke
            public void removeEdge(char keyChar, Node newNode)
            {
                if (edgesList == null) return;
                edgesList.Remove(new CharNode(keyChar, newNode));
                outCount--;
                newNode.inCount--;
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

            public List<Node> nodes = new List<Node>();
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
                //sprawdzenie spójności grafu
                bool connect = connectivityCheck();
                if (!connect) positiveErrorSolve();
                connect = connectivityCheck();
                //naprawa spójności pierwszego stopnia
                if (!connect)
                {
                    Console.WriteLine("Próba odtworzenia brakującego połączenia grafu.");
                    //szukaj lisci
                    List<Node> leafs = new List<Node>();
                    foreach (var n in nodes) if (n.outCount == 0) leafs.Add(n);
                    //if (leafs.Count == 0) foreach (var n in nodes) if (n.outCount == 1) leafs.Add(n);

                    foreach (var l in leafs)
                    {
                        foreach (char c0 in letters)
                        {
                            string f = l.key.Substring(1) + c0;
                            //for (int j = 0; j < nodes.Count; ++j)
                            Node ne = null;
                            foreach (var n2 in nodes)
                            {
                                if (n2.key == f) ne = n2;
                            }
                            if (ne != null)
                            {
                                //dodanie krawedzi
                                //if (l.outCount == 0 && ne.inCount == 0) l.addEdge(c0, ne);  //to nie uwzględnia wszystkich przypadków
                                l.addEdge(c0, ne);
                                //if (connectivityCheck() == false) l.removeEdge(c0, ne);
                                break;
                            }
                        }
                    }
                }
                else return;
                connect = connectivityCheck();
                //naprawa spójności drugiego stopnia
                if (!connect)
                {
                    Console.WriteLine("Próba odtworzenia brakującego węzła grafu.");
                    //szukaj lisci
                    List<Node> leafs = new List<Node>();
                    foreach (var n in nodes) if (n.outCount == 0) leafs.Add(n);
                    //if (leafs.Count == 0) foreach (var n in nodes) if (n.outCount == 1) leafs.Add(n);

                    foreach (var l in leafs)
                    {
                        foreach (char c0 in letters)
                        {
                            foreach (char c1 in letters)
                            {
                                string f0 = l.key.Substring(1) + c0;
                                string f1 = l.key.Substring(2) + c0 + c1;
                                //for (int j = 0; j < nodes.Count; ++j)
                                Node ne = null;
                                foreach (var n2 in nodes)
                                {
                                    if (n2.key == f1) ne = n2;
                                }
                                if (ne != null)
                                {
                                    //dodanie krawedzi
                                    Node n3 = new Node();
                                    n3.key = f0;
                                    n3.g = this;
                                    nodes.Add(n3);
                                    n3.addEdge(c1, ne);
                                    l.addEdge(c0, n3);
                                    break;
                                }
                            }
                        }
                    }
                }
                else return;
                connect = connectivityCheck();
                //naprawa spójności trzeciego stopnia
                if (!connect)
                {
                    Console.WriteLine("Próba odtworzenia dwóch brakujących węzłów grafu.");
                    //szukaj lisci
                    List<Node> leafs = new List<Node>();
                    foreach (var n in nodes) if (n.outCount == 0) leafs.Add(n);
                    //if (leafs.Count == 0) foreach (var n in nodes) if (n.outCount == 1) leafs.Add(n);

                    foreach (var l in leafs)
                    {
                        foreach (char c0 in letters)
                        {
                            foreach (char c1 in letters)
                            {
                                foreach (char c2 in letters)
                                {
                                    string f0 = l.key.Substring(1) + c0;
                                    string f1 = l.key.Substring(2) + c0 + c1;
                                    string f2 = l.key.Substring(3) + c0 + c1 + c2;
                                    //for (int j = 0; j < nodes.Count; ++j)
                                    Node ne = null;
                                    foreach (var n2 in nodes)
                                    {
                                        if (n2.key == f2) ne = n2;
                                    }
                                    if (ne != null)
                                    {
                                        //dodanie krawedzi
                                        Node n3 = new Node();
                                        n3.key = f0;
                                        n3.g = this;
                                        nodes.Add(n3);
                                        Node n4 = new Node();
                                        n4.key = f1;
                                        n4.g = this;
                                        nodes.Add(n4);
                                        l.addEdge(c0, n3);
                                        n3.addEdge(c1, n4);
                                        n4.addEdge(c2, ne);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else return;
            }

            bool connectivityCheck()
            {
                int maxEdges = 1000;
                Node maxNode = null;
                foreach (var tmp in nodes)
                {
                    tmp.buildVirtualEdges();
                    int val = tmp.inCount;
                    if (val < maxEdges)
                    {
                        maxEdges = val;
                        maxNode = tmp;
                    }
                }
                var n0 = maxNode;
                Dictionary<string, Node> dic = new Dictionary<string, Node>();
                dic.Add(maxNode.getKey(), maxNode);
                List<Node> newNodes = new List<Node>();
                newNodes.Add(maxNode);
                for (; ; )
                {
                    List<Node> newNodesDup = new List<Node>(newNodes);
                    newNodes.Clear();
                    int count = newNodesDup.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        var ele = newNodesDup.First();
                        newNodesDup.RemoveAt(0);

                        foreach (var n2 in ele.virtualEdgesList)
                        {
                            if (!dic.ContainsKey(n2.value.getKey()))
                            {
                                newNodes.Add(n2.value);
                                dic.Add(n2.value.getKey(), n2.value);
                            }
                        }
                        foreach (var np in ele.getParents())
                        {
                            if (!dic.ContainsKey(np.getKey()))
                            {
                                newNodes.Add(np);
                                dic.Add(np.getKey(), np);
                            }
                        }
                    }
                    if (newNodes.Count == 0) break;
                }
                int dc = dic.Count;
                int nc = nodes.Count;
                bool sp = (dc == nc);
                return sp;
            }

            void positiveErrorSolve()
            {
                Console.WriteLine("Graf jest niespójny.");
                int positiveErrorCounter = 0;
                List<Node> toDelete = new List<Node>();
                foreach (var node in nodes)
                {
                    //policz węzły nie związane z grafem
                    if (node.inCount == 0 && node.outCount == 1)
                    {
                        var node2 = node.virtualEdgesList.First();
                        if (node2.value.inCount == 1 && node2.value.outCount == 0)
                        {
                            positiveErrorCounter++;
                            toDelete.Add(node);
                            positiveErrorCounter++;
                            toDelete.Add(node2.value);
                            Console.WriteLine("Wykryto błąd pozytywny.");
                        }
                    }
                    if (node.inCount == 1 && node.outCount == 1)
                    {
                        var node2 = node.virtualEdgesList.First();
                        if (node2.value.inCount == 1 && node2.value.outCount == 1 && node.getKey() == node2.value.getKey())
                        {
                            positiveErrorCounter++;
                            toDelete.Add(node);
                            Console.WriteLine("Wykryto błąd pozytywny.");
                        }
                    }
                }
                foreach (var node in toDelete)
                {
                    nodes.Remove(node);
                }
                
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
                    Console.WriteLine("Wykryto problemy z danymi.");
                    /*if (!positiveError) {
                        Console.WriteLine("Możliwy błąd pozytywny.");
                        //obsługa błędu pozytywnego

                        List<Node> suspect = new List<Node>();
                        foreach (var n in nodes)
                        {
                            if (n.outCount - n.inCount > 0 && (n.outCount == 0 || n.inCount == 0))
                            {
                                suspect.Add(n);
                            }
                            if (n.outCount - n.inCount < 0 && (n.outCount == 0 || n.inCount == 0))
                            {
                                suspect.Add(n);
                            }
                        }

                        //prymitywne rozwiązanie, usunięcie wszystkich podejrzanych węzłów
                        foreach (var sus in suspect)
                        {
                            nodes.Remove(sus);
                        }

                        return getEulerPath(true); 
                    }*/
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
