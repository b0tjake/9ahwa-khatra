using System;
using System.Text;
using System.Collections.Generic;

namespace QahwaKhatra.Utils
{
    /// <summary>
    /// Lightweight Arabic and Darija shaper + RTL fixer for Unity UI / OnGUI.
    /// Handles character joining (isolated, initial, medial, final) and reverses text for proper RTL display.
    /// </summary>
    public static class ArabicFixer
    {
        private struct CharForms
        {
            public char Isolated;
            public char Initial;
            public char Medial;
            public char Final;

            public CharForms(char iso, char ini, char med, char fin)
            {
                Isolated = iso;
                Initial = ini;
                Medial = med;
                Final = fin;
            }
        }

        private static readonly Dictionary<char, CharForms> ArabicMap = new Dictionary<char, CharForms>()
        {
            { 'ء', new CharForms('\uFE80', '\uFE80', '\uFE80', '\uFE80') },
            { 'آ', new CharForms('\uFE81', '\uFE81', '\uFE82', '\uFE82') },
            { 'أ', new CharForms('\uFE83', '\uFE83', '\uFE84', '\uFE84') },
            { 'ؤ', new CharForms('\uFE85', '\uFE85', '\uFE86', '\uFE86') },
            { 'إ', new CharForms('\uFE87', '\uFE87', '\uFE88', '\uFE88') },
            { 'ئ', new CharForms('\uFE89', '\uFE8B', '\uFE8C', '\uFE8A') },
            { 'ا', new CharForms('\uFE8D', '\uFE8D', '\uFE8E', '\uFE8E') },
            { 'ب', new CharForms('\uFE8F', '\uFE91', '\uFE92', '\uFE90') },
            { 'ة', new CharForms('\uFE93', '\uFE93', '\uFE94', '\uFE94') },
            { 'ت', new CharForms('\uFE95', '\uFE97', '\uFE98', '\uFE96') },
            { 'ث', new CharForms('\uFE99', '\uFE9B', '\uFE9C', '\uFE9A') },
            { 'ج', new CharForms('\uFE9D', '\uFE9F', '\uFEA0', '\uFE9E') },
            { 'ح', new CharForms('\uFEA1', '\uFEA3', '\uFEA4', '\uFEA2') },
            { 'خ', new CharForms('\uFEA5', '\uFEA7', '\uFEA8', '\uFEA6') },
            { 'د', new CharForms('\uFEA9', '\uFEA9', '\uFEAA', '\uFEAA') },
            { 'ذ', new CharForms('\uFEAB', '\uFEAB', '\uFEAC', '\uFEAC') },
            { 'ر', new CharForms('\uFEAD', '\uFEAD', '\uFEAE', '\uFEAE') },
            { 'ز', new CharForms('\uFEAF', '\uFEAF', '\uFEB0', '\uFEB0') },
            { 'س', new CharForms('\uFEB1', '\uFEB3', '\uFEB4', '\uFEB2') },
            { 'ش', new CharForms('\uFEB5', '\uFEB7', '\uFEB8', '\uFEB6') },
            { 'ص', new CharForms('\uFEB9', '\uFEBB', '\uFEBC', '\uFEBA') },
            { 'ض', new CharForms('\uFEBD', '\uFEBF', '\uFEC0', '\uFEBE') },
            { 'ط', new CharForms('\uFEC1', '\uFEC3', '\uFEC4', '\uFEC2') },
            { 'ظ', new CharForms('\uFEC5', '\uFEC7', '\uFEC8', '\uFEC6') },
            { 'ع', new CharForms('\uFEC9', '\uFECB', '\uFECC', '\uFECA') },
            { 'غ', new CharForms('\uFECD', '\uFECF', '\uFED0', '\uFECE') },
            { 'ف', new CharForms('\uFED1', '\uFED3', '\uFED4', '\uFED2') },
            { 'ق', new CharForms('\uFED5', '\uFED7', '\uFED8', '\uFED6') },
            { 'ك', new CharForms('\uFED9', '\uFEDB', '\uFEDC', '\uFEDA') },
            { 'ل', new CharForms('\uFEDD', '\uFEDF', '\uFEE0', '\uFEDE') },
            { 'م', new CharForms('\uFEE1', '\uFEE3', '\uFEE4', '\uFEE2') },
            { 'ن', new CharForms('\uFEE5', '\uFEE7', '\uFEE8', '\uFEE6') },
            { 'ه', new CharForms('\uFEE9', '\uFEEB', '\uFEEC', '\uFEEA') },
            { 'و', new CharForms('\uFEED', '\uFEED', '\uFEEE', '\uFEEE') },
            { 'ى', new CharForms('\uFEEF', '\uFEEF', '\uFEF0', '\uFEF0') },
            { 'ي', new CharForms('\uFEF1', '\uFEF3', '\uFEF4', '\uFEF2') },
            // Moroccan Darija G (گ / ڭ)
            { 'گ', new CharForms('\uFB92', '\uFB94', '\uFB95', '\uFB93') },
            { 'ڭ', new CharForms('\uFB92', '\uFB94', '\uFB95', '\uFB93') }
        };

