using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Pieces
{
    internal class EnPassant : NoPiece
    {
        public EnPassant(bool isWhite)
        {
            IsWhite = isWhite;
        }
    }
}
