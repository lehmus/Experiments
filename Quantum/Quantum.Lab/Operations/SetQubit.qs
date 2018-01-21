namespace Quantum.Lab.Operations
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

	operation SetQubit (targetState: Result, qubit: Qubit) : ()
    {
        body
        {
            let measuredState = M(qubit);
            if (targetState != measuredState)
            {
                X(qubit);
            }
        }
    }
}
