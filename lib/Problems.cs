using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace lib
{
	public class Problems
	{
		#region kasha

		public SolutionSpec Kashalot(string r1 = "1/4", string r2 = "3/4")
		{
			var p1 = $"{r1},{r1}";
			var p2 = $"{r2},{r2}";
			var sourcePoints = $"0,0 0,1 1,1 1,0 {p1} {p2}".ToPoints();
			var destPoints = $"0,0 1,0 1,1 1,0 {p1} {p2}".ToPoints();
			destPoints[0] = sourcePoints[0].Reflect($"{p1} 1,0");
			destPoints[2] = sourcePoints[2].Reflect($"{p2} 1,0");
			var res = new SolutionSpec(sourcePoints, new Facet[]
			{
				new Facet(0, 1, 4),
				new Facet(4, 1, 5),
				new Facet(1, 2, 5),
				new Facet(0, 4, 3),
				new Facet(3, 4, 5),
				new Facet(3, 5, 2),
			},
				destPoints
				);
			return res;
		}

		public SolutionSpec Kashalot2(string r1 = "1/16", string r2 = "1/4", string r3 = "4/5", string r4 = "24/25")
		{
			var p4 = $"{r1},{r1}";
			var p5 = $"{r2},{r2}";
			var p6 = $"{r3},{r3}";
			var p7 = $"{r4},{r4}";
			var sourcePoints = $"0,0 0,1 1,1 1,0 {p4} {p5} {p6} {p7}".ToPoints();
			var destPoints = sourcePoints.ToArray();
			destPoints[1] = sourcePoints[1].Reflect("0,0 1,1");
			destPoints[4] = sourcePoints[4].Reflect($"{p5} 1,0");
			var p01 = sourcePoints[0].Reflect($"{p5} 1,0");
			destPoints[0] = p01.Reflect($"{destPoints[4]} 1,0");
			destPoints[7] = sourcePoints[7].Reflect($"{p6} 1,0");
			var p21 = sourcePoints[2].Reflect($"{p6} 1,0");
			destPoints[2] = p21.Reflect($"{destPoints[7]} 1,0");
			var res = new SolutionSpec(sourcePoints, new Facet[]
			{
				new Facet(1, 0, 4),
				new Facet(1, 5, 4),
				new Facet(1, 5, 6),
				new Facet(1, 7, 6),
				new Facet(1, 7, 2),
				new Facet(3, 0, 4),
				new Facet(3, 5, 4),
				new Facet(3, 5, 6),
				new Facet(3, 7, 6),
				new Facet(3, 7, 2),
			},
				destPoints
				);
			return res;
		}

		[Test, Explicit]
		public void PrintKashalot()
		{
			Console.WriteLine(Kashalot("4/25", "15/20"));
		}

		[Test, Explicit]
		public void PrintKashalot2()
		{
			Console.WriteLine(Kashalot2());
		}

		#endregion

		//http://2016sv.icfpcontest.org/problem/view/3356
		public SolutionSpec SkewedTriangleWithHole(Rational a, Rational b)
		{
			var src = $"1/2,1/2 0,0 0,{a} 0,1 1,1 1,{1 - a} 1,0 1,{b} {1 - b / 3},{b / 3} {1 - b},0".ToPoints();
			var dst = src.ToArray();
			dst[2] = src[2].Reflect(dst[0], dst[1]);
			dst[3] = src[3].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]);
			dst[4] = src[4].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]).Reflect(dst[0], dst[3]);
			dst[5] = src[5].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]).Reflect(dst[0], dst[3]).Reflect(dst[0], dst[4]);
			dst[7] = src[7].Reflect(dst[0], dst[6]);
			dst[6] = dst[6].Reflect(dst[8], dst[9]);
			var facets = new Facet[]
			{
				new Facet(0, 2, 1),
				new Facet(0, 3, 2),
				new Facet(0, 4, 3),
				new Facet(0, 5, 4),
				new Facet(0, 8, 7, 5),
				new Facet(0, 1, 9, 8),
				new Facet(8, 9, 6),
				new Facet(8, 6, 7)
			};
			return new SolutionSpec(src, facets, dst);
		}

		//http://2016sv.icfpcontest.org/problem/view/3156
		public SolutionSpec SkewedTriangle(Rational a)
		{
			var src = $"1/2,1/2 0,0 0,{a} 0,1 1,1 1,{1 - a} 1,0".ToPoints();
			var dst = src.ToArray();
			dst[2] = src[2].Reflect(dst[0], dst[1]);
			dst[3] = src[3].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]);
			dst[4] = src[4].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]).Reflect(dst[0], dst[3]);
			dst[5] = src[5].Reflect(dst[0], dst[1]).Reflect(dst[0], dst[2]).Reflect(dst[0], dst[3]).Reflect(dst[0], dst[4]);
			var facets = new Facet[]
			{
				new Facet(0, 2, 1),
				new Facet(0, 3, 2),
				new Facet(0, 4, 3),
				new Facet(0, 5, 4),
				new Facet(0, 6, 5),
				new Facet(0, 1, 6),
			};
			return new SolutionSpec(src, facets, dst);
		}

		[Test, Explicit]
		public void PrintSkewedTriangleWithHole()
		{
			var solutionSpec = SkewedTriangleWithHole(new Rational(49, 90), new Rational(1, 10));
			solutionSpec.CreateVisualizerForm().ShowDialog();
			Console.WriteLine(solutionSpec);
		}

		[Test, Explicit]
		public void PrintSkewedTriangle()
		{
			var solutionSpec = SkewedTriangle(new Rational(12, 20));
			solutionSpec.CreateVisualizerForm().ShowDialog();
			Console.WriteLine(solutionSpec);
		}


		public SolutionSpec Triangle(Rational a, Rational b)
		{
			var p12 = $"{a},{a}";
			var p13 = $"{1 - a},{a}";
			var src = $"1/2,1/2 0,0 0,1/2 0,{1 - b} 0,1 {b},1 {1 - b},1 1,1 1,{1 - b} 1,1/2 1,0 1/2,0 {p12} {p13}".ToPoints();
			var dst = src.ToArray();
			dst[1] = src[1].Reflect(src[12], src[11]);
			dst[2] = src[2].Reflect(src[0], src[1]);
			dst[3] = src[3].Reflect(src[0], src[1]).Reflect(dst[2], src[0]);
			dst[4] = src[4].Reflect(src[0], src[1]).Reflect(dst[2], src[0]).Reflect(dst[3], src[0]);
			dst[5] = src[5].Reflect(src[0], src[2]);
			dst[6] = src[6].Reflect(src[0], src[2]);
			dst[7] = src[7].Reflect(src[0], src[2]).Reflect(dst[6], src[0]);
			dst[9] = src[9].Reflect(src[0], src[10]);
			dst[8] = src[8].Reflect(src[0], src[10]).Reflect(dst[9], src[0]);
			dst[10] = src[10].Reflect(src[11], src[13]);
			var facets = new Facet[]
			{
				new Facet(0, 2, 12),
				new Facet(0, 3, 2),
				new Facet(0, 4, 3),
				new Facet(0, 5, 4),
				new Facet(0, 6, 5),
				new Facet(0, 7, 6),
				new Facet(0, 8, 7),
				new Facet(0, 9, 8),
				new Facet(0, 13, 9),
				new Facet(0, 12, 11, 13),
				new Facet(1, 2, 12),
				new Facet(1, 11, 12),
				new Facet(10, 9, 13),
				new Facet(10, 11, 13),
			};
			return new SolutionSpec(src, facets, dst);
		}

		[Test, Explicit]
		public void PrintTriangle()
		{
			var api = new ApiClient();
			var hour = 3600;
			var time = 1470553200 + hour;
			for (int i = 1; i <= 4; i++)
				for (int j = 1; j <= 4; j++)
				{
					var solution = Triangle(new Rational(1, 4) - new Rational(i, 19), new Rational(1, 4) - new Rational(j, 21));
					solution.CreateVisualizerForm().ShowDialog();
					var ans = api.PostProblem(time, solution);
					Console.WriteLine(ans);
					File.WriteAllText(Path.Combine(Paths.ProblemsDir(), $"{time}.problem.txt"), solution + "\r\n\r\n" + ans);
					time += hour;
					Thread.Sleep(1000);
				}
		}

		public SolutionSpec Quatro()
		{
			var src = $"0,0 1/2,0 1,0 1,4/26 1,1 8/14,1 0,1 0,4/6 0,1/2 1/2,1/2".ToPoints();
			var dst = src.ToArray();
			dst[8] = src[8].Reflect(src[9], src[0]);
			dst[7] = src[7].Reflect(src[9], src[0]).Reflect(dst[9], dst[8]);
			dst[6] = src[6].Reflect(src[9], src[0]).Reflect(dst[9], dst[8]).Reflect(dst[9], dst[7]);
			dst[5] = src[5].Reflect(src[9], src[0]).Reflect(dst[9], dst[8]).Reflect(dst[9], dst[7]);
			dst[4] = src[4].Reflect(src[9], src[0]).Reflect(dst[9], dst[8]).Reflect(dst[9], dst[7]).Reflect(dst[9], dst[5]);
			dst[3] = src[3].Reflect(src[9], src[0]).Reflect(dst[9], dst[8]).Reflect(dst[9], dst[7]).Reflect(dst[9], dst[5]);
			dst[2] =
				src[2].Reflect(src[9], src[0])
					.Reflect(dst[9], dst[8])
					.Reflect(dst[9], dst[7])
					.Reflect(dst[9], dst[5])
					.Reflect(dst[9], dst[3]);
			dst[1] =
				src[1].Reflect(src[9], src[0])
					.Reflect(dst[9], dst[8])
					.Reflect(dst[9], dst[7])
					.Reflect(dst[9], dst[5])
					.Reflect(dst[9], dst[3]);
			//dst[0] = src[0].Reflect(src[10], src[9]).Reflect(dst[10], dst[8]).Reflect(dst[10], dst[6]).Reflect(dst[10], dst[3]).Reflect(dst[10], dst[2]).Reflect(dst[10], dst[1]);
			Console.WriteLine($"{dst[1]} {src[1]}");
			var delta = dst[1] - src[1];
			Console.WriteLine($"{delta.X.AsFloat()} {delta.Y.AsFloat()}");

			var facets = new Facet[]
			{
				new Facet(9, 0, 1),
				new Facet(9, 1, 2, 3),
				new Facet(9, 3, 4, 5),
				new Facet(9, 5, 6, 7),
				new Facet(9, 7, 8),
				new Facet(9, 8, 0),
			};
			return new SolutionSpec(src, facets, dst);
		}

		[Test, Explicit]
		public void PrintQuatro()
		{
			Check(Quatro());
		}

		[Test]
		public void SquareRotated()
		{
			var src = "0,0 1,0 1,1 0,1".ToPoints();
			Console.WriteLine(new SolutionSpec(src, new[] { new Facet(0, 1, 2, 3) },
				"4328029871649615121465353437184/8656059743299229793415925725865,-1792728671193156318471947026432/8656059743299229793415925725865 10448788414492386111887872752297/8656059743299229793415925725865,4328029871649615121465353437184/8656059743299229793415925725865 4328029871649614671950572288681/8656059743299229793415925725865,10448788414492386111887872752297/8656059743299229793415925725865 -1792728671193156318471947026432/8656059743299229793415925725865,4328029871649614671950572288681/8656059743299229793415925725865"
					.ToPoints()));
		}

		private static void Check(SolutionSpec solution)
		{
			solution.CreateVisualizerForm().Show();
			solution.CreateVisualizerForm().ShowDialog();
			Console.WriteLine(solution);
		}
	}
}