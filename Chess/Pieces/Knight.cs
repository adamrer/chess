using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess 
{
    internal class Knight : IPiece
    {
        public bool IsWhite { get; init; } = true;
        public char Symbol { get { if (IsWhite) { return 'N'; } else return 'n'; } }
        public bool Moved { get; set; } = false;
        public int Value { get; } = 3;

        public IEnumerable<(int, int)> MoveVectors
        {
            get
            {
                List<int> moveValues = new List<int>() { -2, -1, 1, 2 };
                var combinations = from x in moveValues
                                   from y in moveValues
                                   where x != y
                                   where -x != y
                                   select (x, y);
                return combinations;
            }
        }

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public Knight(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
    }
}
