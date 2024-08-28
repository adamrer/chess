using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Queen : IPiece
    {
        public bool IsWhite { get; init; } = true;
        public bool Moved { get; set; } = false;
        public char Symbol { get { if (IsWhite) { return 'Q'; } else return 'q'; } }
        public int Value { get; } = 9;

        public IEnumerable<(int, int)> MoveVectors
        {
            get
            {
                for (int i = 1; i < 8; i++)
                {
                    // rook moves
                    yield return (i, 0);
                    yield return (-i, 0);
                    yield return (0, i);
                    yield return (0, -i);

                    // bishop moves
                    yield return (i, i);
                    yield return (-i, -i);
                    yield return (i, -i);
                    yield return (-i, i);
                }

            }
        }

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public Queen(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
    }
}
