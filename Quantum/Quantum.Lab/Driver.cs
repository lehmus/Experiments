using System;
using Microsoft.Quantum.Simulation.Core;
using Quantum.Lab.Engine;

namespace Quantum.Lab
{
    class Driver
    {
        static void Main(string[] args)
        {
            // Read input
            var nRuns = 0;
            System.Console.WriteLine("Type in the number of runs (whole number):");
            try
            {
                var nRunsInput = System.Console.ReadLine();
                nRuns = Int32.Parse(nRunsInput);
            }
            catch (Exception exc)
            {
                System.Console.WriteLine(exc.Message);
            }

            // Set initial states and apply the Hadarmard operation
            Result[] inputStates = new Result[] { Result.Zero, Result.One };
            foreach (Result state in inputStates)
            {
                BasicOperations.ApplyHadamard(nRuns, state);
            }

            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}