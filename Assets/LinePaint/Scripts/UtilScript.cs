using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UtilScript
{
    public static bool CompareTwoStringHaveSameCharacters(string s1, string s2)
    {
        for (int i = 0; i < s1.Length; i++)
        {
            if (s2.Contains(s1[i]))
            {
                return false;
            }
        }

        return true;
    }

}
