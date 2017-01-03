using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{

    /// <summary>
    /// Simple class to store the definition of an indexed node
    /// </summary>
    public class IndexedNode
    {
        public int NodeId { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Overridden to base equality on NodeId
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return ((IndexedNode)obj).NodeId == this.NodeId;
        }

        /// <summary>
        /// Overridden to base equality on NodeId
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.NodeId.GetHashCode();
        }
    }
}
