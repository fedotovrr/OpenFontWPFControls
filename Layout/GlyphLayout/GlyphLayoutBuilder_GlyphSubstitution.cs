// Original source code
// https://github.com/LayoutFarm/Typography/blob/master/Typography.GlyphLayout/GlyphSubstitution.cs

using System.Collections.Generic;
using Typography.OpenFont.Tables;

namespace OpenFontWPFControls.Layout
{
    public partial class GlyphLayoutBuilder
    {
        private static readonly HashSet<string> _GSubTags = new HashSet<string>
        {
            "ccmp",
            //arabic-related
            "liga","dlig","falt","rclt","rlig","locl","init","medi","fina","isol",
            //math-glyph related
            "math","ssty","dlts","flac",
            //indic script related
            "abvs","akhn","blwf","blws","cjct","half","haln","nukt","pres","psts","rkrf","rphf"
        };

        private readonly bool _enableGsub = true;
        private readonly bool _enableLigature = true;
        private readonly bool _enableComposition = true;
        private readonly List<GSubLkContext> _GSUBLookupTables = new List<GSubLkContext>();

        private void CreateSubstitutionTables()
        {
            GSUB table = _typeface?.GSUBTable;
            ScriptTable scriptTable = table?.ScriptList[_scriptTag];
            if (scriptTable == null) return;

            ScriptTable.LangSysTable selectedLang = null;
            if (_langTag == 0)
            {
                selectedLang = scriptTable.defaultLang;
                if (selectedLang == null && scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
                {
                    selectedLang = scriptTable.langSysTables[0];
                }
            }
            else
            {
                if (_langTag == scriptTable.defaultLang.langSysTagIden)
                {
                    selectedLang = scriptTable.defaultLang;
                }
                if (scriptTable.langSysTables != null && scriptTable.langSysTables.Length > 0)
                {  
                    for (int i = 0; i < scriptTable.langSysTables.Length; ++i)
                    {
                        ScriptTable.LangSysTable s = scriptTable.langSysTables[i];
                        if (s.langSysTagIden == _langTag)
                        {
                            selectedLang = s;
                            break;
                        }
                    }
                }
            }

            if (selectedLang?.featureIndexList == null)
            {
                return;
            }

            foreach (ushort featureIndex in selectedLang.featureIndexList)
            {
                FeatureList.FeatureTable feature = table.FeatureList.featureTables[featureIndex];
                bool includeThisFeature;
                GSubLkContextName contextName = GSubLkContextName.None;
                switch (feature.TagName)
                {
                    case "ccmp": // glyph composition/decomposition 
                        includeThisFeature = _enableComposition;
                        break;
                    case "liga": // Standard Ligatures --enable by default
                        includeThisFeature = _enableLigature;
                        break;
                    case "init":
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Init;
                        break;
                    case "medi":
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Medi;
                        break;
                    case "fina":
                        //Replaces glyphs for characters that have applicable joining properties with an alternate form when occurring in a final context. 
                        //This applies to characters that have one of the following Unicode Joining_Type property 
                        includeThisFeature = true;
                        contextName = GSubLkContextName.Fina;
                        break;
                    default:
                        includeThisFeature = _GSubTags.Contains(feature.TagName);
                        break;
                }

                if (includeThisFeature)
                {
                    foreach (ushort lookupIndex in feature.LookupListIndices)
                    {
                        _GSUBLookupTables.Add(new GSubLkContext(table.LookupList[lookupIndex]) { ContextName = contextName });
                    }
                }
            }
        }

        public void DoSubstitution(IGlyphIndexList glyphIndexList)
        {
            if (_enableGsub)
            {
                foreach (GSubLkContext lookupCtx in _GSUBLookupTables)
                {
                    GSUB.LookupTable lookupTable = lookupCtx.Lookup;
                    lookupCtx.SetGlyphCount(glyphIndexList.Count);
                    for (int pos = 0; pos < glyphIndexList.Count; ++pos)
                    {
                        if (!lookupCtx.WillCheckThisGlyph(pos))
                        {
                            continue;
                        }

                        lookupTable.DoSubstitutionAt(glyphIndexList, pos, glyphIndexList.Count - pos);
                    }
                }
            }
        }

        private enum GSubLkContextName : byte
        {
            None,
            Fina,
            Init,
            Medi
        }

        private class GSubLkContext
        {
            private int _glyphCount;
            public readonly GSUB.LookupTable Lookup;
            public GSubLkContextName ContextName;

            public GSubLkContext(GSUB.LookupTable lookup)
            {
                Lookup = lookup;
            }

            public void SetGlyphCount(int glyphCount)
            {
                _glyphCount = glyphCount;
            }

            public bool WillCheckThisGlyph(int pos)
            {
                switch (ContextName)
                {
                    //the first one
                    case GSubLkContextName.Init: return _glyphCount > 1 && pos == 0;
                    //in between
                    case GSubLkContextName.Medi: return _glyphCount > 2 && pos > 0 && pos < _glyphCount;
                    //the last one
                    case GSubLkContextName.Fina: return _glyphCount > 1 && pos == _glyphCount - 1;
                    default: return true;
                }
            }
        }
    }
}