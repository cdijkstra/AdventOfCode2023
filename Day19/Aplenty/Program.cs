using System.Text.RegularExpressions;

namespace Aplenty;

class Program
{
    static void Main(string[] args)
    {
        var aplenty = new Aplenty();
        aplenty.Solve1("dummydata");
    }
}

class Workflow
{
    public string Name { get; set; }
    public List<string> Conditions { get; set; }
    public Workflow WorkflowAccept { get; set; }
    public Workflow WorkflowReject { get; set; }
}

class Instructions
{
    public Dictionary<char, int> Conditions { get; set; }
}

class Aplenty
{
    public int Solve1(string fileName)
    {
        var allContent = File.ReadLines($"Data/{fileName}");
        bool emptyLineEncountered = false;
        List<string> workflows = new List<string>();
        List<Instructions> instructions = new();

        // Iterate through the lines
        foreach (var line in allContent)
        {
            // Check if the line is empty or whitespace
            if (string.IsNullOrWhiteSpace(line))
            {
                // Set the flag to true once an empty line is encountered
                emptyLineEncountered = true;
            }
            else if (!emptyLineEncountered)
            {
                var name = line.Split('{')[0];
                string pattern = @"\{(.*?)\}";
                Match match = Regex.Match(line, pattern);
                string conditionString = match.Groups[1].Value;
                string conditionPattern = @"\w[<>]\d+:\w+(,(\w+)(?![<>]))?";
                var conditions = Regex.Matches(line, pattern);
                Console.WriteLine();
            }
            else if (emptyLineEncountered)
            {
                // If an empty line has been encountered, add the line to the list
                var conditions = line.Trim('{').TrimEnd('}').Split(",").Select(item =>
                    {
                        string[] parts = item.Split('=');
                        return new { Key = parts[0][0], Value = int.Parse(parts[1]) };
                    })
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                instructions.Add(new Instructions() { Conditions = conditions});
            }
        }
        return 1;
    }
}