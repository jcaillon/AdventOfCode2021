using System;
using DotUtilities;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode
{
    class Program
    {
        static void Main(string[] args) {
            var subMarine = new SubMarine();
            subMarine.Move(File.ReadAllLines("input").Select(s => new MovementInstruction(s)));
            Console.WriteLine(subMarine.Distance * subMarine.Depth);
            Console.ReadKey();
        }
    }

    class SubMarine {
        public int Depth { get; set; } = 0;
        public int Distance { get; set; } = 0;
        public int Aim { get; set; } = 0;

        public void Move(IEnumerable<MovementInstruction> instructions) {
            foreach (var instruction in instructions) {
                switch (instruction.Order) {
                    case OrderType.Forward:
                        Distance += instruction.Value;
                        Depth += Aim * instruction.Value;
                        break;
                    case OrderType.Up:
                        Aim -= instruction.Value;
                        break;
                    case OrderType.Down:
                        Aim += instruction.Value;
                        break;
                }
            }
        }
    }

    class MovementInstruction {
        public OrderType Order { get; set; }
        public int Value { get; set; }
        public MovementInstruction(string instruction) {
            var splittedInstruction = instruction.Split(' ');
            Order = (OrderType) Enum.Parse(typeof(OrderType), splittedInstruction[0], true);
            Value = int.Parse(splittedInstruction[1]);
        }
    }

    enum OrderType {
        Forward,
        Down,
        Up
    }
}
