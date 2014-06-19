using OpenObjectLoader.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader.Loader
{
    public class WavefrontLoader
    {
        public String Path { get; set; }
        public String MaterialPath { get; set; }
        public List<Material> Materials { get; set; }
        public List<Vector3> Vetricies { get; set; }
        public List<Vector3> Textures { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<String> Lines { get; set; }
        public List<String> MaterialLines { get; set; }
        public String Document { get; set; }

        public WavefrontLoader()
        {
            this.Materials = new List<Material>();
            this.Vetricies = new List<Vector3>();
            this.Textures = new List<Vector3>();
            this.Normals = new List<Vector3>();
            this.Lines = new List<string>();
            this.MaterialLines = new List<string>();
        }

        public void ReadFile(String path)
        {
            if (new System.IO.FileInfo(path).Extension.Equals(".obj"))
            {
                // Clear the lists
                this.Materials.Clear();
                this.Vetricies.Clear();
                this.Textures.Clear();
                this.Normals.Clear();
                this.Lines.Clear();
                this.MaterialLines.Clear();

                // adds the path
                this.Path = path;

                // Parsing Stepp 1, parse the path to the material lib, v, vt, vn and material names
                StringBuilder builder = new StringBuilder();
                System.IO.StreamReader reader = new System.IO.StreamReader(this.Path);
                String line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("mtllib"))
                    {
                        ParseMtLib(line);
                    }
                    else if (line.StartsWith("v "))
                    {
                        ParseVetricies(line);
                    }
                    else if (line.StartsWith("vt"))
                    {
                        ParseTextures(line);
                    }
                    else if (line.StartsWith("vn"))
                    {
                        ParseNormals(line);
                    }
                    else if (line.StartsWith("usemtl"))
                    {
                        ParseMaterialNames(line);
                    }
                    this.Lines.Add(line);
                    builder.AppendLine(line);
                }
                this.Document = builder.ToString();

                //Parsing Stepp 2, parse the material definitions
                for (int i = 0 ; i < this.Lines.Count ; i++)
                {
                    String ln = this.Lines[i];
                    if (ln.StartsWith("usemtl"))
                    {
                        ParseMaterialDefinitions(ln.Split(' ')[1], i);
                    }
                }

                // Parsing Stepp 3, parse the material Textures TODO !!!!!
                System.IO.StreamReader matreader = new System.IO.StreamReader(this.MaterialPath);
                String matline = null;
                while ((matline = matreader.ReadLine()) != null)
                {
                    this.MaterialLines.Add(matline);
                }

                for (int i = 0; i < this.MaterialLines.Count; i++)
                {
                    String ln = this.MaterialLines[i];
                    if (ln.StartsWith("newmtl "))
                    {
                        ParseMaterialTexture(ln.Split(' ')[1], i);
                    }
                }

            }
        }

        /// <summary>
        /// Parse the Material Lib path
        /// </summary>
        /// <param name="line"></param>
        private void ParseMtLib(String line)
        {
            this.MaterialPath = new System.IO.FileInfo(this.Path).DirectoryName + "\\" + line.Split(' ')[1];
        }

        /// <summary>
        /// Parse the Vetricies
        /// </summary>
        /// <param name="line"></param>
        private void ParseVetricies(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            float z = float.Parse(line.Split(' ')[3], CultureInfo.InvariantCulture);
            this.Vetricies.Add(new Vector3(x, y, z));
        }

        /// <summary>
        /// Parse the Texture
        /// </summary>
        /// <param name="line"></param>
        private void ParseTextures(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            this.Textures.Add(new Vector3(x,y,0f));
        }

        /// <summary>
        /// Parse the normals
        /// </summary>
        /// <param name="line"></param>
        private void ParseNormals(String line)
        {
            float x = float.Parse(line.Split(' ')[1], CultureInfo.InvariantCulture);
            float y = float.Parse(line.Split(' ')[2], CultureInfo.InvariantCulture);
            float z = float.Parse(line.Split(' ')[3], CultureInfo.InvariantCulture);
            this.Normals.Add(new Vector3(x, y, z));
        }

        /// <summary>
        /// Parse the material names
        /// </summary>
        /// <param name="line"></param>
        private void ParseMaterialNames(String line)
        {
            Material material = new Material();
            material.Name = line.Split(' ')[1];
            this.Materials.Add(material);
        }

        /// <summary>
        /// Gets the material with the name x
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        private Material getMaterial(String Name)
        {
            foreach (Material mat in this.Materials)
            {
                if (mat.Name.Equals(Name))
                {
                    return mat;
                }
            }
            return null;
        }

        /// <summary>
        /// Parse the Material Definitions
        /// </summary>
        /// <param name="MaterialName"></param>
        /// <param name="line"></param>
        private void ParseMaterialDefinitions(String MaterialName, int line)
        {
            Material mat = this.getMaterial(MaterialName);
            if (mat != null)
            {
                int start = line + 1;
                for (int i = start; i < this.Lines.Count; i++)
                {
                    String ln = this.Lines[i];
                    if (ln.StartsWith("f "))
                    {
                        mat.Definitions.Add(new Definition(ln.Split(' ')[1]));
                        mat.Definitions.Add(new Definition(ln.Split(' ')[2]));
                        mat.Definitions.Add(new Definition(ln.Split(' ')[3]));
                    }
                    else if (ln.StartsWith("usemtl"))
                    {
                        break;
                    }
                }               
            }
        }

        /// <summary>
        /// Parse the texturepath from the material
        /// </summary>
        /// <param name="MaterialName"></param>
        /// <param name="line"></param>
        private void ParseMaterialTexture(String MaterialName, int line)
        {
            Material mat = this.getMaterial(MaterialName);
            if (mat != null)
            {
                int start = line + 1;
                for (int i = start; i < this.MaterialLines.Count; i++)
                {
                    if (this.MaterialLines[i].StartsWith("map_Kd"))
                    {
                        mat.TexturePath = this.MaterialLines[i].Split(' ')[1];
                        break;
                    }
                }
            }
        }


    }
}
