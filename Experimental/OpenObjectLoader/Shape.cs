using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    public class Shape
    {
        public String Name { get; set; }
        public List<int> Materials { get; set; }
        public Dictionary<String, object> Propertys { get; set; }

        private Model _parent;

        public Shape(String name, Model parent) {
            this.Name = name;
            this.Propertys = new Dictionary<String, object>();
            this.Materials = new List<int>();
            this._parent = parent;
        }

    }
}
