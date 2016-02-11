using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Lazy {
    /// <summary>
    /// Basic implementation of manager, that can get something from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    class LazyDataGetter<TEntity> : IDataGetter<TEntity> where TEntity : class, new() {
        readonly IDataGetter<TEntity> _getter;
        readonly IDataManagerFactory _lazyFactory;
        readonly IEntityInfo _mainEntityInfo;

        public LazyDataGetter(IDataGetter<TEntity> getter, IDataManagerFactory lazyFactory) {
            _getter = getter;
            _lazyFactory = lazyFactory;
            _mainEntityInfo = _lazyFactory.InfoAbout<TEntity>();
        }


        void SetupLazyContext(object entity) {
            var lazyEntity = entity as LazyEntity;
            if (lazyEntity != null) {
                if (_mainEntityInfo.Inclusions == null) {
                    lazyEntity.Read = LazyLoad;
                    return;
                }

                using (lazyEntity.TemporarilyBlockLazyLoading()) {
                    SetupLazyChildren(lazyEntity, _mainEntityInfo, lazyEntity.Context);
                }
            }
        }


        void SetupLazyChildren(LazyEntity entity, IEntityInfo info, LazyEntity.LoadingContext context) {
            if (entity == null)
                return;

            entity.Context = context;
            if (entity.Read == null)
                entity.Read = LazyLoad;

            if (info.Inclusions == null)
                return;

            foreach (var inclusion in _mainEntityInfo.Inclusions) {
                object child;
                if (!entity.TryGetValueOnPath(inclusion.TargetPath, out child)) {
                    Internal.TraceEvent(TraceEventType.Warning, $"Can't get property {inclusion.TargetPath} on {_mainEntityInfo.EntityType} to setup it's loading context.");
                    continue;
                }

                if (!inclusion.IsCollection) {
                    SetupLazyChildren(child as LazyEntity, inclusion.Info, context);
                    continue;
                }


                foreach (var item in (IEnumerable)child) {
                    SetupLazyChildren(item as LazyEntity, inclusion.Info, context);
                }
            }


        }


        void CopyLazyContext(LazyEntity parent, object entity) {
            var lazyEntity = entity as LazyEntity;
            if (lazyEntity != null) {
                lazyEntity.Context = parent.Context;
                lazyEntity.Read = LazyLoad;
            }
        }


        IEnumerable<object> LazyLoad(LazyEntity parent, Type type, ICollection<IDataCriterion> criteria) {
            var info = _lazyFactory.InfoAbout(type);
            var repository = _lazyFactory.ReadingRepository();
            return repository.Read(info, criteria).Perform(item => CopyLazyContext(parent, item));
        }


        #region Implementation of IDataFilter<TEntity,IEnumerable<TEntity>>

        /// <summary>
        /// Returns all <typeparamref name="TEntity"/> what it can get
        /// </summary>
        public IEnumerable<TEntity> All() {
            return _getter.All().Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// Criteria will be getted from <paramref name="context"/>
        /// </summary>
        /// <param name="context">Context of <paramref name="expressions"/></param>
        /// <param name="expressions">Expression that selects filtering criteria</param>
        /// <returns><typeparamref name="TEntity"/> filtered with specified criteria.</returns>
        public IEnumerable<TEntity> By<TContext>(TContext context, params Expression<Func<TContext, object>>[] expressions) {
            return _getter.By(context, expressions).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        public IEnumerable<TEntity> Where(params KeyValuePair<string, object>[] criteria) {
            return _getter.Where(criteria).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        public IEnumerable<TEntity> Where(params KeyValuePair<Expression<Func<TEntity, object>>, object>[] criteria) {
            return _getter.Where(criteria).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member">Expression that defines name of criterion</param>
        /// <param name="value">Value of criterion</param>
        public IEnumerable<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> member, TValue value) {
            return _getter.Where(member, value).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        public IEnumerable<TEntity> Where<T1, T2>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2) {
            return _getter.Where(member_1, value_1, member_2, value_2).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        /// <param name="member_3">Expression that defines name of criterion</param>
        /// <param name="value_3">Value of criterion</param>
        public IEnumerable<TEntity> Where<T1, T2, T3>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2, Expression<Func<TEntity, T3>> member_3, T3 value_3) {
            return _getter.Where(member_1, value_1, member_2, value_2, member_3, value_3).Perform(SetupLazyContext);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        /// <param name="member_3">Expression that defines name of criterion</param>
        /// <param name="value_3">Value of criterion</param>
        /// <param name="member_4">Expression that defines name of criterion</param>
        /// <param name="value_4">Value of criterion</param>
        public IEnumerable<TEntity> Where<T1, T2, T3, T4>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2, Expression<Func<TEntity, T3>> member_3, T3 value_3, Expression<Func<TEntity, T4>> member_4, T4 value_4) {
            return _getter.Where(member_1, value_1, member_2, value_2, member_3, value_3, member_4, value_4).Perform(SetupLazyContext);
        }

        #endregion


        #region Implementation of IDataGetter<TEntity>

        /// <summary>
        /// Selects value <paramref name="member"/> of <typeparamref name="TEntity"/>, getted with followed filtration.
        /// </summary>
        /// <returns><see cref="IDataSelector{TEntity, TValue}"/>. Use some of 'Where' methods of <see cref="IDataSelector{TEntity,TResult}"/> to specify criteria</returns>
        public IDataSelector<TEntity, TValue> Select<TValue>(Expression<Func<TEntity, TValue>> member) {
            return _getter.Select(member);
        }

        #endregion
    }
}