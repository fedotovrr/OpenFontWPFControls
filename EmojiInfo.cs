using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Collections;

namespace OpenFontWPFControls
{
    /// <summary>
    /// Present emoji as an image
    /// </summary>
    /// <remarks>
    /// Font name Segoe UI emoji<br/>
    /// Conversion EmojiList.txt in this project<br/>
    /// This class is not used in controls of this project, it is puplic helper
    /// </remarks>
    public static class EmojiInfo
    {
        private static readonly int MinChar = int.MaxValue;
        private static readonly int MaxChar = int.MinValue;
        private static readonly List<HashChars> CacheChars = new List<HashChars>();
        private static readonly Hashtable CacheEmoji = new Hashtable();
        private static readonly List<IEmojiItem> _collection;

        static EmojiInfo()
        {
            _collection = new List<IEmojiItem>(FileSourceEnumerator(null, null).Select(e => new EmojiItem(e.name, e.value)));
            foreach (IEmojiItem item in _collection)
            {
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if (CacheChars.Count <= i)
                    {
                        CacheChars.Add(new HashChars());
                    }
                    CacheChars[i].Add(item.Value[i]);
                }
                CacheEmoji.Add(item.Value, item);
            }
            foreach (HashChars h in CacheChars)
            {
                if (h.Min < MinChar) MinChar = h.Min;
                if (h.Max > MaxChar) MaxChar = h.Max;
            }
        }

        /// <summary>
        /// Cache check
        /// </summary>
        public static bool Contains(string value)
        {
            if (string.IsNullOrEmpty(value) || value[0] < MinChar || value[0] > MaxChar) return false;
            for (int i = 0; i < value.Length; i++)
            {
                if (i > CacheChars.Count || !CacheChars[i].Contains(value[i]))
                {
                    return false;
                }
            }
            return CacheEmoji.ContainsKey(value);
        }

        /// <summary>
        /// Get cache item
        /// </summary>
        public static IEmojiItem Get(string value)
        {
            if (string.IsNullOrEmpty(value) || value[0] < MinChar || value[0] > MaxChar)
            {
                return null;
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (i > CacheChars.Count || !CacheChars[i].Contains(value[i]))
                {
                    return null;
                }
            }
            return CacheEmoji[value] as IEmojiItem;
        }

        /// <summary>
        /// String to string/emoji array, emoji cached
        /// </summary>
        public static List<object> EmojiSplitter(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<object> { source };
            }
            List<object> list = new List<object>();
            int i = 0;
            foreach ((int index, IEmojiItem emoji) e in SearchEmoji(source))
            {
                if (source.Substring(i, e.index - i) is string v && !string.IsNullOrEmpty(v))
                {
                    list.Add(v);
                }
                list.Add(e.emoji);
                i = e.index + e.emoji.Value.Length;
            }
            if (source.Substring(i, source.Length - i) is string ev && !string.IsNullOrEmpty(ev))
            {
                list.Add(ev);
            }
            return list;
        }

        /// <summary>
        /// Search emoji in string, emoji cached
        /// </summary>
        public static IEnumerable<(int index, IEmojiItem emoji)> SearchEmoji(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                int index = 0;
                while (index < value.Length)
                {
                    if (value[index] >= MinChar && value[index] <= MaxChar)
                    {
                        int length = 0;
                        int subIndex = index;
                        while (subIndex < value.Length && length < CacheChars.Count && CacheChars[length].Contains(value[subIndex]))
                        {
                            subIndex++;
                            length++;
                        }
                        while (length > 0)
                        {
                            if (CacheEmoji[value.Substring(index, length)] is IEmojiItem o)
                            {
                                yield return (index, o);
                                index += length;
                                break;
                            }
                            length--;
                        }
                        if (length == 0)
                        {
                            index++;
                        }
                    }
                    else
                    {
                        index++;
                    }
                }
            }
        }

        private class HashChars
        {
            private readonly HashSet<char> _chars = new HashSet<char>();
            public int Min = int.MaxValue;
            public int Max = int.MinValue;

            public bool Contains(char c)
            {
                return c >= Min && c <= Max && _chars.Contains(c);
            }

            public void Add(char c)
            {
                if (c < Min) Min = c;
                if (c > Max) Max = c;
                _chars.Add(c);
            }
        }

        public class SubGroup
        {
            public string Name { get; set; }
            public List<IEmojiItem> Items { get; } = new List<IEmojiItem>();
        }

        public class Group
        {
            public string Name { get; set; }
            public List<SubGroup> SubGroups { get; } = new List<SubGroup>();
        }

        public interface IEmojiItem
        {
            string Value { get; }

            ImageSource Source { get; }
        }

        private class EmojiItem : IEmojiItem
        {
            private readonly Lazy<ImageSource> source;
            private readonly string value;
            private readonly string name;

            public string Name => name;

            public string Value => value;

            public ImageSource Source => source.Value;

            public EmojiItem(string name, string value)
            {
                this.value = value;
                this.name = name;
                source = new Lazy<ImageSource>(ImageCreator);
            }

            private ImageSource ImageCreator()
            {
                DrawingImage image = new DrawingImage(DrawingGlyph.DrawText(value));
                image.Freeze();
                return image;
            }
        }

        public static IEnumerable<IEmojiItem> GetEnumerator()
        {
            foreach (IEmojiItem emoji in _collection)
            {
                yield return emoji;
            }
        }


        public static List<Group> GetSystemGroups()
        {
            List<Group> groups = new List<Group>();
            Group tempGroup = new Group();
            SubGroup tempSubGroup = new SubGroup();

            foreach ((string name, string val) in FileSourceEnumerator(AddGroup, AddSubGroup))
            {
                tempSubGroup.Items.Add(Get(val));
            }

            AddSubGroup();
            AddGroup();
            return groups;

            void AddGroup(string group = null)
            {
                if (tempGroup.SubGroups.Count > 0)
                {
                    groups.Add(tempGroup);
                }
                tempGroup = new Group { Name = group };
            }

            void AddSubGroup(string group = null)
            {
                if (tempSubGroup.Items.Count > 0)
                {
                    tempGroup.SubGroups.Add(tempSubGroup);
                }
                tempSubGroup = new SubGroup { Name = group };
            }
        }

        private static IEnumerable<(string name, string value)> FileSourceEnumerator(Action<string> groupCreateCallBack, Action<string> subGroupCreateCallBack)
        {
            string groupHead = "# group: ";
            string subGroupHead = "# subgroup: ";
            foreach (string line in Properties.Resources.EmojiList.Split('\n'))
            {
                if (line.StartsWith(groupHead))
                {
                    groupCreateCallBack?.Invoke(line.Substring(groupHead.Length));
                }
                else if (line.StartsWith(subGroupHead))
                {
                    subGroupCreateCallBack?.Invoke(line.Substring(subGroupHead.Length));
                }
                else if (!line.StartsWith("#") && line.Split(';') is string[] par && par.Length == 3)
                {
                    yield return new(par[2], string.Join("", par[0].Trim(' ').Split(' ').Select(n => char.ConvertFromUtf32(Convert.ToInt32(n, 16)))));
                }
            }
        }
    }
}