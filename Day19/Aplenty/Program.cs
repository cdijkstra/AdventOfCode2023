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
        aplenty.Solve2("dummydata").Should().Be(167409079868000);

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
    public bool Greater { get; set; } // True for >, false for <
    public int Value { get; set; }
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


public enum Characters {
    x,m,a,s
}
class Aplenty
{
    List<Workflow> _workflows = new();
    List<VariableSet> _variableSet = new();
    
    public int Solve1(string fileName)
    {
        Initialize(fileName);

        var totalSum = 0;
        foreach (var variables in _variableSet)
        {
            var firstWorkflow = _workflows.Single(work => work.Name == "in");
            var result = CalculateAddedValueRecursively(variables, firstWorkflow);
            totalSum += result;
        }
        
        return totalSum;
    }
    
    public long Solve2(string fileName)
    {
        Initialize(fileName);

        long totalCount = 0;

        List<Dictionary<Characters, (int min, int max)>> allValues = new();
        
        foreach (var worflow in _workflows.Where(wf => wf.Conditions.Any(
                     cond => cond.Reject == "A" ||
                     cond.Accept == "A")))
        {
            Dictionary<Characters, (int min, int max)> dict = Enum.GetValues(typeof(Characters))
                .Cast<Characters>()
                .ToDictionary(
                    character => character,
                    range => (min: 1, max: 4000)
                );

            allValues.Add(FindContributionByReversing(dict, worflow, "A"));
        }
        
        // Remove duplicate entries... How?
        // Look at combinations of x,m,a,s and if they were already added
        foreach (var values in allValues)
        {
            Console.WriteLine($"x: {values[Characters.x].min}, {values[Characters.x].max}");
            Console.WriteLine($"m: {values[Characters.m].min}, {values[Characters.m].max}");
            Console.WriteLine($"a: {values[Characters.a].min}, {values[Characters.a].max}");
            Console.WriteLine($"s: {values[Characters.s].min}, {values[Characters.s].max}");
        }

        Dictionary<Characters, int> contributions = Enum.GetValues(typeof(Characters))
            .Cast<Characters>()
            .ToDictionary(
                character => character,
                character => allValues.Any(vals => vals.GetValueOrDefault(character) == (1, 4000)) ? 4000 : 0
            );
        
        return 5;
    }

    private Dictionary<Characters, (int min, int max)> FindContributionByReversing(Dictionary<Characters, (int min, int max)> dict,
        Workflow workflow, string findWorkflow)
    {
        var conditions = workflow.Conditions.Where(cond => cond.Reject == findWorkflow || cond.Accept == findWorkflow).ToList();
        foreach (var condition in conditions)
        {
            Characters charEnum = (Characters)Enum.Parse(typeof(Characters), condition.RequirementChar.ToString());
            var setMinValue = dict[charEnum].min;
            var setMaxValue = dict[charEnum].max;

            if (condition.Accept == findWorkflow)
            {
                if (condition.Greater) // Only Greater and Smaller allowed
                {
                    var minValue = Math.Max(setMinValue, condition.Value + 1);
                    var maxValue = Math.Min(setMaxValue, 4000);
                    dict[charEnum] = (min: minValue, max: maxValue);
                }
                else
                {
                    var minValue = Math.Max(setMinValue, 1);
                    var maxValue = Math.Min(setMaxValue, condition.Value - 1);
                    dict[charEnum] = (min: minValue, max: maxValue);
                }
            }
            if (condition.Reject == findWorkflow)
            {
                if (condition.Greater) // Only Greater and Smaller allowed
                {
                    // a > 20 -> reject when a <= 20
                    // Smaller than or equal
                    var minValue = Math.Max(setMinValue, 1);
                    var maxValue = Math.Min(setMaxValue, condition.Value);
                    dict[charEnum] = (min: minValue, max: maxValue);
                }
                else
                {
                    // a < 20 -> reject when a >= 20
                    var minValue = Math.Max(setMinValue, condition.Value);
                    var maxValue = Math.Min(setMaxValue, 4000);
                    dict[charEnum] = (min: minValue, max: maxValue);
                }
            }

        }
        
        if (workflow.Name == "in")
        {
            return dict;
        }
        
        // Otherwise, call function again until we are at "in"
        var newWorkflow = _workflows.Single(wf => wf.Conditions.Any(
            cond => cond.Reject == workflow.Name ||
                    cond.Accept == workflow.Name));

        return FindContributionByReversing(dict, newWorkflow, workflow.Name);
    }


    private void Initialize(string fileName)
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

                            var value = requirement.Contains('>') ? 
                                    requirement.Split(">")[1] :
                                    requirement.Split("<")[1];
                            
                            var condition = new Condition()
                            {
                                RequirementChar = requirement[0],
                                Greater = requirement.Contains('>'),
                                Value = int.Parse(value),
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
    }
    
    private int CalculateAddedValueRecursively(VariableSet variables, Workflow workflow)
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
                    return CalculateAddedValueRecursively(variables, acceptWorkflow);
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
                return CalculateAddedValueRecursively(variables, rejectWorkflow);
                // Else try for next entry.
            }
        }

        throw new Exception("Should not come here");
    }
}