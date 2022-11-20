using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DefaultNamespace;

public class utils 
{
    public static double kernel(Vector3 x1, Vector3 x2, int h, int d)
    {
        double q = (x1 - x2).magnitude / h;
        
        double f_q = 0;

        if (q <= 0.5)
        {

            f_q = 6 * Math.Pow(q, 3) - 6 * Math.Pow(q, 2) + 1;
        }
        else if (q < 1)
        {
            f_q = Math.Pow((1 - q), 3) * 2;
        }

        else
        {
            f_q = 0;
        }

        return f_q / Math.Pow(h, d);

    }

    

    public static double hash(Vector3 pos, int hashSize = 100151)
    {
        int p1 = 435889;
        int p2 = 777143;
        int p3 = 721397;
        return ((p1 * (int) Math.Floor(pos[0])) + (p2 * (int) Math.Floor(pos[1])) + (p3 * (int) Math.Floor(pos[2]))) % hashSize;
        


    }
    
    
}
