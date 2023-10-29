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
        public Model Model
        {
            get => default;
            set
            {
            }
        }

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
                else if(line.StartsWith("map_Bump") && materialFound)
                {
                    if (line.Replace("map_Bump ", "").StartsWith("-bm")) {
                        String subStr = line.Replace("map_Bump -bm ", "");
                        String[] parts = subStr.Split(' ');

                        float bump_value = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        String filename = parts[1];
                        material.NormalPath = filename;
                        material.Propeterys.Add(Propetery.BUMP_STRENGTH, bump_value);
                    }
                    else
                    {
                        material.NormalPath = line.Replace("map_Bump ", "");
                    }
                }
                else if (line.StartsWith("map_Ka") && materialFound)
                {
                    material.AmbientOcclusionPath = line.Replace("map_Ka ", "");
                }
                else if(line.StartsWith("map_Pm") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_METALLIC_MAP, line.Replace("map_Pm ", ""));
                }
                else if (line.StartsWith("map_Pr") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_ROUGHNESS_MAP, line.Replace("map_Pr ", ""));
                }
                else if (line.StartsWith("Pr ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_ROUGHNESS, float.Parse(line.Replace("Pr ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("Pm ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_METALLIC, float.Parse(line.Replace("Pm ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("Ps ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_SHEEN, float.Parse(line.Replace("Ps ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("Pcr ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_CLEARCOAT_ROUGHNESS, float.Parse(line.Replace("Pcr ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("Pc ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_CLEARCOAT_THICKNESS, float.Parse(line.Replace("Pc ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("anisor ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_ANISO_ROT, float.Parse(line.Replace("anisor ", ""), CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("aniso ") && materialFound)
                {
                    material.Propeterys.Add(Propetery.PBR_ANISO, float.Parse(line.Replace("aniso ", ""), CultureInfo.InvariantCulture));
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
