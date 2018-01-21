using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using Quantum.Lab.Operations;

namespace Quantum.Lab.Engine
{
    class BasicOperations
    {
        public static void ApplyHadamard(int numberOfRuns, Result initialState)
        {
            using (var sim = new QuantumSimulator())
            {
                var result = Hadamard.Run(sim, numberOfRuns, initialState).Result;
                var (numZeros, numOnes) = result;

                System.Console.WriteLine($"Initial state: {initialState,-4}");
                System.Console.WriteLine("State populations after Hadamard operation");
                System.Console.WriteLine($"|0>: {numZeros,-4} |1>: {numOnes,-4}");
                System.Console.WriteLine();
            }
        }
    }
}
