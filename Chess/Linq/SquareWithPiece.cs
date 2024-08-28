using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Chess.Linq
{
    internal class SquareWithPiece
    {
        public IPiece Piece { get; set; }
        public int Row { get; init; }
        public int Column { get; init; }

        public SquareWithPiece(IPiece piece, int row, int column)
        {
            Piece = piece;
            Column = column;
            Row = row;
        }
    }

    internal class SquareComparer : IComparer<SquareWithPiece> // podle tohoto se seřadí políčka
    {
        public int Compare(SquareWithPiece? x, SquareWithPiece? y)
        {
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            int xWeight = x.Row * 10 - x.Column;
            int yWeight = y.Row * 10 - y.Column;
            if (xWeight > yWeight)
                return -1;
            else if (xWeight == yWeight)
                return 0;
            else
                return 1;
        }
    }
}
