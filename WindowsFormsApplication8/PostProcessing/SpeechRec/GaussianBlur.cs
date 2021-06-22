using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    public static class OneDimensionGaussianBlur
    {
        private static List<double> coefficientsSigma1 = new List<double>{ 
            0.004431848412, 0.053990966513, 0.241970724519,
            0.398942280401, 
            0.241970724519, 0.053990966513, 0.004431848412 };
        /// <summary>
        /// Blurs a one dimensional array with sigma = 1
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        static public List<double> blurSigma1(List<double> input)
        {
            List<double> filteredList = new List<double>();

            double sumOfCoefficients = sumListOfDoubles(coefficientsSigma1);

            for(int i = 0; i < input.Count ; i++)
            {
                List<double> allMultipliedValues = new List<double>();

                for(int j = 0; j < 7; j++)
                {
                    double cellInputDouble = 0.0;

                    if (i + j - 3 < 0)
                        cellInputDouble = input[0];
                    else if (i + j - 3 >= input.Count)
                        cellInputDouble = input[input.Count - 1];
                    else
                        cellInputDouble = input[i + j - 3];

                    allMultipliedValues.Add(cellInputDouble * coefficientsSigma1[j]);
                }
                //Divide to normalize, because Sum is slighlty different than 1
                filteredList.Add(sumListOfDoubles(allMultipliedValues) / sumOfCoefficients); 
            }
            return filteredList;
        }

        private static double sumListOfDoubles ( List<double> listOfDoubles)
        {
            double total = 0.0;
            foreach (double dbl in listOfDoubles)
            {
                total += dbl;
            }
            return total;
        }

    }
}