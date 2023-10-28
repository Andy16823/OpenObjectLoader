using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    /// <summary>
    /// This Class contains the face deffinitions
    /// </summary>
    public class Definition
    {
        /// <summary>
        /// The pointer to the vertex index
        /// </summary>
        public int v { get; set; }

        /// <summary>
        /// The pointer to the texture index
        /// </summary>
        public int vt { get; set; }

        /// <summary>
        /// The pointer to the normal index
        /// </summary>
        public int vn { get; set; }

        /// <summary>
        /// Create a new deffinition
        /// </summary>
        public Definition()
        {
            this.v = 0;
            this.vt = 0;
            this.vn = 0;
        }

        /// <summary>
        /// Create a new deffinition
        /// </summary>
        public Definition(int v, int vt, int vn)
        {
            this.v = v;
            this.vt = vt;
            this.vn = vn;
        }

        /// <summary>
        /// Create a new deffinition
        /// </summary>
        public Definition(String tokkeValue)
        {
            String[] tokkens = tokkeValue.Split('/');
            this.v = int.Parse(tokkens[0]);
            this.vt = int.Parse(tokkens[1]);
            this.vn = int.Parse(tokkens[2]);
        }


    }
}
