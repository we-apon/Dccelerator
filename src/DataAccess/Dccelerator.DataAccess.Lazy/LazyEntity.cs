using System;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Lazy {
    
    [Serializable]
    public abstract class LazyEntity {

        [NonSerialized]
        LoadingContext _context = new LoadingContext();

        [NotNull]
        internal LoadingContext Context {
            get { return _context ?? (_context = new LoadingContext()); } // if object was deserialized - constructors and fields initializers was not called
            set { _context = value; }
        }


        internal class LoadingContext {
            internal bool IsLoadingAllowed { get; set; } = true;
        }


        class LoadingBlock : IDisposable {
            readonly LoadingContext _context;
            readonly bool _previousState;

            public LoadingBlock(LoadingContext context) {
                _context = context;
                _previousState = _context.IsLoadingAllowed;
                _context.IsLoadingAllowed = false;
            }


            #region Implementation of IDisposable

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() {
                _context.IsLoadingAllowed = _previousState;
            }

            #endregion
        }


        [NonSerialized]
        Func<LazyEntity, Type, ICollection<IDataCriterion>, IEnumerable<object>> _readCallback;

        internal Func<LazyEntity, Type, ICollection<IDataCriterion>, IEnumerable<object>> Read {
            get { return _readCallback; }
            set {
                if (_readCallback != null)
                    throw new InvalidOperationException($"Callback for {nameof(Read)} property already was setted. Fix it's client code, or something..");

                _readCallback = value;
            }
        }

        
        public IDisposable TemporarilyBlockLazyLoading() {
            return new LoadingBlock(Context);
        }

        

    }
}