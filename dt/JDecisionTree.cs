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


        /*********************************************************************/
        /******************   Constructors   *********************************/
        /*********************************************************************/
        //public JDecisionTree(string inputTrainingFile, int trainingPercent)     // Constructor
        //{
        //    AttributeList = new List<string>();
        //    Outcomes = new Dictionary<string, HashSet<string>>();
        //    TrainDataSet = new JDataSet();
        //    TestDataSetWithInTrainData = new JDataSet();
        //    MadeComplete = false;

        //    makeDataSetFromTrainingFile(inputTrainingFile, trainingPercent);
        //}

        public JDecisionTree(string[] columnNamesList, Dictionary<string, HashSet<string>> OutcomesPerAttribute, JDataSet trainingDataSet)
        {
            AttributeList = new List<string>();
            for(int i = 0; i < columnNamesList.Length - 1; ++i)
            {
                AttributeList.Add(columnNamesList[i]);
            }
            Outcomes = new Dictionary<string, HashSet<string>>(OutcomesPerAttribute);
            TrainDataSet = new JDataSet(trainingDataSet);
            ClassLabelName = columnNamesList[columnNamesList.Length - 1];
            MadeComplete = false;
        }

        /*********************************************************************/
        /****************** Public Methods     *******************************/
        /*********************************************************************/
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

            foreach (JTuple eachTuple in TestDataSetWithInTrainData.TuplesList)
            {
                if ( eachTuple.ClassLabel == testTuple(eachTuple))
                {
                    nCorrectAnswers++;
                }
            }

            return (double)nCorrectAnswers / TestDataSetWithInTrainData.TuplesCount;
        }

        public string testTuple(JTuple testTuple)
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

        //public void processTestDataAndWriteToFile(string testInputFileName, string outputFileName)
        //{
        //    using (StreamWriter resultFileWriter = new StreamWriter(new FileStream(outputFileName, FileMode.Create)))
        //    using (StreamReader testFileReader = new StreamReader(new FileStream(testInputFileName, FileMode.Open)))
        //    {
        //        testFileReader.ReadLine();

        //        for (int i = 0;  i < UpperRow.Length; ++i)
        //        {
        //            resultFileWriter.Write(UpperRow[i]);
        //            if ( i == UpperRow.Length -1)
        //            {
        //                resultFileWriter.WriteLine();
        //            }
        //            else
        //            {
        //                resultFileWriter.Write('\t');
        //            }
        //        }

        //        while (!testFileReader.EndOfStream)
        //        {
        //            JTuple eachTuple = new JTuple();
        //            string eachLine = testFileReader.ReadLine();
        //            string[] attributeValues = eachLine.Split('\t');

        //            resultFileWriter.Write(eachLine);
        //            resultFileWriter.Write('\t');

        //            int index = 0;
        //            foreach (string eachAttributeValue in attributeValues)
        //            {
        //                eachTuple[UpperRow[index++]] = eachAttributeValue;
        //            }

        //            string testTupleLabel = testTuple(eachTuple);
        //            resultFileWriter.WriteLine(testTupleLabel);
        //        }
        //    }
        //}

        /*********************************************************************/
        /****************** PRIVATE METHODS **********************************/
        /*********************************************************************/
        
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
                retTreeNode = new JTreeNode(JTreeNode.JTreeNodeType.RESULT, D.TuplesList[0].ClassLabel);
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

            foreach (JTuple eachTuple in D.TuplesList)
            {
                // ex.) splittingAttribute == "age"
                // ex.) branchName == "<=30"
                string branchName = eachTuple[splittingAttribute];
                if (!branches.ContainsKey(branchName))
                {
                    branches[branchName] = new JDataSet();
                }
                branches[branchName].InsertTuple(eachTuple);
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

        /*********************************************************************/
        /****************** PUBLIC PROPERTIES ********************************/
        /*********************************************************************/
        public bool MadeComplete { get; private set; }
        
        /*********************************************************************/
        /****************** PRIVATE PROPERTIES *******************************/
        /*********************************************************************/
        
        private List<string> AttributeList { get; set; }
        private Dictionary<string, HashSet<string>> Outcomes { get; set; }
        private JDataSet TrainDataSet { get; set; }
        private JDataSet TestDataSetWithInTrainData { get; set; }
        private JTreeNode DecisionTree { get; set; }

        private string ClassLabelName { get; set; }
     
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

                foreach (JTuple eachTuple in D.TuplesList)
                {
                    // ex.) attrValueOfEachTuple == ">=30" 
                    string attrValueOfEachTuple = eachTuple[attrName];

                    // if there is no dataset partition
                    // allocate new JDataSet 
                    if (!dataPartiotions.ContainsKey(attrValueOfEachTuple))
                    {
                        dataPartiotions[attrValueOfEachTuple] = new JDataSet();
                    }

                    dataPartiotions[attrValueOfEachTuple].InsertTuple(eachTuple);
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

                foreach (JTuple eachTuple in D.TuplesList)
                {
                    // ex.)    eachTuple_AttrValue = "<=30"
                    string eachTuple_AttrValue = eachTuple[eachAttributeName];

                    if (!partitions.ContainsKey(eachTuple_AttrValue))
                    {
                        partitions[eachTuple_AttrValue] = new JDataSet();
                    }

                    partitions[eachTuple_AttrValue].InsertTuple(eachTuple);
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
            foreach(JTuple eachTuple in D.TuplesList)
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
