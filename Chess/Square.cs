using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal readonly struct Square(int row, int column)
    {
        public int Row { get; init; } = row;
        public int Column { get; init; } = column;

        public static implicit operator (int, int)(Square square) => (square.Row, square.Column);
        public static implicit operator Square((int, int) coordinates) => new Square(coordinates.Item1, coordinates.Item2);

        public override string ToString()
        {
            return $"{(char)('a'+Column-1)}{Row}";
        }

    }
}
