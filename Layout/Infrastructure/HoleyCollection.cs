using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenFontWPFControls.Layout
{
    public class HoleyCollection<T> : IEnumerable<T>
    {
        private T[] _source;
        private int _holeStart;
        private int _holeLength;
        private int _count;

        public int Count => _count;

        public T this[int index] => _source[ToRealIndex(index)];

        public HoleyCollection(int count)
        {
            _source = new T[count];
            _holeLength = count;
        }

        public HoleyCollection(T[] source)
        {
            _source = source;
            _count = _source.Length;
        }

        private int ToRealIndex(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new IndexOutOfRangeException();
            }
            return index < _holeStart ? index : index + _holeLength;
        }

        private void BaseRemove(int index, int length = 1)
        {
            if (index < 0 || index >= _count)
            {
                throw new IndexOutOfRangeException();
            }

            if (_holeLength == 0)
            {
                _holeStart = index;
                _holeLength = length = index + length <= _source.Length ? length : _source.Length - index;
                for (int i = index + length - 1; i >= index; i--)
                {
                    _source[i] = default;
                }
            }
            else if (index < _holeStart)
            {
                length = index + length <= _count ? length : _count - index;
                int lastRemoveIndex = index + length - 1;
                if (lastRemoveIndex < _holeStart)
                {
                    for (int i = _holeStart - 1; i > lastRemoveIndex; i--)
                    {
                        _source[i + _holeLength] = _source[i];
                    }
                    for (int i = lastRemoveIndex + _holeLength; i >= index; i--)
                    {
                        _source[i] = default;
                    }
                    _holeStart = index;
                    _holeLength += length;
                }
                else
                {
                    lastRemoveIndex += _holeLength;
                    for (int i = index; i < _holeStart; i++)
                    {
                        _source[i] = default;
                    }
                    for (int i = _holeStart + _holeLength; i <= lastRemoveIndex; i++)
                    {
                        _source[i] = default;
                    }
                    _holeStart = index;
                    _holeLength = lastRemoveIndex - index + 1;
                }
            }
            else
            {
                index += _holeLength;
                for (int i = _holeStart + _holeLength; i < index; i++)
                {
                    _source[i - _holeLength] = _source[i];
                }
                length = index + length <= _source.Length ? length : _source.Length - index;
                _holeStart = index - _holeLength;
                _holeLength += length;
                for (int i = index + length - 1; i >= _holeStart; i--)
                {
                    _source[i] = default;
                }
            }

            _count = _source.Length - _holeLength;
        }

        public void RemoveRange(int index, int length)
        {
            BaseRemove(index, length);
            if (_holeStart != index)
            {
                throw new InvalidOperationException();
            }
        }

        public void Replace(int index, T item)
        {
            _source[ToRealIndex(index)] = item;
        }

        public void Replace(int index, int removeLen, T item)
        {
            RemoveRange(index, removeLen);
            _source[_holeStart] = item;
            _holeStart++;
            _holeLength--;
            _count = _source.Length - _holeLength;
        }

        public void Replace(int index, T[] items)
        {
            Replace(index, 1, items);
        }

        public void Replace(int index, int removeLen, T[] items)
        {
            RemoveRange(index, removeLen);
            if (items.Length > _holeLength)
            {
                T[] buf = new T[_count + items.Length];
                Array.Copy(_source, buf, _holeStart);
                Array.Copy(items, 0, buf, _holeStart, items.Length);
                Array.Copy(_source, _holeStart + _holeLength, buf, _holeStart + items.Length, _source.Length - _holeStart - _holeLength);
                _source = buf;
                _holeStart = _holeLength = 0;
            }
            else
            {
                _holeStart = index + items.Length;
                _holeLength -= items.Length;
                for (int i = 0; i < items.Length; i++)
                {
                    _source[index + i] = items[i];
                }
            }
            _count = _source.Length - _holeLength;
        }

        public void Add(T item)
        {
            if (_holeLength > 0 && _holeStart + _holeLength < _source.Length)
            {
                for (int i = _holeStart + _holeLength; i < _source.Length; i++)
                {
                    _source[i - _holeLength] = _source[i];
                }
                _holeStart = _source.Length - _holeLength;
                for (int i = _holeStart; i < _source.Length; i++)
                {
                    _source[i] = default;
                }
            }
            if (_holeLength == 0)
            {
                T[] buf = new T[_count + 4];
                Array.Copy(_source, buf, _count);
                _source = buf;
                _holeStart = _count;
                _holeLength = 4;
            }
            _source[_count] = item;
            _holeStart++;
            _holeLength--;
            _count = _source.Length - _holeLength;
        }


        // Extensions

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_source, 0, array, arrayIndex, _holeStart);
            Array.Copy(_source, _holeStart + _holeLength, array, arrayIndex + _holeStart, _count - _holeStart);
        }

        public T[] ToArray()
        {
            T[] result = new T[_count];
            Array.Copy(_source, result, _holeStart);
            Array.Copy(_source, _holeStart + _holeLength, result, _holeStart, _count - _holeStart);
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int index = 0;
            while (index < _holeStart)
            {
                yield return _source[index];
                index++;
            }

            index += _holeLength;
            while (index < _source.Length)
            {
                yield return _source[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
