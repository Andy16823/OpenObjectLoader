using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    /// <summary>
    /// Material
    /// </summary>
    public class Material
    {
        /// <summary>
        /// The Material Name
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// The path to the diffuse texture
        /// </summary>
        public String TexturePath { get; set; }
        /// <summary>
        /// The path to the normal map
        /// </summary>
        public String NormalPath { get; set; }
        /// <summary>
        /// The path to the ambient occlusion map
        /// </summary>
        public String AmbientOcclusionPath { get; set; }
        /// <summary>
        /// Propeterys dictonary for own parameters.
        /// </summary>
        public Dictionary<String, object> Propeterys { get; set; }
        /// <summary>
        /// A list with face definitions
        /// </summary>
        public List<Definition> Definitions { get; set; }

        /// <summary>
        /// The parent model
        /// </summary>
        private Model _model;

        /// <summary>
        /// Create a new material for the model
        /// </summary>
        /// <param name="model"></param>
        public Material(Model model)
        {
            this._model = model;
            this.Definitions = new List<Definition>(); 
            this.Propeterys = new Dictionary<String, object>();
        }

        /// <summary>
        /// Returns an array with the indexed verticies
        /// </summary>
        /// <returns></returns>
        public float[] IndexVerticies()
        {
            return Material.IndexVerticies(_model, this);
        }

        /// <summary>
        /// Returns an array with the indexed texture coords
        /// </summary>
        /// <returns></returns>
        public float[] IndexTexCoords()
        {
            return Material.IndexTexCoords(_model, this);
        }

        /// <summary>
        /// Returns an array with the indexed normal coords
        /// </summary>
        /// <returns></returns>
        public float[] IndexNormals()
        {
            return Material.IndexNormals(_model, this);
        }

        /// <summary>
        /// Static function to index the vericies
        /// </summary>
        /// <param name="model"></param>
        /// <param name="material"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Static function to index the texture coords
        /// </summary>
        /// <param name="model"></param>
        /// <param name="material"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Static function to index the normal coords
        /// </summary>
        /// <param name="model"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static float[] IndexNormals(Model model, Material material)
        {
            List<float> normals = new List<float>();
            foreach (var face in material.Definitions)
            {
                normals.Add(model.Normals[face.vn - 1].x);
                normals.Add(model.Normals[face.vn - 1].y);
                normals.Add(model.Normals[face.vn - 1].z);
            }
            return normals.ToArray();
        }
    }
}
