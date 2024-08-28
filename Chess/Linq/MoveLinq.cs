using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Linq
{
    internal struct MoveLinq
    {
        public SquareWithPiece From { get; init; }
        public SquareWithPiece To { get; init; }

        public MoveLinq(SquareWithPiece from, SquareWithPiece to)
        {
            From = from;
            To = to;
        }
    }
}
