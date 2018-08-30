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
                    flag1 = Enum.IsDefined(typeof(globals.gm1_bestclear), (int) charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm1_numbindings), (int) charS[2]);
                    break;
                case 1:
                    flag1 = Enum.IsDefined(typeof(globals.gm2_bestclear), (int) charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm2_numbindings), (int) charS[2]);
                    break;
                case 2:
                    flag1 = Enum.IsDefined(typeof(globals.gm3_bestclear), (int) charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm3_numbindings), (int) charS[2]);
                    break;
                case 3:
                    flag1 = Enum.IsDefined(typeof(globals.gm4_bestclear), (int) charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm4_numbindings), (int) charS[2]);
                    break;
                case 4:
                    flag1 = Enum.IsDefined(typeof(globals.gm5_bestclear), (int) charS[1]);
                    flag2 = Enum.IsDefined(typeof(globals.gm5_numbindings), (int) charS[2]);
                    break;
                default:
                    return false;
            }
            //combo_mod.log("String is " + s);
            //combo_mod.log("Found bestclear in first char? " + flag1 + " found numbindings in second char? " + flag2);

            return (flag1 && flag2);
        }

        public static void saveResults()
        {
            char[][] soullessComplete = {
                globals.fileSettings.soulless1.ToCharArray(),
                globals.fileSettings.soulless2.ToCharArray(),
                globals.fileSettings.soulless3.ToCharArray(),
                globals.fileSettings.soulless4.ToCharArray(),
                globals.fileSettings.soulless5.ToCharArray()
            };
            
            
            foreach (globals.bestclear bc in Enum.GetValues(typeof(globals.bestclear)))
            {
                if (bc != globals.ALL_RESULTS[0].bestClear) continue;

                if (bc == globals.bestclear.None)
                    soullessComplete[0][1] = (char) globals.gm1_bestclear.None;
                else if (bc == globals.bestclear.FiveStar)
                    soullessComplete[0][1] = (char) globals.gm1_bestclear.FiveStar;
                else if (bc == globals.bestclear.FourStar)
                    soullessComplete[0][1] = (char) globals.gm1_bestclear.FourStar;
                else if (bc == globals.bestclear.ThreeStar)
                    soullessComplete[0][1] = (char) globals.gm1_bestclear.ThreeStar;
                else if (bc == globals.bestclear.FullCombo)
                    soullessComplete[0][1] = (char) globals.gm1_bestclear.FullCombo;
            }

            foreach (globals.numbindings nb in Enum.GetValues(typeof(globals.numbindings)))
            {
                if (nb != globals.ALL_RESULTS[0].numBind) continue;

                if (nb == globals.numbindings.None)
                    soullessComplete[0][2] = (char) globals.gm1_numbindings.None;
                else if (nb == globals.numbindings.One)
                    soullessComplete[0][2] = (char) globals.gm1_numbindings.One;
                else if (nb == globals.numbindings.Two)
                    soullessComplete[0][2] = (char) globals.gm1_numbindings.Two;
                else if (nb == globals.numbindings.Three)
                    soullessComplete[0][2] = (char) globals.gm1_numbindings.Three;
                else if (nb == globals.numbindings.Four)
                    soullessComplete[0][2] = (char) globals.gm1_numbindings.Four;
            }

            soullessComplete[0][0] =
                checksumString(new string(soullessComplete[0], 1, 2));

            foreach (globals.bestclear bc in Enum.GetValues(typeof(globals.bestclear)))
            {
                if (bc != globals.ALL_RESULTS[1].bestClear) continue;

                if (bc == globals.bestclear.None)
                    soullessComplete[1][1] = (char) globals.gm2_bestclear.None;
                else if (bc == globals.bestclear.FiveStar)
                    soullessComplete[1][1] = (char) globals.gm2_bestclear.FiveStar;
                else if (bc == globals.bestclear.FourStar)
                    soullessComplete[1][1] = (char) globals.gm2_bestclear.FourStar;
                else if (bc == globals.bestclear.ThreeStar)
                    soullessComplete[1][1] = (char) globals.gm2_bestclear.ThreeStar;
                else if (bc == globals.bestclear.FullCombo)
                    soullessComplete[1][1] = (char) globals.gm2_bestclear.FullCombo;
            }

            foreach (globals.numbindings nb in Enum.GetValues(typeof(globals.numbindings)))
            {
                if (nb != globals.ALL_RESULTS[1].numBind) continue;

                if (nb == globals.numbindings.None)
                    soullessComplete[1][2] = (char) globals.gm2_numbindings.None;
                else if (nb == globals.numbindings.One)
                    soullessComplete[1][2] = (char) globals.gm2_numbindings.One;
                else if (nb == globals.numbindings.Two)
                    soullessComplete[1][2] = (char) globals.gm2_numbindings.Two;
                else if (nb == globals.numbindings.Three)
                    soullessComplete[1][2] = (char) globals.gm2_numbindings.Three;
                else if (nb == globals.numbindings.Four)
                    soullessComplete[1][2] = (char) globals.gm2_numbindings.Four;
            }

            soullessComplete[1][0] =
                checksumString(new string(soullessComplete[1], 1, 2));

            foreach (globals.bestclear bc in Enum.GetValues(typeof(globals.bestclear)))
            {
                if (bc != globals.ALL_RESULTS[2].bestClear) continue;

                if (bc == globals.bestclear.None)
                    soullessComplete[2][1] = (char) globals.gm3_bestclear.None;
                else if (bc == globals.bestclear.FiveStar)
                    soullessComplete[2][1] = (char) globals.gm3_bestclear.FiveStar;
                else if (bc == globals.bestclear.FourStar)
                    soullessComplete[2][1] = (char) globals.gm3_bestclear.FourStar;
                else if (bc == globals.bestclear.ThreeStar)
                    soullessComplete[2][1] = (char) globals.gm3_bestclear.ThreeStar;
                else if (bc == globals.bestclear.FullCombo)
                    soullessComplete[2][1] = (char) globals.gm3_bestclear.FullCombo;
            }

            foreach (globals.numbindings nb in Enum.GetValues(typeof(globals.numbindings)))
            {
                if (nb != globals.ALL_RESULTS[2].numBind) continue;

                if (nb == globals.numbindings.None)
                    soullessComplete[2][2] = (char) globals.gm3_numbindings.None;
                else if (nb == globals.numbindings.One)
                    soullessComplete[2][2] = (char) globals.gm3_numbindings.One;
                else if (nb == globals.numbindings.Two)
                    soullessComplete[2][2] = (char) globals.gm3_numbindings.Two;
                else if (nb == globals.numbindings.Three)
                    soullessComplete[2][2] = (char) globals.gm3_numbindings.Three;
                else if (nb == globals.numbindings.Four)
                    soullessComplete[2][2] = (char) globals.gm3_numbindings.Four;
            }

            soullessComplete[2][0] =
                checksumString(new string(soullessComplete[2], 1, 2));


            foreach (globals.bestclear bc in Enum.GetValues(typeof(globals.bestclear)))
            {
                if (bc != globals.ALL_RESULTS[3].bestClear) continue;

                if (bc == globals.bestclear.None)
                    soullessComplete[3][1] = (char) globals.gm4_bestclear.None;
                else if (bc == globals.bestclear.FiveStar)
                    soullessComplete[3][1] = (char) globals.gm4_bestclear.FiveStar;
                else if (bc == globals.bestclear.FourStar)
                    soullessComplete[3][1] = (char) globals.gm4_bestclear.FourStar;
                else if (bc == globals.bestclear.ThreeStar)
                    soullessComplete[3][1] = (char) globals.gm4_bestclear.ThreeStar;
                else if (bc == globals.bestclear.FullCombo)
                    soullessComplete[3][1] = (char) globals.gm4_bestclear.FullCombo;
            }

            foreach (globals.numbindings nb in Enum.GetValues(typeof(globals.numbindings)))
            {
                if (nb != globals.ALL_RESULTS[3].numBind) continue;

                if (nb == globals.numbindings.None)
                    soullessComplete[3][2] = (char) globals.gm4_numbindings.None;
                else if (nb == globals.numbindings.One)
                    soullessComplete[3][2] = (char) globals.gm4_numbindings.One;
                else if (nb == globals.numbindings.Two)
                    soullessComplete[3][2] = (char) globals.gm4_numbindings.Two;
                else if (nb == globals.numbindings.Three)
                    soullessComplete[3][2] = (char) globals.gm4_numbindings.Three;
                else if (nb == globals.numbindings.Four)
                    soullessComplete[3][2] = (char) globals.gm4_numbindings.Four;
            }

            soullessComplete[3][0] =
                checksumString(new string(soullessComplete[3], 1, 2));


            foreach (globals.bestclear bc in Enum.GetValues(typeof(globals.bestclear)))
            {
                if (bc != globals.ALL_RESULTS[4].bestClear) continue;

                if (bc == globals.bestclear.None)
                    soullessComplete[4][1] = (char) globals.gm5_bestclear.None;
                else if (bc == globals.bestclear.FiveStar)
                    soullessComplete[4][1] = (char) globals.gm5_bestclear.FiveStar;
                else if (bc == globals.bestclear.FourStar)
                    soullessComplete[4][1] = (char) globals.gm5_bestclear.FourStar;
                else if (bc == globals.bestclear.ThreeStar)
                    soullessComplete[4][1] = (char) globals.gm5_bestclear.ThreeStar;
                else if (bc == globals.bestclear.FullCombo)
                    soullessComplete[4][1] = (char) globals.gm5_bestclear.FullCombo;
            }

            foreach (globals.numbindings nb in Enum.GetValues(typeof(globals.numbindings)))
            {
                if (nb != globals.ALL_RESULTS[4].numBind) continue;

                if (nb == globals.numbindings.None)
                    soullessComplete[4][2] = (char) globals.gm5_numbindings.None;
                else if (nb == globals.numbindings.One)
                    soullessComplete[4][2] = (char) globals.gm5_numbindings.One;
                else if (nb == globals.numbindings.Two)
                    soullessComplete[4][2] = (char) globals.gm5_numbindings.Two;
                else if (nb == globals.numbindings.Three)
                    soullessComplete[4][2] = (char) globals.gm5_numbindings.Three;
                else if (nb == globals.numbindings.Four)
                    soullessComplete[4][2] = (char) globals.gm5_numbindings.Four;
            }

            soullessComplete[4][0] =
                checksumString(new string(soullessComplete[4], 1, 2));

            bool perfect = true;

            for (int i = 0; i < globals.ALL_RESULTS.Length; i++)
            {
                if (globals.ALL_RESULTS[i].bestClear != globals.bestclear.FullCombo ||
                    globals.ALL_RESULTS[i].numBind != globals.numbindings.Four)
                    perfect = false;
            }

            globals.fileSettings.soulless1 = new string(soullessComplete[0]);
            globals.fileSettings.soulless2 = new string(soullessComplete[1]);
            globals.fileSettings.soulless3 = new string(soullessComplete[2]);
            globals.fileSettings.soulless4 = new string(soullessComplete[3]);
            globals.fileSettings.soulless5 = new string(soullessComplete[4]);
            
            if (globals.fileSettings.soulless5.Length == 3 && perfect)
            {
                globals.fileSettings.soulless5 += "e";
            }
            
            combo_mod.log("Successfully serialized new GM data. Please save now!");
        }

        public static globals.gm_challenge_results parseValidString(string s, int level)
        {
            globals.gm_challenge_results g = new globals.gm_challenge_results();

            char best = s.ToCharArray()[1];
            char bind = s.ToCharArray()[2];
            
            switch (level)
            {
                case 0:
                    foreach (globals.gm1_bestclear gmc in Enum.GetValues(typeof(globals.gm1_bestclear)))
                    {
                        if ((char) gmc != best) continue;
                        
                        if (gmc == globals.gm1_bestclear.None)
                            g.bestClear = globals.bestclear.None;
                        else if (gmc == globals.gm1_bestclear.FiveStar)
                            g.bestClear = globals.bestclear.FiveStar;
                        else if (gmc == globals.gm1_bestclear.FourStar)
                            g.bestClear = globals.bestclear.FourStar;
                        else if (gmc == globals.gm1_bestclear.ThreeStar)
                            g.bestClear = globals.bestclear.ThreeStar;
                        else if (gmc == globals.gm1_bestclear.FullCombo)
                            g.bestClear = globals.bestclear.FullCombo;
                    }
                    foreach (globals.gm1_numbindings gmb in Enum.GetValues(typeof(globals.gm1_numbindings)))
                    {
                        if ((char) gmb != bind) continue;
                        
                        if (gmb == globals.gm1_numbindings.None)
                            g.numBind = globals.numbindings.None;
                        else if (gmb == globals.gm1_numbindings.One)
                            g.numBind = globals.numbindings.One;
                        else if (gmb == globals.gm1_numbindings.Two)
                            g.numBind = globals.numbindings.Two;
                        else if (gmb == globals.gm1_numbindings.Three)
                            g.numBind = globals.numbindings.Three;
                        else if (gmb == globals.gm1_numbindings.Four)
                            g.numBind = globals.numbindings.Four;
                    }
                    break;
                case 1:
                    foreach (globals.gm2_bestclear gmc in Enum.GetValues(typeof(globals.gm2_bestclear)))
                    {
                        if ((char) gmc != best) continue;
                        
                        if (gmc == globals.gm2_bestclear.None)
                            g.bestClear = globals.bestclear.None;
                        else if (gmc == globals.gm2_bestclear.FiveStar)
                            g.bestClear = globals.bestclear.FiveStar;
                        else if (gmc == globals.gm2_bestclear.FourStar)
                            g.bestClear = globals.bestclear.FourStar;
                        else if (gmc == globals.gm2_bestclear.ThreeStar)
                            g.bestClear = globals.bestclear.ThreeStar;
                        else if (gmc == globals.gm2_bestclear.FullCombo)
                            g.bestClear = globals.bestclear.FullCombo;
                    }
                    foreach (globals.gm2_numbindings gmb in Enum.GetValues(typeof(globals.gm2_numbindings)))
                    {
                        if ((char) gmb != bind) continue;
                        
                        if (gmb == globals.gm2_numbindings.None)
                            g.numBind = globals.numbindings.None;
                        else if (gmb == globals.gm2_numbindings.One)
                            g.numBind = globals.numbindings.One;
                        else if (gmb == globals.gm2_numbindings.Two)
                            g.numBind = globals.numbindings.Two;
                        else if (gmb == globals.gm2_numbindings.Three)
                            g.numBind = globals.numbindings.Three;
                        else if (gmb == globals.gm2_numbindings.Four)
                            g.numBind = globals.numbindings.Four;
                    }
                    
                    break;
                case 2:
                    foreach (globals.gm3_bestclear gmc in Enum.GetValues(typeof(globals.gm3_bestclear)))
                    {
                        if ((char) gmc != best) continue;
                        
                        if (gmc == globals.gm3_bestclear.None)
                            g.bestClear = globals.bestclear.None;
                        else if (gmc == globals.gm3_bestclear.FiveStar)
                            g.bestClear = globals.bestclear.FiveStar;
                        else if (gmc == globals.gm3_bestclear.FourStar)
                            g.bestClear = globals.bestclear.FourStar;
                        else if (gmc == globals.gm3_bestclear.ThreeStar)
                            g.bestClear = globals.bestclear.ThreeStar;
                        else if (gmc == globals.gm3_bestclear.FullCombo)
                            g.bestClear = globals.bestclear.FullCombo;
                    }
                    foreach (globals.gm3_numbindings gmb in Enum.GetValues(typeof(globals.gm3_numbindings)))
                    {
                        if ((char) gmb != bind) continue;
                        
                        if (gmb == globals.gm3_numbindings.None)
                            g.numBind = globals.numbindings.None;
                        else if (gmb == globals.gm3_numbindings.One)
                            g.numBind = globals.numbindings.One;
                        else if (gmb == globals.gm3_numbindings.Two)
                            g.numBind = globals.numbindings.Two;
                        else if (gmb == globals.gm3_numbindings.Three)
                            g.numBind = globals.numbindings.Three;
                        else if (gmb == globals.gm3_numbindings.Four)
                            g.numBind = globals.numbindings.Four;
                    }
                    
                    break;
                
                case 3:
                    foreach (globals.gm4_bestclear gmc in Enum.GetValues(typeof(globals.gm4_bestclear)))
                    {
                        if ((char) gmc != best) continue;
                        
                        if (gmc == globals.gm4_bestclear.None)
                            g.bestClear = globals.bestclear.None;
                        else if (gmc == globals.gm4_bestclear.FiveStar)
                            g.bestClear = globals.bestclear.FiveStar;
                        else if (gmc == globals.gm4_bestclear.FourStar)
                            g.bestClear = globals.bestclear.FourStar;
                        else if (gmc == globals.gm4_bestclear.ThreeStar)
                            g.bestClear = globals.bestclear.ThreeStar;
                        else if (gmc == globals.gm4_bestclear.FullCombo)
                            g.bestClear = globals.bestclear.FullCombo;
                    }
                    foreach (globals.gm4_numbindings gmb in Enum.GetValues(typeof(globals.gm4_numbindings)))
                    {
                        if ((char) gmb != bind) continue;
                        
                        if (gmb == globals.gm4_numbindings.None)
                            g.numBind = globals.numbindings.None;
                        else if (gmb == globals.gm4_numbindings.One)
                            g.numBind = globals.numbindings.One;
                        else if (gmb == globals.gm4_numbindings.Two)
                            g.numBind = globals.numbindings.Two;
                        else if (gmb == globals.gm4_numbindings.Three)
                            g.numBind = globals.numbindings.Three;
                        else if (gmb == globals.gm4_numbindings.Four)
                            g.numBind = globals.numbindings.Four;
                    }
                    
                    break;
                case 4:
                    foreach (globals.gm5_bestclear gmc in Enum.GetValues(typeof(globals.gm5_bestclear)))
                    {
                        if ((char) gmc != best) continue;
                        
                        if (gmc == globals.gm5_bestclear.None)
                            g.bestClear = globals.bestclear.None;
                        else if (gmc == globals.gm5_bestclear.FiveStar)
                            g.bestClear = globals.bestclear.FiveStar;
                        else if (gmc == globals.gm5_bestclear.FourStar)
                            g.bestClear = globals.bestclear.FourStar;
                        else if (gmc == globals.gm5_bestclear.ThreeStar)
                            g.bestClear = globals.bestclear.ThreeStar;
                        else if (gmc == globals.gm5_bestclear.FullCombo)
                            g.bestClear = globals.bestclear.FullCombo;
                    }
                    foreach (globals.gm5_numbindings gmb in Enum.GetValues(typeof(globals.gm5_numbindings)))
                    {
                        if ((char) gmb != bind) continue;
                        
                        if (gmb == globals.gm5_numbindings.None)
                            g.numBind = globals.numbindings.None;
                        else if (gmb == globals.gm5_numbindings.One)
                            g.numBind = globals.numbindings.One;
                        else if (gmb == globals.gm5_numbindings.Two)
                            g.numBind = globals.numbindings.Two;
                        else if (gmb == globals.gm5_numbindings.Three)
                            g.numBind = globals.numbindings.Three;
                        else if (gmb == globals.gm5_numbindings.Four)
                            g.numBind = globals.numbindings.Four;
                    }
                    
                    break;
                default:
                    return g;
            }
            return g;
        }
        
        
    }
}