using System;
using System.Diagnostics.Contracts;

namespace lib
{
    public class Segment
    {
        public readonly Vector Start, End;

        public Segment(Vector start, Vector end)
        {
            Start = start;
            End = end;
        }

	    public Vector ToVector()
	    {
		    return End -Start;
	    }

        public Rational QuadratOfLength
        {
            get
            {
                var result = (End.X - Start.X) * (End.X - Start.X) +
                (End.Y - Start.Y) * (End.Y - Start.Y);

                result.Reduce();
                return result;
            }
        }

        public double IrrationalLength
        {
            get
            {
                return Math.Sqrt((double)QuadratOfLength);
            }
        }

		public static implicit operator Segment(string s)
		{
			return Parse(s);
		}
        

		public static Segment Parse(string s)
		{
			var parts = s.Split(' ');
			if (parts.Length != 2) throw new FormatException(s);
			return new Segment(Vector.Parse(parts[0]), Vector.Parse(parts[1]));
		}

		public Segment Reflect(Segment mirror)
		{
			return new Segment(Start.Reflect(mirror), End.Reflect(mirror));
		}

		public override string ToString()
		{
			return $"{Start} {End}";
		}
		[Pure]
		public Segment Move(Rational shiftX, Rational shiftY)
		{
			return new Segment(Start.Move(shiftX, shiftY), End.Move(shiftX, shiftY));
		}

		public override int GetHashCode()
		{
			return Start.GetHashCode() ^ End.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var segment = obj as Segment;
			if (segment == null)
				return false;

			return Tuple.Create(Start, End).Equals(Tuple.Create(segment.Start, segment.End)) || Tuple.Create(End, Start).Equals(Tuple.Create(segment.Start, segment.End));
		}
	}
}