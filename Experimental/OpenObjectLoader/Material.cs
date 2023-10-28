using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    public class Material
    {
        public String Name { get; set; }
        public String TexturePath { get; set; }
        public String TextureName { get; set; }
        public Dictionary<String, object> Propeterys { get; set; }
        public List<Definition> Definitions { get; set; }

        private Model _model;

        public Material(Model model)
        {
            this._model = model;
            this.Definitions = new List<Definition>(); 
            this.Propeterys = new Dictionary<String, object>();
        }

        public float[] IndexVerticies()
        {
            return Material.IndexVerticies(_model, this);
        }

        public float[] IndexTexCoords()
        {
            return Material.IndexTexCoords(_model, this);
        }

        public static float[] IndexVerticies(Model model, Material material)
        {
            List<float> verticies = new List<float>();
            foreach (var face in material.Definitions)
            {
                verticies.Add(model.Verticies[face.v - 1].x);
                verticies.Add(model.Verticies[face.v - 1].y);
                verticies.Add(model.Verticies[face.v - 1].z);
            }
            return verticies.ToArray();
        }

        public static float[] IndexTexCoords(Model model, Material material)
        {
            List<float> texCoords = new List<float>();
            foreach (var face in material.Definitions)
            {
                texCoords.Add(model.TexCoords[face.vt - 1].x);
                texCoords.Add(model.TexCoords[face.vt - 1].y);
            }
            return texCoords.ToArray();
        }
    }
}
