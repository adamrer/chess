using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public interface IPiece
    {
        bool IsWhite { get; init; }
        char Symbol { get; }
        bool Moved { get; set; }
        int Value { get; }
        IEnumerable<(int, int)> MoveVectors { get; }
        IEnumerable<(int, int)> AttackVectors { get; }

    }
}
