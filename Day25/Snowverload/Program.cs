using FluentAssertions;

namespace Snowverload;

class Program
{
    static void Main(string[] args)
    {
        var snowVerload = new SnowverLoad();
        snowVerload.Solve1("dummydata").Should().Be(54);
        // var ans = snowVerload.Solve1("data");
        // Console.WriteLine(ans);
    }
    
    public readonly record struct Edge(string from, string to);

    class SnowverLoad()
    {
        private HashSet<string> _nodes = new();
        private Dictionary<string, List<string>> _graph = new();
        private List<Edge> _edges = new();
        static Random rand = new();

        public int Solve1(string fileName)
        {
            var crossings = 3;
            
            foreach (var line in File.ReadLines($"Data/{fileName}"))
            {
                var lhs = line.Split(':')[0];
                var rhs = line.Split(':')[1].Split().Skip(1).ToList();
                _nodes.Add(lhs);
                rhs.ForEach(entry => _nodes.Add(entry));

                // And add the graph
                _graph.Add(lhs, rhs);
            }
            
            var noCrossings = 500;
            _edges = GetEdges();
            Dictionary<Edge, int> crossingCounts =  _edges.ToDictionary(edge => edge, edge => 0);

            for (int i = 0; i < noCrossings; i++)
            {
                var node1 = _graph.Keys.ToList()[rand.Next(_graph.Count)];
                var node2 = _graph.Keys.Where(key => key != node1).ToArray()[rand.Next(_graph.Count - 1)];
                var path = ShortestPath(node1, node2);
                for (var p = 1; p < path.Count; p++)
                {
                    // Find out which edges were most used
                    var from = path[p - 1];
                    var to = path[p];
                    var edge = _edges.Single(ed =>
                        (ed.from == from && ed.to == to) || (ed.from == to && ed.to == from));

                    crossingCounts[edge]++;
                }
            }

            var topK = crossingCounts.OrderByDescending(kvp => kvp.Value).Take(crossings).ToList();
            // This works
            
            //remove the 3 edges that we are guessing make the min cut
            var g2 = _graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToList());
            for (int i = 0; i < crossings; i++)
            {
                if (g2.ContainsKey(topK[i].Key.from) && g2[topK[i].Key.from].Contains(topK[i].Key.to))
                {
                    g2[topK[i].Key.from].Remove(topK[i].Key.to);
                }
                else if (g2.ContainsKey(topK[i].Key.to) && g2[topK[i].Key.to].Contains(topK[i].Key.from))
                {
                    g2[topK[i].Key.to].Remove(topK[i].Key.from);
                }
            }

            var size1 = FindLongestPath(g2.First().Key, g2);
            // Use a HashSet to store distinct strings
            var distinctStrings = new HashSet<string>();

            // Add all keys to the HashSet
            foreach (var key in g2.Keys)
            {
                distinctStrings.Add(key);
            }

            // Add all values to the HashSet
            foreach (var valueList in g2.Values)
            {
                foreach (var value in valueList)
                {
                    distinctStrings.Add(value);
                }
            }

            // The total count of distinct strings
            int totalDistinctCount = distinctStrings.Count;
            var size2 = totalDistinctCount - size1;
            
            return size1 * size2;
        }
        
        private List<string> ShortestPath(string start, string end)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<(string, List<string>)>();
            visited.Add(start);
            queue.Enqueue((start, new List<string> { start} ));
            while (queue.Any())
            {
                (var curr, var route) = queue.Dequeue();
                if (curr == end)
                {
                    return route;
                }

                var linkedNodes = _graph
                    .Where(kvp => kvp.Value.Contains(curr))
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (_graph.TryGetValue(curr, out var value))
                {
                    linkedNodes.AddRange(value);
                }
                
                foreach (var linkedNode in linkedNodes)
                {
                    if (visited.Contains(linkedNode))
                        continue;

                    visited.Add(linkedNode);
                    var newRoute = route.ToList();
                    newRoute.Add(linkedNode);
                    queue.Enqueue((linkedNode, newRoute));
                }
            }
            return null;
        }
        
        private int FindLongestPath(string start, Dictionary<string, List<string>> graph)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            visited.Add(start);
            queue.Enqueue(start);
            while (queue.Any())
            {
                var curr = queue.Dequeue();

                var linkedNodes = graph
                    .Where(kvp => kvp.Value.Contains(curr))
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (_graph.TryGetValue(curr, out var value))
                {
                    linkedNodes.AddRange(value);
                }
                
                foreach (var linkedNode in linkedNodes)
                {
                    if (visited.Contains(linkedNode))
                        continue;

                    visited.Add(linkedNode);
                    queue.Enqueue(linkedNode);
                }
            }
            return visited.Count;
        }
        
        private List<Edge> GetEdges()
        {
            var edges = new List<Edge>();
            var seenEdges = new HashSet<Edge>();

            foreach (var kvp in _graph)
            {
                foreach (var neighbor in kvp.Value)
                {
                    // Ensure we don't duplicate edges in the undirected graph
                    if (seenEdges.Contains(new(neighbor, kvp.Key))) continue;
                    edges.Add(new(kvp.Key, neighbor));
                    seenEdges.Add(new(kvp.Key, neighbor));
                }
            }
            return edges;
        }
        
        // static List<int> shortestPathExcludingEdges(Dictionary<int, List<int>> graph, int start, int end, Dictionary<Edge, int> without)
        // {
        //     var visited = new HashSet<int>();
        //     var queue = new Queue<(int, List<int>)>();
        //     visited.Add(start);
        //     queue.Enqueue((start, [start]));
        //     while (queue.Any())
        //     {
        //         (var curr, var route) = queue.Dequeue();
        //         if (curr == end)
        //             return route;
        //
        //         foreach (var edge in graph[curr])
        //         {
        //             var newEdge = curr < edge ? new Edge(curr, edge) : new Edge(edge, curr);
        //             if (without.ContainsKey(newEdge))
        //                 continue;
        //
        //             if (visited.Contains(edge))
        //                 continue;
        //
        //             visited.Add(edge);
        //             var newRoute = route.ToList();
        //             newRoute.Add(edge);
        //             queue.Enqueue((edge, newRoute));
        //         }
        //     }
        //     return null;
        // }
    }
}


