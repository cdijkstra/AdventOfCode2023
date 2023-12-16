using FluentAssertions;

namespace LensLibrary;

class Program
{
    static void Main(string[] args)
    {
        var lens = new Lens();
        lens.Solve1("dummydata").Should().Be(1320);
        Console.WriteLine(lens.Solve1("data"));
        lens.Solve2("dummydata").Should().Be(145);
        Console.WriteLine(lens.Solve2("data"));
    }
}

class Lens
{
    public int Solve1(string fileName)
    {
        var contents = File.ReadAllText($"Data/{fileName}").Split(",").ToList();
        var totalHashValue = 0;
        foreach (var content in contents)
        {
            totalHashValue += ComputeHash(content);
        }
        
        return totalHashValue;
    }
    
    public int Solve2(string fileName)
    {
        var contents = File.ReadAllText($"Data/{fileName}").Split(",").ToList();
        Dictionary<int, List<(string boxId, int focusLength)>> hashes = 
            Enumerable.Range(0, 256).ToDictionary(key => key, value => new List<(string, int)>());
        foreach (var content in contents)
        {
            if (content.Contains("="))
            {
                var boxId = content.Split("=")[0];
                var boxNumber = ComputeHash(boxId);
                var value = hashes.ElementAt(boxNumber).Value;
                var focusLength = int.Parse(content.Split("=")[1]);
                if (hashes[boxNumber].Count(entry => entry.boxId == boxId) > 0)
                {
                    hashes[boxNumber] = hashes[boxNumber].Select(tuple => tuple.boxId == boxId ? (boxId, focusLength) : tuple).ToList();
                }
                else
                {
                    value.Add((boxId, focusLength));
                }
            }
            else if (content.Contains("-"))
            {
                var boxId = content.Split("-")[0];
                var boxNumber = ComputeHash(boxId);
                var newValues = hashes[boxNumber].Where(entry => entry.boxId != boxId).ToList();
                hashes[boxNumber] = newValues;
            }
        }

        var cumulativeFocusPower = 0;
        for (var hashIdx = 0; hashIdx != hashes.Count; hashIdx++)
        {
            var value = hashes.ElementAt(hashIdx).Value;
            if (value.Count == 0) continue;
            for (var slotNumber = 1; slotNumber <= value.Count; slotNumber++)
            {
                var focusLength = value[slotNumber - 1].focusLength;
                var focusPower = (1 + hashIdx) * slotNumber * focusLength;
                cumulativeFocusPower += focusPower;
            }
        }
        
        return cumulativeFocusPower;
    }

    private int ComputeHash(string content)
    {
        var hashValue = 0;
        foreach (var ch in content)
        {
            var ascii = (int)ch;
            hashValue = (hashValue + ascii) * 17 % 256;
        }

        return hashValue;
    }
}