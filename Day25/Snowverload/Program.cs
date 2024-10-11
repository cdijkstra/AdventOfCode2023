namespace Snowverload;

class Program
{
    static void Main(string[] args)
    {
        var snowVerload = new SnowverLoad();
        var ans = snowVerload.Solve1("dummydata");
        Console.WriteLine(ans);
    }

    class SnowverLoad()
    {
        private HashSet<string> _nodes = new();
        private Dictionary<string, List<string>> _graph = new();
        
        public int Solve1(string fileName)
        {
            foreach (var line in File.ReadLines($"Data/{fileName}"))
            {
                var lhs = line.Split(':')[0];
                var rhs = line.Split(':')[1].Split().Skip(1).ToList();
                _nodes.Add(lhs);
                rhs.ForEach(entry => _nodes.Add(entry));
                
                // And add the graph
                _graph.Add(lhs, rhs);
            }

            List<int> sizes = new();
            
            // Now cut wires and calculate max size graph
            foreach (var combo in GetCombinations())
            {
                // Create a shallow copy of _graph
                var newGraph = new Dictionary<string, List<string>>(_graph);
                combo.ForEach(con => newGraph.Remove(con));
                foreach (var key in newGraph.Keys)
                {
                    combo.ForEach(com => newGraph[key].RemoveAll(item => item == com));
                }
                
                // Now calculate the total size of the total graph.
                
                var firstEntry = newGraph.Keys.ToArray()[0];
                Queue<string> toVisit = new();
                toVisit.Enqueue(firstEntry);
                List<string> visited = new();

                while (toVisit.Count > 0)
                {
                    var node = toVisit.Dequeue();
                    visited.Add(node);
                    
                    var connectedKeys = newGraph
                        .Where(kvp => kvp.Value.Contains(node))
                        .Select(kvp => kvp.Key)
                        .ToList();
                    var connectedValues = newGraph.GetValueOrDefault(node, new List<string>());
                    
                    var connectedNodes = connectedKeys.Concat(connectedValues);
                    
                    foreach (var newNode in connectedNodes.Where(n => !visited.Contains(n)))
                    {
                        toVisit.Enqueue(newNode);
                    }
                }

                var maxSize = _nodes.Count - 3;
                var size1 = visited.Count;
                var size2 = maxSize - size1;
                var totalSize = size1 * size2;
                if (totalSize != 0)
                {
                    sizes.Add(totalSize);
                }
            }

            return sizes.Max();
        }
        
        private IEnumerable<List<string>> GetCombinations()
        {
            for (var i = 0; i < _graph.Count; i++)
            {
                for (var j = i + 1; j < _graph.Count; j++)
                {
                    for (var k = j + 1; k < _graph.Count; k++)
                    {
                        var keysArray = _graph.Keys.ToArray();
                        yield return new List<string> { keysArray[i], keysArray[j], keysArray[k] };
                    }
                }
            }
        }
    }
}
