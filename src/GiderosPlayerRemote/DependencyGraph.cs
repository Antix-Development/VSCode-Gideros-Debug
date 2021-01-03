using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiderosPlayerRemote
{
    class DependencyGraph
    {
        class Vertex
        {
            public Vertex(string code, bool excludeFromExecution)
            {
                this.code = code;
                this.excludeFromExecution = excludeFromExecution;
                this.visited = false;
            }

            public string code;
            public bool excludeFromExecution;
            public bool visited;
            public HashSet<Vertex> dependencies = new HashSet<Vertex>();
        }
        Dictionary<KeyValuePair<int, string>, Vertex> vertices = new Dictionary<KeyValuePair<int, string>, Vertex>();

        public void Clear()
        {
            vertices.Clear();
        }

        public void AddCode(string code, bool excludeFromExecution)
        {
            vertices[_(code)] = new Vertex(code, excludeFromExecution);
        }

        public void AddDependency(string code0, string code1)
        {
            Vertex vertex0 = vertices.FindR(_(code0));
            Vertex vertex1 = vertices.FindR(_(code1));

            vertex0.dependencies.Add(vertex1);
        }

        public List<KeyValuePair<string, bool>> TopologicalSort()
        {
            List<KeyValuePair<string, bool>> result = new List<KeyValuePair<string, bool>>();

            foreach (var v in vertices)
                v.Value.visited = false;

            var sortedVertices = vertices
                .OrderBy(a => a.Key.Key.ToString() + a.Key.Value, StringComparer.Ordinal)
                .Select(a => a.Value);

            foreach (var v in sortedVertices)
                TopologicalSortHelper(v, result);

            return result;
        }

        void TopologicalSortHelper(Vertex vertex, List<KeyValuePair<string, bool>> result)
        {
            if (vertex.visited == true)
                return;

            vertex.visited = true;

            foreach (var iter in vertex.dependencies)
                TopologicalSortHelper(iter, result);

            result.Add(new KeyValuePair<string, bool>(vertex.code, vertex.excludeFromExecution));
        }

        KeyValuePair<int, string> _(string str)
        {
            if (str == "init.lua")
                return new KeyValuePair<int, string>(0, str);

            if (str == "main.lua")
                return new KeyValuePair<int, string>(2, str);

            return new KeyValuePair<int, string>(1, str);
        }
    }
}
