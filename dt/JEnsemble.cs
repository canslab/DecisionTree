using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
    class JEnsemble
    {
        /************************************************/
        /***********  Public Methods                *****/
        /************************************************/
        public bool MakeResultTextFile(string testFileName, string outputFileName)
        {
            if (!TreeReady)
            {
                return false;
            }

            using (StreamWriter outputFileWriter = new StreamWriter(new FileStream(outputFileName, FileMode.Create)))
            using (StreamReader testFileReader = new StreamReader(new FileStream(testFileName, FileMode.Open)))
            {
                testFileReader.ReadLine();
                //outputFileWriter.WriteLine(mColumnNamesList);

                for (int i = 0; i < mColumnNamesList.Length; ++i)
                {
                    outputFileWriter.Write(mColumnNamesList[i]);
                    if (i == mColumnNamesList.Length - 1)
                    {
                        outputFileWriter.WriteLine();
                    }
                    else
                    {
                        outputFileWriter.Write('\t');
                    }
                }

                while(!testFileReader.EndOfStream)
                {
                    Dictionary<string, int> jury = new Dictionary<string, int>();
                    int maxValue = int.MinValue;
                    string maxLabel = null;

                    string eachLineInTestFile = testFileReader.ReadLine();
                    outputFileWriter.Write(eachLineInTestFile);
                    outputFileWriter.Write('\t');

                    string[] attributeValuesArray = eachLineInTestFile.Split('\t');

                    JTuple testTuple = new JTuple();

                    for(int i = 0; i < attributeValuesArray.Length; ++i)
                    {
                        testTuple[mColumnNamesList[i]] = attributeValuesArray[i];
                    }

                    foreach (JDecisionTree eachDecisionTree in mTrees)
                    {
                        string resultByEachTree = eachDecisionTree.testTuple(testTuple);

                        if (!jury.ContainsKey(resultByEachTree))
                        {
                            jury[resultByEachTree] = 0;
                        }
                        jury[resultByEachTree]++;
                    }
                    
                    // majority voting by the set of Trees
                    // in order to get the class label of each tuple
                    foreach(string key in jury.Keys)
                    {
                        if (maxValue < jury[key])
                        {
                            maxValue = jury[key];
                            maxLabel = key;
                        }
                    }
                    outputFileWriter.WriteLine(maxLabel);
                }
            }
            return true;
        }

        /************************************************/
        /***********  Public Constructors           *****/
        /************************************************/
        public JEnsemble(string trainingDataFile, double percentage, int nTree, JDecisionTree.AttributeSelectionMeasure measureType)
        {
            // make dataset from file, also the column name list, and outcome list 
            makeDataSetFromFile(trainingDataFile, out mColumnNamesList, out mOutcomes, out mTotalDataSet, out mbDataSetReady);

            TrainingDataPercent = percentage;
            int nTrainingData = (int)Math.Ceiling( percentage * mTotalDataSet.TuplesCount );

            mTrees = new List<JDecisionTree>();
            TreeReady = makeEnsemble(nTree, measureType);
        }

        /************************************************/
        /***********  Public Properties             *****/
        /************************************************/
        public int DecisionTreeCount { get; private set; }
        public double TrainingDataPercent { get; }
        public int TrainingTuplesCount
        {
            get
            {
                return (int)Math.Ceiling(TrainingDataPercent * mTotalDataSet.TuplesCount);
            }
        }
        
        public bool TreeReady { get; }
        public bool DataSetReady { get { return mbDataSetReady; } }

        /************************************************/
        /***********  private static methods        *****/
        /************************************************/
        private static void makeDataSetFromFile(string inputFileName,
                                         out string[] columnNamesList, 
                                         out Dictionary<string, HashSet<String>> outComes,
                                         out JDataSet totalDataSet,
                                         out bool bDataSetReady)
        {
            outComes = new Dictionary<string, HashSet<string>>();
            totalDataSet = new JDataSet();

            using (StreamReader inputFileReader =
                   new StreamReader(new FileStream(inputFileName, FileMode.Open)))
            {
                string topLine = inputFileReader.ReadLine();
                columnNamesList = topLine.Split('\t');

                // -1 is needed because the last one is not the attribute name, it is the class label name.
                int nAttributes = columnNamesList.Length - 1;
                for(int i = 0; i < nAttributes; ++i)
                {
                    outComes[columnNamesList[i]] = new HashSet<string>();
                }

                while(!inputFileReader.EndOfStream)
                {
                    string eachLine = inputFileReader.ReadLine();
                    string[] attrValues = eachLine.Split('\t');

                    JTuple eachTuple = new JTuple();

                    for(int i = 0; i < attrValues.Length; ++i)
                    {
                        // last one is the class label
                        if (i == attrValues.Length - 1)
                        {
                            eachTuple.ClassLabel = attrValues[i];
                        }
                        else
                        {
                            eachTuple[columnNamesList[i]] = attrValues[i];
                            outComes[columnNamesList[i]].Add(attrValues[i]);
                        }
                    }
                    // Add this tuple to the data set.
                    // retDataSet is the data set that would be returned to the caller.
                    totalDataSet.InsertTuple(eachTuple);
                }
            }
            if ( totalDataSet.TuplesCount == 0)
            {
                bDataSetReady = false;
            }
            else
            {
                bDataSetReady = true;
            }
        }

        /************************************************/
        /***********  private methods               *****/
        /************************************************/
        private bool makeEnsemble(int nTree, JDecisionTree.AttributeSelectionMeasure measureType)
        {
            Random randomGenerator = new Random();

            if (!DataSetReady)
            {
                return false;
            }

            for (int i = 0; i < nTree; ++i)
            {
                JDataSet partialDataSet = new JDataSet();

                for (int j = 0; j < TrainingTuplesCount; ++j)
                {
                    int index = randomGenerator.Next() % mTotalDataSet.TuplesCount;

                    JTuple chosenTuple = mTotalDataSet[index];

                    partialDataSet.InsertTuple(chosenTuple);
                }

                JDecisionTree memberTree = new JDecisionTree(mColumnNamesList, mOutcomes, partialDataSet);
                memberTree.makeTree(measureType);

                if (!memberTree.MadeComplete)
                {
                    return false;
                }

                mTrees.Add(memberTree);
            }

            return true;
        }

        private List<JDecisionTree> makeDecisionTrees()
        {
            List<JDecisionTree> retDecisionTreeList = new List<JDecisionTree>();
            
            


            return retDecisionTreeList;
        }

        /************************************************/
        /***********  private member variables      *****/
        /************************************************/
        private string[] mColumnNamesList = null;
        private Dictionary<string, HashSet<String>> mOutcomes = null;
        private JDataSet mTotalDataSet = null;

        private List<JDecisionTree> mTrees = null;
        private bool mbDataSetReady = false;     
            
    }
}
