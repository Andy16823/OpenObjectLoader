using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    public class WavefrontLoader
    {
        public WavefrontLoader()
        {

        }

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
                    Material material = new Material(model);
                    material.Name = line.Replace("usemtl ", "");
                    activeShape.Materials.Add(material);
                    _activeMaterial = material;
                    this.LoadMaterial(material, model.MaterialLibary);
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

        private Vector3 ParseVector3(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            float z = float.Parse(line.Split(' ')[3], CultureInfo.InvariantCulture);
            return new Vector3(x, y, z);
        }

        private Vector3 ParseVector2(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            return new Vector3(x, y, 0);
        }
    }
}
