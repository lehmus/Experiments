namespace Quantum.Lab.Operations
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

    operation Hadamard (numberOfRuns : Int, initialState: Result) : (Int,Int)
    {
        body
        {
            mutable numZeros = 0;
            mutable numOnes = 0;
            using (qubits = Qubit[1])
            {
                for (runNumber in 1..numberOfRuns)
                {
                    SetQubit (initialState, qubits[0]);

                    // let result = M (qubits[0]);

					// X(qubits[0]);
                    // let result2 = M (qubits[0]);
					H(qubits[0]);
                    let result = M (qubits[0]);

                    // Count the number of ones we saw:
                    if (result == One)
                    {
                        set numOnes = numOnes + 1;
                    }
                }
				set numZeros = numberOfRuns - numOnes;
                SetQubit(Zero, qubits[0]);
            }
            // Return number of times we saw a |0> and number of times we saw a |1>
            return (numZeros, numOnes);
        }
    }
}
