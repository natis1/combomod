using System;
using UnityEngine;

namespace combomod
{
    public static class proc_gen
    {
        
        
        public static Texture2D generateWhiteCircle()
        {
            Texture2D circle = new Texture2D(100, 100);
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    double dist = getDistance(x, y, 100, 100);
                    if (dist < 35.0)
                    {
                        circle.SetPixel(x, y, Color.white);
                    } else if (dist < 50.0)
                    {
                        circle.SetPixel(x, y, new Color(1.0f, 1.0f, 1.0f, (float)((50.0 - dist) / 1.5)));
                    }
                    else
                    {
                        circle.SetPixel(x, y, Color.clear);
                    }
                }
            }
            return circle;
        }


        private static double getDistance(int x, int y, int width, int height)
        {
            int relX = x - (width / 2 );
            int relY = y - (height / 2);

            return Math.Sqrt( (double) (relX * relX) + (double) (relY * relY) );
        }
    }
}