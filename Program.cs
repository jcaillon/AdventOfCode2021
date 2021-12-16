using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);
        var sb = new StringBuilder();
        foreach (var hexaRepresentation in inputList) {
            var packetDecoder = new PacketDecoder(hexaRepresentation);
            sb.AppendLine($"The expression evaluates to {packetDecoder.GetPacketValue()}");
        }
        return sb.ToString();
    }
}

class PacketDecoder {
    public string BinaryRepresentation { get; private set; }
    public List<Packet> Packets { get; private set; }

    public PacketDecoder(string hexaRepresentation) {
        BinaryRepresentation = string.Concat(hexaRepresentation.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        Packets = (new Parser(BinaryRepresentation)).Packets;
    }

    public int GetSumOfPacketVersions() {
        var sum = 0;
        var q = new Queue<Packet>(Packets);
        while (q.Count > 0) {
            var packet = q.Dequeue();
            if (packet is PacketOperator packetOperator) {
                packetOperator.SubPackets.ForEach(p => q.Enqueue(p));
            }
            sum += packet.Version;
        }
        return sum;
    }
    public long GetPacketValue() {
        return Packets[0].GetValue();
    }

    class Parser {
        private string _data;
        private int _dataLength;
        private int _pos;
        private int _packetStartingPos;
        private Stack<PacketOperator> _lastOperatorPackets = new Stack<PacketOperator>();
        public List<Packet> Packets { get; private set; }

        public Parser(string input) {
            _data = input;
            _dataLength = _data.Length;
            Packets = new List<Packet>();
            ParseData();
        }

        private void ParseData() {
            Packet packet;
            do {
                packet = GetNextPacket();
                var lastOperatorPacket = _lastOperatorPackets.Count > 0 ? _lastOperatorPackets.Peek() : null;
                if (lastOperatorPacket != null && !lastOperatorPacket.IsCompleted()) {
                    lastOperatorPacket.SubPackets.Add(packet);
                    // unstack complete operators
                    while (_lastOperatorPackets.Count > 0 && _lastOperatorPackets.Peek().IsCompleted()) {
                        _lastOperatorPackets.Pop();
                    }
                } else {
                    Packets.Add(packet);
                }
                if (packet is PacketOperator packetOperator)
                    _lastOperatorPackets.Push(packetOperator);
            } while (!(packet is PacketEof));
        }

        private Packet GetNextPacket() {
            if (_dataLength - _pos <= 6)
                return new PacketEof();

            _packetStartingPos = _pos;
            var version = Convert.ToByte(getNextChars(3), 2);
            var type = Convert.ToByte(getNextChars(3), 2);
            switch (type) {
                case 4:
                    return NewPacketLiteralValue(version);
                default:
                    return NewPacketOperator(version, type);
            }
        }

        private Packet NewPacketOperator(byte version, byte type) {
            var isNbSubPackets = getNextChars(1)!.Equals("1");
            var subPacketsLength = Convert.ToInt32(getNextChars(isNbSubPackets ? 11 : 15), 2);
            return new PacketOperator(version, _pos - _packetStartingPos, isNbSubPackets, subPacketsLength, type);
        }

        private PacketLiteralValue NewPacketLiteralValue(byte version) {
            var valueInBinary = new StringBuilder();
            var originalPos = _pos;
            while (true) {
                var bitPrefix = getNextChars(1);
                valueInBinary.Append(getNextChars(4));
                if (bitPrefix![0] == '0') {
                    break;
                }
            }
            //var nbPaddingZeroes = 4 - ((_pos - originalPos) % 4);
            //Debug.Assert(nbPaddingZeroes == 4 || Convert.ToInt32(getNextChars(nbPaddingZeroes)) == 0);
            return new PacketLiteralValue(version, _pos - _packetStartingPos, Convert.ToInt64(valueInBinary.ToString(), 2));
        }

        private string? getNextChars(int length) {
            var correctedLength = Math.Min(length, _dataLength - _pos);
            var s = correctedLength > 0 ? _data.Substring(_pos, correctedLength) : null;
            _pos += correctedLength;
            return s;
        }
    }

    public class Packet {
        public byte Version { get; private set; }
        public int Length { get; private set; }
        public Packet(byte version, int length) {
            Version = version;
            Length = length;
        }
        public long GetValue() => this is PacketOperator spo ? spo.GetResult() : this is PacketLiteralValue spv ? spv.Value : 0;
    }
    public class PacketEof : Packet {
        public PacketEof() : base(0, 0) {}
    }
    public class PacketLiteralValue : Packet {
        public long Value { get; private set; }
        public PacketLiteralValue(byte version, int length, long value) : base(version, length) => Value = value;
    }
    public class PacketOperator : Packet {
        public bool IsNumberOfPackets { get; private set; }
        public int Value { get; private set; }
        public int SubPacketsLength { get; private set; }
        public List<Packet> SubPackets { get; private set; }
        private OperationTypeEnum OperationType { get; set; }
        public PacketOperator(byte version, int length, bool isNbSubPackets, int subPacketsLength, byte operationType) : base(version, length) {
            IsNumberOfPackets = isNbSubPackets;
            SubPacketsLength = subPacketsLength;
            SubPackets = new List<Packet>();
            OperationType = (OperationTypeEnum) operationType;
        }
        public bool IsCompleted() => IsNumberOfPackets ? SubPackets.Count == SubPacketsLength : GetSubPacketsTotalLength() == SubPacketsLength;
        public int GetSubPacketsTotalLength() {
            var total = 0;
            var q = new Queue<Packet>(SubPackets);
            while (q.Count > 0) {
                var packet = q.Dequeue();
                if (packet is PacketOperator packetOperator) {
                    packetOperator.SubPackets.ForEach(p => q.Enqueue(p));
                }
                total += packet.Length;
            }
            return total;
        }
        public long GetResult() {
            switch (OperationType) {
                case OperationTypeEnum.Sum:
                    return SubPackets.Select(sp => sp.GetValue()).Sum();
                case OperationTypeEnum.Product:
                    long total = 1;
                    SubPackets.Select(sp => sp.GetValue()).ToList().ForEach(v => total *= v);
                    return total;
                case OperationTypeEnum.Min:
                    return SubPackets.Select(sp => sp.GetValue()).Min();
                case OperationTypeEnum.Max:
                    return SubPackets.Select(sp => sp.GetValue()).Max();
                case OperationTypeEnum.GreaterThan:
                    return SubPackets[0].GetValue() > SubPackets[1].GetValue() ? 1 : 0;
                case OperationTypeEnum.LowerThan:
                    return SubPackets[0].GetValue() < SubPackets[1].GetValue() ? 1 : 0;
                case OperationTypeEnum.EqualsTo:
                    return SubPackets[0].GetValue() == SubPackets[1].GetValue() ? 1 : 0;
            }
            throw new InvalidDataException();
        }
        public enum OperationTypeEnum : byte {
            Sum = 0,
            Product = 1,
            Min = 2,
            Max = 3,
            GreaterThan = 5,
            LowerThan = 6,
            EqualsTo = 7
        }
    }
}
