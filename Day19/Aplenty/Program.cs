using System.Text.RegularExpressions;
using FluentAssertions;

namespace Aplenty;

class Program
{
    static void Main(string[] args)
    {
        var aplenty = new Aplenty();
        aplenty.Solve1("dummydata").Should().Be(19114);
        Console.WriteLine(aplenty.Solve1("data"));
    }
}

class Workflow
{
    public string Name { get; set; }
    public List<Condition> Conditions { get; set; }
}

class Condition
{
    public char RequirementChar { get; set; }
    public Func<int, bool> Requirement { get; private set; }
    public string RequirementString { get; set; }
    public string Accept { get; set; }
    public string Reject { get; set; }
    
    public void SetRequirement(string requirement)
    {
        var parts = requirement.Split(new[] { '>', '<' }, StringSplitOptions.RemoveEmptyEntries);
        var variable = parts[0].Trim();
        var value = int.Parse(parts[1].Trim());

        if (requirement.Contains('>'))
        {
            Requirement = input => input > value;
        }
        else if (requirement.Contains('<'))
        {
            Requirement = input => input < value;
        }
        else
        {
            throw new ArgumentException("Invalid requirement format");
        }
    }
}

class VariableSet
{
    public Dictionary<char, int> KeyValues { get; set; }
}

class Aplenty
{
    List<Workflow> _workflows = new();
    List<VariableSet> _variableSet = new();
    
    public int Solve1(string fileName)
    {
        _workflows = new();
        _variableSet = new();
        
        var allContent = File.ReadLines($"Data/{fileName}");
        bool emptyLineEncountered = false;

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
                var conditions = Regex.Matches(line, conditionPattern);
                _workflows.Add(new()
                {
                    Name = name,
                    Conditions = conditions
                        .Select(match => match.Value)
                        .Select(cond =>
                        {
                            var requirement = cond.Split(":")[0];
                            var sendToString = cond.Split(":")[1];
                            var accept = string.Empty;
                            var reject = string.Empty;
                            if (sendToString.Contains(','))
                            {
                                accept = sendToString.Split(",")[0];
                                reject = sendToString.Split(",")[1];
                            }
                            else
                            {
                                accept = sendToString;
                            }
                            
                            var condition = new Condition()
                            {
                                RequirementChar = requirement[0],
                                Accept = accept,
                                Reject = reject,
                                RequirementString = requirement
                            };
                            condition.SetRequirement(requirement);
                            return condition;
                        }).ToList()
                });
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
                _variableSet.Add(new VariableSet() { KeyValues = conditions});
            }
        }

        var totalSum = 0;
        foreach (var variables in _variableSet)
        {
            var firstWorkflow = _workflows.Single(work => work.Name == "in");
            var result = CalculateAddedValue(variables, firstWorkflow);
            totalSum += result;
        }
        
        bool satisfiesCondition = _workflows.First().Conditions.First().Requirement(5000);
        
        return totalSum;
    }

    private int CalculateAddedValue(VariableSet variables, Workflow workflow)
    {
        foreach (var condition in workflow.Conditions) // Abort when new workflow has been found
        {
            if (variables.KeyValues.Keys.Contains(condition.RequirementChar))
            {
                var satisfied = condition.Requirement(variables.KeyValues[condition.RequirementChar]);
                if (satisfied)
                {
                    if (condition.Accept == "A")
                    {
                        return variables.KeyValues.Values.Sum();
                    }
                    if (condition.Accept == "R")
                    {
                        return 0;
                    }

                    var acceptWorkflow = _workflows.Single(work => work.Name == condition.Accept);
                    return CalculateAddedValue(variables, acceptWorkflow);
                }

                if (condition.Reject == String.Empty) continue;
                
                if (condition.Reject == "A")
                {
                    return variables.KeyValues.Values.Sum();
                }
                if (condition.Reject == "R")
                {
                    return 0;
                }
                
                var rejectWorkflow = _workflows.Single(work => work.Name == condition.Reject);
                return CalculateAddedValue(variables, rejectWorkflow);
                // Else try for next entry.
            }
        }

        throw new Exception("Should not come here");
    }
}