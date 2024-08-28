using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class King : IPiece
    {
        public bool IsWhite { get; init; } = true;
        //public bool ShowUnicodeSymbol { get; init; } = true; 
        public char Symbol { 
            get { 
                if (IsWhite)
                {
                    //if (ShowUnicodeSymbol)
                    //{
                    //    return '\u2654';
                    //}
                    //else
                        return 'K';
                }
                else { 
                    //if (ShowUnicodeSymbol)
                    //{
                    //    return '\u265A';
                    //}
                    //else 
                        return 'k'; 
                }
            } }
        public bool Moved { get; set; } = false;
        public int Value { get; } = 0;

        public IEnumerable<(int, int)> MoveVectors
        {
            get
            {
                List<int> moveValues = new List<int>() { -1, 0, 1 };
                var combinations = from x in moveValues 
                                   from y in moveValues 
                                   where x != 0 || y != 0
                                   select (x, y);
                return combinations; // all pairs of values from moreValues without (0,0)
            }
        }

        public IEnumerable<(int, int)> AttackVectors => MoveVectors;

        public King(bool isWhite = true)
        {
            IsWhite = isWhite;
        }
    }
}
