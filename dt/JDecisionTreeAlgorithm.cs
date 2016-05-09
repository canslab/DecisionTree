using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace dt
{
    // delegate in order to save the reference to attribute selection method 
    delegate string AttributeSelectDelegate(JDataSet D, List<string> attributeList);

    /// <summary>
    /// 
    /// JUtility is the collection of algorithms related to decision tree making
    /// 
    /// 
    /// </summary>
    class JDecisionTreeAlgorithm
    {
        /// <summary>
        /// 
        /// read training set file 
        /// 
        /// 
        /// </summary>
        /// <param name="inputFileName"> the list that will contain the attributes' names </param>
        /// <param name="attributeList"> the list of attributes </param>
        /// <param name="trainDataSet"> the dataSet that will contain tuples </param>
        /// <returns></returns>
        public static bool readFileAndMakeDataSet(string inputFileName, List<string> attributeList,
            Dictionary<string, HashSet<string>> outcomes, 
            JDataSet trainDataSet, int trainingPercent, JDataSet testDataSet)
        {
            bool bReadWell = false;

            // open training set file 
            StreamReader sr = new StreamReader(new FileStream(inputFileName, FileMode.Open));

            double a = (double)trainingPercent / 100;

            int upToThisLine = (int)Math.Ceiling(a * getTotalLineOfFile(sr));

            // first get the attribute names and save them to attribute list
            string line = sr.ReadLine();

            // every column names are seperated with '\t' so delimeter is '\t'
            string[] namesOfAttributeAndClass = line.Split('\t');
            
            // 0 to (# of column - 1) because the last one is class label.
            for(int i = 0; i < namesOfAttributeAndClass.Length - 1; ++i)
            {
                attributeList.Add(namesOfAttributeAndClass[i]);
                outcomes[namesOfAttributeAndClass[i]] = new HashSet<string>();
            }

            // read each line and also each one is a tuple 
            
            int row = 1;
            JDataSet targetDataSet = trainDataSet;

            while (!sr.EndOfStream)
            {
                string eachLine = sr.ReadLine();
                // each attribute values are separated with '\t'
                string[] attrValues = eachLine.Split('\t');

                JTuple eachTuple = new JTuple();

                if (row > upToThisLine)
                {
                    targetDataSet = testDataSet;
                }

                // save values to eachTuple(JTuple)
                for (int i = 0; i < attrValues.Length ; ++i)
                {
                    if ( i != attrValues.Length - 1 )
                    {
                        eachTuple.setAttrAndItsValue(namesOfAttributeAndClass[i], attrValues[i]);
                        outcomes[namesOfAttributeAndClass[i]].Add(attrValues[i]);
                    }
                    else
                    {
                        eachTuple.ClassLabel = attrValues[i];
                    }
                }
                targetDataSet.insertTuple(eachTuple);
                row++;
            }
            
            return bReadWell;
        }


        /// <summary>
        /// 
        /// select the attrbute that maximizes information gain 
        /// 
        /// </summary>
        /// <param name="D"> data set </param>
        /// <param name="attributeList"> the list of attributes </param>
        /// <returns></returns>
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
        /// 
        /// Select the attribute that maximizes gain ratio
        /// </summary>
        /// <param name="D"> dataset </param>
        /// <param name="attributeList"> attribute list </param>
        /// <returns> selected attribute name </returns>
        public static string selectAttributeUsingGainRatio(JDataSet D, List<string> attributeList)
        {
            string retAttributeName = "";
            int totalCount = D.TuplesCount;
            double previousEntropy = getEntropy(D);
            double maxGainRatio = Double.MinValue;

            // iterate over the list (=set of attributes)
            foreach(string eachAttributeName in attributeList)
            {
                // <Key, Value> = < value of corresponding attribute, associated data partition >
                // example)   <">=30", D1> 
                Dictionary<string, JDataSet> partitions = new Dictionary<string, JDataSet>();
                
                foreach(JTuple eachTuple in D.Tuples)
                {
                    // ex.)    eachTuple_AttrValue = "<=30"
                    string eachTuple_AttrValue = eachTuple.getAttrValue(eachAttributeName);
                    
                    if (!partitions.ContainsKey(eachTuple_AttrValue))
                    {
                        partitions[eachTuple_AttrValue] = new JDataSet();
                    }

                    partitions[eachTuple_AttrValue].insertTuple(eachTuple);
                }

                double curaAvgEntropy = 0;
                double curSplittingInfo = 0;
                double curInfoGain = 0;
                double curGainRatio = 0;

                foreach (JDataSet eachDataPartition in partitions.Values)
                {
                    double eachWeight = (double)eachDataPartition.TuplesCount / totalCount;
                    curaAvgEntropy += (eachWeight * getEntropy(eachDataPartition));
                    curSplittingInfo += (eachWeight * Math.Log(eachWeight, 2));
                }

                curInfoGain = previousEntropy - curaAvgEntropy;
                curSplittingInfo = -curSplittingInfo;

                curGainRatio = curInfoGain / curSplittingInfo;
                
                if (curGainRatio > maxGainRatio)
                {
                    maxGainRatio = curGainRatio;
                    retAttributeName = eachAttributeName;
                }
            }

            return retAttributeName;
        }

        public static string selectAttributeUsingGiniIndex(JDataSet D, List<string> attributeList)
        {
            double curGiniIndex = 0;

            return "ooowa";
        }

        /// <summary>
        /// It creates decision tree and return its reference 
        /// 
        /// </summary>
        /// <param name="D">D is the dataSet</param>
        /// <param name="attributeList"> Attribute List, it used to branch nodes </param>
        /// <returns> Refrence of tree node </returns>
        public static JTreeNode generateDecisionTree(JDataSet D, List<string> attributeList,
                                                    AttributeSelectDelegate attributeSelector, 
                                                    Dictionary<string, HashSet<string>> outcomes)
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
                retTreeNode.TuplesCount = D.TuplesCount;
                return retTreeNode;
            }

            // termination condition 3
            else if (attributeList.Count == 0)  // majority voting
            {
                retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.MajorityClass);
                retTreeNode.TuplesCount = D.TuplesCount;
                return retTreeNode;
            }

            // select the attribute that classifies tuples as pure as possible 
            string splittingAttribute = attributeSelector(D, attributeList);

            // make a test node (attribute check node)
            retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.TEST, splittingAttribute);
            retTreeNode.TuplesCount = D.TuplesCount;

            // attribute_list <- attribute_list - splitting_attribute
            attributeList.Remove(splittingAttribute);
 
            // Example
            //        Key     Value
            //      '<=30', D1(=t1+t2+t3)
            // 
            // Therefore, branches["<=30"] means the data partition that satisfies 'splitting attribute <= 30'
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
            
            foreach(string eachOutcome in outcomes[splittingAttribute])
            {
                if (branches.ContainsKey(eachOutcome))
                {
                    retTreeNode.PathDirectory[eachOutcome] = generateDecisionTree(branches[eachOutcome], attributeList, attributeSelector, outcomes);
                }
                else
                {
                    // if there is no data partition that satisfies attr(splittingAttribute) == eachOutcome
                    // majority voting 
                    retTreeNode.PathDirectory[eachOutcome] = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.MajorityClass);
                }
            }

            // restore for further use
            attributeList.Add(splittingAttribute);

            return retTreeNode;
        }

        /// <summary>
        /// 
        /// It determines the class of testTuple
        /// 
        /// </summary>
        /// <param name="decisionTree"> decision tree </param>
        /// <param name="testTuple"> the tuple that you wants to classify </param>
        /// <returns></returns>
        public static string testTuple(JTreeNode decisionTree, JTuple testTuple)
        {
            JTreeNode follower = decisionTree;
            
            // follow until the node that is pointed by follower is RESULT type node. (it means classification process has done )
            while(!(follower.NodeType == JTreeNode.JTreeNodeType.RESULT))
            {
                follower = follower.PathDirectory[testTuple.getAttrValue(follower.Value)];
            }

            return follower.Value;
        }

        public static double getAccuracyWithinTrainedDataSet(JTreeNode decisionTree, JDataSet testDataSet)
        {
            int nCorrectAnswers = 0;
            if (testDataSet.TuplesCount == 0)
            {
                return 1;
            }

            foreach (JTuple eachTuple in testDataSet.Tuples)
            {
                if ( eachTuple.ClassLabel == testTuple(decisionTree, eachTuple))
                {
                    nCorrectAnswers++;
                }
            }
            

            return (double)nCorrectAnswers / testDataSet.TuplesCount;
        }
       
        /// <summary>
        /// get the entropy value from D
        /// 
        /// </summary>
        /// <param name="D"> target DataSet </param>
        /// <returns> entropy value </returns>
        private static double getEntropy(JDataSet D)
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

        private static int getTotalLineOfFile(StreamReader sr)
        {
            int totalLine = 0;
            
            while(!sr.EndOfStream)
            {
                sr.ReadLine();
                totalLine++;
            }

            var baseStream = sr.BaseStream;
            baseStream.Seek(0, SeekOrigin.Begin);
            
            return totalLine--;
        }
    }
}
