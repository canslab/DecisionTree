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
            this.mValue = value;
            this.TuplesCount = 0;
        }
        public JTreeNode()
        {
            this.mNodeType = JTreeNodeType.NOTYET;
            this.mValue = null;
            this.TuplesCount = 0;
        }

        private Dictionary<string, JTreeNode> mPathDirectory = new Dictionary<string, JTreeNode>();

        public JTreeNode this[string outcome]
        {
            get
            {
                if (mPathDirectory.ContainsKey(outcome))
                {
                    return mPathDirectory[outcome];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mPathDirectory[outcome] = value;
            }
        }

        public JTreeNodeType NodeType
        {
            get
            {
                return mNodeType;
            }
        }

        // when the type of tree node is "RESULT" --> indicator means "CLASS LABEL"
        // when the type of tree node is "TEST" --> indicator means "attribute name"
        public string Indicator
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
