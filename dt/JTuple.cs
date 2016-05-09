using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
    /*
     * This class is equivalent to an tuple  
     * 
     * So it contains 2 elements 
     * 
     * 1) attributes and its corresponding values
     * 2) class label (= classification result ) 
     * 
     */

    class JTuple
    {
    
        // constructor     
        public JTuple()
        {
            // instantiate dictionary ( which contains all column name and its corresponding value) 
            mAttrAndValue = new Dictionary<string, string>();

            // normal value of finalClass starts from 1 to (numberOfClass)
            ClassLabel = null;
        }

        public string ClassLabel
        {
            get; set;
        }
         // this indicates the classification result

        public void setAttrAndItsValue(string attrName, string attrValue)
        {
            mAttrAndValue[attrName] = attrValue;
        }

        public string getAttrValue(string attrName)
        {
            return mAttrAndValue[attrName];
        }
       
        
        private Dictionary<string, string> mAttrAndValue;
    }
}
