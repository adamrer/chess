using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public struct Move
    {
        public Square From { get; init; }
        public Square To { get; init; }

        public Move(Square from, Square to)
        {
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"{From} {To}";
        }
    }
}
