using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Pawn : IPiece
    {
        public bool Moved { get; set; } = false;
        public bool IsWhite { get; init; } = true;
        public char Symbol { get { if (IsWhite) { return 'P'; } else return 'p'; } }
        public int Value { get; } = 1;

        public IEnumerable<(int, int)> MoveVectors
        {
            get
            {
                if (!IsWhite)
                {

                    if (Moved) yield return (-1, 0);
                    else
                    {
                        yield return (-1, 0);
                        yield return (-2, 0);
                    }
                }
                else
                {
                    if (Moved) yield return (1, 0);
                    else
                    {
                        yield return (1, 0);
                        yield return (2, 0);
                    }
                }
            }
        }

        public IEnumerable<(int, int)> AttackVectors {
            get
            {
                if (!IsWhite)
                {
                    yield return (-1, -1);
                    yield return (-1, 1);
                }
                else
                {
                    yield return (1, -1);
                    yield return (1, 1);
                }
            }
        }

        public Pawn(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
        public Pawn()
        {
            IsWhite = true;
        }
    }
}
