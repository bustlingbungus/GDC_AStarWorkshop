using UnityEngine;

public class Utils
{
    // for a float, x, whose factors are a and b, 
    // given a and x finds b rounded to the nearest integer
    public static int closestIntegerFactor(float x, float a)
    {
        if (a == 0f) return 0; // avoid divide by zero
        float b = x / a; // the answer as a float
        int res = (int)b; // truncated result
        // if diff > 0.5, round away from 0 (add sign). Otherwise, truncated is correct
        return (Mathf.Abs(b - res) >= 0.5f) ? res + (int)Mathf.Sign(b) : res;
    }
}
