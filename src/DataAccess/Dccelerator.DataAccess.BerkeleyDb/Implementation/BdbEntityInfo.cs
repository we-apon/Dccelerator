using System;
using System.Collections.Generic;
using System.Reflection;
using Dccelerator.DataAccess.Implementation;


namespace Dccelerator.DataAccess.BerkeleyDb.Implementation {
    class BDbEntityInfo : BaseEntityInfo<IBDbRepository>, IBDbEntityInfo {

        public Dictionary<string, PropertyInfo> NavigationProperties { get; }

        public IEnumerable<IIncludeon> Inclusions { get; }


        public BDbEntityInfo(Type type) : base(type) { }


        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj) {
            return Equals(obj as IBDbEntityInfo);
        }


        #region Equality members

        protected bool Equals(IBDbEntityInfo other) {
            return string.Equals(EntityName, other.EntityName) && Equals(EntityType, other.EntityType) && Equals(Repository, other.Repository);
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                var hashCode = EntityName?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (EntityType?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Repository?.GetHashCode() ?? 0);
                return hashCode;
            }
        }


        public static bool operator ==(BDbEntityInfo left, BDbEntityInfo right) {
            return Equals(left, right);
        }


        public static bool operator !=(BDbEntityInfo left, BDbEntityInfo right) {
            return !Equals(left, right);
        }

        #endregion


        #endregion
    }
}