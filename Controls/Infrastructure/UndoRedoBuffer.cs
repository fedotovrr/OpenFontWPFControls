using System.Collections.Generic;
using System.Windows;

namespace OpenFontWPFControls.Controls
{
    // Base

    internal class UndoBuffer<T> : BaseUndoBuffer<T>
    {

    }

    internal abstract class BaseUndoBuffer<T>
    {
        private readonly List<T> _stack = new List<T>();
        private int _current = -1;

        public void Add(T value)
        {
            _current++;
            _stack.RemoveRange(_current, _stack.Count - _current);
            _stack.Add(value);
        }

        public bool CanUndo => _current > 0;

        public bool CanRedo => _current < _stack.Count - 1;

        public T MoveNextUndo => CanUndo ? _stack[--_current] : default;

        public T MoveNextRedo => CanRedo ? _stack[++_current] : default;

        public T Current => _current > -1 ? _stack[_current] : default;

        public virtual void Clear()
        {
            _stack.Clear();
            _current = -1;
        }
    }


    // Text fragments buffer

    internal class TextUndoUnit
    {
        public int Offset;
        public string OldValue;
        public string NewValue;

        public TextUndoUnit(int offset = 0, string oldValue = null, string newValue = null)
        {
            Offset = offset;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    internal class TextUndoBuffer : BaseUndoBuffer<TextUndoUnit>
    {
        public TextUndoBuffer()
        {
            Add(new TextUndoUnit()); 
        }

        public delegate void BaseMover(ref string source, out int offset, out int length);

        public void DoUndo(ref string source, out int offset, out int length)
        {
            length = offset = 0;
            if (CanUndo)
            {
                TextUndoUnit cur = Current;
                TextUndoUnit next = MoveNextUndo;
                source = source.Substring(0, cur.Offset) + cur.OldValue + source.Substring(cur.Offset + cur.NewValue.Length);
                offset = cur.Offset;
                length = cur.OldValue.Length;
            }
        }

        public void DoRedo(ref string source, out int offset, out int length)
        {
            length = offset = 0;
            if (CanRedo)
            {
                TextUndoUnit next = MoveNextRedo;
                source = source.Substring(0, next.Offset) + next.NewValue + source.Substring(next.Offset + next.OldValue.Length);
                offset = next.Offset;
                length = next.NewValue.Length;
            }
        }

        public override void Clear()
        {
            base.Clear();
            Add(new TextUndoUnit());
        }
    }


    // Alternative

    internal struct DependencyPropertyUndoUnit
    {
        public DependencyObject Context;
        public DependencyProperty Property;
        public object Value;

        public DependencyPropertyUndoUnit(DependencyObject context, DependencyProperty property, object value)
        {
            Context = context;
            Property = property;
            Value = value;
        }
    }

    internal class DependencyPropertyUndoBuffer : BaseUndoBuffer<DependencyPropertyUndoUnit>
    {
        public void Add(DependencyObject context, DependencyProperty property, object value)
        {
            Add(new DependencyPropertyUndoUnit(context, property, value));
        }

        public void DoUndo()
        {
            if (CanUndo)
            {
                DependencyPropertyUndoUnit unit = MoveNextUndo;
                unit.Context.SetValue(unit.Property, unit.Value);
            }
        }

        public void DoRedo()
        {
            if (CanRedo)
            {
                DependencyPropertyUndoUnit unit = MoveNextRedo;
                unit.Context.SetValue(unit.Property, unit.Value);
            }
        }
    }
}
