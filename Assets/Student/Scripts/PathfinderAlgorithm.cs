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
        // Most of your solution should be implemented in this method.

        Debug.LogWarning("FindShortestPath is not implemented.");
        return null;
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
