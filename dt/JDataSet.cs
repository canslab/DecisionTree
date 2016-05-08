using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
   
    /// <summary>
    /// JDataSet is a collection of tuples 
    /// </summary>
    class JDataSet
    {
        public JDataSet()
        {
            Tuples = new List<JTuple>();
            mMajorityClass = null;
        }
        public JDataSet(List<JTuple> srcTuples)
        {
            Tuples = srcTuples;
            mMajorityClass = null;
        }
        public bool SameClass
        {
            get
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
        }

        // get # of tuples in this JDataSet
        public int TuplesCount
        {
            get
            {
                return Tuples.Count;
            }            
        }
        public string MajorityClass
        {
            get
            {
                if (mMajorityClass != null)
                {
                    return mMajorityClass;
                }
                
                Dictionary<string, int> record = new Dictionary<string, int>();

                foreach(JTuple eachTuple in Tuples)
                {
                    if (!record.ContainsKey(eachTuple.ClassLabel))
                    {
                        record[eachTuple.ClassLabel] = 0;
                    }
                    record[eachTuple.ClassLabel]++;
                }

                var keys = record.Keys;
                int retMax = 0;

                foreach(string eachKey in keys)
                {
                    if (retMax < record[eachKey])
                    {
                        retMax = record[eachKey];
                        mMajorityClass = eachKey;
                    }
                }
                return mMajorityClass;
            }
        }

        private string mMajorityClass;
        public double getProbability(string classLabel)
        {
            int count = 0;
            if (this.SameClass)
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
    }
}
