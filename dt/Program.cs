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

            Console.WriteLine("Test file name = " + args[0]);

            JEnsemble ens1 = new JEnsemble(args[0], 0.9, 3000, JDecisionTree.AttributeSelectionMeasure.INFO_GAIN);

            ens1.MakeResultTextFile(args[1], "dt_result.txt");
        }
    }
}