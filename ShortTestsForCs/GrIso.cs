﻿
using System;
using System.Collections.Generic;


namespace GrIso
{
    class AbortException: Exception
    {
        public AbortException(string message) : base(message) { }
    }

    class GraphVertex : ShortHashList   
    {
        public new GraphVertex Clone()
        {
            var clone = new GraphVertex();
            base.Clone(clone);
            return clone;
        }
    }

    class Graph: List<GraphVertex>    
    {    
        public Graph(int vertex_count)
            :base(vertex_count)
        {
            for (int i = 0; i < vertex_count; ++i)
                this[i] = new GraphVertex();
        }
        public void Append(ushort some_vertex, ushort other_vertex)
        {
            this[some_vertex].Append(other_vertex);
            this[other_vertex].Append(some_vertex);
        }
        public bool Hash()
        {
            for (int i = 0; i < Count; ++i)
            {
                if (!this[i].Hash())
                    return false;
            }
            return true;
        }
        public Graph Clone()
        {
            var clone = new Graph(Count);
            for (int i = 0; i < Count; ++i)
                clone[i] = this[i].Clone();
            return clone;
        }
        public bool Find(ushort some_vertex, ushort other_vertex)
        {
            return this[some_vertex].Find(other_vertex);
        }
        public bool Compare(Graph graph)
        {
            if (Count != graph.Count)
                return false;
            for (int i = 0; i < Count; ++i)
                if (graph[i].Count() != this[i].Count())
                    return false;

            for (ushort i1 = 0; i1 < Count; ++i1)
                for (var i2 = this[i1].First(); i2 != GraphVertex.None; i2 = this[i1].Next(i2))
                    if (!graph.Find(i1, i2))
                        return false;

                return true;
        }
        

    };

    class GraphFun
    {
        static void Abort(string message) { throw new AbortException(message); }
        RandQuick rand_quick;

        public GraphFun(uint rand_seed)
        {
            rand_quick = new RandQuick(rand_seed);
        }

        public Graph Generate(int vertex_count, int edge_count)
        {
            if (edge_count < vertex_count - 1)
                Abort("too less edge - only connected graph");
            if (edge_count >= vertex_count*( vertex_count - 1))
                Abort("too many edge - symetric graph");

            /* Explicite added and removed edges depending on bool flag. 
             * For dense graph filled only removed edges excepts those have been choosen to make connected graph. For not dense graph is simpler.
             * This gives 1/2 chance of add a random edge.
             */
            var edges = new HashSet<(int, int,bool)>(); 


            var graph = new Graph(vertex_count);

            //  make graph connected
            var permutation = new List<int>();
            for (int i = 0; i < vertex_count; ++i)
                permutation.Add(i);
            for (int i=1;i<vertex_count;++i)
            {
                int i1 = rand_quick.Next(0, i-1);
                int i2 = rand_quick.Next(i, vertex_count - 1);
                edges.Add((i1, i2, true));
                edges.Add((i2, i1, true));
                int i3 = permutation[i1];
                permutation[i1] = permutation[i2];
                permutation[i2] = i3;
            }

            if (edge_count > vertex_count*(vertex_count-1)/4)
            {
                for (int i = vertex_count - 1; i < vertex_count*(vertex_count-1)/2-edge_count;++i)
                {
                    int i1 = rand_quick.Next(0, vertex_count - 1);
                    int i2 = rand_quick.Next(0, vertex_count - 1);
                    if (i2 < i1) { int i3 = i1; i1 = i2; i2 = i3; }
                    if (i1 != i2 && !edges.Contains((i1, i2, true)) && !edges.Contains((i1,i2 ,false)))
                        edges.Add((i1, i2, false));
                }
                for (int i1 = 0; i1 < vertex_count; ++i1)
                    for (int i2 = i1+1; i2 < vertex_count; ++i2)
                        if (edges.Contains((i1, i2,true)) || !edges.Contains((i1, i2, false)))
                            graph.Append((ushort)i1,(ushort) i2);
            }
            else
            {
                for (int i = vertex_count - 1; i < edge_count; ++i)
                {
                    int i1 = rand_quick.Next(0, vertex_count - 1);
                    int i2 = rand_quick.Next(0, vertex_count - 1);
                    if (i2<i1)  { int i3 = i1;i1 = i2;i2 = i3; }
                    if (i1 != i2 && !edges.Contains((i1,i2, true)))
                        edges.Add((i1, i2, true));
                }

                foreach (var edge in edges)
                    graph.Append((ushort)edge.Item1, (ushort)edge.Item2);

                if (!graph.Hash())
                    Abort("Cannot make short hash");
            }

            return graph;
        }        

