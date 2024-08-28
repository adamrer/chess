using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Rook : IPiece
    {
        public bool IsWhite { get; init; } = true;
        public bool Moved { get; set; } = false;
        public char Symbol { get { if (IsWhite) { return 'R'; } else return 'r'; } }
        public int Value { get; } = 5;

        public IEnumerable<(int, int)> MoveVectors
        {
            get
            {
                for (int i = 1; i < 8; i++)
                {
                    yield return (i, 0);
                    yield return (-i, 0);
                    yield return (0, i);
                    yield return (0, -i);
                }
            }
        }

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public Rook(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
    }
}
