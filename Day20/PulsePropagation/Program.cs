using System.Diagnostics;
using FluentAssertions;
namespace PulsePropagation;

class Program
{
    static void Main(string[] args)
    {
        var pulse = new Pulse();
        pulse.Solve1("dummydata", 1000).Should().Be(32000000);
        pulse.Solve1("dummydata2", 1000).Should().Be(11687500);
        pulse.Solve1("data", 1000);
        pulse.Solve2("data");
    }
}

class Module
{
    public string Name { get; set; }
    public List<string> SendTo { get; set; }
}

class Flipflop : Module
{
    public bool On { get; set; } = false;
}

class Conjuction : Module
{
    public Dictionary<string, bool> InputHigh = new();
}

class Pulse
{
    private string[] _content;
    private List<Flipflop> _flipflops = new();
    private List<Conjuction> _conjuctions = new();
    private record Signal(string origin, string ModuleName, bool highPulse);
    private Queue<Signal> _signalQueue = new();
    private int _lowSignals = 0;
    private int _highSignals = 0;
    private static readonly string BroadCaster = "broadcaster";
    private static readonly string RX = "rx";
    
    private void Init(string fileName)
    {
        _content = File.ReadAllLines($"Data/{fileName}");
        _flipflops = _content.Where(line => line.StartsWith("%")).Select(entry =>
        {
            var name = entry.Split(" -> ")[0].Trim('%');
            var sendTo = entry.Split(" -> ")[1].Split(',').Select(entry => entry.Trim()).ToList();
            var on = false;
            return new Flipflop
            {
                Name = name,
                SendTo = sendTo,
                On = false,
            };
        }).ToList();
        _conjuctions = _content.Where(line => line.StartsWith("&")).Select(entry =>
        {
            var name = entry.Split(" -> ")[0].Trim('&');
            var sendTo = entry.Split(" -> ")[1].Split(',').Select(entry => entry.Trim()).ToList();
            var on = false;
            return new Conjuction()
            {
                Name = name,
                SendTo = sendTo,
            };
        }).ToList();

        foreach (var conjunction in _conjuctions)
        {
            conjunction.InputHigh = _flipflops
                .Where(ff => ff.SendTo.Contains(conjunction.Name))
                .Select(ff => ff.Name)
                .Concat(_conjuctions
                    .Where(cj => cj.SendTo.Contains(conjunction.Name))
                    .Select(cj => cj.Name)
                ).ToDictionary(entry => entry, entry => false);
        }
        
        _lowSignals = 0;
        _highSignals = 0;
        _signalQueue = new();
    }
    
    public int Solve1(string fileName, int buttonPushes)
    {
        Init(fileName);

        foreach (var buttonPush in Enumerable.Range(0, buttonPushes))
        {
            _signalQueue.Enqueue(new Signal("start", BroadCaster, false));
            _lowSignals++;
            while (_signalQueue.Count > 0)
            {
                var signal = _signalQueue.Dequeue();
                ModuleLogic(signal);
            }
        }
        
        Console.WriteLine(_highSignals * _lowSignals);
        return _highSignals * _lowSignals;
    }
    
    public long Solve2(string fileName)
    {
        Init(fileName);
        var buttonPresses = 0;
        var bnSourcesCycles = FindRxSources("bn");
        var amountOfTimesSeen = bnSourcesCycles.ToDictionary(entry => entry.Key, entry => 0);
        
        while(amountOfTimesSeen.All(sources => sources.Value < 10))
        {
            buttonPresses++;
            _signalQueue.Enqueue(new Signal("start", BroadCaster, false));
            while (_signalQueue.Count > 0)
            {
                var signal = _signalQueue.Dequeue();
                // Logic to find the cyclicity of low signals of the feed.
                // Abort after verifying this works for 10 loops
                if (bnSourcesCycles.Keys.Contains(signal.ModuleName) && !signal.highPulse)
                {
                    amountOfTimesSeen[signal.ModuleName]++;
                    if (bnSourcesCycles[signal.ModuleName] == 0)
                    {
                        bnSourcesCycles[signal.ModuleName] = buttonPresses;
                    }
                    
                    if (buttonPresses == amountOfTimesSeen[signal.ModuleName] * bnSourcesCycles[signal.ModuleName])
                    {
                        throw new Exception("Assumption of cyclicity violated");
                    }
                }
                ModuleLogic(signal);
            }
        }
        
        var answer = LCM.CalculateLCM(bnSourcesCycles.Values.ToArray());
        Console.WriteLine(answer);
        return answer;
    }

    private void ModuleLogic(Signal signal)
    {
        if (signal.ModuleName == BroadCaster)
        {
            var sendToModules = _content.Single(line => line.StartsWith(BroadCaster)).Split("-> ")[1].Split(",")
                .Select(entry => entry.Trim()).ToList();
            SendSignals(signal.ModuleName, sendToModules, false);
        }
        else if (_conjuctions.Any(x => x.Name == signal.ModuleName))
        {
            var conjunction = _conjuctions.Single(x => x.Name == signal.ModuleName);
            conjunction.InputHigh[signal.origin] = signal.highPulse;

            var newPulseHigh = !conjunction.InputHigh.All(kv => kv.Value);
            SendSignals(signal.ModuleName, conjunction.SendTo, newPulseHigh);
        }
        else if (_flipflops.Any(x => x.Name == signal.ModuleName))
        {
            if (signal.highPulse) return;

            var flipflop = _flipflops.Single(x => x.Name == signal.ModuleName);
            flipflop.On = !flipflop.On;
            SendSignals(signal.ModuleName, flipflop.SendTo, flipflop.On);
        }
    }
    
    private void SendSignals(string origin, List<string> sendToModules, bool newPulseHigh)
    {
        foreach (var newModule in sendToModules)
        {
            _ = newPulseHigh ? _highSignals++ : _lowSignals++;
            _signalQueue.Enqueue(new Signal(origin, newModule, newPulseHigh));
        }
    }
    
    private Dictionary<string, int> FindRxSources(string feedRx)
    {
        var bnSourcesCycles = _flipflops
            .Where(ff => ff.SendTo.Contains(feedRx))
            .Select(ff => ff.Name)
            .Concat(_conjuctions
                .Where(cj => cj.SendTo.Contains(feedRx))
                .Select(cj => cj.Name)
            ).ToDictionary(entry => entry, entry => 0);
        return bnSourcesCycles;
    }
}