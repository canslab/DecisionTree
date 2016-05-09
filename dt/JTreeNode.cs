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
            this.mValue = value;
            this.TuplesCount = 0;
        }
        public JTreeNode()
        {
            this.mNodeType = JTreeNodeType.NOTYET;
            this.mValue = null;
            PathDirectory  = new Dictionary<string, JTreeNode>();
            this.TuplesCount = 0;
        }

        public Dictionary<string, JTreeNode> PathDirectory
        {
            get;set;
        }

        public JTreeNodeType NodeType
        {
            get
            {
                return mNodeType;
            }
        }
        public string Value
        {
            get
            {
                return mValue;
            }
        }
        public int TuplesCount
        {
            get;set;
        }

        private string mValue;
        private JTreeNodeType mNodeType;          
    }
}
