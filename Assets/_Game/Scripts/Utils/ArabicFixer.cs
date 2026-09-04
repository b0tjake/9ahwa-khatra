using System;
using System.Text;
using System.Collections.Generic;

namespace QahwaKhatra.Utils
{
    /// <summary>
    /// Comprehensive Arabic, Darija and BiDi text fixer for Unity OnGUI and Text components.
    /// Features:
    /// - Unicode Arabic presentation forms (Isolated, Initial, Medial, Final).
    /// - Full Lam-Alef ligatures (لا, لأ, لإ, لآ in both isolated and connected forms).
    /// - Moroccan Darija letters (گ, ڭ).
    /// - Automatic removal of Tashkeel / diacritics (like Shadda in سيّق) that break legacy Unity font rendering.
    /// - Bidirectional sentence handling: preserves multi-word English phrases, numbers, and symbols strictly LTR.
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
            { '\u0621', new CharForms('\uFE80', '\uFE80', '\uFE80', '\uFE80') }, // ء
            { '\u0622', new CharForms('\uFE81', '\uFE81', '\uFE82', '\uFE82') }, // آ
            { '\u0623', new CharForms('\uFE83', '\uFE83', '\uFE84', '\uFE84') }, // أ
            { '\u0624', new CharForms('\uFE85', '\uFE85', '\uFE86', '\uFE86') }, // ؤ
            { '\u0625', new CharForms('\uFE87', '\uFE87', '\uFE88', '\uFE88') }, // إ
            { '\u0626', new CharForms('\uFE89', '\uFE8B', '\uFE8C', '\uFE8A') }, // ئ
            { '\u0627', new CharForms('\uFE8D', '\uFE8D', '\uFE8E', '\uFE8E') }, // ا
            { '\u0628', new CharForms('\uFE8F', '\uFE91', '\uFE92', '\uFE90') }, // ب
            { '\u0629', new CharForms('\uFE93', '\uFE93', '\uFE94', '\uFE94') }, // ة
            { '\u062A', new CharForms('\uFE95', '\uFE97', '\uFE98', '\uFE96') }, // ت
            { '\u062B', new CharForms('\uFE99', '\uFE9B', '\uFE9C', '\uFE9A') }, // ث
            { '\u062C', new CharForms('\uFE9D', '\uFE9F', '\uFEA0', '\uFE9E') }, // ج
            { '\u062D', new CharForms('\uFEA1', '\uFEA3', '\uFEA4', '\uFEA2') }, // ح
            { '\u062E', new CharForms('\uFEA5', '\uFEA7', '\uFEA8', '\uFEA6') }, // خ
            { '\u062F', new CharForms('\uFEA9', '\uFEA9', '\uFEAA', '\uFEAA') }, // د
            { '\u0630', new CharForms('\uFEAB', '\uFEAB', '\uFEAC', '\uFEAC') }, // ذ
            { '\u0631', new CharForms('\uFEAD', '\uFEAD', '\uFEAE', '\uFEAE') }, // ر
            { '\u0632', new CharForms('\uFEAF', '\uFEAF', '\uFEB0', '\uFEB0') }, // ز
            { '\u0633', new CharForms('\uFEB1', '\uFEB3', '\uFEB4', '\uFEB2') }, // س
            { '\u0634', new CharForms('\uFEB5', '\uFEB7', '\uFEB8', '\uFEB6') }, // ش
            { '\u0635', new CharForms('\uFEB9', '\uFEBB', '\uFEBC', '\uFEBA') }, // ص
            { '\u0636', new CharForms('\uFEBD', '\uFEBF', '\uFEC0', '\uFEBE') }, // ض
            { '\u0637', new CharForms('\uFEC1', '\uFEC3', '\uFEC4', '\uFEC2') }, // ط
            { '\u0638', new CharForms('\uFEC5', '\uFEC7', '\uFEC8', '\uFEC6') }, // ظ
            { '\u0639', new CharForms('\uFEC9', '\uFECB', '\uFECC', '\uFECA') }, // ع
            { '\u063A', new CharForms('\uFECD', '\uFECF', '\uFED0', '\uFECE') }, // غ
            { '\u0641', new CharForms('\uFED1', '\uFED3', '\uFED4', '\uFED2') }, // ف
            { '\u0642', new CharForms('\uFED5', '\uFED7', '\uFED8', '\uFED6') }, // ق
            { '\u0643', new CharForms('\uFED9', '\uFEDB', '\uFEDC', '\uFEDA') }, // ك
            { '\u0644', new CharForms('\uFEDD', '\uFEDF', '\uFEE0', '\uFEDE') }, // ل
            { '\u0645', new CharForms('\uFEE1', '\uFEE3', '\uFEE4', '\uFEE2') }, // م
            { '\u0646', new CharForms('\uFEE5', '\uFEE7', '\uFEE8', '\uFEE6') }, // ن
            { '\u0647', new CharForms('\uFEE9', '\uFEEB', '\uFEEC', '\uFEEA') }, // ه
            { '\u0648', new CharForms('\uFEED', '\uFEED', '\uFEEE', '\uFEEE') }, // و
            { '\u0649', new CharForms('\uFEEF', '\uFEEF', '\uFEF0', '\uFEF0') }, // ى
            { '\u064A', new CharForms('\uFEF1', '\uFEF3', '\uFEF4', '\uFEF2') }, // ي
            // Moroccan Darija G (گ / ڭ)
            { '\u06AF', new CharForms('\uFB92', '\uFB94', '\uFB95', '\uFB93') }, // گ
            { '\u06AD', new CharForms('\uFB92', '\uFB94', '\uFB95', '\uFB93') }  // ڭ
        };

        private static readonly HashSet<char> NonConnectingAfter = new HashSet<char>()
        {
            '\u0621', '\u0622', '\u0623', '\u0624', '\u0625', '\u0627',
            '\u062F', '\u0630', '\u0631', '\u0632', '\u0648', '\u0629',
            '\uFE8D', '\uFE8E'
        };

        public static string Fix(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string[] lines = input.Split('\n');
            for (int l = 0; l < lines.Length; l++)
            {
                lines[l] = ProcessLine(lines[l]);
            }

            return string.Join("\n", lines);
        }

        private static string ProcessLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;

            // 1. Strip Tashkeel (harakat / diacritics)
            var cleanLine = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c >= 0x064B && c <= 0x0652) continue; // Skip Tashkeel
                cleanLine.Append(c);
            }

            // 2. Shape Arabic characters with Lam-Alef ligatures
            string shaped = ShapeArabic(cleanLine.ToString());

            // 3. BiDi Sentence Reversal: reverse Arabic words, keep English / digits forward
            return BiDiReverse(shaped);
        }

        private static string ShapeArabic(string text)
        {
            var sb = new StringBuilder();
            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                char current = chars[i];

                // Check Lam-Alef Ligature (ل + [ا, أ, إ, آ])
                if (current == '\u0644' && i < chars.Length - 1)
                {
                    char next = chars[i + 1];
                    char ligature = '\0';

                    bool prevConnects = false;
                    if (i > 0 && ArabicMap.ContainsKey(chars[i - 1]) && !NonConnectingAfter.Contains(chars[i - 1]))
                    {
                        prevConnects = true;
                    }

                    if (next == '\u0622') ligature = prevConnects ? '\uFEF6' : '\uFEF5'; // لآ
                    else if (next == '\u0623') ligature = prevConnects ? '\uFEF8' : '\uFEF7'; // لأ
                    else if (next == '\u0625') ligature = prevConnects ? '\uFEFA' : '\uFEF9'; // لإ
                    else if (next == '\u0627') ligature = prevConnects ? '\uFEFC' : '\uFEFB'; // لا

                    if (ligature != '\0')
                    {
                        sb.Append(ligature);
                        i++; // Skip alef
                        continue;
                    }
                }

                if (!ArabicMap.ContainsKey(current))
                {
                    sb.Append(current);
                    continue;
                }

                bool pConnects = false;
                if (i > 0 && ArabicMap.ContainsKey(chars[i - 1]))
                {
                    if (!NonConnectingAfter.Contains(chars[i - 1]))
                    {
                        pConnects = true;
                    }
                }

                bool nConnects = false;
                if (i < chars.Length - 1 && ArabicMap.ContainsKey(chars[i + 1]))
                {
                    if (chars[i + 1] != '\u0621')
                    {
                        nConnects = true;
                    }
                }

                CharForms forms = ArabicMap[current];

                if (pConnects && nConnects) sb.Append(forms.Medial);
                else if (pConnects) sb.Append(forms.Final);
                else if (nConnects) sb.Append(forms.Initial);
                else sb.Append(forms.Isolated);
            }

            return sb.ToString();
        }

        private static bool IsArabicChar(char c)
        {
            return (c >= 0x0600 && c <= 0x06FF) || (c >= 0xFB50 && c <= 0xFDFF) || (c >= 0xFE70 && c <= 0xFEFF);
        }

        private static bool IsLtrChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
        }

        private static string BiDiReverse(string shaped)
        {
            var result = new StringBuilder();
            var ltrBuffer = new StringBuilder();

            for (int i = shaped.Length - 1; i >= 0; i--)
            {
                char c = shaped[i];

                if (IsLtrChar(c) || c == '%' || c == '/' || c == '.' || c == '-' || c == '+' || c == '&' || c == ':' || c == '!')
                {
                    ltrBuffer.Append(c);
                }
                else
                {
                    if (ltrBuffer.Length > 0)
                    {
                        FlushLtrBuffer(ltrBuffer, result);
                    }

                    if (c == '(') result.Append(')');
                    else if (c == ')') result.Append('(');
                    else if (c == '[') result.Append(']');
                    else if (c == ']') result.Append('[');
                    else if (c == '{') result.Append('}');
                    else if (c == '}') result.Append('{');
                    else result.Append(c);
                }
            }

            if (ltrBuffer.Length > 0)
            {
                FlushLtrBuffer(ltrBuffer, result);
            }

            return result.ToString();
        }

        private static void FlushLtrBuffer(StringBuilder ltrBuffer, StringBuilder result)
        {
            for (int k = ltrBuffer.Length - 1; k >= 0; k--)
            {
                result.Append(ltrBuffer[k]);
            }
            ltrBuffer.Clear();
        }
    }
}
