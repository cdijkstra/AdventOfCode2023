using FluentAssertions;

namespace PulsePropagation;

class Program
{
    static void Main(string[] args)
    {
        var pulse = new Pulse();
        pulse.Solve1("dummydata").Should().Be(4);
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
    private List<Flipflop> _flipflops = new();
    private List<Conjuction> _conjuctions = new();
    public int Solve1(string fileName)
    {
        var content = File.ReadAllLines($"Data/{fileName}");
        _flipflops = content.Where(line => line.StartsWith("%")).Select(entry =>
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
        _conjuctions = content.Where(line => line.StartsWith("&")).Select(entry =>
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

        var broadcaster = "broadcaster";
        
        var broadcasterSendTo = content.Single(line => line.StartsWith(broadcaster)).Split("-> ")[1].Split(",").Select(entry => entry.Trim()).ToList();
        Queue<(string, bool)> sendSignal = new();
        foreach (var entry in broadcasterSendTo)
        {
            sendSignal.Enqueue((entry, false));
        }
        
        
    }

    private void SendPulses(string name, List<string> sendTos, bool high)
    {
        foreach (var sendTo in sendTos)
        {
            if (_conjuctions.Any(x => x.Name == sendTo))
            {
                // Conjuction
                var conjuction = _conjuctions.Single(x => x.Name == sendTo);
                SendPulses(sendTo, conjuction.SendTo, high);
            }
            else
            {
                // Flipflop
                var flipflop = _flipflops.Single(x => x.Name == sendTo);
                flipflop.On = !flipflop.On;
                SendPulses(sendTo, flipflop.SendTo, high);
            }
        }
    }
}