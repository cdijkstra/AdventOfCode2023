using FluentAssertions;

namespace CamelCards;

// The answer is not 247688577 and not 247654005 and not 247627786

class Program
{
    static void Main(string[] args)
    {
        var camelCard = new CamelCards();
        // Lots of tests that succeed.. what is the issue?
        Game2.FiveOfAKind(new char[] { 'J', 'J', 'A', 'A', 'A' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { '2', '2', '2', '2', '2' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { 'J', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { '8', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { '8', '8', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { 'K', 'T', 'K', 'K', 'J' }).Should().BeFalse();
        Game2.FourOfAKind(new char[] { 'K', 'T', 'K', 'J', 'J' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { '8', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '8', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.OnePair(new char[] { '8', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '1', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '1', '1', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '1', '1', '1', 'J' }).Should().BeFalse();
        Game2.FullHouse(new char[] { '1', '2', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '2', '2', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '2', '2', '2', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '1', '2', '2', '1', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '8', 'J', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'K', 'T', 'K', 'K', 'T' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'K', 'T', 'K', 'K', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'T', 'K', 'K', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'T', 'K', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'J', 'K', 'J', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'T', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.TwoPair(new char[] { 'K', 'T', 'K', 'K', 'J' }).Should().BeTrue();
        Game2.OnePair(new char[] { 'K', 'T', 'K', 'K', 'J' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { 'K', 'T', 'K', 'K', 'J' }).Should().BeFalse();
        Game2.FiveOfAKind(new char[] { '2', '2', 'J', '2', '2' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { '2', 'J', 'J', '2', '2' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { 'J', '2', 'J', 'J', 'J' }).Should().BeTrue();
        Game2.FiveOfAKind(new char[] { 'J', '2', '2', 'J', '3' }).Should().BeFalse();
        Game2.FourOfAKind(new char[] { '2', '2', '2', '2', '3' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { '2', '2', '2', '3', 'J' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { 'J', '2', '2', 'J', '3' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { '2', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { 'J', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.FourOfAKind(new char[] { '2', 'J', 'J', '3', '4' }).Should().BeFalse();
        Game2.FullHouse(new char[] { '8', '5', '5', '5', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '2', '2', '2', '3', '3' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '2', '2', '2', '3', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', '2', '2', '3', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '2', '2', '2', '3', 'J' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', '2', '2', '3', '3' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '2', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.FullHouse(new char[] { 'J', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.FullHouse(new char[] { '2', 'J', 'J', '3', '4' }).Should().BeFalse();     
        Game2.ThreeOfAKind(new char[] { 'J', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { '2', '2', '2', '3', '3' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { '2', '2', '2', '3', 'J' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { 'J', '2', '2', '3', '3' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { 'J', '1', '2', '3', '3' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { 'J', '1', 'J', '3', '3' }).Should().BeTrue();
        Game2.ThreeOfAKind(new char[] { '2', 'J', '5', '3', '4' }).Should().BeFalse();
        Game2.TwoPair(new char[] { '2', 'J', 'J', '3', '3' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '2', 'J', '2', '3', '3' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '2', 'J', 'J', '3', '2' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '2', 'J', '4', '3', '2' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '2', '3', '4', '3', '2' }).Should().BeTrue();
        Game2.TwoPair(new char[] { '2', 'J', '1', '3', '4' }).Should().BeFalse();
        Game2.OnePair(new char[] { '2', '1', '2', '3', '4' }).Should().BeTrue();
        Game2.OnePair(new char[] { '2', 'J', '3', '3', '4' }).Should().BeTrue();
        Game2.OnePair(new char[] { '2', 'J', 'J', '1', '3' }).Should().BeTrue();
        Game2.OnePair(new char[] { 'J', 'J', 'J', '1', '2' }).Should().BeTrue();
        Game2.OnePair(new char[] { '2', 'J', '1', '3', '4' }).Should().BeTrue();
        Game2.OnePair(new char[] { '1', '2', '3', '5', '4' }).Should().BeFalse();
        camelCard.Solve1("dummydata").Should().Be(6440);
        Console.WriteLine(camelCard.Solve1("data"));
        
        camelCard.Solve2("dummydata").Should().Be(5905);
        Console.WriteLine(camelCard.Solve2("data"));
    }
}

class CamelCards
{
    public int Solve1(string fileName)
    {
        string[] lines = File.ReadAllLines($"data/{fileName}");
        var sortedGames = lines.Select(line =>
        {
            var parts = line.Split();
            return new Game { Hand = parts[0].ToCharArray(), Bid = int.Parse(parts[1]) };
        }).OrderBy(game => game).ToList();
        int totalScore = sortedGames.Select((game, index) => game.Bid * (index + 1)).Sum();
        
        return totalScore;
    }
    
    public int Solve2(string fileName)
    {
        string[] lines = File.ReadAllLines($"data/{fileName}");
        var sortedGames = lines.Select(line =>
        {
            var parts = line.Split();
            return new Game2 { Hand = parts[0].ToCharArray(), Bid = int.Parse(parts[1]) };
        }).OrderBy(game => game).ToList();
        sortedGames.ForEach(x => Console.WriteLine(x.Hand));
        
        
        int totalScore = sortedGames.Select((game, index) => game.Bid * (index + 1)).Sum();
        
        return totalScore;
    }
}

class Game2 : IComparable<Game2>
{ 
    private Dictionary<char, int> _pokerValues = new Dictionary<char, int>
    {
        {'A', 14}, {'K', 13}, {'Q', 12}, {'T', 10}, {'9', 9}, {'8', 8}, {'7', 7}, {'6', 6}, {'5', 5}, {'4', 4}, {'3', 3}, {'2', 2}, {'J', 1}
    };
    
    private List<Func<char[], bool>> rules = new()
    {
        FiveOfAKind, FourOfAKind, FullHouse, ThreeOfAKind, TwoPair, OnePair // HighCard logic handles separately
    };
    
    public char[] Hand { get; set; }
    public int Bid { get; set; }
    public int CompareTo(Game2? other)
    {
        // Positive if myHand wins, negative if other hand wins
        foreach (var rule in rules)
        {
            bool ruleSatisfiedMyhand = rule(Hand);
            bool ruleSatisfiedOtherHand = rule(other.Hand);
            if (ruleSatisfiedMyhand && ruleSatisfiedOtherHand)
            {
                return FindWinningHandBasedOnHighestCard(Hand, other.Hand);
            }
            if (ruleSatisfiedMyhand != ruleSatisfiedOtherHand)
            {
                return ruleSatisfiedMyhand ? 1 : -1;
            }
        }

        // If none of the previous rules determine a winner, compare high cards
        return FindWinningHandBasedOnHighestCard(Hand, other.Hand);
    }
    
    public static bool FiveOfAKind(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        return jCount > 3 || hand.Where(c => c != 'J').GroupBy(c => c)
            .Any(group => group.Count() + jCount == 5);
    }
    
    public static bool FourOfAKind(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        // If j > 2 we always have four of a kind by adding a random card
        var satisfied = jCount > 2 || hand.Where(c => c != 'J').GroupBy(c => c)
            .Any(group => group.Count() + jCount == 4);
        return satisfied;
    }
    
    public static bool FullHouse(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        var handWithoutJ = hand.Where(c => c != 'J').ToList();
        return jCount switch
        {
            // This can probably be refactored
            > 2 => true,
            // For 2 we have two options xJJyy and JJyyy
            2 => (handWithoutJ.GroupBy(c => c).Any(cards => cards.Count() == 1) &&
                  handWithoutJ.GroupBy(c => c).Any(cards => cards.Count() == 2)) ||
                 handWithoutJ.GroupBy(c => c).Any(cards => cards.Count() == 3),
            // For 1 we have two options xJyyy and xxJyy
            1 => (handWithoutJ.GroupBy(c => c).Any(cards => cards.Count() == 3) &&
                  handWithoutJ.GroupBy(c => c).Any(cards => cards.Count() == 1)) ||
                 handWithoutJ.GroupBy(c => c).Count(cards => cards.Count() == 2) == 2,
            // Like it was in exercise 1
            _ => handWithoutJ.GroupBy(c => c).Count(cards => cards.Count() == 3) == 1 &&
                 handWithoutJ.GroupBy(c => c).Count(cards => cards.Count() == 2) == 1
        };
    }
    
    public static bool ThreeOfAKind(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        return jCount > 1 || hand.Where(c => c != 'J').GroupBy(c=> c).Any(group => group.Count() + jCount >= 3);
    }

    public static bool TwoPair(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        if (jCount > 1) return true;
        
        return hand.GroupBy(c => c).Count(cards => cards.Count() >= 2) >= 2 - jCount;
    }
    
    public static bool OnePair(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        return jCount > 0 || hand.Where(c => c != 'J').GroupBy(c => c).Count(cards => cards.Count() >= 2) == 1;
    }

    public int FindWinningHandBasedOnHighestCard(char[] hand, char[] otherHand)
    {
        var idx = 0;
        var continueSearching = true;
        
        while (continueSearching && idx < 5)
        {
            if (_pokerValues[hand[idx]] == _pokerValues[otherHand[idx]])
            {
                idx++;
            }
            else
            {
                continueSearching = false;
            }
        }

        return idx < 5 ?_pokerValues[hand[idx]] - _pokerValues[otherHand[idx]] : 0;
    }
}

class Game : IComparable<Game>
{ 
    private Dictionary<char, int> _pokerValues = new Dictionary<char, int>
    {
        {'A', 14}, {'K', 13}, {'Q', 12}, {'J', 11}, {'T', 10}, {'9', 9}, {'8', 8}, {'7', 7}, {'6', 6}, {'5', 5}, {'4', 4}, {'3', 3}, {'2', 2}
    };
    
    private List<Func<char[], bool>> rules = new()
    {
        FiveOfAKind, FourOfAKind, FullHouse, ThreeOfAKind, TwoPair, OnePair
        // HighCard logic handles separately
    };
    
    public char[] Hand { get; set; }
    public int Bid { get; set; }
    public int CompareTo(Game? other)
    {
        // Positive if myHand wins, negative if other hand wins
        foreach (var rule in rules)
        {
            bool ruleSatisfiedMyhand = rule(Hand);
            bool ruleSatisfiedOtherHand = rule(other.Hand);
            if (ruleSatisfiedMyhand && ruleSatisfiedOtherHand)
            {
                return FindWinningHandBasedOnHighestCard(Hand, other.Hand);
            }
            if (ruleSatisfiedMyhand != ruleSatisfiedOtherHand)
            {
                return ruleSatisfiedMyhand ? 1 : -1;
            }
        }

        // If none of the previous rules determine a winner, compare high cards
        return FindWinningHandBasedOnHighestCard(Hand, other.Hand);
    }
    
    private static bool FiveOfAKind(char[] hand)
    {
        return hand.All(c => c == hand.First());
    }
    
    private static bool FourOfAKind(char[] hand)
    {
        return hand.GroupBy(c=> c).Any(c => c.Count() == 4);
    }
    
    private static bool FullHouse(char[] hand)
    {
        return hand.GroupBy(c => c)
                   .Count(cards => cards.Count() == 3) == 1 &&
               hand.GroupBy(c => c)
                   .Count(cards => cards.Count() == 2) == 1;
    }
    
    private static bool ThreeOfAKind(char[] hand)
    {
        return hand.GroupBy(c=> c).Any(c => c.Count() == 3);
    }

    private static bool TwoPair(char[] hand)
    {
        return hand.GroupBy(c => c)
            .Count(cards => cards.Count() == 2) == 2;
    }
    
    private static bool OnePair(char[] hand)
    {
        return hand.GroupBy(c => c)
            .Count(cards => cards.Count() == 2) == 1;
    }
    
    private int FindWinningHandBasedOnHighestCard(char[] hand, char[] otherHand)
    {
        var idx = 0;
        var continueSearching = true;
        
        while (continueSearching && idx < 5)
        {
            if (_pokerValues[hand[idx]] == _pokerValues[otherHand[idx]])
            {
                idx++;
            }
            else
            {
                continueSearching = false;
            }
        }

        return idx < 5 ?_pokerValues[hand[idx]] - _pokerValues[otherHand[idx]] : 0;
    }
}