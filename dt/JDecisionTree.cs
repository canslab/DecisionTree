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
    class JDecisionTree
    {
        public enum AttributeSelectionMeasure { INFO_GAIN, GAIN_RATIO, GINI_INDEX };
        
        // public properties
        public bool MadeComplete { get; private set; }

        // Constructor
        public JDecisionTree(string inputFileName, int trainingPercent)
        {
            AttributeList = new List<string>();
            Outcomes = new Dictionary<string, HashSet<string>>();
            TrainDataSet = new JDataSet();
            TestDataSetWithInTrainData = new JDataSet();
            MadeComplete = false;

            readFileAndMakeDataSet(inputFileName, trainingPercent);
        }
        
        // when user call this method, it starts to make decision tree,
        // if it is done successfully, MadeComplete will be true
        // otherwise, it will be false                      
        public bool makeTree(AttributeSelectionMeasure measureType)
        {
            switch(measureType)
            {
                case AttributeSelectionMeasure.GAIN_RATIO:
                    DecisionTree = generateDecisionTree(TrainDataSet, AttributeList, JDecisionTree.selectAttributeUsingGainRatio);
                    break;

                case AttributeSelectionMeasure.GINI_INDEX:
                    DecisionTree = generateDecisionTree(TrainDataSet, AttributeList, JDecisionTree.selectAttributeUsingGiniIndex);
                    break;

                case AttributeSelectionMeasure.INFO_GAIN:
                    DecisionTree = generateDecisionTree(TrainDataSet, AttributeList, JDecisionTree.selectAttributeUsingIG);
                    break;
            }

            if (DecisionTree == null)
            {
                return MadeComplete = false;
            }

            return MadeComplete = true;
        }
        
        public double getAccuracyWithinTrainedDataSet()
        {
            int nCorrectAnswers = 0;
            if (TestDataSetWithInTrainData.TuplesCount == 0)
            {
                return 1;
            }

            foreach (JTuple eachTuple in TestDataSetWithInTrainData.Tuples)
            {
                if ( eachTuple.ClassLabel == testTuple(eachTuple))
                {
                    nCorrectAnswers++;
                }
            }

            return (double)nCorrectAnswers / TestDataSetWithInTrainData.TuplesCount;
        }

        public void processTestDataAndWriteToFile(string testInputFileName, string outputFileName)
        {
            using (StreamWriter resultFileWriter = new StreamWriter(new FileStream(outputFileName, FileMode.Create)))
            using (StreamReader testFileReader = new StreamReader(new FileStream(testInputFileName, FileMode.Open)))
            {
                // before starting the process, read one line from testInputFile.
                // because the first line is the list of attribute names, so it is needed to be ignored.
                string attributeLine = testFileReader.ReadLine();
                string[] attributeNames = attributeLine.Split('\t');
                                                
                while(!testFileReader.EndOfStream)
                {
                    JTuple eachTuple = new JTuple();
                    string eachLine = testFileReader.ReadLine();
                    string[] attributeValues = eachLine.Split('\t');

                    int index = 0;
                    foreach(string eachAttributeValue in attributeValues)
                    {
                        eachTuple[attributeNames[index++]] = eachAttributeValue;
                    }

                    


                }



            }
        }

        /*********************************************************************/
        /****************** PRIVATE METHODS **********************************/
        /*********************************************************************/
        
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
        private bool readFileAndMakeDataSet(string inputFileName, int trainingPercent)
        {
            bool bReadWell = false;

            // open training set file 
            using (StreamReader sr = new StreamReader(new FileStream(inputFileName, FileMode.Open)))
            {
                double a = (double)trainingPercent / 100;

                int upToThisLine = (int)Math.Ceiling(a * getTotalLineOfFile(sr));

                // first line is the list of attribute names separated with '\t'
                // first get the attribute names and save them to attribute list
                string line = sr.ReadLine();

                // every attribute names are seperated with '\t' so delimeter is '\t'
                string[] namesOfAttributeAndClass = line.Split('\t');

                // 0 to (# of column - 1) because the last one is class label.
                for (int i = 0; i < namesOfAttributeAndClass.Length - 1; ++i)
                {
                    AttributeList.Add(namesOfAttributeAndClass[i]);
                    Outcomes[namesOfAttributeAndClass[i]] = new HashSet<string>();
                }

                // read each line and also each one is a tuple 
                int row = 1;
                JDataSet targetDataSet = TrainDataSet;

                while (!sr.EndOfStream)
                {
                    string eachLine = sr.ReadLine();
                    // each attribute values are separated with '\t'
                    string[] attrValues = eachLine.Split('\t');

                    JTuple eachTuple = new JTuple();

                    if (row > upToThisLine)
                    {
                        targetDataSet = TestDataSetWithInTrainData;
                    }

                    // save values to eachTuple(JTuple)
                    for (int i = 0; i < attrValues.Length; ++i)
                    {
                        if (i != attrValues.Length - 1)
                        {
                            eachTuple[namesOfAttributeAndClass[i]] = attrValues[i];
                            Outcomes[namesOfAttributeAndClass[i]].Add(attrValues[i]);
                        }
                        else
                        {
                            eachTuple.ClassLabel = attrValues[i];
                        }
                    }
                    targetDataSet.insertTuple(eachTuple);
                    row++;
                }
            }
            return bReadWell;
        }

        /// <summary>
        /// It creates decision tree and return its reference 
        /// 
        /// </summary>
        /// <param name="D">D is the dataSet</param>
        /// <param name="attributeList"> Attribute List, it used to branch nodes </param>
        /// <returns> Refrence of tree node </returns>
        private JTreeNode generateDecisionTree(JDataSet D, List<string> attributeList,
                                                    AttributeSelectDelegate attributeSelector)
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

            foreach (JTuple eachTuple in D.Tuples)
            {
                // ex.) splittingAttribute == "age"
                // ex.) branchName == "<=30"
                string branchName = eachTuple[splittingAttribute];
                if (!branches.ContainsKey(branchName))
                {
                    branches[branchName] = new JDataSet();
                }
                branches[branchName].insertTuple(eachTuple);
            }

            foreach (string eachOutcome in Outcomes[splittingAttribute])
            {
                if (branches.ContainsKey(eachOutcome))
                {
                    retTreeNode[eachOutcome] = generateDecisionTree(branches[eachOutcome], attributeList, attributeSelector);
                }
                else
                {
                    // if there is no data partition that satisfies attr(splittingAttribute) == eachOutcome
                    // majority voting 
                    retTreeNode[eachOutcome] = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.MajorityClass);
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
        private string testTuple(JTuple testTuple)
        {
            JTreeNode follower = DecisionTree;

            // follow until the node that is pointed by follower is RESULT type node. (it means classification process has done )
            while (!(follower.NodeType == JTreeNode.JTreeNodeType.RESULT))
            {
                follower = follower[testTuple[follower.Indicator]];
            }

            // after the while loop, Indicator means the class label of the testTuple
            return follower.Indicator;
        }

        /*********************************************************************/
        /****************** PRIVATE PROPERTIES *******************************/
        /*********************************************************************/
        
        private List<string> AttributeList { get; set; }
        private Dictionary<string, HashSet<string>> Outcomes { get; set; }
        private JDataSet TrainDataSet { get; set; }
        private JDataSet TestDataSetWithInTrainData { get; set; }
        private JTreeNode DecisionTree { get; set; }
        
        /****************************************************************************/
        /****************** PRIVATE STATIC METHODS **********************************/
        /****************************************************************************/

        /// <summary>
        /// 
        /// select the attrbute that maximizes information gain 
        /// 
        /// </summary>
        /// <param name="D"> data set </param>
        /// <param name="attributeList"> the list of attributes </param>
        /// <returns></returns>
        private static string selectAttributeUsingIG(JDataSet D, List<string> attributeList)
        {
            int totalCount = D.TuplesCount;
            double minEntropy = Double.MaxValue;
            string retAttributeName = "";

            // ex.) attrName = "age"
            foreach (string attrName in attributeList)
            {
                Dictionary<string, JDataSet> dataPartiotions = new Dictionary<string, JDataSet>();

                foreach (JTuple eachTuple in D.Tuples)
                {
                    // ex.) attrValueOfEachTuple == ">=30" 
                    string attrValueOfEachTuple = eachTuple[attrName];

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
                    weightedSum += ((double)dataPartiotions[eachAttributeValue].TuplesCount / totalCount) * getEntropy(dataPartiotions[eachAttributeValue]);
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
        private static string selectAttributeUsingGainRatio(JDataSet D, List<string> attributeList)
        {
            string retAttributeName = "";
            int totalCount = D.TuplesCount;
            double previousEntropy = getEntropy(D);
            double maxGainRatio = Double.MinValue;

            // iterate over the list (=set of attributes)
            foreach (string eachAttributeName in attributeList)
            {
                // <Key, Value> = < value of corresponding attribute, associated data partition >
                // example)   <">=30", D1> 
                Dictionary<string, JDataSet> partitions = new Dictionary<string, JDataSet>();

                foreach (JTuple eachTuple in D.Tuples)
                {
                    // ex.)    eachTuple_AttrValue = "<=30"
                    string eachTuple_AttrValue = eachTuple[eachAttributeName];

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

        private static string selectAttributeUsingGiniIndex(JDataSet D, List<string> attributeList)
        {
            double curGiniIndex = 0;

            return "ooowa";
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
