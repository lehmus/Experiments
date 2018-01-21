using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.Tutorials
{
    class Driver
    {
        static void Main(string[] args)
        {
            using (var sim = new QuantumSimulator())
            {
                // Try initial values
                Result[] initials = new Result[] { Result.Zero, Result.One };
                foreach (Result initial in initials)
                {
                    var result1 = BellTest1.Run(sim, 1000, initial).Result;
                    var (numZeros1, numOnes1) = result1;
                    var result2 = BellTest2.Run(sim, 1000, initial).Result;
                    var (numZeros2, numOnes2) = result2;

                    System.Console.WriteLine(
                        $"Init (1 qubit):{initial,-4} 0s={numZeros1,-4} 1s={numOnes1,-4}"
                        );
                    System.Console.WriteLine(
                        $"Init (2 qubits):{initial,-4} 0s={numZeros2,-4} 1s={numOnes2,-4}"
                        );
                }
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}