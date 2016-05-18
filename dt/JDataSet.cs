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
        /************************************************************************/
        /***********        Public Methods                              *********/
        /************************************************************************/
        public double GetProbability(string classLabel)
        {
            int count = 0;
            if (this.SameClass)
            {
                return 0;
            }

            foreach (JTuple eachTuple in TuplesList)
            {
                if (classLabel == eachTuple.ClassLabel)
                {
                    count++;
                }
            }

            return ((double)count / TuplesList.Count);
        }
        public void InsertTuple(JTuple tuple)
        {

            if (tuple != null)
            {
                TuplesList.Add(tuple);
            }
        }

        /************************************************************************/
        /***********        Constructors                                *********/
        /************************************************************************/
        public JDataSet()
        {
            mTupleList = new List<JTuple>();
        }
        public JDataSet(List<JTuple> srcTuples)
        {
            mTupleList = new List<JTuple>(srcTuples);
        }
        public JDataSet(JDataSet sourceDataSet)         // copy constructor
        {
            mTupleList = new List<JTuple>(sourceDataSet.mTupleList);
        }

        /************************************************************************/
        /************           Properites                              *********/
        /************************************************************************/
        public JTuple this[int index]
        {
            get
            {
                return mTupleList[index];
            }
        }
        public bool SameClass
        {
            get
            {
                string firstClass;
                if (TuplesList.Count == 0)
                {
                    return true;
                }
                else
                {
                    firstClass = TuplesList[0].ClassLabel;
                }
                foreach (JTuple eachTuple in TuplesList)
                {
                    if (firstClass != eachTuple.ClassLabel)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public int TuplesCount
        {
            get
            {
                return TuplesList.Count;
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

                foreach(JTuple eachTuple in TuplesList)
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
        public List<JTuple> TuplesList
        {
            get
            {
                return mTupleList;
            }
        }

        /*************************************************************************/
        /**************         Member variables                            ******/
        /*************************************************************************/
        private string mMajorityClass = null;
        private List<JTuple> mTupleList = null;
    }
}
