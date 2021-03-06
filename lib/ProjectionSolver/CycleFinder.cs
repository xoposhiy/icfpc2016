using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using lib.Graphs;
using NUnit.Framework;

namespace lib
{
	[TestFixture]
	public class GraphExt_Should
	{
		[Test]
		[Explicit]
		public void SubmitSolution()
		{
			var problemsRepo = new ProblemsRepo();
			var goodTasks = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 15, 16, 38, 39, 40, 41, 42, 46 };
//			var goodTasks = new[] { 13 };
			var apiClient = new ApiClient();
			foreach (var p in goodTasks)
			{
				try
				{
					Console.WriteLine($"!solving: {p}");
					var problemSpec = problemsRepo.Get(p);
					var solutionSpec = UltraSolver.AutoSolve(problemSpec);
					var postSolution = apiClient.PostSolution(p, solutionSpec);
					Console.WriteLine(postSolution);
					problemsRepo.PutSolution(p, solutionSpec);
					problemsRepo.PutResponse(p, postSolution);
				}
				catch (Exception e)
				{
					problemsRepo.PutSolution(p, e.ToString());
				}
				Thread.Sleep(1000);
			}
		}

		[Test]
		[Explicit]
		public void VisualiseSolution()
		{
			var problemsRepo = new ProblemsRepo();
			var problemSpec = problemsRepo.Get(1170);
			Console.WriteLine("problem");
			Console.WriteLine(problemSpec);
			problemSpec.CreateVisualizerForm().ShowDialog();
			var solutionSpec = UltraSolver.AutoSolve(problemSpec);
			solutionSpec.CreateVisualizerForm().ShowDialog();
			Console.WriteLine("solution");
			Console.WriteLine(solutionSpec);
		}
	}

    public class GNode<TEdge, TNode>
	{
		public readonly Edge<TEdge, TNode> Edge;
		public Node<TEdge, TNode> From => Edge.From;
		public Node<TEdge, TNode> To => Edge.To;
		public bool InCycle;
		public GNode<TEdge, TNode> Next;
		public readonly bool FromFrom;

		public GNode(Edge<TEdge, TNode> edge, bool direction)
		{
			Edge = edge;
			FromFrom = direction;
		}
	}

	public class CycleFinder<TEdge, TNode>
	{
		private readonly Func<Node<TEdge, TNode>, Vector> vectorSelector;
		private readonly List<GNode<TEdge, TNode>> nodes;

		private readonly Dictionary<Edge<TEdge, TNode>,
			List<GNode<TEdge, TNode>>> referenceMap;

		private Rational GetVectorProd(GNode<TEdge, TNode> startNode, Edge<TEdge, TNode> x)
		{
			Vector a;
			Vector bX;
			if (startNode.FromFrom)
			{
				a = vectorSelector(startNode.From) - vectorSelector(startNode.To);
				if (vectorSelector(x.From).Equals(vectorSelector(startNode.To)))
					bX = vectorSelector(x.To) - vectorSelector(x.From);
				else
					bX = vectorSelector(x.From) - vectorSelector(x.To);
			}
			else
			{
				a = vectorSelector(startNode.To) - vectorSelector(startNode.From);
				if (vectorSelector(x.From).Equals(vectorSelector(startNode.From)))
					bX = vectorSelector(x.To) - vectorSelector(x.From);
				else
					bX = vectorSelector(x.From) - vectorSelector(x.To);
			}
			return a.VectorProdLength(bX);
		}

		public CycleFinder(Graph<TEdge, TNode> graph, Func<Node<TEdge, TNode>, Vector> vectorSelector)
		{
			this.vectorSelector = vectorSelector;
			foreach (var edge in graph.Edges)
			{
				edge.From.EnsureIncidentEdge(edge);
				edge.To.EnsureIncidentEdge(edge);
			}
			referenceMap = new Dictionary<Edge<TEdge, TNode>,
				List<GNode<TEdge, TNode>>>();
			nodes = new List<GNode<TEdge, TNode>>();
			foreach (var edge in graph.Edges)
			{
				nodes.Add(new GNode<TEdge, TNode>(edge, false));
				nodes.Add(new GNode<TEdge, TNode>(edge, true));
				var l = nodes.Count - 1;
				referenceMap[edge] = new List<GNode<TEdge, TNode>> { nodes[l], nodes[l - 1] };
			}
		}

		public List<List<GNode<TEdge, TNode>>> GetCycles()
		{
			foreach (var node in nodes)
				FindCycles(node);
			var used = new HashSet<GNode<TEdge, TNode>>();
			var res = new List<List<GNode<TEdge, TNode>>>();
			foreach (var node in nodes)
			{
				if (!used.Contains(node))
				{
					var n = node;
					var cycle = new List<GNode<TEdge, TNode>>();
					while (n != null && used.Add(n))
					{
						cycle.Add(n);
						n = n.Next;
					}
					if (n == node)
						res.Add(cycle);
				}
			}
			return res;
		}

		private void FindCycles(GNode<TEdge, TNode> startNode)
		{
			while (!startNode.InCycle)
			{
				startNode.InCycle = true;
				var commonNode = startNode.FromFrom ? startNode.Edge.To : startNode.Edge.From;
				var edges = commonNode.IncidentEdges.Where(e => !e.Equals(startNode.Edge)).ToList();
				Edge<TEdge, TNode> nextEdge = null;
				foreach (var edge in edges)
				{
					var checkGoodProd = GetVectorProd(startNode, edge);
					if (checkGoodProd <= 0)
					{
						if (nextEdge == null)
							nextEdge = edge;
						else
						{
							var n = nextEdge.From == commonNode
								? vectorSelector(nextEdge.To) - vectorSelector(nextEdge.From)
								: vectorSelector(nextEdge.From) - vectorSelector(nextEdge.To);
							var e = edge.From == commonNode
								? vectorSelector(edge.To) - vectorSelector(edge.From)
								: vectorSelector(edge.From) - vectorSelector(edge.To);
							var candidateProd = e.VectorProdLength(n);
							if (candidateProd < 0)
								nextEdge = edge;
						}
					}
				}
				if (nextEdge == null)
					return;
				var nextFromFrom = vectorSelector(nextEdge.From).Equals(vectorSelector(commonNode));
				startNode.Next = referenceMap[nextEdge].First(x => x.FromFrom == nextFromFrom);
				startNode = startNode.Next;
			}
		}
	}

	public static class GraphExtensons
	{
		public static Graph<Segment, Vector> CreateGraphFromSegmentsArray(Segment[] segments)
		{
			var segmentsEndings = new List<Vector>();
			foreach (var segment in segments)
			{
				segmentsEndings.Add(segment.End);
				segmentsEndings.Add(segment.Start);
			}
			var nodes = segmentsEndings.Distinct().ToList();
			var nodesIndeces = nodes
				.Select((x, i) => new { x, i })
				.ToDictionary(x => x.x, x => x.i);
			var segmentsFromNode = nodes.ToDictionary(n => n, _ => new List<Segment>());
			foreach (var segment in segments)
			{
				segmentsFromNode[segment.Start].Add(segment);
				segmentsFromNode[segment.End].Add(segment);
			}
			var graph = new Graph<Segment, Vector>(nodes.Count);
			foreach (var nodesIndece in nodesIndeces)
				graph[nodesIndece.Value].Data = nodesIndece.Key;
			var connectedPairs = new HashSet<Tuple<int, int>>();
			foreach (var graphNode in graph.Nodes)
			{
				var graphNodeIndex = nodesIndeces[graphNode.Data];
				foreach (var segment in segmentsFromNode[graphNode.Data])
				{
					var otherNode = graphNode.Data.Equals(segment.Start) ? segment.End : segment.Start;
					var otherNodeIndex = nodesIndeces[otherNode];
					if (connectedPairs.Contains(Tuple.Create(graphNodeIndex, otherNodeIndex)))
						continue;
					graph.DirectedConnect(graphNodeIndex, otherNodeIndex).Data = segment;
					connectedPairs.Add(Tuple.Create(graphNodeIndex, otherNodeIndex));
					connectedPairs.Add(Tuple.Create(otherNodeIndex, graphNodeIndex));
				}
			}
			return graph;
		}
	}

	[TestFixture]
	public class GraphExtensions_Should
	{
		[Test]
		public void DoSomething_WhenSomething()
		{
			var res = GraphExtensons.CreateGraphFromSegmentsArray(new[]
			{
				new Segment(new Vector(0, 0), new Vector(1, 0)),
				new Segment(new Vector(0, 0), new Vector(0, 1)),
				new Segment(new Vector(1, new Rational(1, 3)), new Vector(1, 0)),
				new Segment(new Vector(1, new Rational(1, 3)), new Vector(1, 1)),
				new Segment(new Vector(1, new Rational(1, 3)), new Vector(new Rational(1, 3), 1)),
				new Segment(new Vector(0, 1), new Vector(new Rational(1, 3), 1)),
				new Segment(new Vector(1, 1), new Vector(new Rational(1, 3), 1))
			});
			var cycles = new CycleFinder<Segment, Vector>(res, n => n.Data).GetCycles();
		}
	}
}