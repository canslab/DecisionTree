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
            Dictionary<string, HashSet<string>> distinctOutcomes 
                = new Dictionary<string, HashSet<string>>();

            JUtility.readFile("dt_train.txt", attributeList, dataSet);
            
            Console.WriteLine( dataSet.MajorityClass );
            Console.WriteLine( JUtility.selectAttributeUsingIG(dataSet, attributeList) );

            var tree = JUtility.generateDecisionTree(dataSet, attributeList);
            
            int b = 40;
      
            
        }
    }
}