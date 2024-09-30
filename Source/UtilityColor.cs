using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;

namespace EdB.PrepareCarefully {
    public static class UtilityColor {

        public static bool AlmostEqual(Color a, Color b) {
            return CloseEnough(a.r, b.r) && CloseEnough(a.g, b.g) && CloseEnough(a.b, b.b);
        }
        private static bool CloseEnough(float a, float b) {
            if (a > b - 0.0001f && a < b + 0.0001f) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