        List<ushort> Permutate(int vertex_count)
        {
            var permutation = new List<ushort>(vertex_count);
            for (ushort i = 0; i < vertex_count; ++i)
                permutation[i] = i;
            for (int repeat=2*vertex_count;repeat>0;--repeat)
            {
                int i1 = rand_quick.Next(0, vertex_count - 1);
                int i2 = rand_quick.Next(0, vertex_count - 1);
                int i3 = permutation[i1];
                permutation[i1] = permutation[i2];
                permutation[i2] = (ushort)i3;
            }

            return permutation;
        }

        public bool Compare(Graph some_graph, Graph other_graph, List<ushort> permutation)
        {
            if (some_graph.Count != other_graph.Count)
                return false;
            for (int i = 0; i < some_graph.Count; ++i)
                if (some_graph[i].Count() != other_graph[i].Count())
                    return false;

            for (ushort i1 = 0; i1 < some_graph.Count; ++i1)
                for (var i2 = some_graph[i1].First(); i2 != GraphVertex.None; i2 = some_graph[i1].Next(i2))
                    if (!other_graph.Find(permutation[i1], permutation[i2]))
                        return false;

            return true;
        }

        public Graph Permutate(Graph some_graph, List<ushort> permutation)
        {
            var perm_graph = new Graph(some_graph.Count);
            for (ushort i1 = 0; i1 < some_graph.Count; ++i1)
                for (ushort i2 = some_graph[i1].First(); i2 != GraphVertex.None; i2 = some_graph[i1].Next(i2))
                    if (permutation[i1] < permutation[i2])
                        perm_graph.Append(permutation[i1], permutation[i2]);
            return perm_graph;
        }
    }

    class GraphIso
    {
        const int None = GraphVertex.None;
        struct VertexData
        {
            public int match_vertex;
            public int border_edges;
        }
        struct EdgeData
        {
            public int inside_vertex;
            public int outside_vertex;
            public int matched_vertex;
        }
        VertexData[] vertex_array;
        EdgeData[] edge_stack;
        int vertex_len;
        int edge_top;
        Graph some_graph;
        Graph other_graph;

        public List<int>TryIso(Graph some_graph, Graph other_graph)
        {
            
        }

        void NextSomeVertex()
        {
            int outside_vertex = -1;
            int inside_vertex = -1;
            int border_edges = 0;
            for (int i = 0; i < vertex_len; ++i)
                if (vertex_array[i].match_vertex == None && vertex_array[i].border_edges > border_edges)
                    inside_vertex = i;

            border_edges = some_graph.Count;
            for (int i = some_graph[inside_vertex].First(); i != None; i = some_graph[inside_vertex].Next(i))
                if (vertex_array[i].border_edges < border_edges)
                    outside_vertex = i;

            for (int i = some_graph[outside_vertex].First(); i != None; i = some_graph[outside_vertex].Next(i))
                vertex_array[i].border_edges += vertex_array[i].match_vertex == None ? 1 : -1;

            ++edge_top;
            edge_stack[edge_top] = new EdgeData { outside_vertex = outside_vertex, inside_vertex = inside_vertex, matched_vertex = None };
        }

        bool TryIso()
        {
            while (true)
            {
                if (!NextSomeVertex())
                    return true;

                while (true)
                {
                    if (NextOtherVertex())
                        return true;
                    if (--edge_top == 0)
                        return false;
                }
            }
        }

        bool NextOtherVertex()
        {
            var edge_data = edge_stack[edge_top];
            while (true)
            {
                edge_data.matched_vertex = other_graph[vertex_array[edge_data.inside_vertex]].Next(edge_data.matched_vertex);
                if (edge_data.matched_vertex == None)
                    return false;

                if (other_graph[edge_data.matched_vertex].Count != some_graph[edge_data.outside_vertex].Count)
                    continue;

                for (int i = some_graph[edge_data.outside_vertex].First(); i != None; i = some_graph[edge_data.outside_vertex].Next(i))
                    if (vertex_array[i].match_vertex != None && !other_graph.Find(vertex_array[i].match_vertex, edge_data.matched_vertex))
                        continue;

                return true;
            }
        }
    }
}