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
        
            JUtility.readFile("dt_train.txt", attributeList, dataSet, distinctOutcomes);

            Console.WriteLine( JUtility.selectAttributeUsingIG(dataSet, attributeList) );

            //Console.WriteLine(JUtility.getEntropy(dataSet));



            //Dictionary<string, string> ee = new Dictionary<string, string>();
            //HashSet<int> h = new HashSet<int>();


        
            
        }
    }
}