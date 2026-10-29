using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // FindShortestPath() metoden för att hitta en väg från start till goal i labyrinten
    // Metoden har två versioner: BFS och Dijkstra
    //
    // BFS bygger en vanlig graf med BuildGraph()-metoden och använder en Queue för att
    // undersöka noderna i den ordning de upptäcks. Den hittar en väg med minst antal
    // kanter, där varje förflyttning räknas som ett steg.
    //
    // Dijkstra bygger en viktad graf med BuildWeightedGraph()-metoden och använder
    // både kant kostnader (Edge) och Relax()-metoden för att hita vägen med lägst kostnad.
    //
    // Båda versionerna sparar previous noder i parentMap och återskapar den hittade vägen genom
    // att gå bakåt från målet till starten.
    //
    // För att växla mellan algoritmerna måste man avkommentera versionen man vill testa
    // (båda versionerna är markerade med "//=====[Namn]====//") och kommentera ut den
    // andra versionen.
    // Just nu är Dijkstra versionen den aktiva och JsonLoader har kartan: ex2.json
    // som den aktiva kartan. Jag har lagt till 2 kartor, en av dem är "maskhål" kartan från canvas och
    // en karta som heter "testMap" som är bra för att visa skillnaden mellan Dijkstra och BFS.
    //
    // Jag rekommenderar att testa både Dijkstra och BFS genom att ändra den aktiva kartan till
    // "testMap.json" (skickade med den i inlämningen) genom att klicka på "JsonLoader" i Hierarchy-fönstret
    // och skriva in "testMap.json" i Inspectorn där det står "File Name". Annars om du har en existerande
    // karta för att testa algoritmer så är det såklart även bättre.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        //=====[FindShortestPath: BFS versionen]=====//
        // bygger grafen med hjälpfunktionen BuildGraph()
        //Dictionary<Vector2Int, List<Vector2Int>> graph = BuildGraph(mapData);

        //// failsafe
        //if (!graph.ContainsKey(start) || !graph.ContainsKey(goal))
        //{
        //    Debug.LogWarning("Start- eller målnod finns inte i grafen!");
        //    return new List<Vector2Int>();
        //}

        //// skapar datastruktur för BFS
        //Queue<Vector2Int> queue = new Queue<Vector2Int>();
        //HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        //// dictionary för att spara vägen tillbaka. TKey = current node, TValue = previous noden
        //Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        //// initiera sökningen med startnoden
        //queue.Enqueue(start);
        //visited.Add(start);

        //// bool så att den bara söker efter path en gång
        //bool pathFound = false;

        //// BFS loopen
        //while (queue.Count > 0)
        //{
        //    Vector2Int current = queue.Dequeue();

        //    // sluta söka om man har redan hittat målet
        //    if (current == goal)
        //    {
        //        pathFound = true;
        //        break;
        //    }

        //    // gå igenom alla grannar till current node / rutan
        //    foreach (Vector2Int neighbor in graph[current])
        //    {
        //        if (!visited.Contains(neighbor))
        //        {
        //            visited.Add(neighbor);
        //            parentMap[neighbor] = current; // spara att den kom till neighbor noden från current noden
        //            queue.Enqueue(neighbor);
        //        }
        //    }
        //}
        //// failsafe
        //if (!pathFound)
        //{
        //    Debug.LogWarning("Ingen väg kunde hittas mellan start och mål.");
        //    return new List<Vector2Int>(); // Returnera tom lista om väg saknas
        //}

        //Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        //Vector2Int currentBacktrackNode = goal;

        //// backa från målet tills det når startnoden
        //while (currentBacktrackNode != start)
        //{
        //    pathStack.Push(currentBacktrackNode);
        //    currentBacktrackNode = parentMap[currentBacktrackNode];
        //}
        //pathStack.Push(start);  // lägger till start noden efter loopen till stacken

        //// konvertar stacken till en list
        //List<Vector2Int> shortestPath = new List<Vector2Int>();
        //while (pathStack.Count > 0)
        //{
        //    shortestPath.Add(pathStack.Pop());
        //}
        //return shortestPath;



        //=====[FindShortestPath: Dijkstra versionen]=====//
        // bygger grafen med hjälpfunktionen BuildWeightedGraph()
        Dictionary<Vector2Int, List<Edge>> graph = BuildWeightedGraph(mapData);

        // initierar alla noders kostnader till positiv oändlighet
        // start noden får kostnaden 0 eftersom det inte ska kosta något att starta
        Dictionary<Vector2Int, float> distTo = new Dictionary<Vector2Int, float>();

        foreach (Vector2Int node in graph.Keys)
        {
            distTo[node] = float.PositiveInfinity;
        }
        distTo[start] = 0.0f;

        // sparar previous nod på den hittils billigaste vägen
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        // skapar en lista över noder som väntar på att undersökas
        // noden med lägst känd kostnad (distTo) väljs genom att söka igenom
        // listan efter noden med lägst kostnad.
        List<Vector2Int> nodesToVisit = new List<Vector2Int>();

        // börjar med startnoden
        nodesToVisit.Add(start);

        // noder som redan har undersökts
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        while (nodesToVisit.Count > 0)
        {
            // börjar med att anta att den första noden är billigast
            int lowestCostIndex = 0;

            // letar efter noden med lägst känd kostnad
            for (int i = 1; i < nodesToVisit.Count; i++)
            {
                if (distTo[nodesToVisit[i]] < distTo[nodesToVisit[lowestCostIndex]])
                {
                    lowestCostIndex = i;
                }
            }
            // hämtar den billigaste noden och tar bort den från listan
            Vector2Int currentNode = nodesToVisit[lowestCostIndex];
            nodesToVisit.RemoveAt(lowestCostIndex);

            // hoppar över noden om den redan har undersökts
            if (visited.Contains(currentNode)) continue;

            visited.Add(currentNode);

            // avslutar while-loopen när karaktären har nått goal noden
            if (currentNode == goal) break;

            // undersök alla grannar till currentNode
            foreach (Edge edge in graph[currentNode])
            {
                // hoppar över grannar som redan har undersökts
                if (visited.Contains(edge.To)) continue;

                // anropar Relax för att beräkna kostnader för nodens grannar och uppdaterar listorna om en billigare väg hittas
                Relax(currentNode, edge, distTo, parentMap, nodesToVisit);
            }
        }
        // lista för den hittade vägen
        List<Vector2Int> path = new List<Vector2Int>();

        // börjar vid goal och går tillbaka till start
        Vector2Int pathNode = goal;
        while (pathNode != start)
        {
            path.Add(pathNode);
            pathNode = parentMap[pathNode];
        }
        // lägger till start noden i path (while-loopen lägger inte till den)
        path.Add(start);

        // vänder listan så att vägen börjar vid start noden
        path.Reverse();

        // TEST, Debug kod
        // Använder för att testa om kostnaden faktiskt stämmer
        float calculatedPathCost = 0.0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2Int from = path[i];
            Vector2Int to = path[i + 1];

            float edgeCost = float.PositiveInfinity;

            foreach (Edge edge in graph[from])
            {
                if (edge.To == to && edge.Cost < edgeCost) edgeCost = edge.Cost;
            }

            calculatedPathCost += edgeCost;
        }
        Debug.Log($"Dijkstra cost: {distTo[goal]}");
        Debug.Log($"Recalculated path cost: {calculatedPathCost}");

        // returnerar den färdiga vägen
        return path;
    }

    // Hjälpfunktion som bygger en graf av labyrinten för BFS.
    // Grafen representeras av en Dictionary där varje nyckel är en nod
    // (rutans koordinater) och värdet är en lista med nodens grannar.
    //
    // Metoden går igenom alla rutor i labyrinten och undersöker möjliga
    // förflyttningar i fyra riktningar (vänster, höger, ovan, under).
    // - Om en granne ligger utanför labyrinten läggs den inte till.
    // - Om IsMovementBlocked() visar att förflyttningen är blockerad läggs grannen inte till
    // - Om förflyttningen är tillåten läggs grannen till i listan.
    // - Alla jump som utgår från den aktuella noden läggs till i listan över grannar
    // Eftersom grafen inte beräknar kostnader så räknas varje förflyttning (som är tillåtet) som ett steg.
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

                // kollar varje neighboring nodes och ser om de är innomför labyrinten
                if (x > 0)                  // vänster node
                {
                    // skapar "targetNode" för grannen
                    Vector2Int targetNode = new Vector2Int(x - 1, y);

                    // om IsMovementBlocked är false, lägger till targetNode till currentNode's lista av neighbors
                    if (!IsMovementBlocked(currentNode, targetNode, mapData)) neighbors.Add(targetNode);
                }
                if (x < mapData.Width - 1)      // höger node
                {
                    Vector2Int targetNode = new Vector2Int(x + 1, y);

                    if (!IsMovementBlocked(currentNode, targetNode, mapData)) neighbors.Add(targetNode);
                }
                if (y > 0)                  // under node
                {
                    Vector2Int targetNode = new Vector2Int(x, y - 1);

                    if (!IsMovementBlocked(currentNode, targetNode, mapData)) neighbors.Add(targetNode);
                }
                if (y < mapData.Height - 1) // ovan node
                {
                    Vector2Int targetNode = new Vector2Int(x, y + 1);

                    if (!IsMovementBlocked(currentNode, targetNode, mapData)) neighbors.Add(targetNode);
                }

                // Hämtar alla jump som utgår från currentNode
                List<JumpConnection> jumps = mapData.GetJumpsFrom(currentNode);
                foreach (JumpConnection jump in jumps)
                {
                    // Lägger till jumpens destinationsnod i listan
                    neighbors.Add(jump.To);
                }
            }
        }
        // retunera grafen som byggdes
        return graph;
    }

    // Hjälpfunktion för att bygga en viktad graf av labyrinten för Dijkstra.
    // Grafen representeras av en Dictionary där varje nyckel är en nod
    // (rutans koordinat) och värdet är en lista av nodens viktade grannar (Edge).
    //
    // Metoden går igenom alla rutor i labyrinten och undersöker möjliga
    // förflyttningar i fyra riktningar (vänster, höger, ovan, under) från currentNode
    // med hjälp av IsMovementBlocked()-metoden.
    // - Om en förflyttning är blockerad läggs ingen kant (Edge) till i listan
    // - Om förflyttningen är tillåten kollar det om det finns ett "jump"
    // - Om det finns ett jump används dess kostnad (TryGetJumpCost)
    // - Annars hämtas väggkostnaden (GetVerticalWallCost(x, y)/ GetHorizontalWallCost(x, y).
    //   Om det inte finns någon vägg används standardkostnaden för ett steg (1.0f)
    // - Och till slut så läggs jumps till grafen tillsammans med deras kostnader.
    private static Dictionary<Vector2Int, List<Edge>> BuildWeightedGraph(IMapData mapData)
    {
        // TKey (Vector2Int) = koordinaterna för rutan, TValue (List<Edge>) = lista av rutans viktade grannar
        Dictionary<Vector2Int, List<Edge>> graph = new Dictionary<Vector2Int, List<Edge>> ();

        // for-loop som går igenom varje koordinat (x, y) i labyrinten och
        // skapar en nod för varje ruta/ nod
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                // skapa koordinaterna för rutan loopen är på
                Vector2Int currentNode = new Vector2Int(x, y);

                // varje nod får en tom lista av viktade förbindelser
                graph[currentNode] = new List<Edge>();

                // undersöker varje granne för currentNode för att see om det finns förbindelse mellan dem
                // kollar om grannen är inom labyrinten (inte utanför)
                if (x > 0)  // vänster node
                {
                    // skapar "targetNode" för grannen
                    Vector2Int targetNode = new Vector2Int(x - 1, y);

                    // kollar om vägen mellan current och targetNode är blockerad genom att använda IsMovementBlocked() bool metoden
                    if (!IsMovementBlocked(currentNode, targetNode, mapData))
                    {
                        // float för vägens kostnad
                        float edgeCost;

                        // kollar om det finns ett hopp mellan current och target node, om det finns, hämta kostnaden för jump
                        if (mapData.TryGetJumpCost(currentNode, targetNode, out float jumpCost)) edgeCost = jumpCost;
                        else
                        {
                            // om det inte finns ett jump, hämta kostnaden för väggen mellan rutorna. Om det inte finns en vägg
                            // så returneras den vanliga kostnaden för ett steg mellan två nodes (1.0f)
                            edgeCost = mapData.GetVerticalWallCost(x, y);
                        }
                        // lägg förbindelsen mellan noderna i currentNodes grannlista
                        graph[currentNode].Add(new Edge(targetNode, edgeCost));
                    }
                }
                // kollar om grannen är inom labyrinten (inte utanför)
                if (x < mapData.Width - 1)  // höger node
                {
                    // skapar "targetNode" för grannen
                    Vector2Int targetNode = new Vector2Int(x + 1, y);

                    // kollar om vägen mellan current och targetNode är blockerad genom att använda IsMovementBlocked() bool metoden
                    if (!IsMovementBlocked(currentNode, targetNode, mapData))
                    {
                        // float för vägens kostnad
                        float edgeCost;

                        // kollar om det finns ett hopp mellan current och target node, om det finns, hämta kostnaden för jump
                        if (mapData.TryGetJumpCost(currentNode, targetNode, out float jumpCost)) edgeCost = jumpCost;
                        else
                        {
                            // om det inte finns ett jump, hämta kostnaden för väggen mellan rutorna. Om det inte finns en vägg
                            // så returneras den vanliga kostnaden för ett steg mellan två nodes (1.0f)
                            edgeCost = mapData.GetVerticalWallCost(x + 1, y);
                        }
                        // lägg förbindelsen mellan noderna i currentNodes grannlista
                        graph[currentNode].Add(new Edge(targetNode, edgeCost));
                    }
                }
                // kollar om grannen är inom labyrinten (inte utanför)
                if (y > 0)  // under node
                {
                    // skapar "targetNode" för grannen
                    Vector2Int targetNode = new Vector2Int(x, y - 1);

                    // kollar om vägen mellan current och targetNode är blockerad genom att använda IsMovementBlocked() bool metoden
                    if (!IsMovementBlocked(currentNode, targetNode, mapData))
                    {
                        // float för vägens kostnad
                        float edgeCost;

                        // kollar om det finns ett hopp mellan current och target node, om det finns, hämta kostnaden för jump
                        if (mapData.TryGetJumpCost(currentNode, targetNode, out float jumpCost)) edgeCost = jumpCost;
                        else
                        {
                            // om det inte finns ett jump, hämta kostnaden för väggen mellan rutorna. Om det inte finns en vägg
                            // så returneras den vanliga kostnaden för ett steg mellan två nodes (1.0f)
                            edgeCost = mapData.GetHorizontalWallCost(x, y);
                        }
                        // lägg förbindelsen mellan noderna i currentNodes grannlista
                        graph[currentNode].Add(new Edge(targetNode, edgeCost));
                    }
                }
                // kollar om grannen är inom labyrinten (inte utanför)
                if (y < mapData.Height - 1)  // ovan node
                {
                    // skapar "targetNode" för grannen
                    Vector2Int targetNode = new Vector2Int(x, y + 1);

                    // kollar om vägen mellan current och targetNode är blockerad genom att använda IsMovementBlocked() bool metoden
                    if (!IsMovementBlocked(currentNode, targetNode, mapData))
                    {
                        // float för vägens kostnad
                        float edgeCost;

                        // kollar om det finns ett hopp mellan current och target node, om det finns, hämta kostnaden för jump
                        if (mapData.TryGetJumpCost(currentNode, targetNode, out float jumpCost)) edgeCost = jumpCost;
                        else
                        {
                            // om det inte finns ett jump, hämta kostnaden för väggen mellan rutorna. Om det inte finns en vägg
                            // så returneras den vanliga kostnaden för ett steg mellan två nodes (1.0f)
                            edgeCost = mapData.GetHorizontalWallCost(x, y + 1);
                        }
                        // lägg förbindelsen mellan noderna i currentNodes grannlista
                        graph[currentNode].Add(new Edge(targetNode, edgeCost));
                    }
                }
                // hämtar alla hopp
                List<JumpConnection> jumps = mapData.GetJumpsFrom(currentNode);

                foreach (JumpConnection jump in jumps)
                {
                    // för varje jump i listan, lägg till hoppet med dess destination och kostnad till grafen
                    graph[currentNode].Add(new Edge(jump.To, jump.Cost));
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

    // Struct för Dijkstra som representerar en viktad kant i grafen (Edge).
    // Innehåller destinations noden (To) och kostnaden (Cost)
    // För att förflytta sig från currentNode till targetNode
    private struct Edge
    {
        public Vector2Int To;   // koordinaten för kantens destinations nod
        public float Cost;      // kostnaden för förflyttningen till destinations noden

        // konstruktor som skapar en kant med destination och en kostnad.
        // Exempel: Edge edge = new Edge(new Vector2Int(3, 2), 5.5f);
        // betyder att en kant till noden (3, 2) har kostnaden 5.5f
        public Edge(Vector2Int to, float cost)
        {
            To = to;
            Cost = cost;
        }
    }

    // Relax metod för Dijkstra, används för att beräkna kostnader och uppdatera listorna om vägen är billigare
    private static void Relax(Vector2Int currentNode, Edge edge, Dictionary<Vector2Int, float> distTo, Dictionary<Vector2Int, Vector2Int> parentMap, List<Vector2Int> nodesToVisit)
    {
        // beräknar kostnaden för att nå destinationen via currentNode
        float newCost = distTo[currentNode] + edge.Cost;

        // om den nya vägen är billigare än den tidigare vägen
        if (newCost < distTo[edge.To])
        {
            // uppdaterar den billigaste kända kostnaden till destinationen
            distTo[edge.To] = newCost;

            // sparar currentNode som previous nod för att kunna återskapa vägen senare
            parentMap[edge.To] = currentNode;

            // lägger till destinationen om den inte redan väntar på att undersökas
            if (!nodesToVisit.Contains(edge.To)) nodesToVisit.Add(edge.To);
        }
    }
}
