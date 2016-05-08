using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
    class JTreeNode
    {
        public enum JTreeNodeType { NOTYET, TEST, RESULT };

        public JTreeNode(JTreeNodeType type, string value)
        {
            this.mNodeType = type;
            PathDirectory = new Dictionary<string, JTreeNode>();
            this.value = value;
        }
        public JTreeNode()
        {
            this.mNodeType = JTreeNodeType.NOTYET;
            this.value = null;
            PathDirectory  = new Dictionary<string, JTreeNode>();
        }

        public Dictionary<string, JTreeNode> PathDirectory
        {
            get;set;
        }

        private string value;
        private JTreeNodeType mNodeType;          
    }
}
