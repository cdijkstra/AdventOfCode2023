using FluentAssertions;
namespace CamelCards;

class Program
{
    static void Main(string[] args)
    {
        var camelCard = new CamelCards();
        camelCard.Solve<Game>("dummydata").Should().Be(6440);
        Console.WriteLine(camelCard.Solve<Game>("data"));
        camelCard.Solve<Game2>("dummydata").Should().Be(5905);
        Console.WriteLine(camelCard.Solve<Game2>("data"));
    }
}

class CamelCards
{
    public int Solve<TGame>(string fileName) where TGame : IGame, new()
    {
        string[] lines = File.ReadAllLines($"data/{fileName}");
        var sortedGames = lines.Select(line =>
        {
            var parts = line.Split();
            return new TGame { Hand = parts[0].ToCharArray(), Bid = int.Parse(parts[1]) };
        }).OrderBy(game => game).ToList();
        int totalScore = sortedGames.Select((game, index) => game.Bid * (index + 1)).Sum();
        
        return totalScore;
    }
}

public interface IGame : IComparable<IGame>
{
    public char[] Hand { get; set; }
    public int Bid { get; set; }
}

class Game2 : IGame
{ 
    private Dictionary<char, int> _pokerValues = new Dictionary<char, int>
    {
        {'A', 14}, {'K', 13}, {'Q', 12}, {'T', 10}, {'9', 9}, {'8', 8}, {'7', 7}, {'6', 6}, {'5', 5}, {'4', 4}, {'3', 3}, {'2', 2}, {'J', 1}
    };
    private List<Func<char[], bool>> rules = new()
    {
        FiveOfAKind, FourOfAKind, FullHouse, ThreeOfAKind, TwoPair, OnePair // HighCard logic handled separately
    };
    public char[] Hand { get; set; }
    public int Bid { get; set; }
    
    public int CompareTo(IGame? other)
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
        var jCount = hand.Count(c => c == 'J');
        return jCount > 3 || hand.Where(c => c != 'J').GroupBy(c => c)
            .Any(group => group.Count() + jCount == 5);
    }
    
    private static bool FourOfAKind(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        // If j > 2 we always have four of a kind by adding a random card
        var satisfied = hand.Where(c => c != 'J').GroupBy(c => c).Any(group => group.Count() + jCount >= 4);
        return satisfied;
    }
    
    private static bool FullHouse(char[] hand)
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
    
    private static bool ThreeOfAKind(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        return jCount > 1 || hand.Where(c => c != 'J').GroupBy(c=> c).Any(group => group.Count() + jCount >= 3);
    }

    private static bool TwoPair(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        if (jCount > 1) return true;
        
        return hand.GroupBy(c => c).Count(cards => cards.Count() >= 2) >= 2 - jCount;
    }
    
    private static bool OnePair(char[] hand)
    {
        var jCount = hand.Count(c => c == 'J');
        return jCount > 0 || hand.Where(c => c != 'J').GroupBy(c => c).Count(cards => cards.Count() >= 2) == 1;
    }

    private int FindWinningHandBasedOnHighestCard(char[] hand, char[] otherHand)
    {
        var idx = 0;
        while (idx < 5 && _pokerValues[hand[idx]] == _pokerValues[otherHand[idx]])
        {
            idx++;
        }
        return idx < 5 ? _pokerValues[hand[idx]] - _pokerValues[otherHand[idx]] : 0;
    }
}

class Game : IGame
{ 
    private Dictionary<char, int> _pokerValues = new Dictionary<char, int>
    {
        {'A', 14}, {'K', 13}, {'Q', 12}, {'J', 11}, {'T', 10}, {'9', 9}, {'8', 8}, {'7', 7}, {'6', 6}, {'5', 5}, {'4', 4}, {'3', 3}, {'2', 2}
    };
    
    private List<Func<char[], bool>> rules = new()
    {
        FiveOfAKind, FourOfAKind, FullHouse, ThreeOfAKind, TwoPair, OnePair
        // HighCard logic handled separately
    };
    
    public char[] Hand { get; set; }
    public int Bid { get; set; }
    public int CompareTo(IGame? other)
    {
        // Positive if myHand wins, negative if other hand wins
        rules.ForEach(rule => {});
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
        return hand.GroupBy(c => c).Count(cards => cards.Count() == 3) == 1 &&
               hand.GroupBy(c => c).Count(cards => cards.Count() == 2) == 1;
    }
    
    private static bool ThreeOfAKind(char[] hand)
    {
        return hand.GroupBy(c=> c).Any(c => c.Count() == 3);
    }

    private static bool TwoPair(char[] hand)
    {
        return hand.GroupBy(c => c).Count(cards => cards.Count() == 2) == 2;
    }
    
    private static bool OnePair(char[] hand)
    {
        return hand.GroupBy(c => c).Count(cards => cards.Count() == 2) == 1;
    }
    
    private int FindWinningHandBasedOnHighestCard(char[] hand, char[] otherHand)
    {
        var idx = 0;
        while (idx < 5 && _pokerValues[hand[idx]] == _pokerValues[otherHand[idx]])
        {
            idx++;
        }
        return idx < 5 ? _pokerValues[hand[idx]] - _pokerValues[otherHand[idx]] : 0;
    }
}