        private static readonly HashSet<char> NonConnectingAfter = new HashSet<char>()
        {
            'ء', 'آ', 'أ', 'ؤ', 'إ', 'ا', 'د', 'ذ', 'ر', 'ز', 'و', 'ة', '\uFE8D', '\uFE8E'
        };

        public static string Fix(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Split into lines to preserve multi-line layouts
            string[] lines = input.Split('\n');
            for (int l = 0; l < lines.Length; l++)
            {
                lines[l] = ShapeAndReverseLine(lines[l]);
            }

            return string.Join("\n", lines);
        }

        private static string ShapeAndReverseLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;

            char[] chars = line.ToCharArray();
            char[] shaped = new char[chars.Length];

            // 1. Shape Arabic characters (connecting letters)
            for (int i = 0; i < chars.Length; i++)
            {
                char current = chars[i];

                if (!ArabicMap.ContainsKey(current))
                {
                    shaped[i] = current;
                    continue;
                }

                bool prevConnects = false;
                if (i > 0 && ArabicMap.ContainsKey(chars[i - 1]))
                {
                    if (!NonConnectingAfter.Contains(chars[i - 1]))
                    {
                        prevConnects = true;
                    }
                }

                bool nextConnects = false;
                if (i < chars.Length - 1 && ArabicMap.ContainsKey(chars[i + 1]))
                {
                    if (chars[i + 1] != 'ء')
                    {
                        nextConnects = true;
                    }
                }

                CharForms forms = ArabicMap[current];

                if (prevConnects && nextConnects)
                {
                    shaped[i] = forms.Medial;
                }
                else if (prevConnects)
                {
                    shaped[i] = forms.Final;
                }
                else if (nextConnects)
                {
                    shaped[i] = forms.Initial;
                }
                else
                {
                    shaped[i] = forms.Isolated;
                }
            }

            // 2. Reverse for RTL while keeping Latin words / numbers LTR
            return ReverseRTLWithLTRWords(shaped);
        }

        private static string ReverseRTLWithLTRWords(char[] shaped)
        {
            var result = new StringBuilder();
            var ltrBuffer = new StringBuilder();

            for (int i = shaped.Length - 1; i >= 0; i--)
            {
                char c = shaped[i];

                // English letter, digit, or symbol
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '%' || c == '/' || c == '.')
                {
                    ltrBuffer.Append(c);
                }
                else
                {
                    if (ltrBuffer.Length > 0)
                    {
                        // Reverse the LTR word back to readable direction
                        for (int k = ltrBuffer.Length - 1; k >= 0; k--)
                        {
                            result.Append(ltrBuffer[k]);
                        }
                        ltrBuffer.Clear();
                    }

                    // Swap matching brackets in RTL
                    if (c == '(') result.Append(')');
                    else if (c == ')') result.Append('(');
                    else if (c == '[') result.Append(']');
                    else if (c == ']') result.Append('[');
                    else result.Append(c);
                }
            }

            if (ltrBuffer.Length > 0)
            {
                for (int k = ltrBuffer.Length - 1; k >= 0; k--)
                {
                    result.Append(ltrBuffer[k]);
                }
            }

            return result.ToString();
        }
    }
}
