using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFontWPFControls.Controls
{
    partial class TextBox
    {
        private CancellationTokenSource _cancelSpell;
        private List<(int index, int length)> _errors = new List<(int, int)>();

        private void SpellerOnTextChanged()
        {
            if (SpellCheck)
            {
                _errors.Clear();
                _cancelSpell?.Cancel();
                _cancelSpell = new CancellationTokenSource();
                CancellationToken token = _cancelSpell.Token;
                token.ThrowIfCancellationRequested();
                string text = Text;
                string lang = SpellLanguage;
                Task.Run(() => UpdateErrors(text, lang, token), token);
            }
        }

        private Task UpdateErrors(string text, string lang, CancellationToken token)
        {
            List<(int index, int length)> errors = new List<(int, int)>();
            foreach (var error in Speller.GetErrorPoints(text, lang))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                errors.Add(error);
            }

            if (!token.IsCancellationRequested)
            {
                _errors = errors;
                Dispatcher.Invoke(_visualHost.UpdateErrorLayer);
            }

            return Task.CompletedTask;
        }

        private bool CharIsError(int charIndex)
        {
            return _errors.Any(x => charIndex >= x.index && charIndex <= x.index + x.length - 1);
        }

        public IEnumerable<SuggestionPoint> SpellerGetSuggestions(int charIndex)
        {
            if (SpellCheck)
            {
                (int index, int length) word = _errors.FirstOrDefault(x => charIndex >= x.index && charIndex <= x.index + x.length - 1);
                if (word.length > 0)
                {
                    foreach (string suggestion in Speller.GetSuggestions(Text.Substring(word.index, word.length), SpellLanguage))
                    {
                        yield return new SuggestionPoint(word.index, word.length, suggestion);
                    }
                }
            }
        }

        public struct SuggestionPoint
        {
            public int Offset;
            public int Length;
            public string Suggestion;

            public SuggestionPoint(int offset, int length, string suggestion)
            {
                Offset = offset;
                Length = length;
                Suggestion = suggestion;
            }
        }
    }
}
