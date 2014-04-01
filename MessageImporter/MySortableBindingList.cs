﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MessageImporter
{
    /// <summary>
    /// http://www.codeproject.com/Articles/31418/Implementing-a-Sortable-BindingList-Very-Very-Quic
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MySortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;
        private readonly PropertyDescriptorCollection _propertyDescriptors =
                         TypeDescriptor.GetProperties(typeof(T));


        public MySortableBindingList(IEnumerable<T> enumerable) : base(enumerable.ToList()) { }

        public MySortableBindingList() { }


        protected override bool SupportsSortingCore { get { return true; } }

        protected override bool IsSortedCore { get { return _isSorted; } }

        protected override ListSortDirection SortDirectionCore { get { return _sortDirection; } }

        protected override PropertyDescriptor SortPropertyCore { get { return _sortProperty; } }

        protected override void ApplySortCore(PropertyDescriptor prop,
                                              ListSortDirection direction)
        {
            _isSorted = true;
            _sortDirection = direction;
            _sortProperty = prop;

            Func<T, object> predicate = n => n.GetType().GetProperty(prop.Name)
                                                        .GetValue(n, null);

            ResetItems(_sortDirection == ListSortDirection.Ascending
                           ? Items.OrderBy(predicate)
                           : Items.OrderByDescending(predicate));
        }

        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortDirection = base.SortDirectionCore;
            _sortProperty = base.SortPropertyCore;

            ResetBindings();
        }

        private void ResetItems(IEnumerable<T> items)
        {
            RaiseListChangedEvents = false;
            var tempList = items.ToList();
            ClearItems();

            foreach (var item in tempList) Add(item);

            RaiseListChangedEvents = true;
            ResetBindings();
        }

        public MySortableBindingList<T> Load(IEnumerable<T> enumeration)
        {
            ResetItems(enumeration);
            return this;
        }
    }
}
