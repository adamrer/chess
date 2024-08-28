using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Pieces
{
    internal class NoPiece : IPiece
    {
        public bool IsWhite { get; init; } = false;

        public char Symbol => ' ';

        public bool Moved { get; set; } = false;
        public int Value { get; } = 0;


        public IEnumerable<(int, int)> MoveVectors { get; set; } = new List<(int, int)>();

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public NoPiece(bool isWhite = false)
        {
            IsWhite = isWhite;
        }
    }
}
