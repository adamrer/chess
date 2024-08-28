using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal interface IPiece
    {
        bool IsWhite { get; init; }
        char Symbol { get; }
        bool Moved { get; set; }
        int Value { get; }
        // TODO: Unicode chess symbols?
        //bool ShowUnicodeSymbol { get; init; }
        IEnumerable<(int, int)> MoveVectors { get; }
        IEnumerable<(int, int)> AttackVectors { get; }

    }
}
