#nullable enable

using System;
using System.Collections.Generic;

namespace Cobalt.Core
{
    public class DirectedAcyclicGraph<V, E>
    {
        public class GraphVertex
        {
            internal GraphVertex(V userdata)
            {
                UserData = userdata;
            }

            public V UserData { get; }

            public override bool Equals(object? obj)
            {
                return obj switch
                {
                    null => false,
                    GraphVertex vert => vert.UserData!.Equals(UserData),
                    _ => false
                };
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(UserData);
            }
        }

        public class GraphEdge
        {
            internal GraphEdge(E userData, GraphVertex source, GraphVertex dest)
            {
                UserData = userData;

                Source = source;
                Destination = dest;
            }

            public E UserData { get; }

            public GraphVertex Source { get; }
            public GraphVertex Destination { get; }

            public override bool Equals(object? obj)
            {
                return obj switch
                {
                    null => false,
                    GraphEdge edge => edge.UserData!.Equals(UserData) && edge.Source == Source &&
                                      edge.Destination == Destination,
                    _ => false
                };
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(UserData, Source.GetHashCode(), Destination.GetHashCode());
            }
        }

        private readonly Dictionary<GraphVertex, List<GraphEdge>> _adjacencyList = new Dictionary<GraphVertex, List<GraphEdge>>();

        public GraphVertex AddVertex(V vertex)
        {
            GraphVertex graphVertex = new GraphVertex(vertex);
            if (!_adjacencyList.ContainsKey(graphVertex))
            {
                _adjacencyList.Add(graphVertex, new List<GraphEdge>());
            }

            return graphVertex;
        }

        public bool RemoveVertex(GraphVertex vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex)) return false;
            
            _adjacencyList.Remove(vertex);

            foreach (var member in _adjacencyList)
            {
                member.Value.RemoveAll(edge => edge.Destination.Equals(vertex));
            }

            return true;

        }

        public GraphEdge AddEdge(E edge, GraphVertex source, GraphVertex dest)
        {
            GraphEdge graphEdge = new GraphEdge(edge, source, dest);
            
            _adjacencyList[source].Add(graphEdge);

            return graphEdge;
        }

        public bool RemoveEdge(GraphEdge edge)
        {
            return _adjacencyList[edge.Source].Remove(edge);
        }

        public List<GraphEdge> GetEdges(GraphVertex vertex)
        {
            return _adjacencyList.ContainsKey(vertex) ? _adjacencyList[vertex] : new List<GraphEdge>();
        }

        public List<GraphEdge> GetEdges()
        {
            List<GraphEdge> edges = new List<GraphEdge>();
            foreach (var (_, e) in _adjacencyList)
            {
                edges.AddRange(e);
            }

            return edges;
        }

        private void TopoSortUtil(GraphVertex vertex, Dictionary<GraphVertex, bool> visited, Stack<GraphVertex> stack)
        {
            visited[vertex] = true;

            foreach (var (v, _) in _adjacencyList)
            {
                if (!visited.ContainsKey(v) || !visited[v])
                {
                    TopoSortUtil(v, visited, stack);
                }
            }

            stack.Push(vertex);
        }

        public List<GraphVertex> Sort()
        {
            Stack<GraphVertex> stack = new Stack<GraphVertex>();
            Dictionary<GraphVertex, bool> visited = new Dictionary<GraphVertex, bool>();

            foreach (var (v, _) in _adjacencyList)
            {
                if (!visited.ContainsKey(v) || !visited[v])
                {
                    TopoSortUtil(v, visited, stack);
                }
            }

            return new List<GraphVertex>(stack.ToArray());
        }
    }
}
