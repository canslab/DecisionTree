using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace dt
{
    /// <summary>
    /// 
    /// JUtility is the collection of algorithms related to decision tree making
    /// 
    /// 
    /// </summary>
    class JUtility
    {
        /// <summary>
        /// 
        /// read training set file 
        /// 
        /// 
        /// </summary>
        /// <param name="inputFileName"> the list that will contain the attributes' names </param>
        /// <param name="attributeList"> the list of attributes </param>
        /// <param name="D"> the dataSet that will contain tuples </param>
        /// <returns></returns>
        public static bool readFile(string inputFileName, List<string> attributeList, JDataSet D)
        {
            bool bReadWell = false;

            // open training set file 
            StreamReader sr = new StreamReader(new FileStream("dt_train.txt", FileMode.Open));

            // first get the attribute names and save them to attribute list
            string line = sr.ReadLine();

            // every column names are seperated with '\t' so delimeter is '\t'
            string[] namesOfAttributeAndClass = line.Split('\t');
            
            // 0 to (# of column - 1) because the last one is class label.
            for(int i = 0; i < namesOfAttributeAndClass.Length - 1; ++i)
            {
                attributeList.Add(namesOfAttributeAndClass[i]);
            }
           
            // read each line and also each one is a tuple 
            while (!sr.EndOfStream)
            {
                string eachLine = sr.ReadLine();
                // each attribute values are separated with '\t'
                string[] attrValues = eachLine.Split('\t');

                JTuple eachTuple = new JTuple();

                // save values to eachTuple(JTuple)
                for (int i = 0; i < attrValues.Length ; ++i)
                {
                    if ( i != attrValues.Length - 1 )
                    {
                        eachTuple.setAttrAndItsValue(namesOfAttributeAndClass[i], attrValues[i]);
                    }
                    else
                    {
                        eachTuple.ClassLabel = attrValues[i];
                    }
                }
                D.insertTuple(eachTuple);
            }

            return bReadWell;
        }

        /// <summary>
        /// get the entropy value from D
        /// 
        /// </summary>
        /// <param name="D"> target DataSet </param>
        /// <returns> entropy value </returns>
        public static double getEntropy(JDataSet D)
        {
            Dictionary<string, int> labelCount = new Dictionary<string, int>();
        
            // iterate through the tuples and count the classification label
            // examples
            //         Age   Income  Student     ClassLabel
            //         '<=30' 'xx'    'xx'         'yes'
            //         '<=30' 'xx'    'xx'         'no'
            //         '<=30' 'xx'    'xx'         'yes'
            // 
            //   labelCount['yes'] == 2
            //   labelCount['no'] == 1
            foreach(JTuple eachTuple in D.Tuples)
            {
                if (!labelCount.ContainsKey(eachTuple.ClassLabel))
                {
                    labelCount[eachTuple.ClassLabel] = 0;
                }
                labelCount[eachTuple.ClassLabel]++;
            }

            var keys = labelCount.Keys;
            double sum = 0;

            foreach (string eachClassLabel in keys)
            {
                double prob = (double)labelCount[eachClassLabel] / D.TuplesCount;
                 sum += (Math.Log(prob, 2) * prob);
                
            }
            sum = -sum;

            return sum;
        }
        public static string selectAttributeUsingIG(JDataSet D, List<string> attributeList)                                    
        {
            int totalCount = D.TuplesCount;
            double minEntropy = Double.MaxValue;
            string retAttributeName = "";

            // ex.) attrName = "age"
            foreach(string attrName in attributeList)
            {
                Dictionary<string, JDataSet> dataPartiotions = new Dictionary<string, JDataSet>();

                foreach (JTuple eachTuple in D.Tuples)
                {
                    // ex.) attrValueOfEachTuple == ">=30" 
                    string attrValueOfEachTuple = eachTuple.getAttrValue(attrName);

                    // if there is no dataset partition
                    // allocate new JDataSet 
                    if (!dataPartiotions.ContainsKey(attrValueOfEachTuple))
                    {
                        dataPartiotions[attrValueOfEachTuple] = new JDataSet();
                    }

                    dataPartiotions[attrValueOfEachTuple].insertTuple(eachTuple);        
                }

                var keys = dataPartiotions.Keys;
               
                double weightedSum = 0.0;

                foreach (string eachAttributeValue in keys)
                {
                    weightedSum += ( (double)dataPartiotions[eachAttributeValue].TuplesCount / totalCount ) * getEntropy(dataPartiotions[eachAttributeValue]);    
                    //dataPartiotions[eachAttributeValue].getProbability();
                }
                
                if (weightedSum <= minEntropy)
                {
                    minEntropy = weightedSum;
                    retAttributeName = attrName;
                }
            }

            return retAttributeName;
        }

        /// <summary>
        /// It creates decision tree and return its reference 
        /// 
        /// </summary>
        /// <param name="D">D is the dataSet</param>
        /// <param name="attributeList"> Attribute List, it used to branch nodes </param>
        /// <returns> Refrence of tree node </returns>
        public static JTreeNode generateDecisionTree(JDataSet D, List<string> attributeList)
                                                    
        {
            JTreeNode retTreeNode = null;

            // termination condition 1
            if (D.TuplesCount == 0) // if there is no tuple, return null ! 
            {
                return retTreeNode;
            }

            // termination condition 2
            if (D.SameClass) // if all the tuples are same class
            {
                retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.Tuples[0].ClassLabel);
                return retTreeNode;
            }

            // termination condition 3
            else if (attributeList.Count == 0)  // majority voting
            {
                retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.MajorityClass);
                return retTreeNode;
            }

            // select the attribute that classifies tuples as pure as possible 
            string splittingAttribute = selectAttributeUsingIG(D, attributeList);

            // make a test node (attribute check node)
            retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.TEST, splittingAttribute);
            
            // attribute_list <- attribute_list - splitting_attribute
            attributeList.Remove(splittingAttribute);

            // it is used to store 
            // ex.) '<=30', D1(=t1+t2+t3)
            Dictionary<string, JDataSet> branches = new Dictionary<string, JDataSet>();

            foreach(JTuple eachTuple in D.Tuples)
            {
                // ex.) splittingAttribute == "age"
                // ex.) branchName == "<=30"
                string branchName = eachTuple.getAttrValue(splittingAttribute);
                if (!branches.ContainsKey(branchName))
                {
                    branches[branchName] = new JDataSet();
                }
                branches[branchName].insertTuple(eachTuple);
            }
            
            var branchesNames = branches.Keys;

            foreach(string eachBranchName in branchesNames)
            {
                retTreeNode.PathDirectory[eachBranchName] = generateDecisionTree(branches[eachBranchName], attributeList);
            }
            

            return retTreeNode;
        }
    }

}
