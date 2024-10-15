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
public enum Character {
    X,M,A,S
}
public enum Operator
{
    Greater, Smaller
}

class Condition
{
    public char RequirementChar { get; set; }
    public Operator Operator { get; set; }
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
    public Dictionary<char, int> KeyValues { get; set; } = new();
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
        Dictionary<Character, ValueRange> initialAllowedVariables = Enum.GetValues(typeof(Character))
            .Cast<Character>()
            .ToDictionary(
                character => character,
                range => new ValueRange(1, 4000) 
            );
        
        return ProcessRanges("in", initialAllowedVariables);
    }

    Dictionary<Character, ValueRange> acceptedValues = Enum.GetValues(typeof(Character))
        .Cast<Character>()
        .ToDictionary(
            character => character,
            range => new ValueRange(0, 1) 
        );
    
    // Recursive function. All conditions are of A > B or A < B type,
    //  so we can always keep track of a single range for each XMAS char, no need for List<ValueRange>
    private long ProcessRanges(string workflowName, Dictionary<Character, ValueRange> ranges)
    {
        switch (workflowName)
        {
            case "A":
                var contribution = ranges.Values.Aggregate<ValueRange, long>(1, (current, range) => current * (range.Max - range.Min + 1));
                return contribution;
            case "R":
                return 0;
        }
        
        long result = 0;
        var workflow = _workflows.Single(wf => wf.Name == workflowName);
        foreach (var condition in workflow.Conditions)
        {
            Character charEnum = (Character)Enum.Parse(typeof(Character), condition.RequirementChar.ToString().ToUpper());
            switch (condition.Operator)
            {
                case Operator.Greater when ranges[charEnum].Max <= condition.Value:
                    result += ProcessRanges(condition.ConditionFailedGoTo, ranges);
                    break;
                case Operator.Greater when ranges[charEnum].Min > condition.Value:
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, ranges);
                    break;
                case Operator.Greater:
                {
                    // Part goes to succeeded part and other part goes to rejected.
                    var succeedPart = new Dictionary<Character, ValueRange>(ranges);
                    succeedPart[charEnum] = new(condition.Value + 1, ranges[charEnum].Max);
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, succeedPart);

                    var failedPart = new Dictionary<Character, ValueRange>(ranges);
                    failedPart[charEnum] = new(ranges[charEnum].Min, condition.Value);
                    result += condition.ConditionFailedGoTo == "" 
                        ? 0 
                        : ProcessRanges(condition.ConditionFailedGoTo, failedPart);
                    
                    ranges = failedPart;
                    break;
                }
                // m<2090:A,rfg
                case Operator.Smaller when ranges[charEnum].Max < condition.Value:
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, ranges);
                    break;
                case Operator.Smaller when ranges[charEnum].Min >= condition.Value:
                    result += ProcessRanges(condition.ConditionFailedGoTo, ranges);
                    break;
                case Operator.Smaller:
                {
                    // Part goes to rejected and part goes to satisfied.
                    var succeedPart = new Dictionary<Character, ValueRange>(ranges);
                    succeedPart[charEnum] = new ValueRange(ranges[charEnum].Min, condition.Value - 1);
                    result += ProcessRanges(condition.ConditionSatisfiedGoTo, succeedPart);

                    var failedPart = new Dictionary<Character, ValueRange>(ranges);
                    failedPart[charEnum] = new ValueRange(condition.Value, ranges[charEnum].Max);
                    result += condition.ConditionFailedGoTo == ""
                    ? 0 
                    : ProcessRanges(condition.ConditionFailedGoTo, failedPart);
                    
                    ranges = failedPart;
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }
        
        return result;
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
                                Operator = requirement.Contains('>') ? Operator.Greater : Operator.Smaller,
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
                    switch (condition.ConditionSatisfiedGoTo)
                    {
                        case "A":
                            return variables.KeyValues.Values.Sum();
                        case "R":
                            return 0;
                        default:
                        {
                            var acceptWorkflow = _workflows.Single(work => work.Name == condition.ConditionSatisfiedGoTo);
                            return CalculateAddedValueRecursively(variables, acceptWorkflow);
                        }
                    }
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