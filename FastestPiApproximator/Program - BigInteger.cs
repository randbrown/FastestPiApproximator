using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastestPiApproximator
{
    class Program
    {
        //A very fast method to compute pi to a said number of digits

        //DO NOT TOUCH! This is the variable that the digits of pi are stored on. Should start at 0.
        static BigInteger final = 0;

        //The path at which we write the final file. Do not change unless you know what you are doing.
        static string path = @"C:\Temp\Pi_Calculator\";

        //Number of threads working. Optimal is 1 per logical processor
        static int numThreads = 1;

        //How many digits do you want to calculate? Any number under 2 trillion will work for this program.
        static int digits = 100000;

        //How many trials of the experiment do you want to perform? Choose .9 * digits / numThreads for best results, but, digits/numThreads is guarenteed to work.
        static int numTerms = 100000;

        //DO NOT TOUCH! Used for internal calculations. Program will not work if you tamper.
        static BigInteger arb = BigInteger.Pow(10, digits);

        //DO NOT TOUCH! Used for internal calculations. Program will not work if you tamper.
       // static BigInteger numThreadsExp = BigInteger.Pow(16, numThreads);
        static long numThreadsExp = (long)Math.Pow(16, numThreads);

        //Capturing it to compare later.
        static DateTime started = DateTime.Now;

        static void Main(string[] args)
        {
            //Creating our collection of tasks that compute the partial sums.
            List<Task<BigInteger>> tasks = new List<Task<BigInteger>>();
            //For loop for assigning a dynamic number of threads.
            for (int k = 0; k < numThreads; k++)
            {
                //DO NOT REMOVE! This is neccesary, because of how threading works. k will change each iteration, but tempk stays at whatever it is for that thread. The important part is, DO NOT TOUCH!
                int tempk = k;
                //Add a new method to compute for each k and add it to the tasks list.
                tasks.Add(Task.Factory.StartNew(() => {
                   //Each one calculates a piece of the digits.
                   return PSum_Arctan_2Term(numTerms / numThreads, tempk);
                }));
            }
            //Makes sure all are done before adding them up. If you don't it messes up the final value. One doesn't finish much faster than another thread, due to the nature of the algorithm.
            Task.WaitAll(tasks.ToArray());
            //Adding all the parts of the digits together in a loop.
            for (int j = 0; j < tasks.Count; j++)
            {
                //Adding all the parts of pi to the final static variable.
                final += tasks[j].Result;
            }
            //We write out a message so people see that the calculations are done, and the time it took.
            Console.WriteLine("#" + (numThreads * numTerms) + " numTerms of " + digits + " digits took " + (DateTime.Now - started) + " time. Your file is located at " + path + "pi_numTerms=" + (numThreads * numTerms) + "_digits=" + digits + ".txt");
            //Write to the file
            File.WriteAllText(path + "pi_numTerms=" + (numThreads * numTerms) + "_digits=" + digits + ".txt", "#" + (numThreads * numTerms) + " of " + digits + " digits " + (DateTime.Now - started) + Environment.NewLine + final);
            //Used so the program doesnt stop. Just press enter after the message appears to stop it.
            Console.ReadLine();
        }

        static BigInteger PSum_Arctan_2Term(int l, int id)
        {
            //Don't change. This is what keeps track of the digits this one thread produces.
            BigInteger total = 0;
            //Used for optimization purposes. Instead of doing a .Pow every for loop, we just do it here and muliply it by itself every loop. This is due to the fact that (16)^(k)=(16)^(k-1)*16
            BigInteger k_16 = BigInteger.Pow(16, id);

            BigInteger arb4 = 4 * arb;
            BigInteger arb2 = 2 * arb;

            int loopMax = numThreads*l;

            BigInteger lastTotal = 0;

            int checkEvery = 10;

            string totalString = "";
            string lastTotalString = "";

            int lastTermLength = 0;

            StringBuilder totalStringBuilder = new StringBuilder();

            List<BigInteger> totals = new List<BigInteger>();


            //Our for loop that calculates digits.
            for ( // id is the number thread this one is. We essentially syncopate the sums so that each thread is only doing 1/thread of the calculations. That way, all cores work for us.
                int k = id; 
                // This isn't very "correct", but is the most efficient for our purposes
                k <= loopMax; 
                // This is due to the fact that each individual thread does 1/threads of the calculations
                k += numThreads)
            {
                if (k%10 == 0)
                {
                    Console.WriteLine("#{0} Time: {1}", k, DateTime.Now - started);
                }
                //Used for optimization, instead of finding 8 * k four times each loop, we do it here.
                long kt8 = k * 8;

                //long kt8Plus4 = kt8 + 4; 
                //long kt8Plus1 = kt8 + 1; 
                //long kt8Plus5 = kt8 + 5; 
                //long kt8Plus6 = kt8 + 6;

                //long temp1 = (kt8Plus4*kt8Plus5*kt8Plus6);
                //long temp2 = (kt8Plus1*kt8Plus5*kt8Plus6);
                //long temp3 = (kt8Plus4 * kt8Plus1 * kt8Plus6);
                //long temp4 = (kt8Plus4 * kt8Plus1 * kt8Plus5);

                //A BBP formula for pi. The arb is for digits of precision. Do not worry about that, though.
                //total += (((arb) / (k_16)) * (4 * (arb) / (kt8 + 1) - 2 * (arb) / (kt8 + 4) - (arb) / (kt8 + 5) - (arb) / (kt8 + 6))) / arb;
                //var term = (((arb) / (k_16)) * (arb4 / (kt8 + 1) - arb2 / (kt8 + 4) - (arb) / (kt8 + 5) - (arb) / (kt8 + 6))) / arb;

                //var termString = term.ToString();
                ////var totalCurrentString = total.ToString();
                //if (lastTermLength > 0 && termString.Length > lastTermLength)
                //{
                //    Console.WriteLine("Term larger than last term");
                //}
                //lastTermLength = termString.Length;


                //total += term;
                
                // total += ((arb/k_16) * (arb4 / (kt8 + 1) - arb2 / (kt8 + 4) - (arb) / (kt8 + 5) - (arb) / (kt8 + 6))) / arb;
                total += ((arb4 / (kt8 + 1) - arb2 / (kt8 + 4) - (arb) / (kt8 + 5) - (arb) / (kt8 + 6))) / k_16;

                //if (total == lastTotal)
                //{
                //    Console.WriteLine("Total for thread [{0}] achieved at k={1}", id, k);
                //    //break;
                //}

                //lastTotalString = totalString;
                //lastTotal = total;

                //The original method
                //total += (((arb) / (BigInteger.Pow(16, k * numThreads)) * (4 * (arb) / (8 * k + 1) - 2 * (arb) / (8 * k + 4) - (arb) / (8 * k + 5) - (arb) / (8 * k + 6))) / arb;

                //For optimization, we just adjust the value here instead of the .Pow each time.
                k_16 *= numThreadsExp;
            }

            //string totalToReturn = totalStringBuilder.ToString();

            //if (total > 0)
            //{
            //    totalToReturn += total.ToString();
            //}

            //foreach (var tot in totals)
            //{
            //    total += tot;
            //}
            
            //return total;
            //BigInteger totalFromString = BigInteger.Parse(totalToReturn);
            //return totalFromString;

            //We return our part of the sum
            return total;
        }
    }
}
