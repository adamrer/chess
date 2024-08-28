using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class MoveValue(Move? move, int value)
    {
        public Move? Move { get; set; } = move;
        public int Value { get; set; } = value;
    }
}
