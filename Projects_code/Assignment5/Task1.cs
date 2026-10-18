namespace Assignment5;

public static class Task1
{
    public static decimal ApplyDiscount(decimal rate, decimal amount) => amount - amount * rate;
    
    public static decimal AddFee(decimal fee, decimal amount) => amount + fee;
    
    public static decimal RoundToCents(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static Func<A, Func<B, C>> Curry<A, B, C>(Func<A, B, C> f)
    {
        return a => b => f(a, b);
    }

    public static void Run()
    {
        Console.WriteLine("--- Task 1: Currying & Partial Application ---");

        var curriedDiscount = Curry<decimal, decimal, decimal>(ApplyDiscount);
        var curriedFee = Curry<decimal, decimal, decimal>(AddFee);

        Func<decimal, decimal> discount10 = curriedDiscount(0.10m);
        Func<decimal, decimal> fee5 = curriedFee(5m);

        decimal initialAmount = 1234.56m;
        decimal discounted = discount10(initialAmount);
        decimal withFee = fee5(discounted);
        decimal finalPrice = RoundToCents(withFee);

        Console.WriteLine($"Initial Amount: {initialAmount}");
        Console.WriteLine($"After 10% discount: {discounted}");
        Console.WriteLine($"After 5 flat fee: {withFee}");
        Console.WriteLine($"After rounding: {finalPrice}");
        Console.WriteLine();
    }
}
