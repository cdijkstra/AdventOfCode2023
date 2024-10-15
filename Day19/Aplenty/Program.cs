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

record ValueRange(int Min, int Max);

class Condition
{
    public char RequirementChar { get; set; }
    public bool Greater { get; set; } // True for >, false for <
    public int Value { get; set; }
    public Func<int, bool> Requirement { get; private set; }
    public string RequirementString { get; set; }
    public string ConditionSatisfiedGoTo { get; set; }
    public string ConditionViolatedGoTo { get; set; }
    
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
        Dictionary<Characters, List<ValueRange>> initialAllowedVariables = Enum.GetValues(typeof(Characters))
            .Cast<Characters>()
            .ToDictionary(
                character => character,
                range => new List<ValueRange>{ new(1, 4000) }
            );
        _acceptedValues = Enum.GetValues(typeof(Characters))
            .Cast<Characters>()
            .ToDictionary(
                character => character,
                range => new List<ValueRange>()
            );
        
        _queue = new();
        _queue.Enqueue((_workflows.Single(x => x.Name == "in"), initialAllowedVariables));
        while (_queue.Count > 0)
        {
            var (workflow, allowedVariables) = _queue.Dequeue();
            
            foreach (var condition in workflow.Conditions)
            {
                Characters charEnum = (Characters)Enum.Parse(typeof(Characters), condition.RequirementChar.ToString());
                var allowedChars = allowedVariables[charEnum];

                var pass = new List<ValueRange>();
                var fail = new List<ValueRange>();
                
                if (condition.Greater)
                {
                    foreach (var range in allowedVariables[charEnum]
                                 .Where(range => range.Max > condition.Value))
                    {
                        if (range.Min >= condition.Value)
                        {
                            pass.Add(range);
                        }
                        else
                        {
                            pass.Add(range with { Min = condition.Value + 1 });
                        }
                    }
                    
                    foreach (var range in allowedVariables[charEnum]
                                 .Where(range => range.Min < condition.Value))
                    {
                        if (range.Max <= condition.Value)
                        {
                            fail.Add(range);
                        }
                        else
                        {
                            fail.Add(range with { Max = condition.Value - 1 });
                        }
                    }
                }
                else
                {
                    foreach (var range in allowedVariables[charEnum]
                                 .Where(range => range.Min < condition.Value))
                    {
                        if (range.Max <= condition.Value)
                        {
                            fail.Add(range);
                        }
                        else
                        {
                            fail.Add(range with { Max = condition.Value });
                        }
                    }
                    
                    foreach (var range in allowedVariables[charEnum]
                                 .Where(range => range.Max > condition.Value))
                    {
                        if (range.Min >= condition.Value)
                        {
                            pass.Add(range);
                        }
                        else
                        {
                            pass.Add(range with { Min = condition.Value });
                        }
                    }
                }
                
                switch (condition.ConditionSatisfiedGoTo)
                {
                    case "A":
                    {
                        _acceptedValues[charEnum].AddRange(pass);
                        break;
                    }
                    // case "R": // Remove entries
                    // {
                    //     allowedVariables = condition.Greater ? 
                    //         RetainRangesBelowValue(charEnum, condition.Value, allowedVariables) :
                    //         RetainRangesAboveValue(charEnum, condition.Value, allowedVariables);
                    //     break;
                    // }
                    default:
                        // Enqueue new workflow
                        _queue.Enqueue((_workflows.Single(work => work.Name == condition.ConditionSatisfiedGoTo),
                            allowedVariables));
                        break;
                }

                switch (condition.ConditionViolatedGoTo)
                {
                    case "A":
                    {
                        _acceptedValues[charEnum].AddRange(fail);
                        break;
                    }
                    // case "R":
                    //     allowedVariables = condition.Greater ? 
                    //         RetainRangesAboveValue(charEnum, condition.Value - 1, allowedVariables) :
                    //         RetainRangesBelowValue(charEnum, condition.Value + 1, allowedVariables);
                    //     break;
                    default:
                    {
                        if (condition.ConditionViolatedGoTo != "")
                        {
                            // Enqueue new workflow
                            _queue.Enqueue((_workflows.Single(work => work.Name == condition.ConditionViolatedGoTo),
                                allowedVariables));
                        }

                        break;
                    }
                }
            }
        }

        var totalProduct = 1;
        var contributions = _acceptedValues
            .ToDictionary(
                kvp => kvp.Key, // The key is the character
                kvp => kvp.Value.Sum(range => range.Max - range.Min + 1) // Sum contributions for each key
            );
        
        foreach (var contribution in contributions.Values)
        {
            totalProduct *= contribution; // Multiply the current contribution to the total
        }
        
        return totalProduct;
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
                                ConditionViolatedGoTo = reject,
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

                switch (condition.ConditionViolatedGoTo)
                {
                    case "":
                        continue;
                    case "A":
                        return variables.KeyValues.Values.Sum();
                    case "R":
                        return 0;
                    default:
                    {
                        var rejectWorkflow = _workflows.Single(work => work.Name == condition.ConditionViolatedGoTo);
                        return CalculateAddedValueRecursively(variables, rejectWorkflow);
                        // Else try for next entry.
                    }
                }
            }
        }

        throw new Exception("Should not come here");
    }
}