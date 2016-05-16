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
            JDecisionTree infoGainTree = new JDecisionTree("dt_train1.txt", 90);
            infoGainTree.makeTree(JDecisionTree.AttributeSelectionMeasure.INFO_GAIN);
            Console.WriteLine("INFO GAIN RATIO's trained accuracy = " + infoGainTree.getAccuracyWithinTrainedDataSet());

            JDecisionTree gainRatioTree = new JDecisionTree("dt_train1.txt", 90);
            gainRatioTree.makeTree(JDecisionTree.AttributeSelectionMeasure.GAIN_RATIO);
            Console.WriteLine("GAIN RATIO's trained accuracy = " + gainRatioTree.getAccuracyWithinTrainedDataSet());

            //Console.WriteLine("most impacting factor (using information gain) = {0} ", JDecisionTreeAlgorithm.selectAttributeUsingIG(dataSet, attributeList));
            //Console.WriteLine("most impacting factor (using gain ratio ) = {0} \n", JDecisionTreeAlgorithm.selectAttributeUsingGainRatio(dataSet, attributeList));
        }

    }
}