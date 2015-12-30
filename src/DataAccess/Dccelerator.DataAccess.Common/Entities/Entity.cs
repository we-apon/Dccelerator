namespace Dccelerator.DataAccess.Entities {
    public abstract class Entity /*: IEntity*/ {
        internal LoadingContext LoadingContext;



        protected internal void CopyLoadingContextFrom( Entity other) {
            LoadingContext = other.LoadingContext;
        }


        internal void Internal_AllowLazyLoading(IInternalReadingRepository repository) {
            LoadingContext = new LoadingContext(repository) {IsLazyLoadingAllowed = true};
        }

        internal void Internal_AllowLazyLoading() {
            LoadingContext = new LoadingContext {IsLazyLoadingAllowed = true};
        }


/*        public void ForbidCaching() {
            LoadingContext.DontUseCache = true;
        }*/
    }
}