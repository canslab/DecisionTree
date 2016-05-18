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
        /************************************************************************/
        /***********        Constructors                                *********/
        /************************************************************************/
        public JTuple()
        {
            // instantiate dictionary ( which contains every attribute name and its corresponding value) 
            mAttrAndValue = new Dictionary<string, string>();

            // normal value of finalClass starts from 1 to (numberOfClass)
            ClassLabel = null;
        }
        public JTuple(JTuple srcTuple)      // copy constructor
        {
            mAttrAndValue = new Dictionary<string, string>(srcTuple.mAttrAndValue);
            ClassLabel = srcTuple.ClassLabel;
        }

        /************************************************************************/
        /***********        Properties                                  *********/
        /************************************************************************/
        public string ClassLabel        // this indicates the classification result
        {
            get; set;
        }

        /// <summary>
        /// this is used to know the value of the corresponding attribute name 
        /// </summary>
        /// <param name="arg"> attribute name </param>
        /// <returns> corresponding value </returns>
        public string this[string arg]
        {
            get
            {
                if(mAttrAndValue.ContainsKey(arg) == true)
                {
                    return mAttrAndValue[arg];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mAttrAndValue[arg] = value;
            }
        }

        /************************************************************************/
        /***********        Private member Variables                    *********/
        /************************************************************************/
        private Dictionary<string, string> mAttrAndValue;   // this member variable is encapsulated using above indexer.
    }
}
