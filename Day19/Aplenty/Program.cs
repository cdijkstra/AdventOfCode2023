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
        Console.WriteLine(aplenty.Solve2("data"));
    }
}

class Workflow
{
    public string Name { get; set; }
    public List<Condition> Conditions { get; set; }
}

record ValueRange(int Min, int Max);

class Condition
{
    public char RequirementChar { get; set; }
    public bool Greater { get; set; } // True for >, false for <
    public int Value { get; set; }
    public Func<int, bool> Requirement { get; private set; }
    public string RequirementString { get; set; }
    public string ConditionSatisfiedGoTo { get; set; }
    public string ConditionFailedGoTo { get; set; }
    
    public void SetRequirement(string requirement)
    {
        var parts = requirement.Split(new[] { '>', '<' }, StringSplitOptions.RemoveEmptyEntries);
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
    Dictionary<Characters, List<ValueRange>> _acceptedValues = new();
    private Queue<(Workflow, Dictionary<Characters, List<ValueRange>>)> _queue = new();
    
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
        Dictionary<Characters, ValueRange> initialAllowedVariables = Enum.GetValues(typeof(Characters))
            .Cast<Characters>()
            .ToDictionary(
                character => character,
                range => new ValueRange(1, 4000) 
            );
        
        return ProcessRanges("in", initialAllowedVariables, new List<string> {"in"});
    }

    Dictionary<Characters, ValueRange> acceptedValues = Enum.GetValues(typeof(Characters))
        .Cast<Characters>()
        .ToDictionary(
            character => character,
            range => new ValueRange(0, 1) 
        );
    
    // Recursive function. All conditions are of A > B or A < B type,
    //  so we can always keep track of a single range for each XMAS char, no need for List<ValueRange>
    private long ProcessRanges(string workflowName, Dictionary<Characters, ValueRange> ranges, List<string> path)
    {
        switch (workflowName)
        {
            case "A":
                var contribution = ranges.Values.Aggregate<ValueRange, long>(1, (current, range) => 
                    current * (range.Max - range.Min + 1));
                Console.WriteLine($"Contribution from {string.Join(',', path)}");
                Console.WriteLine($"Adding contribution: x = {ranges[Characters.x]}, m = {ranges[Characters.m]}, a = {ranges[Characters.a]}, s = {ranges[Characters.s]}");
                return contribution;
            case "R":
                return 0;
        }
        
        long result = 0;
        var workflow = _workflows.Single(wf => wf.Name == workflowName);
        foreach (var condition in workflow.Conditions)
        {
            var succeedPath = path.Append(condition.ConditionSatisfiedGoTo).ToList();
            var failedPath = path.Append(condition.ConditionFailedGoTo).ToList();
            
            Characters charEnum = (Characters)Enum.Parse(typeof(Characters), condition.RequirementChar.ToString());
            if (condition.Greater) // m>2090:A,rfg
            {
                if (ranges[charEnum].Max <= condition.Value)
                {
                    result += ProcessRanges(condition.ConditionFailedGoTo, ranges, failedPath);
                }
                else if (ranges[charEnum].Min > condition.Value)
                {
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, ranges, succeedPath);
                }
                else
                {
                    // Part goes to rejected and part goes to satisfied.
                    var succeedPart = new Dictionary<Characters, ValueRange>(ranges)
                    {
                        [charEnum] = new ValueRange(condition.Value + 1, ranges[charEnum].Max)
                    };
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, succeedPart, succeedPath);

                    var failedPart = new Dictionary<Characters, ValueRange>(ranges)
                    {
                        [charEnum] = new ValueRange(ranges[charEnum].Min, condition.Value)
                    };
                    
                    if (condition.ConditionFailedGoTo == "")
                    {
                        // Update values for next rule
                        ranges = failedPart;
                    }
                    else
                    {
                        result += ProcessRanges(condition.ConditionFailedGoTo, failedPart, failedPath);
                    }
                }
            }
            else // m<2090:A,rfg
            {
                if (ranges[charEnum].Max < condition.Value)
                {
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, ranges, succeedPath);
                }
                else if (ranges[charEnum].Min >= condition.Value)
                {
                    result += ProcessRanges(condition.ConditionFailedGoTo, ranges, failedPath);
                }
                else
                {
                    // Part goes to rejected and part goes to satisfied.
                    var succeedPart = new Dictionary<Characters, ValueRange>(ranges);
                    succeedPart[charEnum] = new ValueRange(ranges[charEnum].Min, condition.Value - 1);
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, succeedPart, succeedPath);

                    var failedPart = new Dictionary<Characters, ValueRange>(ranges);
                    failedPart[charEnum] = new ValueRange(condition.Value, ranges[charEnum].Max);
                    
                    if (condition.ConditionFailedGoTo == "")
                    {
                        // Update values for next rule
                        ranges = failedPart;
                    }
                    else
                    {
                        result += ProcessRanges(condition.ConditionFailedGoTo, failedPart, failedPath);
                    }
                }
            }
        }
        
        return result;
    }
    

    private Dictionary<Characters, List<ValueRange>> RetainRangesBelowValue(Characters charEnum, int retainValuesBelow, Dictionary<Characters, List<ValueRange>> acceptedValues)
    {
        acceptedValues[charEnum].RemoveAll(val => val.Min > retainValuesBelow);
        var modifyValue = acceptedValues[charEnum]
            .SingleOrDefault(val => val.Min < retainValuesBelow && val.Max > retainValuesBelow);
        if (modifyValue != default)
        {
            acceptedValues[charEnum].RemoveAll(val => val.Min < retainValuesBelow && val.Max > retainValuesBelow);
            acceptedValues[charEnum].Add(modifyValue with { Max = retainValuesBelow - 1 });
        }

        return acceptedValues;
    }
    
    private Dictionary<Characters, List<ValueRange>> RetainRangesAboveValue(Characters charEnum, int retainValuesAbove, Dictionary<Characters, List<ValueRange>> acceptedValues)
    {
        acceptedValues[charEnum].RemoveAll(val => val.Max < retainValuesAbove);
        var modifyValue = acceptedValues[charEnum]
            .SingleOrDefault(val => val.Min < retainValuesAbove && val.Max > retainValuesAbove);
        if (modifyValue != default)
        {
            acceptedValues[charEnum].RemoveAll(val => val.Min < retainValuesAbove && val.Max > retainValuesAbove);
            acceptedValues[charEnum].Add(modifyValue with { Min = retainValuesAbove + 1 });
        }
        
        return acceptedValues;
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
                                ConditionSatisfiedGoTo = accept,
                                ConditionFailedGoTo = reject,
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
                    if (condition.ConditionSatisfiedGoTo == "A")
                    {
                        return variables.KeyValues.Values.Sum();
                    }
                    if (condition.ConditionSatisfiedGoTo == "R")
                    {
                        return 0;
                    }

                    var acceptWorkflow = _workflows.Single(work => work.Name == condition.ConditionSatisfiedGoTo);
                    return CalculateAddedValueRecursively(variables, acceptWorkflow);
                }

                switch (condition.ConditionFailedGoTo)
                {
                    case "":
                        continue;
                    case "A":
                        return variables.KeyValues.Values.Sum();
                    case "R":
                        return 0;
                    default:
                    {
                        var rejectWorkflow = _workflows.Single(work => work.Name == condition.ConditionFailedGoTo);
                        return CalculateAddedValueRecursively(variables, rejectWorkflow);
                        // Else try for next entry.
                    }
                }
            }
        }

        throw new Exception("Should not come here");
    }
}