﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lib
{
	public class SolutionSpec
	{
		public readonly string Raw;
		public readonly Vector[] SourcePoints;
		public readonly Facet[] Facets;
		public readonly Vector[] DestPoints;
		private static readonly Vector[] initialSquare = "0,0 1,0 1,1 0,1".ToPoints();

		public static SolutionSpec CreateTrivial(Func<Vector, Vector> transform = null)
		{
			return new SolutionSpec(initialSquare, new[] { new Facet(0, 1, 2, 3) }, initialSquare.Select(transform ?? (x => x)).ToArray());
		}

		public SolutionSpec(string raw)
		{
			Raw = raw;
		}

		public SolutionSpec(Vector[] sourcePoints, Facet[] facets, Vector[] destPoints)
		{
			if (sourcePoints.Length != destPoints.Length)
				throw new ArgumentException();
			SourcePoints = sourcePoints;
			Facets = facets;
			DestPoints = destPoints;
		}

		public Polygon[] Polygons => Facets.Select(FacetToPolygon).ToArray();
		public Polygon[] PolygonsDest => Facets.Select(FacetToPolygonDst).ToArray();

		private Polygon FacetToPolygon(Facet f)
		{
			return new Polygon(f.Vertices.Select(i => SourcePoints[i]).ToArray());
		}
		private Polygon FacetToPolygonDst(Facet f)
		{
			return new Polygon(f.Vertices.Select(i => DestPoints[i]).ToArray());
		}

		public IEnumerable<Segment> GetAllDestSegments()
		{
			return PolygonsDest.SelectMany(p => p.Segments);
		}

		public override string ToString()
		{
			if (Raw != null)
				return Raw;
			var sb = new StringBuilder();
			sb.AppendLine(SourcePoints.Length.ToString());
			sb.AppendLine(SourcePoints.StrJoin(Environment.NewLine));
			sb.AppendLine(Facets.Length.ToString());
			sb.AppendLine(Facets.StrJoin(Environment.NewLine));
			sb.Append(DestPoints.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		
		public int Size()
		{
			return ToString().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
		}

        public bool AreFacetsValid()
        {
            return AreFacetsValid(1);
        }

		public bool AreFacetsValid(Rational requiredSize)
		{
			if (Raw != null)
				return true;
			Rational totalSquare = 0;
			foreach (var facet in Facets)
			{
				var sourcePolygon = new Polygon(facet.Vertices.Select(index => SourcePoints[index]).ToArray());
				var destPolygon = new Polygon(facet.Vertices.Select(index => DestPoints[index]).ToArray());
				var sourceSquare = sourcePolygon.GetUnsignedSquare();
				if (sourceSquare != destPolygon.GetUnsignedSquare())
					return false;

				totalSquare += sourceSquare;
			}
			return totalSquare == requiredSize;
		}
	}
}