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

class Flipflop
{
    public string Name { get; set; }
    public bool On { get; set; }
    public List<string> SendTo { get; set; }
}

class Conjuction
{
    public string Name { get; set; }
    public bool High { get; set; }
    public List<string> SendTo { get; set; }
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
                On = false,
                SendTo = sendTo
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
                High = false,
                SendTo = sendTo
            };
        }).ToList();

        var broadcasterSendTo = content.Single(line => line.StartsWith("broadcaster")).Split("-> ")[1].Split(",").Select(entry => entry.Trim()).ToList(); 
        var initialPulseHigh = false;
        SendPulses(broadcasterSendTo, initialPulseHigh);
        return 1;
    }

    private void SendPulses(List<string> sendTos, bool high)
    {
        foreach (var sendTo in sendTos)
        {
            if (_conjuctions.Any(x => x.Name == sendTo))
            {
                var conjuction = _conjuctions.Single(x => x.Name == sendTo);
                conjuction.High = !high;
                SendPulses(conjuction.SendTo, conjuction.High);
            }
            else
            {
                if (high) continue;
                
                var flipflop = _flipflops.Single(x => x.Name == sendTo);
                flipflop.On = !flipflop.On;
                SendPulses(flipflop.SendTo, flipflop.On);
            }
        }
    }
}