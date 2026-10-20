using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    // Metod för att hitta den kortaste vägen från start till goal i labyrinten
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // Steg 1 (innan BFS). Ta reda på vilka grannar en ruta har:
        // (varje ruta i labyrinten är en nod. En kant kopplar ihop två rutor som går att förflytta sig direkt mellan. Ignorera kostnad för BFS steget)
        //
        // 1. Vilka fyra grannar kan en ruta ha?
        // kontrollera för varje granne:
        // * ligger grannan innanför labyrintens gränser?
        // * Finns det en vägg mellan rutorna?
        //
        // 2. Hur kontrollerar vi gränserna?
        // om labyrinten är t.ex Width = 10 och Height = 10, då är giltiga koordinater: x: 0 till 9, y: 0 till 9
        // fär en ruta på (0, 2) är den vänstra grannen (-1, 2), den ligger utanför labyrinten (ska ignorera negativa koordinater)
        // Kan kontrollera gränserna med kod som t.ex:
        // if (x > 0) { det finns en ruta till vänster }   if (x < mapData.Width - 1) { det finns en ruta till höger }  (samma med y, för upp eller ner)
        //
        // 3. En skillnad från IsMovementBlocked()
        // IsMovementBlocked() metoden är användbar till senare när man använder djikstra och ska tänka på vikten, men behövs inte för BFS versionen (steg 1)
        // första versionen (BFS) ska istället använda: HasVerticalWall() och HasHorizontalWall() för att avgöra om vägen är blockerad eller inte.
        // Ignorerar hopp och kostnad för första versionen (BFS), och senare använder det för nästa version (djikstra)

        // Steg 2. Välj en datastruktur för grafen
        // Man måste lagra varje ruta och vilka rutor som går att nå direkt från den. Man kan göra det med en "adjacency list".
        // Man kan representera en adjacency list med en Dictionary, där varje nyckel är en ruta och värdet är en lista över dess grannar
        // (Dictionary<TKey = node/ ruta i labyrinten, TValue = lista med nodens grannar>. Dictionary<Vector2Int, List<Vector2Int>> graph;)
        // Dictionary lagrar nyckel-värde-par, Vector2Int är nyckeln/ koordinaterna för en ruta, List<Vector2Int> är värdet/ rutans lista av grannar

        // Steg 3. Skapa en tom graf (graf för sökning, inte själva labyrinten som redan finns)
        // Ska skriva en hjälpfunktion för att ta emot mapData och retunera grafen.
        // mapData.Width och MapData.Height anger hur många rutor labyrinten har, medan koordinaterna börjar på 0 (top left)
        // Börja med en hjälp funktion som t.ex:private static Dictionary<Vector2Int, List<Vector2Int>> BuildGraph(IMapData mapData), placera under denna metod:

        // Steg 4. Skapa en grannlista för varje ruta
        // BuildGraph() måste fyllas med alla rutorna i labyrinten.
        // Kan fylla metoden genom att skapa 2 loopar som går igenom varje koordinat (x och y) i labyrinten (går igenom varje ruta) och spara dem

        // Steg 5. Lägg till vänster- och högergrannar
        // efter loopen har lagt till rutan som nyckel och kopplat den till sin lista av grannar ( graph [currentNode] = neighbors; )
        // skriv kod som:
        // 1. kontrollerar att det finns en ruta till det hållet ( kolla steg 1, del 2: if (x > 0) { det finns en ruta till vänster } )
        // 2. kontrollera att ingen vägg blockerar rörelsen åt det hållet ( kolla steg 1, del 3: HasVerticalWall() och HasHorizontalWall() )
        // 3. om en ruta finns och vägen ditt är inte blockerad av en vägg, lägg till rutan till neighbors listan ( neighbors.Add(new Vector2Int(x - 1, y)); )

        // Steg 6. Låt FindShortestPath använda grafen från BuildGraph för att köra BFS
        // extra: använd stack för att visa den kortaste vägen

        // steg 7. Omvandla algorithmen från BFS till Djikstra
        // BFS använder: Dictionary<Vector2Int, List<Vector2Int>> graph; för att lagra varje ruta och dess grannar.
        // Djikstra måste veta vad en vägg kostar (weight). Så måste ändra från "gå till granne som är på shortestPath vägen" till "gå till granne med minst kostnad".
        // 1. Bestäm hur grafen ska representeras (mer än bara nodes och grannar, nu med weight)
        // Behåll grannlistan, men nu beräkna grannens weight innan "currentNode" tar steg mot grannen (kollar alla grannar -> vem har minst kostnad mot "goal"? -> gå ditt)
        // IMapData har:
        // * GetHorizontalWallCost(x, y)
        // * GetVerticalWallCost(x, y)
        // * GetJumpsFrom(from)
        // * TryGetJumpCost(from, to, out cost)
        // 
        // graf regler:
        // * Kostnad 1 (det kostar vanligt 1 för karaktären i labyrinten för att röra sig från en marker till en annan)
        // * halvvägg (karaktären kan gå över en halv vägg för ca 5.5f kostnad)
        // * vägg utan cost (en vägg har ingen kostnad, alltså karaktären kan inte gå igenom vägen eftersom varje "step" ska ha en kostnad)
        // * direkthopp/ "teleport" (ett "jump" är en seperat, riktat förbindelse. Om hoppet går från A - B betyder det inte automatiskt att man kan hoppa från B - A, "oneway")
        //
        // Djikstra och Relax:
        // två värden:
        // * distTo, den hittills billigaste kända kostnaden från start till den rutan
        // * edgeTo, vilken ruta karaktären kom från när den hittade den bästa vägen (previous node)
        // I början av algorithmen, sätt alla rutors kostnader till oändlighet (IsMovementBlocked() har IsPositiveInfinity)
        // och start rutan får kostnaden 0. Sen när karaktären kollar på en nod så ska grannens kostnad "förbättras" (Relax() metod)
        // Relax:
        // Ett exempel, karaktären har hittat en vägg till ruta A med kostnaden 3, och förflyttningen från A till B kostar 5.5, den nya
        // möjliga kostnaden för B blir: 3 + 5.5 = 8.5
        // Om B's tidigare kända kostnad var oändlig, är 8.5 förbättrad. Då uppdateras kostnaden och sparar A som B's föregegångare (previous node)
        // In short, Relax metoden kontrollerar om en ny väg ger lägre kostnad än den bästa karaktären redan känner till.
        // exempel kod:
        // float newCost = distTo[current] + edgeCost;
        // if (newCost < distTo[neighbor])
        // {
        //     distTo[neighbor] = newCost;
        //     parentMap[neighbor] = current;
        // }
        //
        // Behöver också en datastruktur som väljer nästa ruta med lägst känd kostnad, som en prioritetskö.

        // Steg 8. Hur ska BFS omvandlas?
        // behåller BFS grannlistan och beräknar kostnaden när en granne undersöks (start node = 0 kostnad -> kollar grannar -> vad är grannars kostnader?)
        // kan skapa en Edge struct utanför PathfinderAlgorithm.cs för att beräkna vägen och kostnaden
        // och en seperat BuildGraph() för att Djikstra ( BuildWeightedGraph() ) som använder den Edge structen
        // exempel:
        // * ingen vägg mellan node A och B: cost = 1
        // * passerbar vägg mellan node A och B: cost = 5.5 (exempel värde)
        // * omöjlig vägg mellan node A och B: cost = maxValue (kan inte gå till noden)
        // * hopp från node A till B: cost = kostnaden som anges för jump


        // bygger grafen med hjälpfunktionen BuildGraph()
        Dictionary<Vector2Int, List<Vector2Int>> graph = BuildGraph(mapData);

        // failsafe
        if (!graph.ContainsKey(start) || !graph.ContainsKey(goal))
        {
            Debug.LogWarning("Start- eller målnod finns inte i grafen!");
            return new List<Vector2Int>();
        }

        // skapar datastruktur för BFS
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // dictionary för att spara vägen tillbaka. TKey = current node, TValue = previous noden
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        // initiera sökningen med startnoden
        queue.Enqueue(start);
        visited.Add(start);

        // bool så att den bara söker efter path en gång
        bool pathFound = false;

        // BFS loopen
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // sluta söka om man har redan hittat målet
            if (current == goal)
            {
                pathFound = true;
                break;
            }

            // gå igenom alla grannar till current node / rutan
            foreach (Vector2Int neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parentMap[neighbor] = current; // spara att den kom till neighbor noden från current noden
                    queue.Enqueue(neighbor);
                }
            }
        }

        // failsafe
        if (!pathFound)
        {
            Debug.LogWarning("Ingen väg kunde hittas mellan start och mål.");
            return new List<Vector2Int>(); // Returnera tom lista om väg saknas
        }

        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        Vector2Int currentBacktrackNode = goal;

        // backa från målet tills det når startnoden
        while (currentBacktrackNode != start)
        {
            pathStack.Push(currentBacktrackNode);
            currentBacktrackNode = parentMap[currentBacktrackNode];
        }
        pathStack.Push(start);  // lägger till start noden efter loopen till stacken

        // konvertar stacken till en list
        List<Vector2Int> shortestPath = new List<Vector2Int>();
        while (pathStack.Count > 0)
        {
            shortestPath.Add(pathStack.Pop());
        }

        return shortestPath;
    }

    // Hjälpfunktion för att bygga graf av labyrinten
    private static Dictionary<Vector2Int, List<Vector2Int>> BuildGraph(IMapData mapData)
    {
        // börja med att skapa en tom dictionary för grafen
        // TKey (<Vector2Int) = koordinaterna för rutan, TValue (List<Vector2Int>>) = lista av rutans grannar
        Dictionary<Vector2Int, List<Vector2Int>> graph = new Dictionary<Vector2Int, List<Vector2Int>> ();

        // for loopar som går igenom varje koordinat (x, y) i labyrinten (loopar så länge koordinaterna är mindre än labyrintens height och width)
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                // skapa koordinaterna för rutan loopen är på
                Vector2Int currentNode = new Vector2Int (x, y);

                // skapa en tom lista för rutans grannar
                List<Vector2Int> neighbors = new List<Vector2Int> ();

                // lägg till rutan som nyckel i grafen och koppla den till sin lista av grannar
                graph [currentNode] = neighbors;

                // Lägger till grannarna till neighbors listan
                // om noden inte är utanför labyrinten (x > 0) och det är ingen vägg som blockerar vägen (!HasVerticalWall())
                if (x > 0 && !mapData.HasVerticalWall(x, y))                        // vänster node
                {
                    // lägger till noden till currentNode's lista av neighbors
                    // ( x - 1 eftersom den ligger vänster från currentNode, men samma y koordinat )
                    neighbors.Add(new Vector2Int(x - 1, y));
                }

                // om noden inte är utanför labyrinten (x < mapData.Width - 1) och det är ingen vägg som blockerar vägen (!HasVerticalWall(x + 1))
                // ( mapData.Width - 1 för att kontrollera att noden är inte utanför labyrintens width, och x + 1 för ? )
                if (x < mapData.Width - 1 && !mapData.HasVerticalWall(x + 1, y))    // höger node
                {
                    // ( x + 1 eftersom den ligger höger från currentNode, men samma y koordinat )
                    neighbors.Add(new Vector2Int(x + 1, y));
                }

                // om noden inte är utanför labyrinten (y > 0) och det är ingen vägg som blockerar vägen (!HasHorizontalWall())
                if (y > 0 && !mapData.HasHorizontalWall(x, y))                      // under node
                {
                    // ( y - 1 eftersom den ligger under currentNode, men samma x koordinat )
                    neighbors.Add(new Vector2Int(x, y - 1));
                }

                // om noden inte är utanför labyrinten (y < mapData.height - 1) och det är ingen vägg som blockerar vägen (!hasHorizontalWall(y + 1))
                if (y < mapData.Height - 1 && !mapData.HasHorizontalWall(x, y + 1)) // ovan node
                {
                    // (y + 1 eftersom den ligger ovanför currentNode, men samma x koordinat )
                    neighbors.Add(new Vector2Int(x, y + 1));
                }
            }
        }

        // retunera grafen som byggdes
        return graph;
    }

    // Hjälpfunktion för att bygga en viktad graf av labyrinten (för Djikstra)
    private static Dictionary<Vector2Int, List<Edge>> BuildWeightedGraph(IMapData mapData)
    {
        // TKey (Vector2Int) = koordinaterna för rutan, TValue (List<Edge>) = lista av rutans viktade grannar
        Dictionary<Vector2Int, List<Edge>> graph = new Dictionary<Vector2Int, List<Edge>> ();

        // standard kostnad för ett steg i labyrinten
        float stepCost = 1.0f;

        // for-loop som går igenom varje koordinat (x, y) i labyrinten
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                // skapa koordinaterna för rutan loopen är på
                Vector2Int currentNode = new Vector2Int(x, y);

                // skapa en tom lista för rutans viktade grannar (Edges)
                List<Edge> neighbors = new List<Edge>();

                // lägg till rutan som nyckel i grafen och koppla den till sin lista av Edges (grannar)
                graph[currentNode] = neighbors;

                // Vänster Node
                if (x > 0 && !mapData.HasVerticalWall(x, y))
                {
                    Vector2Int targetNode = new Vector2Int(x - 1, y);

                    // skapar en Edge till koordinaten med kostnad
                    neighbors.Add(new Edge(targetNode, stepCost));
                }
                // höger node
                if (x < mapData.Width - 1 && !mapData.HasVerticalWall(x + 1, y))
                {
                    Vector2Int targetNode = new Vector2Int(x +1, y);
                    neighbors.Add(new Edge(targetNode, stepCost));
                }
                // under node
                if (y > 0 && !mapData.HasHorizontalWall(x, y))
                {
                    Vector2Int targetNode = new Vector2Int(x, y - 1);
                    neighbors.Add(new Edge(targetNode, stepCost));
                }
                // ovan node
                if (y < mapData.Height - 1 && !mapData.HasHorizontalWall(x, y + 1))
                {
                    Vector2Int targetNode = new Vector2Int(x, y + 1);
                    neighbors.Add(new Edge(targetNode, stepCost));
                }
            }
        }

        return graph;
    }

    // bool metod som tar emot två positioner (from och to) och kollar om det är möjligt att förflytta sig direkt från from till to
    // retunerar true om förflyttningen är blockerad eller ogiltig
    // retunerar false om förflyttningen är tillåten
    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        // om det finns ett "jump" från from till to
        // om ett "jump" finns, retunerar TryGetJumpCost() true och IsMovementBlocked() retunerar false
        // om dett "jump" inte finns så retunerar det false och man går vidare in i IsMovementBlocked()
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;   

        // sparar delta x och y värdena från from till to (from - to, hur många "rutor" i labyrinten från from till to)
        // (t.ex, om figuren flyttar sig en ruta åt vänster blir DeltaX alltså -1)
        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        // Math.Abs() ger absoluta värdet, så negativa tal blir positivia (alltså -1 blir 1)
        // kontrollerar att rutorna figurern provar att förflytta sig ligger exakt ett steg ifrån varandra
        // (ett steg höger = 1 + 0 = 1, ett steg ner = 0 + 1 = 1, två steg höger = 2 + 0 = 2, ett steg höger och ett steg upp = 1 + 1 = 2)
        // om suman av DeltaX + DeltaY inte är lika med 1 retunerars IsMovementBlocked() true (alltså det är inte ett tillåtet förflyttning mellan grannar, mer än 1 steg)
        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        // om X-koordinaten förändras (figuren rör sig åt höger eller vänster)
        if (deltaX != 0)
        {
            // kollar vilken väg som ska kontrolleras (om deltaX > 0, wallX = to.x, else wallX = from.x)
            int wallX = deltaX > 0 ? to.x : from.x;

            // frågar kartan (labyrinten) vad det kostar att passera väggen
            // (float 1 = vanlig väg, 1 steg kostar 1 float, float 5.5 = en vägg som man kan passera för en högre kostnad, oändlig float = kan inte passera väggen)
            float wallCost = mapData.GetVerticalWallCost(wallX, from.y);

            // kollar om väggen är omöjlig att passera, retunerar true för IsPositiveInfinity() om
            // kostnaden är oändlig eller om kostnaden är lika med eller större än MaxValue av float.
            return float.IsPositiveInfinity(wallCost) || wallCost >= float.MaxValue;
        }

        // Gör samma som ovan med deltaX (Vertical movement, vänster eller höger), men kollar deltaY (Horizontal movement, upp eller ner)
        int wallY = deltaY > 0 ? to.y : from.y;
        float horizontalWallCost = mapData.GetHorizontalWallCost(from.x, wallY);
        return float.IsPositiveInfinity(horizontalWallCost) || horizontalWallCost >= float.MaxValue;
    }

    // Edge struct för Djikstra, representerar förbindelse från en node till en annan (en node och dess grannar från grannlistan)
    private struct Edge
    {
        public Vector2Int To;   // koordinaten för noden man provar att ta sig till
        public float Cost;      // kostnaden för att förflytta sig från t.ex Node A -> B

        // konstruktur för att skapa förbindelse för både destination och kostnad
        // ( exempel: Edge edge = new Edge(new Vector2Int(3, 2), 5.5f);, betyder att förbindelsen leder till noden vid position (3, 2) och kostar 5.5f )
        public Edge(Vector2Int to, float cost)
        {
            To = to;
            Cost = cost;
        }
    }
}

