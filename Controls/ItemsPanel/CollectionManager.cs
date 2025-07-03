using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace OpenFontWPFControls.Controls
{
    internal class CollectionManager : IWeakEventListener
    {
        private PropertyInfo _reflectedCount;
        private PropertyInfo _reflectedItemAt;
        private MethodInfo _reflectedIndexOf;

        private IList _list;
        private CollectionView _collectionView;

        private object _source;

        private Action _collectionChangedCallBack;


        public object Source => _source;


        public CollectionManager(object collection, Action collectionChangedCallBack)
        {
            switch (collection)
            {
                case null:
                    _list = new List<object>();
                    break;

                case IList list:
                    _list = list;
                    break;

                case CollectionView collectionView:
                    _collectionView = collectionView;
                    break;

                default:
                {
                    Type srcType = collection.GetType();

                    MethodInfo indexer = srcType.GetMethod("IndexOf", new Type[] { typeof(object) });
                    if (indexer != null && indexer.ReturnType == typeof(int))
                    {
                        _reflectedIndexOf = indexer;
                    }

                    MemberInfo[] defaultMembers = srcType.GetDefaultMembers();
                    for (int i = 0; i <= defaultMembers.Length - 1; i++)
                    {
                        PropertyInfo pi = defaultMembers[i] as PropertyInfo;
                        if (pi != null)
                        {
                            ParameterInfo[] indexerParameters = pi.GetIndexParameters();
                            if (indexerParameters.Length == 1)
                            {
                                if (indexerParameters[0].ParameterType.IsAssignableFrom(typeof(int)))
                                {
                                    _reflectedItemAt = pi;
                                    break;
                                }
                            }
                        }
                    }

                    _reflectedCount = srcType.GetProperty("Count", typeof(int));

                    if (_reflectedIndexOf == null || _reflectedItemAt == null || _reflectedCount == null)
                    {
                        throw new ArgumentException("The source object must be a collection with indexer support");
                    }

                    break;
                }
            }

            _source = collection;
            _collectionChangedCallBack = collectionChangedCallBack;

            if (_source is INotifyCollectionChanged notifier)
            {
                CollectionChangedEventManager.AddListener(notifier, this);
            }
        }

        public int Count
        {
            get
            {
                if (_list != null)
                {
                    return _list.Count;
                }

                if (_collectionView != null)
                {
                    return _collectionView.Count;
                }

                if (_reflectedCount != null)
                {
                    return (int)_reflectedCount.GetValue(_source, null);
                }

                return 0;
            }
        }


        public object TryGetItem(int index)
        {
            try
            {
                if (_list != null)
                {
                    return _list[index];
                }

                if (_collectionView != null)
                {
                    return _collectionView.GetItemAt(index);
                }

                if (_reflectedItemAt != null)
                {
                    return _reflectedItemAt.GetValue(_source, new object[] { index });
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public int IndexOf(object item)
        {
            if (_list != null)
            {
                return _list.IndexOf(item);
            }

            if (_collectionView != null)
            {
                return _collectionView.IndexOf(item);
            }

            if (_reflectedIndexOf != null)
            {
                return (int)_reflectedIndexOf.Invoke(_source, new object[] { item });
            }

            return -1;
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is NotifyCollectionChangedEventArgs args)
            {
                _collectionChangedCallBack?.Invoke();
            }
            return true;
        }

        public void Dispose()
        {
            if (_source is INotifyCollectionChanged notifier)
            {
                CollectionChangedEventManager.RemoveListener(notifier, this);
            }

            _reflectedCount = null;
            _reflectedItemAt = null;
            _reflectedIndexOf = null;
            _list = null;
            _collectionView = null;
            _source = null;
            _collectionChangedCallBack = null;
        }
    }
}
