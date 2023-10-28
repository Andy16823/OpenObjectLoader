using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    public class Definition
    {
        public int v { get; set; }
        public int vt { get; set; }
        public int vn { get; set; }

        public Definition()
        {
            this.v = 0;
            this.vt = 0;
            this.vn = 0;
        }

        public Definition(int v, int vt, int vn)
        {
            this.v = v;
            this.vt = vt;
            this.vn = vn;
        }

        public Definition(String tokkeValue)
        {
            String[] tokkens = tokkeValue.Split('/');
            this.v = int.Parse(tokkens[0]);
            this.vt = int.Parse(tokkens[1]);
            this.vn = int.Parse(tokkens[2]);
        }


    }
}
