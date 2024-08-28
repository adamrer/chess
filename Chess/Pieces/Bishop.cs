using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Bishop : IPiece
    {
        public bool IsWhite { get; init; } = true;
        public bool Moved { get; set; } = false;
        public char Symbol { get { if (IsWhite) { return 'B'; } else return 'b'; } }
        public int Value { get; } = 3;

        public IEnumerable<(int, int)> MoveVectors { 
            get
            {
                for (int i = 1; i < 8; i++)
                {
                    yield return (i, i);
                    yield return (-i, -i);
                    yield return (i, - i);
                    yield return (-i, i);
                }
                
            } }

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public Bishop(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
    }
}
