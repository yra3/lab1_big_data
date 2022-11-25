using System;
using System.Collections.Generic;
namespace TestHelpers;
public class WeightedRandomIndexGenerator {
	// original: https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp/blob/v1.0.2/Weighted%20Randomizer/StaticWeightedRandomizer.cs
	readonly List<ProbabilityBox> probabilityBoxes = new();
	readonly long heightPerBox;
	public WeightedRandomIndexGenerator(IReadOnlyList<long> weights) {
		checked {
			long totalWeight = 0;
			foreach (var weight in weights) {
				if (weight < 0) {
					throw new ArgumentOutOfRangeException(nameof(weights), "Weights can't be negative");
				}
				totalWeight += weight;
			}
			if (totalWeight <= 0) {
				throw new ArgumentOutOfRangeException(nameof(weights), "Total weight must be positive");
			}
			var count = weights.Count;
			var gcd = GreatestCommonDivisor(count, totalWeight);
			var weightMultiplier = count / gcd;
			heightPerBox = totalWeight / gcd;
			var smallStack = new Stack<KeyBallsPair>();
			var largeStack = new Stack<KeyBallsPair>();
			for (var i = 0; i < count; i++) {
				var weight = weights[i];
				var newWeight = weight * weightMultiplier;
				var targetStack = newWeight > heightPerBox ? largeStack : smallStack;
				targetStack.Push(new KeyBallsPair(i, newWeight));
			}
			while (largeStack.Count != 0) {
				var largeItem = largeStack.Pop();
				var smallItem = smallStack.Pop();
				probabilityBoxes.Add(new ProbabilityBox(smallItem.Key, largeItem.Key, smallItem.NumBalls));
				largeItem = largeItem with { NumBalls = largeItem.NumBalls - (heightPerBox - smallItem.NumBalls) };
				var targetStack = largeItem.NumBalls > heightPerBox ? largeStack : smallStack;
				targetStack.Push(largeItem);
			}
			while (smallStack.Count != 0) {
				var smallItem = smallStack.Pop();
				probabilityBoxes.Add(new ProbabilityBox(smallItem.Key, smallItem.Key, heightPerBox));
			}
		}
	}
	readonly record struct KeyBallsPair(int Key, long NumBalls);
	readonly record struct ProbabilityBox(int Key, int AltKey, long NumBallsInBox);
	static long GreatestCommonDivisor(long a, long b) {
		checked {
			while (b != 0) {
				var remainder = a % b;
				a = b;
				b = remainder;
			}
			return a;
		}
	}
	public int NextIndex(Random random) {
		var randomIndex = random.Next(probabilityBoxes.Count);
		var randomNumBalls = random.NextInt64(heightPerBox);
		if (randomNumBalls < probabilityBoxes[randomIndex].NumBallsInBox) {
			return probabilityBoxes[randomIndex].Key;
		}
		else {
			return probabilityBoxes[randomIndex].AltKey;
		}
	}
}