// Varje ruta i labyrinten har X- och Y-koordinater, och varje ruta motsvarar en nod i grafen.
// Algoritmen undersöker vilka noder den kan nå och försöker hitta den billigaste vägen från start till mål.
// Alla rutor är inte automatiskt förbundna med varandra. Alltså algoritmen kan inte bara gå till vilken ruta som helst. Det måste följa vägen

// Uppgiften kräver att man ska skapa en grafdatastruktur.
// Grafdatastrukturern ska beskriva både vilka rutor som finns och vilka förflyttningar som är möjliga mellan dem.
// Sammanfattning:
// * Noder: Rutor i labyrinten
// * Kanter: Tillåtna förflyttningar mellan rutorna
// * Vikter: Kostnaden för att använda en kant
// * Sökalgoritmen: Undersöker grafen för att hitta vägen med lägst total kostnad

// Kod från BFS grafen (föreläsnings slides)
// private void bfs(Graph G, int s) {
//Queue<Integer> q = new Queue<Integer>();
//q.enqueue(s);
//marked[s] = true;
//while (!q.isEmpty())
//{
//    int v = q.dequeue();
//    for (int w : G.adj(v))
//        if (!marked[w])
//        {
//            q.enqueue(w);
//            marked[w] = true;
//            edgeTo[w] = v;
//        }
//}
//}
//
// Beskrivning:
// G.adj(v) från: for (int w : G.adj(v))
// * G: Grafen
// * v: Noden algoritmen undersöker just nu
// * G.adj(v): Ger grannarna som algoritmen kan nå från noden "v"
// * w: En av grannarna
// * adj(): som en fråga till grafen: "vilka noder kan jag ta mig direkt till från den här noden?" Om en node är blockerad,
//          då visas den inte i resultatet från adj(v). Visar bara de granner som går att förflytta sig till.

