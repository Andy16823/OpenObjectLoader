using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenObjectLoader
{
    /// <summary>
    /// A simple storage for coordiantes
    /// </summary>
    public class Vector3
    {
        /// <summary>
        /// The x value
        /// </summary>
        public float x { get; set; }

        /// <summary>
        /// The y value
        /// </summary>
        public float y { get; set; }

        /// <summary>
        /// the z value
        /// </summary>
        public float z { get; set; }

        /// <summary>
        /// Create a new empty vector
        /// </summary>
        public Vector3()
        {
            this.x = 0f;
            this.y = 0f;
            this.z = 0f;
        }

        /// <summary>
        /// Create a new vector with the given values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
