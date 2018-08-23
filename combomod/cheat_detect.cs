using System;
using System.Linq;

namespace combomod
{
    public static class cheat_detect
    {
        public static char checksumString(string s)
        {
            if (s.Length != 2)
            {
                return ' ';
            }

            int i = s.ToCharArray().Aggregate(0, (current, c) => current + c);
            int d = i % 10;
            string outString = d.ToString();
            return outString[0];
        }

        public static bool verifyString(string s, int level)
        {
            if (s.Length != 3 && (s.Length != 4 || level != 4))
            {
                combo_mod.log("Incorrect string length " + s + " for level " + level);
                return false;
            }

            char[] charS = s.ToCharArray();
            
            string meme = char.ToString(charS[1]) + char.ToString(charS[2]);
            int correct = int.Parse(checksumString(meme).ToString());
            int i = int.Parse(s.ToCharArray()[0].ToString());

            if (i != correct)
            {
                combo_mod.log("Incorrect checksum " + i + ". Expected checksum for " + s + " is " + correct);
                return false;
            }

            bool flag1;
            bool flag2;
            switch (level)
            {
                case 0:
                    flag1 = Enum.IsDefined(typeof(globals.gm1_bestclear), charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm1_numbindings), charS[2]);
                    break;
                case 1:
                    flag1 = Enum.IsDefined(typeof(globals.gm2_bestclear), charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm2_numbindings), charS[2]);
                    break;
                case 2:
                    flag1 = Enum.IsDefined(typeof(globals.gm3_bestclear), charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm3_numbindings), charS[2]);
                    break;
                case 3:
                    flag1 = Enum.IsDefined(typeof(globals.gm4_bestclear), charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm4_numbindings), charS[2]);
                    break;
                case 4:
                    flag1 = Enum.IsDefined(typeof(globals.gm5_bestclear), charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm5_numbindings), charS[2]);
                    break;
                default:
                    return false;
            }
            combo_mod.log("String is " + s);
            combo_mod.log("Found bestclear in first char? " + flag1 + " found numbindings in second char? " + flag2);

            return (flag1 && flag2);
        }
    }
}