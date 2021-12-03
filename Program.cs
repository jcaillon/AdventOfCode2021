var input = File.ReadAllLines("input");
var numberInput = input.Select(s => Convert.ToUInt32(s, 2)).ToArray();
var bitNumber = input[0].Length;

var maxValue = (uint) Convert.ToUInt32(new string('1', bitNumber), 2);
var gammaRate = Compute.ComputeRate(numberInput, bitNumber);
var epsilonRate = gammaRate ^ maxValue;
Console.WriteLine($"power consumption = {gammaRate * epsilonRate}");

var oxygenRating = Compute.ComputeRateByFiltering(numberInput, bitNumber, false);
var scrubberRating = Compute.ComputeRateByFiltering(numberInput, bitNumber, true);

Console.WriteLine(Convert.ToString(oxygenRating, toBase: 2));
Console.WriteLine(Convert.ToString(scrubberRating, toBase: 2));

Console.WriteLine($"life support rating = {oxygenRating * scrubberRating}");

static class Compute {

    public static uint ComputeRateByFiltering(uint[] input, int bitNumber, bool invertBitCriteria) {
        var dataMatchingCriteria = input.ToArray();
        for (int i = bitNumber - 1; i >= 0; i--) {
            var mask = (uint) 0b_1 << i;
            var mostCommonBitForMask = MostCommonBitForMask(dataMatchingCriteria, mask);
            var bitCriteria = mostCommonBitForMask ^ invertBitCriteria;
            dataMatchingCriteria = dataMatchingCriteria.Where(n => bitCriteria ? (n & mask) > 0 : (n & mask) == 0).ToArray();
            if (dataMatchingCriteria.Length <= 1) {
                if (dataMatchingCriteria.Length == 0) {
                    break;
                }
                return dataMatchingCriteria.First();
            }
        }
        throw new Exception("Did not match a unique number.");
    }

    public static uint ComputeRate(uint[] input, int bitNumber) {
        uint finalNumber = 0;
        for (int i = 0; i < bitNumber; i++) {
            var mask = (uint) 0b_1 << i;
            finalNumber += MostCommonBitForMask(input, mask) ? mask : 0;
        }
        return (uint) finalNumber;
    }

    public static bool MostCommonBitForMask(uint[] input, uint mask) {
        var bitSetOccurrence = input.Select(n => n & mask).Count(n => n > 0);
        var bitUnsetOccurence = input.Length - bitSetOccurrence;
        return bitSetOccurrence >= bitUnsetOccurence;
    }

}
