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

            JUtility.readFileAndMakeDataSet("dt_train1.txt", attributeList, outcomes, dataSet, 70, testDataSet);

            Console.WriteLine("most impacting factor (using information gain) = {0} ",JUtility.selectAttributeUsingIG(dataSet, attributeList));

            var tree = JUtility.generateDecisionTree(dataSet, attributeList, outcomes);

            Console.WriteLine("accuracy = {0} ",JUtility.getAccuracyWithinTrainedDataSet(tree, testDataSet));

            

        }

    }
}