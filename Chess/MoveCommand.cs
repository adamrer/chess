using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal readonly struct MoveCommand(char pieceSymbol, Square destination, char? pieceIdentification)
    {
        public char PieceSymbol { get; init; } = pieceSymbol;
        public Square Destination { get; init; } = destination;
        public char? PieceIdentification { get; init; } = pieceIdentification;

    }
}
