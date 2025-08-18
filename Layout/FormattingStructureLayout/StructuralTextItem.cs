using System;
using System.Windows;
using System.Windows.Media;
using OpenFontWPFControls.FormattingStructure;

namespace OpenFontWPFControls.Layout
{
    public class StructuralTextItem
    {
        private object _sourceObject;

        private float _fontSize;
        private Brush _foreground;
        private bool _underline;
        private bool _strike;
        private TypefaceInfo _typefaceInfo;
        private GlyphPoint[] _glyphPoints;

        private bool _hitValue;


        public object SourceObject
        {
            get => _sourceObject;
            set => PropertySetter(ref _sourceObject, value);
        }

        public float FontSize
        {
            get => _fontSize;
            set => PropertySetter(ref _fontSize, value);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => PropertySetter(ref _foreground, value);
        }

        public bool Underline
        {
            get => _underline;
            set => PropertySetter(ref _underline, value);
        }

        public bool Strike
        {
            get => _strike;
            set => PropertySetter(ref _strike, value);
        }

        public string FontFamily
        {
            get => _typefaceInfo?.TypefaceName;
            set => PropertySetter(() => _typefaceInfo?.TypefaceName.Equals(value) == false, ref _typefaceInfo, () => TypefaceInfo.CacheGetOrTryCreate(value, FontStyle, FontWeight, FontExtension));
        }

        public string FontExtension
        {
            get => _typefaceInfo?.ExtensionName;
            set => PropertySetter(() => _typefaceInfo?.ExtensionName.Equals(value) == false, ref _typefaceInfo, () => TypefaceInfo.CacheGetOrTryCreate(FontFamily, FontStyle, FontWeight, value));
        }

        public FontWeight FontWeight
        {
            get => _typefaceInfo?.Weight ?? default;
            set => PropertySetter(() => _typefaceInfo?.Weight.Equals(value) == false, ref _typefaceInfo, () => TypefaceInfo.CacheGetOrTryCreate(FontFamily, FontStyle, value, FontExtension));
        }

        public FontStyle FontStyle
        {
            get => _typefaceInfo?.Style ?? default;
            set => PropertySetter(() => _typefaceInfo?.Style.Equals(value) == false, ref _typefaceInfo, () => TypefaceInfo.CacheGetOrTryCreate(FontFamily, value, FontWeight, FontExtension));
        }


        public string Chars
        {
            get
            {
                switch (_sourceObject)
                {
                    case string str:
                        return str;
                    case IText iText:
                        return iText.Text ?? string.Empty;
                    case IHyperlink hyperlink:
                        return hyperlink.Text ?? string.Empty;
                    case IInlineImage:
                        return " ";
                    default:
                        return string.Empty;
                }
            }
        }

        public bool HitValue
        {
            get => _hitValue;
            set => PropertySetter(ref _hitValue, value);
        }

        public object HitObject
        {
            get
            {
                switch (_sourceObject)
                {
                    case IHyperlink:
                    case IInlineImage:
                        return _sourceObject;
                    default:
                        return null;
                }
            }
        }

        public Brush HitColor
        {
            get
            {
                switch (_sourceObject)
                {
                    case IHyperlink hyperlink:
                        return hyperlink.OverForeground;
                    default:
                        return null;
                }
            }
        }

        public Brush NowColor => HitValue ? HitColor ?? _foreground : _foreground;


        private void PropertySetter<T>(Func<bool> trySet, ref T field, Func<T> valueCreator)
        {
            if (trySet())
            {
                field = valueCreator();
                _glyphPoints = null;
            }
        }

        private void PropertySetter<T>(ref T field, T value)
        {
            //if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                _glyphPoints = null;
            }
        }


        public GlyphPoint this[int index] => GetGlyphPoints()[index];

        public int GlyphsCount => GetGlyphPoints().Length;

        public int GetGlyphLength(int glyphIndex)
        {
            return (glyphIndex + 1 < GlyphsCount ? this[glyphIndex + 1].CharOffset : Chars.Length) - this[glyphIndex].CharOffset;
        }

        public string GetGlyphChars(int glyphIndex)
        {
            return Chars.Substring(this[glyphIndex].CharOffset, GetGlyphLength(glyphIndex));
        }

        private GlyphPoint[] GetGlyphPoints()
        {
            if (_glyphPoints == null && _typefaceInfo != null)
            {
                if (HitObject is IInlineImage img)
                {
                    FontSize = img.Height / (_typefaceInfo.DefaultBuilder.ClipedAscender / _typefaceInfo.DefaultBuilder.Typeface.UnitsPerEm);
                    _glyphPoints = _typefaceInfo.GetGlyphPoints(new StringCharacterBuffer(Chars));
                    GlyphPoint imgPoint = new GlyphPoint(
                        width: (short)(img.Width / _fontSize * _typefaceInfo.DefaultBuilder.Typeface.UnitsPerEm), 
                        glyphLayoutBuilder: _typefaceInfo.DefaultBuilder);
                    _glyphPoints = new GlyphPoint[] { imgPoint };
                }
                else
                {
                    _glyphPoints = _typefaceInfo.GetGlyphPoints(new StringCharacterBuffer(Chars));
                }
            }
            return _glyphPoints;
        }


        public StructuralTextItem GetFormatCopy()
        {
            return new StructuralTextItem
            {
                _sourceObject = SourceObject,
                _fontSize = FontSize,
                _foreground = Foreground,
                _underline = Underline,
                _strike = Strike,
                _typefaceInfo = TypefaceInfo.CacheGetOrTryCreate(FontFamily, FontStyle, FontWeight, FontExtension),
                _hitValue = HitValue,
            };
        }

        public static StructuralTextItem CreateDefault(StructuralLayout def)
        {
            return new StructuralTextItem
            {
                _fontSize = def.FontSize,
                _foreground = def.Foreground,
                _underline = def.Underline,
                _strike = def.Strike,
                _typefaceInfo = def.TypefaceInfo,
            };
        }


        // Debug

        public override string ToString() => $"Text: {Chars}";
    }
}



