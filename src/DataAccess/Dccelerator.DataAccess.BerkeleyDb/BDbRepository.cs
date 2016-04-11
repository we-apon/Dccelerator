using System;
using System.Collections.Generic;
using System.Diagnostics;
using BerkeleyDB;
using Dccelerator.DataAccess.BerkeleyDb.Implementation;
using Dccelerator.DataAccess.Infrastructure;

namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbRepository : IBDbRepository {
        readonly IBDbSchema _schema;


        public BDbRepository(IBDbSchema schema) {
            _schema = schema;
        }


        /// <exception cref="NotSupportedException">Can't provide id for <paramref name="entity"/>.</exception>
        protected virtual DatabaseEntry KeyOf(object entity, IBDbEntityInfo info) {
            var bytesIdentified = entity as IIdentified<byte[]>;
            if (bytesIdentified != null)
                return new DatabaseEntry(bytesIdentified.Id);

            var guidIdentified = entity as IIdentified<Guid>;
            if (guidIdentified != null)
                return new DatabaseEntry(guidIdentified.Id.ToByteArray());

            var longIdentified = entity as IIdentified<long>;
            if (longIdentified != null)
                return new DatabaseEntry(BitConverter.GetBytes(longIdentified.Id));

            var intIdentified = entity as IIdentified<int>;
            if (intIdentified != null)
                return new DatabaseEntry(BitConverter.GetBytes(intIdentified.Id));

            throw new NotSupportedException("By default, supported only identifying of entities that implements at least one of the following interfaces:\n" +
                                            $"{nameof(IIdentified<byte[]>)},\n" +
                                            $"{nameof(IIdentified<Guid>)},\n" +
                                            $"{nameof(IIdentified<long>)},\n" +
                                            $"{nameof(IIdentified<int>)}.\n" +
                                            $"Please, implement at least one of these interfaces in entity '{info.EntityType.FullName}', or manually override that logic.");
        }


        protected virtual DatabaseEntry DataOf(object entity) {
            if (entity == null)
                return null;

            return new DatabaseEntry(entity.ToBinnary());
        }


        /// <exception cref="InvalidOperationException">Database already contains entity.</exception>
        protected virtual void Insert(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);

            var cursor = primaryDb.Cursor(transaction);
            if (!cursor.Move(key, exact:true))
                cursor.Add(new KeyValuePair<DatabaseEntry, DatabaseEntry>(key, DataOf(element.Entity)));
            else {
                var message = $"On inserting, database already contains entity with key {key.Data}\n{element.Entity}";
                Internal.TraceEvent(TraceEventType.Error, message);
                throw new InvalidOperationException(message);
            }
        }


        /// <exception cref="InvalidOperationException">Database doesn't contains entity.</exception>
        protected virtual void Update(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);

            var cursor = primaryDb.Cursor(transaction);
            if (cursor.Move(key, exact:true))
                cursor.Overwrite(DataOf(element.Entity));
            else {
                var message = $"On updating, database doesn't contains entity with key {key.Data}\n{element.Entity}";
                Internal.TraceEvent(TraceEventType.Error, message);
                throw new InvalidOperationException(message);
            }
        }


        /// <exception cref="InvalidOperationException">Database doesn't contains entity.</exception>
        /// <exception cref="KeyEmptyException">The element has already been deleted.</exception>
        protected virtual void Delete(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);

            var cursor = primaryDb.Cursor(transaction);
            if (cursor.Move(key, exact:true))
                cursor.Delete();
            else {
                
                var message = $"On deleting, database doesn't contains entity with key {key.Data}\n{element.Entity}";
                Internal.TraceEvent(TraceEventType.Error, message);
                throw new InvalidOperationException(message);
            }
        }




        #region Implementation of IBDbRepository

        public virtual bool IsPrimaryKey(IDataCriterion criterion) {
            return criterion.Name == nameof(IIdentified<byte[]>.Id);
        }


        public virtual DatabaseEntry EntryFrom(IDataCriterion criterion) {
            return new DatabaseEntry(criterion.Value.ToBinnary());
        }


        public virtual IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName) {
            Cursor cursor = null;
            try {
                var primaryDb = _schema.GetPrimaryDb(entityName);

                cursor = primaryDb.Cursor();
                while (cursor.MoveNext())
                    yield return cursor.Current.Value;
            }
            finally {
                cursor?.Close();
            }
        }


        public virtual IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName) {
            var primaryDb = _schema.GetPrimaryDb(entityName);
            var cursor = primaryDb.Cursor();
            if (cursor.Move(key, exact: true))
                yield return cursor.Current.Value;
        }


        public virtual IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, SecondaryKeyAttribute secondaryKey) {
            Cursor cursor = null;
            try {
                var primaryDb = _schema.GetPrimaryDb(entityName);
                var secondaryDb = _schema.GetSecondaryDb(primaryDb, secondaryKey);

                cursor = secondaryDb.Cursor();
                if (!cursor.Move(key, exact: true)) {
                    yield break;
                }

                if (secondaryKey.DuplicatesPolicy == DuplicatesPolicy.NONE) {
                    yield return cursor.Current.Value;
                }
                else {
                    yield return cursor.Current.Value;

                    while (cursor.MoveNextDuplicate())
                        yield return cursor.Current.Value;
                }
            }
            finally {
                cursor?.Close();
            }
        }


        /// <exception cref="InvalidOperationException">Missed secondary key for some criterion.</exception>
        public virtual IEnumerable<DatabaseEntry> GetByJoin(IBDbEntityInfo info, ICollection<IDataCriterion> criteria) {

            var cursorsLength = 0;
            var secondaryCursors = new SecondaryCursor[criteria.Count];
            JoinCursor joinCursor = null;

            try {
                var primaryDb = _schema.GetPrimaryDb(info.EntityName);

                foreach (var criterion in criteria) {
                    SecondaryKeyAttribute secondaryKey;
                    if (!info.SecondaryKeys.TryGetValue(criterion.Name, out secondaryKey)) {
                        var msg = $"Missed secondaty key '{criterion.Name}' for entity {info.EntityName} ({info.EntityType}).";
                        Internal.TraceEvent(TraceEventType.Critical, msg);
                        throw new InvalidOperationException(msg);
                    }

                    var secondaryDb = _schema.GetSecondaryDb(primaryDb, secondaryKey);

                    var cursor = secondaryDb.SecondaryCursor();
                    secondaryCursors[cursorsLength++] = cursor;

                    var key = new DatabaseEntry(criterion.Value.ToBinnary());
                    if (!cursor.Move(key, exact:true))
                        yield break;
                }

                joinCursor = primaryDb.Join(secondaryCursors, true);
                while (joinCursor.MoveNext())
                    yield return joinCursor.Current.Value;
            }
            finally {
                joinCursor?.Close();

                for (var i = 0; i < cursorsLength; i++) {
                    secondaryCursors[i].Close();
                }
            }
        }


        protected virtual DuplicatesPolicy DefaultDuplicatesPolicy => DuplicatesPolicy.UNSORTED;

        
        public virtual bool PerformInTransaction(ICollection<IBDbEntityInfo> entityInfos, IEnumerable<TransactionElement> elements) {

            Transaction transaction = null;

            try {

                transaction = _schema.BeginTransactionFor(entityInfos);
                
                foreach (var element in elements) {
                    var primaryDb = _schema.GetPrimaryDb(element.Info.EntityName);

                    switch (element.ActionType) {
                        case ActionType.Insert:
                            Insert(element, primaryDb, transaction);
                            break;

                        case ActionType.Update:
                            Update(element, primaryDb, transaction);
                            break;

                        case ActionType.Delete:
                            Delete(element, primaryDb, transaction);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
                
                transaction.Commit();
                return true;
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, e.ToString());
                transaction?.Abort();
                return false;
            }
        }

        #endregion
    }
}