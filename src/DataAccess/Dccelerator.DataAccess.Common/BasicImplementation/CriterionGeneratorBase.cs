using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.BasicImplementation {
    public abstract class CriterionGeneratorBase<TEntity> where TEntity : class {

        protected static readonly Type EntityType = RUtils<TEntity>.Type;

        // ReSharper disable once StaticMemberInGenericType
        static readonly ConcurrentDictionary<string, object> _expressionMethods = new ConcurrentDictionary<string, object>();



        protected virtual ICollection<IDataCriterion> GetCriteriaFrom<TContext>(TContext context, params Expression<Func<TContext, object>>[] expressions) {
            var criteria = new List<IDataCriterion>(expressions.Length);

            var contextFullName = string.Concat(typeof (TContext).FullName, ".");

            foreach (Expression<Func<TContext, object>> expression in expressions) {
                Func<TContext, object> method;

                var path = expression.Path();
                var identityString = string.Concat(contextFullName, path);
                object func;

                if (_expressionMethods.TryGetValue(identityString, out func))
                    method = (Func<TContext, object>) func;
                else {
                    method = expression.Compile();
                    if (!_expressionMethods.TryAdd(identityString, method))
                        method = (Func<TContext, object>) _expressionMethods[identityString];
                }

                var value = method(context);
                if (value == null)
                    continue;

                criteria.Add(new DataCriterion {
                    Name = path,
                    Value = value,
                    Type = value.GetType()
                });
            }

            return criteria;
        }



        protected virtual ICollection<IDataCriterion> GetCriteriaFrom(KeyValuePair<string, object>[] criteria) {
            var dataCriteria = new List<IDataCriterion>(criteria.Length);
            dataCriteria.AddRange(criteria.Where(x => x.Value != null).Select(x => new DataCriterion {
                Name = x.Key,
                Value = x.Value,
                Type = x.Value.GetType()
            }));
            return dataCriteria;
        }


        protected virtual ICollection<IDataCriterion> GetCriteriaFrom(KeyValuePair<Expression<Func<TEntity, object>>, object>[] criteria) {
            var dataCriteria = new List<IDataCriterion>(criteria.Length);
            dataCriteria.AddRange(criteria.Where(x => x.Value != null).Select(x => new DataCriterion {
                Name = x.Key.Path(),
                Value = x.Value,
                Type = x.Value.GetType()
            }));
            return dataCriteria;
        }


        protected virtual IDataCriterion[] GetCriteriaFrom<TValue>(Expression<Func<TEntity, TValue>> member, TValue value) {
            var criteria = value == null
                ? new IDataCriterion[0]
                : new[] {
                    new DataCriterion {
                        Name = member.Path(),
                        Value = value,
                        Type = value.GetType()
                    }
                };
            return criteria;
        }


        protected virtual List<IDataCriterion> GetCriteriaFrom<T1, T2>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2) {

            var criteria = new List<IDataCriterion>(2);
            if (value_1 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_1.Path(),
                    Value = value_1,
                    Type = value_1.GetType()
                });
            }

            if (value_2 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_2.Path(),
                    Value = value_2,
                    Type = value_2.GetType()
                });
            }
            return criteria;
        }


        protected virtual List<IDataCriterion> GetCriteriaFrom<T1, T2, T3>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2, 
            Expression<Func<TEntity, T3>> member_3, T3 value_3) {

            var criteria = new List<IDataCriterion>(3);
            if (value_1 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_1.Path(),
                    Value = value_1,
                    Type = value_1.GetType()
                });
            }
            if (value_2 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_2.Path(),
                    Value = value_2,
                    Type = value_2.GetType()
                });
            }
            if (value_3 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_3.Path(),
                    Value = value_3,
                    Type = value_3.GetType()
                });
            }
            return criteria;
        }


        protected virtual List<IDataCriterion> GetCriteriaFrom<T1, T2, T3, T4>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3,
            Expression<Func<TEntity, T4>> member_4, T4 value_4) {

            var criteria = new List<IDataCriterion>(4);
            if (value_1 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_1.Path(),
                    Value = value_1,
                    Type = value_1.GetType()
                });
            }
            if (value_2 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_2.Path(),
                    Value = value_2,
                    Type = value_2.GetType()
                });
            }
            if (value_3 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_3.Path(),
                    Value = value_3,
                    Type = value_3.GetType()
                });
            }
            if (value_4 != null) {
                criteria.Add(new DataCriterion {
                    Name = member_4.Path(),
                    Value = value_4,
                    Type = value_4.GetType()
                });
            }
            return criteria;
        }
    }
}