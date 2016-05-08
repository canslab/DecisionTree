using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
    /** 
     * It is the collection of tuples 
     */

    class JDataSet
    {
        public JDataSet()
        {
            mMajorityClass = 0;
            Tuples = new List<JTuple>();
        }
        public JDataSet(List<JTuple> srcTuples)
        {
            Tuples = srcTuples;
        }
        public bool bSameClass()
        {
            string firstClass;
            if (Tuples.Count == 0)
            {
                return true;
            }
            else
            {
                firstClass = Tuples[0].ClassLabel;
            }
            foreach (JTuple eachTuple in Tuples)
            {
                if (firstClass != eachTuple.ClassLabel)
                {
                    return false;
                }
            }
            return true;
        }

        // get # of tuples in this JDataSet
        public int TuplesCount
        {
            get
            {
                return Tuples.Count;
            }            
        }

        public double getProbability(string classLabel)
        {
            int count = 0;
            if (this.bSameClass())
            {
                return 0;
            }

            foreach(JTuple eachTuple in Tuples)
            {
                if (classLabel == eachTuple.ClassLabel)
                {
                    count++;
                }    
            }

            return ((double)count / Tuples.Count);
        }

        public void insertTuple(JTuple tuple)
        {
            
            if (tuple != null)
            {
                Tuples.Add(tuple);
            }
        }

        public List<JTuple> Tuples
        {
            get;
        }
        private int mMajorityClass;
    }
}
