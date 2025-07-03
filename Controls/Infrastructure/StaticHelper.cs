using System;
using System.Windows;

namespace OpenFontWPFControls.Controls
{
    internal static class StaticHelper
    {
        public static int GetPreviousWordOffset(string text, int offset)
        {
            if (offset < text.Length && offset >= 0)
            {
                int param = CharType(text[offset]);
                while (true)
                {
                    offset--;
                    if (offset < 0)
                        break;
                    if (param != CharType(text[offset]))
                        break;
                }
            }
            return offset + 1;
        }

        public static int GetNextWordOffset(string text, int offset)
        {
            if (offset < text.Length && offset >= 0)
            {
                int param = CharType(text[offset]);
                while (true)
                {
                    offset++;
                    if (offset >= text.Length)
                        break;
                    if (param != CharType(text[offset]))
                        break;
                }
            }
            return offset;
        }

        public static (int offset, int length) GetWordByOffset(string text, int offset)
        {
            if (text.Length == 0 || offset < 0 || offset >= text.Length)
            {
                return new ValueTuple<int, int>(0, 0);
            }

            int length = text.Length;
            offset = offset < 0 ? 0 : offset < length ? offset : length - 1;
            int current = CharType(text[offset]);
            int left = offset;
            int right = offset;

            while (true)
            {
                right++;
                if (right >= length)
                    break;
                if (current != CharType(text[right]))
                {
                    break;
                }
            }

            while (true)
            {
                left--;
                if (left < 0)
                    break;
                if (current != CharType(text[left]))
                {
                    left++;
                    break;
                }
            }

            return new ValueTuple<int, int>(left, right - left);
        }

        private static int CharType(char c)
        {
            return char.IsSeparator(c) ? 1 : char.IsPunctuation(c) ? 2 : char.IsDigit(c) ? 3 : char.IsLetter(c) ? 4 : 0;
        }

        public static string ClipboardGetText()
        {
            try
            {
                return Clipboard.GetData(DataFormats.UnicodeText) as string;
            }
            catch
            {
                return null;
            }
        }


        public static string StringReplace(string text, string value, int offset, int length)
        {
            char[] result = new char[text.Length - length + value.Length];
            text.CopyTo(0, result, 0, offset);
            value.CopyTo(0, result, offset, value.Length);
            text.CopyTo(offset + length, result, offset + value.Length, text.Length - offset - length);
            return new string(result);
        }

        public static string LineBreaksRemover(string input)
        {
            int i;
            int count = 0;
            for (i = 0; i < input.Length; i++)
            {
                if (input[i] != '\r' && input[i] != '\n')
                {
                    count++;
                }
            }

            int index = 0;
            char[] result = new char[count];
            for (i = 0; i < input.Length; i++)
            {
                if (input[i] != '\r' && input[i] != '\n')
                {
                    result[index++] = input[i];
                }
            }

            return new string(result);
        }
    }
}
