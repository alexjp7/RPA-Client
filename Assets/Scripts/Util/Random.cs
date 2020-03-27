
namespace Assets.Scripts.Util
{
    class Random
    {
        public static System.Random rand = new System.Random();

        public static int getInt(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static int getInt(int limit)
        {
            return rand.Next(limit);
        }

        public static double getDouble(double limit)
        {
            return rand.NextDouble() * limit;
        }
    }
}
