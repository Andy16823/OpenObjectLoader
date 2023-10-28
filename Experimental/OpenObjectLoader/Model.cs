using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    public class Model
    {
        public String Name { get; set; }
        public List<Vector3> Verticies { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector3> TexCoords { get; set; }
        public List<Shape> Shapes { get; set; }
        public Dictionary<String, object> Propertys { get; set; }
        public List<Material> Materials { get; set; }
        public String MaterialLibary { get; set; }

        /// <summary>
        /// Create a new model
        /// </summary>
        /// <param name="name"></param>
        public Model() {
            this.Propertys = new Dictionary<String, object>();
            this.Verticies = new List<Vector3>();
            this.Normals = new List<Vector3>();
            this.TexCoords = new List<Vector3>();
            this.Shapes = new List<Shape>();
            this.Materials = new List<Material>();
        }

        /// <summary>
        /// Gets the material with the given name. If not material found it returns null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Material GetMaterial(String name)
        {
            foreach (var item in this.Materials)
            {
                if(item.Name.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }   
        
        /// <summary>
        /// Returns the index of the material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetMaterialIndex(String name) {
            int index = 0;
            foreach (var item in this.Materials)
            {
                if (item.Name.Equals(name))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

    }
}
