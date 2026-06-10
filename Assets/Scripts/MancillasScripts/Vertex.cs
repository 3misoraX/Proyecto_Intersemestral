using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
        // nombre del vertice
        public string Name {get; set;}
        // valor del vertice actual
        public int Valeu {get; set;} 
        // lista de aristas conectadas
        public List<Vertex> Edges {get;set;} 
        // guardamos un padre para tener mas manejo durante la ejecucion
        public Vertex ParentVertex {get;set;}
        // constructor
        public  Vertex(int newValeu = 0, string newName = "null Node", Vertex newParent = null) 
        {
                this.Valeu = newValeu;
                this.Name = newName;
                this.ParentVertex = newParent;
                Edges = new List<Vertex>();
        }
}