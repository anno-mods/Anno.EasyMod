using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Anno.EasyMod.Attributes
{
    public class ModAttributeCollection : IModAttributeCollection
    {
        private List<IModAttribute> _attributes;
        private object _lock = new Object();

        public IModAttribute this[int index] 
        {
            get => _attributes[index];
            set => throw new NotImplementedException();
        }

        public int Count => _attributes.Count;

        bool ICollection<IModAttribute>.IsReadOnly => throw new NotImplementedException();

        public bool IsReadOnly = false;

        public event NotifyCollectionChangedEventHandler? CollectionChanged = delegate { };

        public ModAttributeCollection()
        { 
            _attributes = new List<IModAttribute>();
        }

        public void Add(IModAttribute item)
        {
            if (item is null)
                return;
            lock (_lock)
            {
                if (!item.MultipleAllowed && this.Any(x => x.AttributeType == item.AttributeType))
                    return;
                _attributes.Add(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            _attributes.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(IModAttribute item) => _attributes.Contains(item);

        public void CopyTo(IModAttribute[] array, int arrayIndex)
        {
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException($"Data is too large for the array provided! Needed Size: {Count + arrayIndex}, Actual Size: {array.Length}");

            int i = arrayIndex;
            foreach (var item in _attributes)
            {
                array[i] = item;
                i++;
            }
        }

        public IEnumerator<IModAttribute> GetEnumerator() => _attributes.GetEnumerator();

        public int IndexOf(IModAttribute item) => _attributes.IndexOf(item);

        public void Insert(int index, IModAttribute item)
        {
            if (item is null)
                return;
            lock (_lock)
            {
                if (!item.MultipleAllowed && this.Any(x => x.AttributeType == item.AttributeType))
                    return;
                _attributes.Insert(index, item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public bool Remove(IModAttribute item)
        {
            if (item is null)
                return false;
            bool result;
            lock (_lock)
            {
                result = _attributes.Remove(item);
            }
            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                var element = _attributes.ElementAt(index);
                _attributes.RemoveAt(index); 
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _attributes.GetEnumerator();

        public IEnumerable<IModAttribute> OfAttributeType(string attributeType)
        {
            return this.Where(x => x.AttributeType == attributeType);
        }

        public bool HasAttributeOfType(string attributeType)
        {
            return this.Any(x => x.AttributeType == attributeType);
        }

        public void RemoveByType(string attributeType)
        {
            var toRemove = OfAttributeType(attributeType).ToArray();
            foreach (var item in toRemove)
            {
                _attributes.Remove(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, toRemove));
        }
    }
}