// Närhetslista: En närhetslista (adjacency list) är ett sätt att lagra vilka grannar varje nod har
// Exempelvis:
// Nod          Grannar
// A            B, C
// B            A, D
// C            A, D
// D            B, C
//
// Det betyder att A kan nå B och C direkt, men inte D direkt.
// En C# variant av det skulle vara en dictionary: Dictionary<Vector2Int, List<Vector2Int>>
// * Dictionary kopplar en nod till dess grannlista
// * List lagrar grannarna för just den noden.
// Exempelvis skulle noden (1, 1) kunna kopplas till en lista med tre grannar, om en vägg blockerar den fjärde riktningen

// Uppgift "flöde":
// JSON / IMapData (Innehåller labyrintens dimensioner, väggar och andra egenskaper) ->
// Grafen byggs en gång (Varje nod får en lista över tillåtna förflyttningar) ->
// BFS eller Dijkstra (Algoritmen frågar grafen efter grannar och använder dessa för att hitta en väg)
//
// Sepererar uppgiften i två delar:
// 1. Hur labyrinten representeras.
// 2. Hur man söker efter en väg i den.

// kod från Dijkstra grafen (föreläsnings slides)
//public class DijkstraSP
//{
//private DirectedEdge[] edgeTo;
//private double[] distTo;
//private IndexMinPQ<Double> pq;
//public DijkstraSP(EdgeWeightedDigraph G, int s)
//{
//    edgeTo = new DirectedEdge[G.V()];
//    distTo = new double[G.V()];
//    pq = new IndexMinPQ<Double>(G.V());
//    for (int v = 0; v < G.V(); v++)
//        distTo[v] = Double.POSITIVE_INFINITY;
//    distTo[s] = 0.0;
//    pq.insert(s, 0.0);
//    while (!pq.isEmpty())
//    {
//        int v = pq.delMin();
//        for (DirectedEdge e : G.adj(v))
//           relax(e);
//    }
//  }
//}
//
//private void relax(DirectedEdge e)
//{
//    int v = e.from(), w = e.to();
//    if (distTo[w] > distTo[v] + e.weight())
//    {
//        distTo[w] = distTo[v] + e.weight();
//        edgeTo[w] = e;
//        if (pq.contains(w)) pq.decreaseKey(w, distTo[w]);
//        else pq.insert(w, distTo[w]);
//    }
//}
//
// Hur Dijkstra algoritmen kopplas till uppgiften:
// Dijkstra koden använder: distTo[w] = distTo[v] + e.weight();
//                          edgeTo[w] = e;
//
// Senare när man kommer till Dijkstra delen av uppgiften så kommer grafens grannlista (dictionary ovan) att ge oss
// de utgående kanterna från den aktuella noden. Varje kant behöver då ha både en destionation och en kostnad.
// 
// I den första BFS-versionen av uppgiften räcker det att känna till grannarna. När man senare lägger till vikter utökar man grafen
// så att den kan beskriva kostnaden för varje förflyttning
// (jobba stegvist: först bara algoritm med noder och granner (BFS), sen lägg till vikter och beräkna förflyttning (Djikstra))
//
// Uppgiften görs i 3 steg:
// 1. Grafpresentation (skapar en datastruktur som lagrar vilka rutor som går att nå från varje ruta. Uppfyller kravet "bygga en explicit graf")
// 2. BFS (använder grafen för att hitta en väg utan att ta hänsyn till kostnader)
// 3. Dijkstra (utökar grafen med kantvikter och hittar vägen med lägst total kostnad, inklusive relevanta hopp)
