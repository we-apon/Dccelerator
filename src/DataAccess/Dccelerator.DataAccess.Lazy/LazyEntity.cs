using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess.Lazy {

    [Serializable]
    public abstract class LazyEntity {

        [NonSerialized]
        Func<Type, ICollection<IDataCriterion>, IEnumerable<object>> _readCallback;

        internal Func<Type, ICollection<IDataCriterion>, IEnumerable<object>> Read {
            get { return _readCallback; }
            set {
                if (_readCallback != null)
                    throw new InvalidOperationException(/*todo: error message*/);

                _readCallback = value;
            }
        }


/*

        Func<Type, IEntityInfo> _getEntityInfoCallback;
        internal Func<Type, IEntityInfo> GetEntityInfo {
            get { return _getEntityInfoCallback; }
            set {
                if (_getEntityInfoCallback != null)
                    throw new InvalidOperationException(/*todo: error message#1#);

                _getEntityInfoCallback = value;
            }
        }

*/

    }
}