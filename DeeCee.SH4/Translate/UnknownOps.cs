namespace DeeCee.SH4.Translate;

public static class UnknownOps
{
    public static void Unimplemented(Sh4EmitterContext ir)
    {
        Console.WriteLine("Unimplemented: " + ir.Op.ToString());
        //throw new NotImplementedException();
    }

    public static void Invalid(Sh4EmitterContext ir)
    {
        throw new InvalidOperationException();
    }
}