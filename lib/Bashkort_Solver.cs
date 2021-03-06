using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using lib.Api;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	[Explicit]
	public class Bashkort_Solver
	{
		[Test]
		public void PostSolutionsForSameProblems3()
		{
			var repo = new ProblemsRepo();
			var solutions = new Dictionary<int, Tuple<string, double, int>>();
			Console.Out.WriteLine($"calculating");
			foreach (var problemSpec in repo.GetAll().Where(p => repo.FindSolution(p.id) != null))
			{
				var perms = GetAllPerms(problemSpec);
				var polygonsHashCode = perms.Select(p => p.MoveToOrigin().GetPolygonsHashCode()).ToList().Min();
				Tuple<string, double, int> prev;
				if (!solutions.TryGetValue(polygonsHashCode, out prev) || prev.Item2 < repo.GetProblemResemblance(problemSpec.id))
					solutions[polygonsHashCode] = Tuple.Create(repo.FindSolution(problemSpec.id), repo.GetProblemResemblance(problemSpec.id), problemSpec.id);
			}
			foreach (var problemSpec in repo.GetAll())
			{
				var perms = GetAllPerms(problemSpec);
				var polygonsHashCode = perms.Select(p => p.MoveToOrigin().GetPolygonsHashCode()).ToList().Min();
				Tuple<string, double, int> best;
				if (solutions.TryGetValue(polygonsHashCode, out best))
				{
					if (repo.GetProblemResemblance(problemSpec.id) < best.Item2)
					{
						Console.Out.Write($"{problemSpec.id} -> {best.Item3}: ");
						Console.Out.Write($"{repo.GetProblemResemblance(problemSpec.id)} -> {best.Item2}");
						Console.Out.WriteLine();
					}
				}
//					ProblemsSender.Post(new SolutionSpec(best.Item1), problemSpec.id, pack: false);
			}
		}

		private List<ProblemSpec> GetAllPerms(ProblemSpec source)
		{
			var result = new List<ProblemSpec>();
			result.Add(source);
			for (int i = 0; i < 3; i++)
			{
				source = Rotate90(source);
				result.Add(source);
			}
			source = ReflectProblem(source, "0,0 1,1");
			for (int i = 0; i < 4; i++)
			{
				source = Rotate90(source);
				result.Add(source);
			}
			return result;

		}

		private static ProblemSpec Rotate90(ProblemSpec source)
		{
			return ReflectProblem(ReflectProblem(source, "1/2,0 1/2,1"), "0,0 1,1");
		}

		private static ProblemSpec ReflectProblem(ProblemSpec source, Segment segment)
		{
			return new ProblemSpec(source.Polygons.Select(v => v.Reflect(segment)).ToArray(), new Segment[0]);
		}

		[Test]
		public void PostSolutionsForSameProblems2()
		{
			var repo = new ProblemsRepo();
			var directories = Directory.GetDirectories(@"c:\Work\icfpc\other\icfp-2016\data\problems\", "*");
			var otherProblems = directories.Select(x =>
			{
				int result;
				if (int.TryParse(Path.GetFileName(x), out result))
					return result;
				return -1;
			}).Where(x => x != -1);
			foreach (var otherProblem in otherProblems)
			{
				if (repo.FindResponse(otherProblem) == null)
					Console.Out.WriteLine(otherProblem);
			}
		}

		[Test]
		public void PostSolutionsForSameProblems()
		{
			var repo = new ProblemsRepo();
			var solutions = new Dictionary<int, Tuple<string, double, int>>();
			foreach (var problemSpec in repo.GetAll().Where(p => repo.FindSolution(p.id) != null))
			{
				var polygonsHashCode = problemSpec.GetPolygonsHashCode();
				Tuple<string, double, int> prev;
				if (!solutions.TryGetValue(polygonsHashCode, out prev) || prev.Item2 < repo.GetProblemResemblance(problemSpec.id))
					solutions[polygonsHashCode] = Tuple.Create(repo.FindSolution(problemSpec.id), repo.GetProblemResemblance(problemSpec.id), problemSpec.id);
			}
			foreach (var problemSpec in repo.GetAll())
			{
				var polygonsHashCode = problemSpec.GetPolygonsHashCode();
				Tuple<string, double, int> best;
				if (solutions.TryGetValue(polygonsHashCode, out best) && repo.GetProblemResemblance(problemSpec.id) < best.Item2)
				{
					Console.Write($"{problemSpec.id}->{best.Item3}: ");
					ProblemsSender.Post(new SolutionSpec(best.Item1), problemSpec.id, pack: false);
					Console.WriteLine();
				}
			}
		}

		[Test]
		[TestCase(2146)]
		public void DoSomething_WhenSomething2(int problemId)
		{
			var repo = new ProblemsRepo();
			var problemSpec = repo.Get(problemId);
			var sourcePoints = new List<Vector>();
			var destPoints = new List<Vector>();
			var facets = new List<Facet>();
			for (int iX = 0; iX <= 31; iX++)
				for (int iY = 0; iY <= 6; iY++)
				{
					sourcePoints.Add(new Vector(iX/(Rational)31, iY / (Rational)6));
					if (iY%2 == 0)
						if (iX%2 == 0)
							destPoints.Add("0,0");
						else
							destPoints.Add("1/31,0");
					else
						if (iX % 2 == 0)
							destPoints.Add("0,1/6");
						else
							destPoints.Add("1/31,1/6");
				}
			for (int iX = 0; iX < 31; iX++)
				for (int iY = 0; iY < 6; iY++)
					facets.Add(new Facet(iX * 7 + iY, iX * 7 + 1 + iY, (iX+1) * 7 + iY + 1, (iX+1) * 7 + iY));
			var solution = new SolutionSpec(sourcePoints.ToArray(), facets.ToArray(), destPoints.ToArray());

			Console.Out.WriteLine($"size: {solution.Size()}; packed: {solution.Pack().Size()}");
			Console.Out.WriteLine($"facets: {solution.Facets.Length}; sourcePoints: {solution.SourcePoints.Length}; destPoints: {solution.DestPoints.Length}");

			//solution.CreateVisualizerForm().ShowDialog();

			var post = ProblemsSender.Post(solution, problemSpec.id);
			Console.Out.WriteLine(post);
		}

		[Test]
		[TestCase(3852, "9999077/69994655", "1116/69994655", false)]
		[TestCase(3965, "997/9981", "11/9981", true)]
		[TestCase(4008, "11173797/89390393", "17/89390393", true)]
		[TestCase(4010, "199191999799187/1195151998796239", "1117/1195151998796239", true)]
		[TestCase(4229, "1077/9709", "16/9709", false)]
		[TestCase(4234, "997/9981", "11/9981", true)]
		[TestCase(4236, "1077/9709", "16/9709", false)]
		[TestCase(4237, "997/9981", "11/9981", true)]
		[TestCase(4239, "1077/9709", "16/9709", false)]
		[TestCase(5195, "1077/9709", "16/9709", false)]
		[TestCase(5197, "997/9981", "11/9981", true)]
		[TestCase(5199, "1077/9709", "16/9709", false)]
		[TestCase(5203, "997/9981", "11/9981", true)]
		[TestCase(5204, "1077/9709", "16/9709", false)]
		[TestCase(5705, "997/9981", "11/9981", true)]
		[TestCase(5724, "1077/9709", "16/9709", false)]
		[TestCase(5725, "997/9981", "11/9981", true)]
		[TestCase(5726, "1077/9709", "16/9709", false)]
		[TestCase(5987, "1077/9709", "16/9709", false)]
		[TestCase(5988, "1077/9709", "16/9709", false)]
		[TestCase(5990, "1077/9709", "16/9709", false)]
		[TestCase(6100, "1077/9709", "16/9709", false)]
		[TestCase(6101, "1077/9709", "16/9709", false)]
		[TestCase(3854, "479887358674887/2399436793374547", "112/2399436793374547", false)]
		[TestCase(3560, "1077/9709", "16/9709", false)]
		[TestCase(4242, "997/9981", "11/9981", true)]
		public void DoSomething_WhenSomething(int problemId, string s887, string s112, bool toCorner)
		{
			var repo = new ProblemsRepo();
			var problemSpec = repo.Get(problemId);

			var v887 = (Rational)s887;
			var v112 = (Rational)s112;

			var vRate = (1 - v112) / v887;
			vRate.Denomerator.Should().Be(1, $"rate={vRate.Numerator}/{vRate.Denomerator}");

			var rate = (int)vRate.Numerator;
			var rateOddity = 1-rate%2;
			
			var sourcePoints = new List<Vector>();
			var destPoints = new List<Vector>();
			var facets = new List<Facet>();

			for (int iX = 0; iX <= rate; iX++)
				for (int iY = 0; iY <= rate; iY++)
				{
					sourcePoints.Add(new Vector(iX * v887, iY * v887));
					if ((iX + iY) % 2 == 0)
					{
						destPoints.Add(new Vector(v887, v887));
					}
					else if (iX % 2 == rateOddity)
					{
						destPoints.Add(new Vector(0, v887));
					}
					else
					{
						destPoints.Add(new Vector(v887, 0));
					}
				}

			for (int iX = 0; iX < rate; iX++)
				for (int iY = 0; iY < rate; iY++)
				{
					if ((iX + iY) % 2 == 0)
					{
						facets.Add(new Facet(iX * (rate + 1) + iY, iX * (rate + 1) + iY + 1, iX * (rate + 1) + iY + rate + 1));
						facets.Add(new Facet(iX * (rate + 1) + iY + 1, iX * (rate + 1) + iY + rate + 1, iX * (rate + 1) + iY + rate + 2));
					}
					else
					{
						facets.Add(new Facet(iX * (rate + 1) + iY, iX * (rate + 1) + iY + 1, iX * (rate + 1) + iY + rate + 2));
						facets.Add(new Facet(iX * (rate + 1) + iY, iX * (rate + 1) + iY + rate + 2, iX * (rate + 1) + iY + rate + 1));
					}
				}

			for (int iX = 0; iX <= rate; iX++)
			{
				sourcePoints.Add(new Vector(iX * v887, 1));
				destPoints.Add(new Vector(iX % 2 == rateOddity ? 0 : v887, v887 - v112));
			}
			for (int iY = 0; iY <= rate; iY++)
			{
				sourcePoints.Add(new Vector(1, iY * v887));
				destPoints.Add(new Vector(v887 - v112, iY % 2 == rateOddity ? 0 : v887));
			}

			for (int iX = 0; iX < rate; iX++)
			{
				var start = (rate + 1) * (rate + 1);
				facets.Add(new Facet(start + iX, start + iX + 1, (iX + 2) * (rate + 1) - 1, (iX + 1) * (rate + 1) - 1));
			}

			for (int iY = 0; iY < rate; iY++)
			{
				var start = (rate + 1) * (rate + 2);
				var start2 = (rate + 1) * rate;
				facets.Add(new Facet(start + iY, start + iY + 1, start2 + iY + 1, start2 + iY));
			}

			sourcePoints.Add(new Vector(1, 1));
			destPoints.Add(new Vector(v887 - v112, v887 - v112));
			facets.Add(new Facet((rate + 1) * (rate + 1) - 1, (rate + 1) * (rate + 2) - 1, sourcePoints.Count - 1, sourcePoints.Count - 2));

			var solution = new SolutionSpec(sourcePoints.ToArray(), facets.ToArray(), destPoints.ToArray());
			if (toCorner)
				solution = solution.Reflect(new Segment(new Vector(v887, 0), new Vector(0, v887)));

			Console.Out.WriteLine($"size: {solution.Size()}; packed: {solution.Pack().Size()}");
			Console.Out.WriteLine($"facets: {solution.Facets.Length}; sourcePoints: {solution.SourcePoints.Length}; destPoints: {solution.DestPoints.Length}");

			//solution.CreateVisualizerForm().ShowDialog();


			var post = ProblemsSender.Post(solution, problemSpec.id);
			Console.Out.WriteLine(post);


			//			var v775 = (Rational)"479887358674775";
			//			var rational = v887*5 + 112;

			//			var rational = (denominator - numerator)/112;
			//			var rational = v775/112;

			//			rational.Reduce();
			//			Console.Out.WriteLine($"{rational.Numerator}/{rational.Denomerator}");
			//			Console.Out.WriteLine(2399436793374547);
			//Console.Out.WriteLine((double)(Rational)"479887358674775/2399436793374547");
		}

		//[TestCase(2414)]
		//[TestCase(2225)]
		//[TestCase(2267)]

		//		[TestCase(2668)]
		//		[TestCase(2777)]
		//		[TestCase(2966)]
		//		[TestCase(3180)]
		//		[TestCase(3404)]
		[TestCase(3854)]

		public void Solve(int problemId)
		{
			var problemsRepo = new ProblemsRepo();
			var problem = problemsRepo.Get(problemId);
			var poly = problem.Polygons.Single();
			//			var dx = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.X.Denomerator).Max();
			//			var dy = (int) problem.Polygons.SelectMany(p => p.Vertices).Select(x => x.Y.Denomerator).Max();
			//			foreach (var x in Enumerable.Range(0, dx).Select(x => new Rational(x, dx)))
			//				foreach (var y in Enumerable.Range(0, dy).Select(y => new Rational(y, dy)))
			{
				//					var shift = new Vector(x, y);
				var shift = new Vector(0, 0);
				var initialSolution = SolutionSpec.CreateTrivial(v => v + shift);
				var solution = ConvexPolygonSolver.Solve(poly.GetConvexBoundary(), initialSolution);
				var packedSolution = solution.Pack();
				var packedSolutionSize = packedSolution.Size();
				var solutionSize = solution.Size();
				Console.WriteLine($"{shift}: {solutionSize}; packed: {packedSolutionSize}");
				if (packedSolutionSize <= 5000)
				{
					ProblemsSender.Post(packedSolution, problemId, false);
					//						return;
				}
			}
		}
	}
}