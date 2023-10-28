using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    /// <summary>
    /// The Loader Class.
    /// </summary>
    public class WavefrontLoader
    {
        /// <summary>
        /// Load the model from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Model LoadModel(String path)
        {
            Model model = new Model();
            Shape activeShape = null;
            Material _activeMaterial = null;

            String[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if(line.StartsWith("mtllib"))
                {
                    model.MaterialLibary = this.GetMaterialLibPath(path, line);
                }
                else if(line.StartsWith("o"))
                {
                    //Create a new model and set the active model
                    Shape shape = new Shape(line.Replace("o ", ""), model);
                    model.Shapes.Add(shape);
                    activeShape = shape;
                }
                else if(line.StartsWith("vn"))
                {
                    model.Normals.Add(this.ParseVector3(line));
                }
                else if(line.StartsWith("vt"))
                {
                    model.TexCoords.Add(this.ParseVector2(line));
                }
                else if (line.StartsWith("v"))
                {
                    model.Verticies.Add(this.ParseVector3(line));
                }
                else if(line.StartsWith("usemtl"))
                {
                    String name = line.Replace("usemtl ", "");
                    Material material = model.GetMaterial(name);
                    if (material == null)
                    {
                        material = new Material(model);
                        material.Name = line.Replace("usemtl ", "");
                        this.LoadMaterial(material, model.MaterialLibary);
                        model.Materials.Add(material);
                    }
                    activeShape.Materials.Add(model.Materials.IndexOf(material));
                    _activeMaterial = material;
                }
                else if(line.StartsWith("f"))
                {
                    _activeMaterial.Definitions.Add(new Definition(line.Split(' ')[1]));
                    _activeMaterial.Definitions.Add(new Definition(line.Split(' ')[2]));
                    _activeMaterial.Definitions.Add(new Definition(line.Split(' ')[3]));
                }
            }
            return model;
        }

        /// <summary>
        /// Load the material data from the given file
        /// </summary>
        /// <param name="material">The material to get loaded</param>
        /// <param name="file">The file with the material data</param>
        public void LoadMaterial(Material material, String file)
        {
            bool materialFound = false;
            String[] lines = System.IO.File.ReadAllLines(file);
            foreach (String line in lines)
            {
                if(line.StartsWith("newmtl"))
                {
                    if(line.Replace("newmtl ", "").Equals(material.Name) && materialFound == false)
                    {
                        materialFound = true;
                    }
                    else if(materialFound) {
                        return;
                    }
                }
                else if(line.StartsWith("map_Kd") && materialFound)
                {
                    material.TexturePath = line.Replace("map_Kd ", "");
                }
                else if(line.StartsWith("map_Disp") && materialFound)
                {
                    material.NormalPath = line.Replace("map_Disp ", "");
                }
                else if (line.StartsWith("map_Ka") && materialFound)
                {
                    material.AmbientOcclusionPath = line.Replace("map_Ka ", "");
                }
            }
        }

        /// <summary>
        /// Parse the Material Lib path
        /// </summary>
        /// <param name="line"></param>
        private String GetMaterialLibPath(String path, String line)
        {
            return new System.IO.FileInfo(path).DirectoryName + "\\" + line.Split(' ')[1];
        }

        /// <summary>
        /// Parse a vector3
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Vector3 ParseVector3(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            float z = float.Parse(line.Split(' ')[3], CultureInfo.InvariantCulture);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Parse a vector2
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Vector3 ParseVector2(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            return new Vector3(x, y, 0);
        }
    }
}
