// Original source code
// https://github.com/LayoutFarm/Typography/blob/master/Typography.GlyphLayout/GlyphSetPosition.cs

using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace OpenFontWPFControls.Layout
{
    public partial class GlyphLayoutBuilder
    {
        private readonly bool _enableGpos = true;
        internal List<GPOS.LookupTable> _GPOSLookupTables = new List<GPOS.LookupTable>();

        private void CreateGlyphPositionTables()
        {
            GPOS table = _typeface?.GPOSTable;
            ScriptTable scriptTable = table?.ScriptList[_scriptTag];
            ScriptTable.LangSysTable selectedLang = scriptTable?.defaultLang;
            if (selectedLang == null) return;

            if (_langTag != 0 && scriptTable.langSysTables != null)
            {
                for (int i = 0; i < scriptTable.langSysTables.Length; ++i)
                {
                    if (scriptTable.langSysTables[i].langSysTagIden == _langTag)
                    {
                        selectedLang = scriptTable.langSysTables[i];
                        break;
                    }
                }
            }

            if (selectedLang.featureIndexList == null) return;

            for (int i = 0; i < selectedLang.featureIndexList.Length; ++i)
            {
                FeatureList.FeatureTable feature = table.FeatureList.featureTables[selectedLang.featureIndexList[i]];
                bool includeThisFeature = false;
                switch (feature.TagName)
                {
                    case "mark":
                    case "mkmk":
                        includeThisFeature = true;
                        break;
                    case "kern":
                        includeThisFeature = true;
                        break;
                    case "abvm":
                    case "blwm":
                    case "dist":
                        includeThisFeature = true;
                        break;
                }

                if (includeThisFeature)
                {
                    foreach (ushort lookupIndex in feature.LookupListIndices)
                    {
                        _GPOSLookupTables.Add(table.LookupList[lookupIndex]);
                    }
                }
            }
        }

        public void DoGlyphPosition(IGlyphPositions glyphPositions)
        {
            if (_enableGpos)
            {
                foreach (GPOS.LookupTable context in _GPOSLookupTables)
                {
                    context.DoGlyphPosition(glyphPositions, 0, glyphPositions.Count);
                }
            }
        }
    }
}