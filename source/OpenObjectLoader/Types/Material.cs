using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader.Types
{
    public class Material
    {
        public String Name { get; set; }
        public String TexturePath { get; set; }
        public List<Definition> Definitions { get; set; }

        public Material()
        {
            this.Definitions = new List<Definition>();
        }
    }
}
