namespace CommonHelpers;
public class StatsAccumulator {
	public void Add(double value) {
		Sum += value;
		Count += 1;
		if (value < Min) {
			Min = value;
		}
		if (value > Max) {
			Max = value;
		}
	}
	public long Count { get; private set; }
	public double Sum { get; private set; }
	public double Min { get; private set; } = double.PositiveInfinity;
	public double Max { get; private set; } = double.NegativeInfinity;
	public double Avg => Sum / Count;
	public void Reset() {
		Count = 0;
		Sum = 0;
		Min = double.PositiveInfinity;
		Max = double.NegativeInfinity;
	}
}
