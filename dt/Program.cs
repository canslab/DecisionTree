using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dt
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> attributeList = new List<string>();
            JDataSet dataSet = new JDataSet();
            JDataSet testDataSet = new JDataSet();
            Dictionary<string, HashSet<string>> outcomes = new Dictionary<string, HashSet<string>>();

            JDecisionTreeAlgorithm.readFileAndMakeDataSet("dt_train1.txt", attributeList, outcomes, dataSet, 98, testDataSet);

            Console.WriteLine("most impacting factor (using information gain) = {0} ", JDecisionTreeAlgorithm.selectAttributeUsingIG(dataSet, attributeList));
            Console.WriteLine("most impacting factor (using gain ratio ) = {0} \n", JDecisionTreeAlgorithm.selectAttributeUsingGainRatio(dataSet, attributeList));

            var igTree = JDecisionTreeAlgorithm.generateDecisionTree(dataSet, attributeList, JDecisionTreeAlgorithm.selectAttributeUsingIG, outcomes);
            var grTree = JDecisionTreeAlgorithm.generateDecisionTree(dataSet, attributeList, JDecisionTreeAlgorithm.selectAttributeUsingGainRatio, outcomes);


            Console.WriteLine("INFO GAIN accuracy = {0} ", JDecisionTreeAlgorithm.getAccuracyWithinTrainedDataSet(igTree, testDataSet));
            Console.WriteLine("GAIN RATIO accuracy = {0} \n ", JDecisionTreeAlgorithm.getAccuracyWithinTrainedDataSet(grTree, testDataSet));
        }

    }
}