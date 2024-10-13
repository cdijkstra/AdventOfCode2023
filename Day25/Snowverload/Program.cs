using FluentAssertions;

namespace Snowverload;

class Program
{
    static void Main(string[] args)
    {
        var snowVerload = new SnowverLoad();
        // snowVerload.Solve1("dummydata").Should().Be(54);
        var ans = snowVerload.Solve1("data");
        Console.WriteLine(ans);
        
        // 22920 is too low
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
            
            // Now cut wires, where GetCombination() finds all possible KV combinations that are cut
            foreach (var combo in GetCombinations())
            {
                // Create a deep copy of _graph
                var newGraph = _graph.ToDictionary(
                    entry => entry.Key,                    // Copy the key as-is
                    entry => new List<string>(entry.Value) // Create a new list from the value (deep copy)
                );
                // Remove the three values
                combo.ForEach(kvPair => newGraph[kvPair.key].Remove(kvPair.value));

                // Now remove any keys without values
                var keysToRemove = newGraph
                    .Where(entry => entry.Value.Count == 0)
                    .Select(entry => entry.Key).ToList();
                keysToRemove.ForEach(key => newGraph.Remove(key));
                
                // Now calculate the total size of the total graph.
                var firstEntry = newGraph.Keys.ToArray()[0];
                Queue<string> toVisit = new();
                toVisit.Enqueue(firstEntry);
                HashSet<string> visited = new();

                while (toVisit.Count > 0)
                {
                    var node = toVisit.Dequeue();
                    visited.Add(node);
                    
                    var connectedNodes = newGraph // Find keys of occurrence of node at RHS
                        .Where(kvp => kvp.Value.Contains(node))
                        .Select(kvp => kvp.Key).ToList()
                        .Concat(newGraph.GetValueOrDefault(node, new List<string>())); // Append values at RHS if key occurs

                    // We have now found all nodes we can traverse to from the current node,
                    // but still have to exclude those places we've already been
                    var validConnectedNodes = connectedNodes.Where(n => !visited.Contains(n)).ToList();
                    
                    validConnectedNodes.ForEach(newNode => toVisit.Enqueue(newNode));
                }

                var maxSize = FindUniqueStrings(newGraph);
                var size1 = visited.Count;
                var size2 = maxSize - size1;
                var totalSize = size1 * size2;
                if (totalSize != 0)
                {
                    return totalSize;
                }
            }

            return 0;
        }
        
        private IEnumerable<List<(string key, string value)>> GetCombinations()
        {
            // Create a list of (key, value) pairs from the dictionary
            var valuePairs = new List<(string key, string value)>();

            foreach (var kvp in _graph)
            {
                foreach (var value in kvp.Value)
                {
                    valuePairs.Add((kvp.Key, value));
                }
            }

            for (var i = 0; i < valuePairs.Count; i++)
            {
                for (var j = i + 1; j < valuePairs.Count; j++)
                {
                    for (var k = j + 1; k < valuePairs.Count; k++)
                    {
                        yield return new List<(string key, string value)>
                        {
                            valuePairs[i], valuePairs[j], valuePairs[k]
                        };
                    }
                }                
            }
            
        }
        
        public static int FindUniqueStrings(Dictionary<string, List<string>> dict)
        {
            // Use a HashSet to store unique strings
            HashSet<string> uniqueStrings = new HashSet<string>();

            // Add all keys to the HashSet (keys are unique by default)
            foreach (var key in dict.Keys)
            {
                uniqueStrings.Add(key);
            }

            // Add all values from the value lists to the HashSet
            foreach (var valueList in dict.Values)
            {
                foreach (var value in valueList)
                {
                    uniqueStrings.Add(value);  // Add each value to the HashSet
                }
            }

            // The count of unique strings is the size of the HashSet
            return uniqueStrings.Count;
        }
    }
}
