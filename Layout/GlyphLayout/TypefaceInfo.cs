using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Typography.OpenFont;

namespace OpenFontWPFControls.Layout
{
    public class TypefaceInfo : IEquatable<TypefaceInfo>
    {
        public const char ZwjChar = '\u200d';
        public const string DefaultFontFamily = "Segoe UI";
        public const string DefaultExtension = "Segoe UI emoji";

        private static readonly List<TypefaceInfo> CacheInfos = new List<TypefaceInfo>();
        private static readonly List<GlyphLayoutBuilder> CacheBuilders = new List<GlyphLayoutBuilder>();
        private static readonly SemaphoreSlim CacheInfosSem = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim CacheBuildersSem = new SemaphoreSlim(1, 1);

        private GlyphLayoutBuilder _defaultBuilder;
        private GlyphLayoutBuilder _extensionBuilder;

        public string TypefaceName => _defaultBuilder?.Typeface.Name;

        public string ExtensionName => _extensionBuilder?.Typeface.Name;

        public FontStyle Style => _defaultBuilder.GlyphTypeface.Style;

        public FontWeight Weight => _defaultBuilder.GlyphTypeface.Weight;

        public GlyphLayoutBuilder DefaultBuilder => _defaultBuilder;

        public GlyphLayoutBuilder ExtensionBuilder => _extensionBuilder;


        public TypefaceInfo(string name = DefaultFontFamily, string extension = DefaultExtension)
        {
            GlyphTypeface tf = GetGlyphTypeface(name) ?? throw new FileNotFoundException("File font not found");
            GlyphTypeface ex = GetGlyphTypeface(extension);
            Init(tf, ex);
        }

        private TypefaceInfo(GlyphTypeface glyph, GlyphTypeface extension)
        {
            Init(glyph ?? throw new NullReferenceException(), extension);
        }

        private void Init(GlyphTypeface typeface, GlyphTypeface extension = null)
        {
            CacheBuildersSem.Wait();
            try
            {
                _defaultBuilder = CacheBuilders.FirstOrDefault(r => r.GlyphTypeface.Equals(typeface));
                if (_defaultBuilder == null)
                {
                    _defaultBuilder = new GlyphLayoutBuilder(ToOpenFontTypeface(typeface)) { GlyphTypeface = typeface };
                    CacheBuilders.Add(_defaultBuilder);
                }

                if (extension != null)
                {
                    _extensionBuilder = CacheBuilders.FirstOrDefault(r => r.GlyphTypeface.Equals(extension));
                    if (_extensionBuilder == null)
                    {
                        _extensionBuilder = new GlyphLayoutBuilder(ToOpenFontTypeface(extension)) { GlyphTypeface = extension };
                        CacheBuilders.Add(_extensionBuilder);
                    }
                }
            }
            finally
            {
                CacheBuildersSem.Release();
            }
        }

        private static Typography.OpenFont.Typeface ToOpenFontTypeface(GlyphTypeface typeface)
        {
            Typography.OpenFont.Typeface result = null;
            if (typeface != null)
            {
                using Stream stream = typeface.GetFontStream();
                OpenFontReader reader = new OpenFontReader();
                result = reader.Read(stream);
            }
            return result;
        }

        public static TypefaceInfo CacheGetOrTryCreate(string fontFamily = DefaultFontFamily, FontStyle style = default, FontWeight weight = default, string fontFamilyExtension = DefaultExtension)
        {
            CacheInfosSem.Wait();
            try
            {
                TypefaceInfo result = CacheInfos.FirstOrDefault(x => Equals(x, fontFamily, style, weight, fontFamilyExtension));
                if (result == null)
                {
                    result = TryCreate(fontFamily, style, weight, fontFamilyExtension);
                    CacheInfos.Add(result);
                }
                return result;
            }
            finally
            {
                CacheInfosSem.Release();
            }
        }

        public static TypefaceInfo TryCreate(string fontFamily = DefaultFontFamily, FontStyle style = default, FontWeight weight = default, string fontFamilyExtension = DefaultExtension)
        {
            if (GetGlyphTypeface(fontFamily, style, weight) is GlyphTypeface def)
            {
                GlyphTypeface extension = GetGlyphTypeface(fontFamilyExtension);
                return new TypefaceInfo(def, extension);
            }
            return null;
        }

        private static GlyphTypeface GetGlyphTypeface(string family, FontStyle style = default, FontWeight weight = default, FontStretch stretch = default)
        {
            if (family == null)
            {
                return null;
            }
            try
            {
                if (new System.Windows.Media.Typeface(new FontFamily(family), style, weight, stretch).TryGetGlyphTypeface(out GlyphTypeface result))
                    return result;
            }
            catch
            {
                // ignored
            }
            try
            {
                return new GlyphTypeface(new Uri(family));
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public bool Equals(TypefaceInfo other)
        {
            return other != null && Equals(this, other.TypefaceName, other.Style, other.Weight, other.ExtensionName);
        }

        public static bool Equals(TypefaceInfo o, string fontFamily, FontStyle style, FontWeight weight, string fontFamilyExtension)
        {
            return o.Style.Equals(style) && o.Weight.Equals(weight) &&
                   o.TypefaceName.Equals(fontFamily, StringComparison.OrdinalIgnoreCase) &&
                   o.ExtensionName.Equals(fontFamilyExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}