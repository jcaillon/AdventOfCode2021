using System.Numerics;

var input = File.ReadAllLines("input");
var bitNumber = input[0].Length;
var maxValue = (uint) Convert.ToUInt32(new string('1', bitNumber), 2);
var gammaRate = Compute.ComputeRate(input.Select(s => Convert.ToUInt32(s, 2)).ToArray(), bitNumber);
var epsilonRate = gammaRate ^ maxValue;
Console.WriteLine(gammaRate * epsilonRate);

static class Compute {

    public static uint ComputeRate(uint[] input, int bitNumber) {
        uint finalNumber = 0;
        for (int i = 0; i < bitNumber; i++) {
            var mask = (uint) BigInteger.Pow(2, i);
            finalNumber += MostCommonBitForMask(input, mask) ? mask : 0;
        }
        return (uint) finalNumber;
    }
    public static bool MostCommonBitForMask(uint[] input, uint mask) {
        var bitSetOccurrence = input.Select(n => n & mask).Count(n => n > 0);
        var bitUnsetOccurence = input.Length - bitSetOccurrence;
        return bitSetOccurrence > bitUnsetOccurence;
    }

}
