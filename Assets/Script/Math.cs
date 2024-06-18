
namespace MathOfBach
{
    public static class Math
    {
        public static bool IsBetweenRange(float a,float L,float R)
        {
            return L <= a && a <= R;
        }    
        public static bool IsBetweenRange(int a, int L,int R)
        { 
            return L <= a && a <= R;
        }
    }
}
