﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace dt
{
    class JUtility
    {

        public static bool readFile(string inputFileName, List<string> attributeList, JDataSet D, 
                                    Dictionary<string, HashSet<string>> distinctOutcomes)
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
                distinctOutcomes[namesOfAttributeAndClass[i]] = new HashSet<string>();
                attributeList.Add(namesOfAttributeAndClass[i]);
            }
            distinctOutcomes["CLASS"] = new HashSet<string>();

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
                        distinctOutcomes[namesOfAttributeAndClass[i]].Add(attrValues[i]);
                    }
                    else
                    {
                        distinctOutcomes["CLASS"].Add(attrValues[i]);
                        eachTuple.ClassLabel = attrValues[i];
                    }
                }
                D.insertTuple(eachTuple);
            }

            return bReadWell;
        }
        public static double getEntropy(JDataSet D)
        {
            Dictionary<string, int> labelCount = new Dictionary<string, int>();
        
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
        

        public static JTreeNode generateDecisionTree(JDataSet D, List<string> attributeList)
        {
            JTreeNode retTreeNode = null;

           


        }
        

    }

}