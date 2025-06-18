using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenFontWPFControls
{
    /// <summary>
    /// Base on Win32 Spell Checking API
    /// </summary>
    public static class Speller
    {
        public static IEnumerable<(int index, int length)> GetErrorPoints(string text, string lang)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SpellCheckAPI.SpellCheckerFactoryClass factory = null;
                SpellCheckAPI.ISpellCheckerFactory ifactory = null;
                SpellCheckAPI.ISpellChecker checker = null;
                SpellCheckAPI.ISpellingError error = null;
                SpellCheckAPI.IEnumSpellingError errors = null;

                try
                {
                    factory = new SpellCheckAPI.SpellCheckerFactoryClass();
                    ifactory = (SpellCheckAPI.ISpellCheckerFactory)factory;

                    if (ifactory.IsSupported(lang) > 0)
                    {
                        checker = ifactory.CreateSpellChecker(lang);
                        errors = checker.Check(text);
                        while (true)
                        {
                            ReleaseComObject(ref error);
                            error = errors.Next();
                            if (error == null)
                            {
                                break;
                            }

                            switch (error.CorrectiveAction)
                            {
                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_DELETE:
                                    break;

                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_REPLACE:
                                    yield return ((int)error.StartIndex, (int)error.Length);
                                    break;

                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_GET_SUGGESTIONS:
                                    yield return ((int)error.StartIndex, (int)error.Length);
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    ReleaseComObject(ref factory);
                    ReleaseComObject(ref ifactory);
                    ReleaseComObject(ref checker);
                    ReleaseComObject(ref error);
                    ReleaseComObject(ref errors);
                }
            }
        }

        public static IEnumerable<string> GetSuggestions(string word, string lang)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                SpellCheckAPI.SpellCheckerFactoryClass factory = null;
                SpellCheckAPI.ISpellCheckerFactory ifactory = null;
                SpellCheckAPI.ISpellChecker checker = null;
                SpellCheckAPI.IEnumString suggestions = null;

                try
                {
                    factory = new SpellCheckAPI.SpellCheckerFactoryClass();
                    ifactory = (SpellCheckAPI.ISpellCheckerFactory)factory;
                    if (ifactory.IsSupported(lang) > 0)
                    {
                        checker = ifactory.CreateSpellChecker(lang);
                        suggestions = checker.Suggest(word);
                        while (true)
                        {
                            suggestions.Next(1, out string suggestion, out uint count);
                            if (count == 1)
                            {
                                yield return suggestion;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    ReleaseComObject(ref suggestions);
                    ReleaseComObject(ref factory);
                    ReleaseComObject(ref ifactory);
                    ReleaseComObject(ref checker);
                }
            }
        }


        public static IEnumerable<Error> GetSpellingErrors(string text, string lang)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SpellCheckAPI.SpellCheckerFactoryClass factory = null;
                SpellCheckAPI.ISpellCheckerFactory ifactory = null;
                SpellCheckAPI.ISpellChecker checker = null;
                SpellCheckAPI.ISpellingError error = null;
                SpellCheckAPI.IEnumSpellingError errors = null;
                SpellCheckAPI.IEnumString suggestions = null;

                try
                {
                    factory = new SpellCheckAPI.SpellCheckerFactoryClass();
                    ifactory = (SpellCheckAPI.ISpellCheckerFactory)factory;

                    if (ifactory.IsSupported(lang) > 0)
                    {
                        checker = ifactory.CreateSpellChecker(lang);
                        errors = checker.Check(text);
                        while (true)
                        {
                            ReleaseComObject(ref error);
                            error = errors.Next();
                            if (error == null)
                            {
                                break;
                            }

                            switch (error.CorrectiveAction)
                            {
                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_DELETE:
                                    break;

                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_REPLACE:
                                    yield return new Error
                                    {
                                        Offset = (int)error.StartIndex, 
                                        Length = (int)error.Length,
                                        Suggestions = new List<string> { error.Replacement }
                                    };
                                    break;

                                case SpellCheckAPI.CORRECTIVE_ACTION.CORRECTIVE_ACTION_GET_SUGGESTIONS:
                                    {
                                        string word = text.Substring((int)error.StartIndex, (int)error.Length);
                                        ReleaseComObject(ref suggestions);
                                        suggestions = checker.Suggest(word);
                                        List<string> variants = new List<string>();
                                        while (true)
                                        {
                                            suggestions.Next(1, out string suggestion, out uint count);
                                            if (count == 1)
                                            {
                                                variants.Add(suggestion);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        yield return new Error
                                        {
                                            Offset = (int)error.StartIndex,
                                            Length = (int)error.Length,
                                            Suggestions = variants
                                        };
                                    }
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    ReleaseComObject(ref suggestions);
                    ReleaseComObject(ref factory);
                    ReleaseComObject(ref ifactory);
                    ReleaseComObject(ref checker);
                    ReleaseComObject(ref error);
                    ReleaseComObject(ref errors);
                }
            }
        }

        private static void ReleaseComObject<T>(ref T o)
        {
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
                o = default;
            }
        }

        public class Error
        {
            public int Offset;
            public int Length;
            public List<string> Suggestions;
        }
    }
}
