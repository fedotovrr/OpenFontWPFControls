using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace OpenFontWPFControls.Layout
{
    public partial class GlyphLayoutBuilder
    {
        private static readonly ScriptLang _latin = new ScriptLang("latn");
        private static readonly ScriptLang _math = new ScriptLang("math");

        private readonly Typeface _typeface;
        private readonly uint _scriptTag;
        private readonly uint _langTag;
        private readonly bool _enableBuiltinMathItalicCorrection;
        
        private readonly int _baseline;
        public readonly ushort ZwjIndex;

        public Typeface Typeface => _typeface;


        public GlyphLayoutBuilder(
            Typeface typeface,
            bool enableGpos = true,
            bool enableGsub = true,
            bool enableLigature = true,
            bool enableComposition = true,
            bool enableBuiltinMathItalicCorrection = false,
            ScriptLang? scriptLang = null)
        {
            scriptLang = scriptLang ?? new ScriptLang("DFLT");
            _typeface = typeface ?? throw new NullReferenceException();
            _scriptTag = scriptLang.Value.scriptTag;
            _langTag = scriptLang.Value.sysLangTag;
            _enableGpos = enableGpos;
            _enableGsub = enableGsub;
            _enableLigature = enableLigature;
            _enableComposition = enableComposition;
            _enableBuiltinMathItalicCorrection = enableBuiltinMathItalicCorrection;
            
            _baseline = _typeface.CalculateLineSpacing(LineSpacingChoice.Windows);
            ZwjIndex = _typeface.GetGlyphIndex(0x200d, 0, out _);

            CreateSubstitutionTables();
            CreateGlyphPositionTables();
            InitTools();
        }

        public GlyphLayout Build(CharacterBuffer chars)
        {
            chars ??= new StringCharacterBuffer();
            GlyphLayout data = new GlyphLayout(chars.Length);
            if (chars.Length > 0)
            {
                FillGlyphIndexes(chars, data.GlyphPoints);
                DoSubstitution(data);
                foreach (GlyphPoint point in data.GlyphPoints)
                {
                    Glyph glyph = _typeface.GetGlyph(point.GlyphIndex);
                    if (!Glyph.HasOriginalAdvancedWidth(glyph))
                    {
                        Glyph.SetOriginalAdvancedWidth(glyph, _typeface.GetAdvanceWidthFromGlyphIndex(point.GlyphIndex));
                    }
                    point.Width = (short)glyph.OriginalAdvanceWidth;
                    point.GlyphLayoutBuilder = this;
                }
                DoGlyphPosition(data);
                ItalicCorrection(data);
            }
            return data;
        }

        private void FillGlyphIndexes(CharacterBuffer chars, ICollection<GlyphPoint> buffer)
        {
            bool skipNextCodepoint;
            ushort glyphIndex;
            int curOffset = 0;
            int nextOffset = 0;
            int index = 0;
            int next;
            int current = GetCodePoint(chars, ref index);
            while (index < chars.Length)
            {
                nextOffset = index;
                next = GetCodePoint(chars, ref index);
                glyphIndex = _typeface.GetGlyphIndex(current, next, out skipNextCodepoint);
                buffer.Add(new GlyphPoint(glyphIndex, curOffset));
                if (skipNextCodepoint)
                {
                    curOffset = index;
                    current = GetCodePoint(chars, ref index);
                }
                else
                {
                    curOffset = nextOffset;
                    current = next;
                }
            }
            glyphIndex = _typeface.GetGlyphIndex(current, 0, out _);
            buffer.Add(new GlyphPoint(glyphIndex, curOffset));
        }

        private static int GetCodePoint(CharacterBuffer chars, ref int index)
        {
            int codepoint = 0;
            if (index < chars.Length)
            {
                char ch = chars[index];
                codepoint = ch;
                index++;
                if (ch >= 0xd800 && ch <= 0xdbff && index < chars.Length) //high surrogate
                {
                    char nextCh = chars[index];
                    if (nextCh >= 0xdc00 && nextCh <= 0xdfff) //low-surrogate 
                    {
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                        index++;
                    }
                }
            }
            return codepoint;
        }

        private void ItalicCorrection(GlyphLayout data)
        {
            if (_scriptTag == _math.scriptTag)
            {
                if (_enableBuiltinMathItalicCorrection)
                {
                    float noneSigCorrection = 0.33f / _typeface.CalculateScaleToPixelFromPointSize(8);
                    short lastCorrection = 0;
                    for (int i = 0; i < data.GlyphPoints.Count; ++i)
                    {
                        Glyph glyph = _typeface.GetGlyph(data.GlyphPoints[i].GlyphIndex);
                        if (glyph?.MathGlyphInfo?.ItalicCorrection is Typography.OpenFont.MathGlyphs.MathValueRecord value && value.Value > noneSigCorrection)
                        {
                            lastCorrection = value.Value;
                        }
                        else
                        {
                            if (lastCorrection != 0)
                            {
                                ((IGlyphPositions)data).AppendGlyphAdvance(i - 1, lastCorrection, 0);
                            }
                            lastCorrection = 0;
                        }
                    }
                }
            }
        }

    }
}