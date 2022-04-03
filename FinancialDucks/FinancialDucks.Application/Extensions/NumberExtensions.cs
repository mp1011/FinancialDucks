namespace FinancialDucks.Application.Extensions
{
    public static class NumberExtensions
    {
        public static int ModPositive(this int Number, int Mod)
        {
            while (Number < 0)
                Number += Mod;

            return Number % Mod;
        }

        public static double ModPositive(this double Number, double Mod)
        {
            while (Number < 0)
                Number += Mod;

            return Number % Mod;
        }
    }
}
