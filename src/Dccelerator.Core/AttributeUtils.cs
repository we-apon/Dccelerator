using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dccelerator
{
    /// <summary>
    /// Declarative extensions for <see cref="Attribute"/>'s operations
    /// </summary>
    public static class AttributeUtils
    {
        /// <summary>
        /// Returns single <typeparamref name="T"/> if <paramref name="member"/> has it.
        /// Otherwise returns null.
        /// </summary>
        /// <typeparam name="T"><see cref="Attribute"/>'s type</typeparam>
        /// <param name="member">Member maked with <typeparamref name="T"/></param>
        /// <exception cref="InvalidOperationException">When <paramref name="member"/> contains more than one <typeparamref name="T"/>.</exception>
        /// <seealso cref="GetMany{T}"/>
        public static T Get<T>(this MemberInfo member, bool inherit = true) where T : Attribute {
            return GetMany<T>(member, inherit).SingleOrDefault();
        }



        /// <summary>
        /// Returns collection of <typeparamref name="T"/> if <paramref name="member"/> has it.
        /// Otherwise returns empty collection.
        /// </summary>
        /// <typeparam name="T"><see cref="Attribute"/>'s type</typeparam>
        /// <param name="member">Member maked with <typeparamref name="T"/></param>
        public static IEnumerable<T> GetMany<T>(this MemberInfo member, bool inherit = true) where T : Attribute {
            return member.GetCustomAttributes(typeof (T), inherit).Cast<T>();
        }



#if NET40

#endif


#if !DOTNET

        public static string XmlName(this MemberInfo member) {
            var data = member.GetCustomAttributesData();

#if NET40
            var dataMember = data.SingleOrDefault(x => x.Constructor.DeclaringType?.Name == "DataMemberAttribute");
#else
            var dataMember = data.SingleOrDefault(x => x.AttributeType.Name == "DataMemberAttribute");
#endif

            if (dataMember?.NamedArguments != null) {
                var name = dataMember.NamedArguments.SingleOrDefault(x => x.MemberInfo.Name == "Name");
                return name != null ? (string)name.TypedValue.Value : member.Name;
            }

            return member.Name;
        }

#endif


/*

                    /// <summary>
                    /// Returns <see langword="true"/> if can get <typeparamref name="T"/>, otherwise returns <see langword="false"/>
                    /// </summary>
                    /// <typeparam name="T"><see cref="Attribute"/>'s type</typeparam>
                    /// <param name="member">Member maked with <typeparamref name="T"/></param>
                    /// <param name="attribute"><typeparamref name="T"/> instance</param>
                    /// <exception cref="InvalidOperationException">When <paramref name="member"/> contains more than one <typeparamref name="T"/>.</exception>
                    public static bool TryGet<T>( this MemberInfo member, out T attribute) where T : Attribute {
                        var attributes = Gik.Attributes.Of(member, typeof (T));
                        attribute = attributes.Length == 1 ? (T) attributes[0] : null;
                        return attribute != null;
                    }


                    /// <summary>
                    /// Returns <see langword="true"/> if can get <typeparamref name="T"/>, otherwise returns <see langword="false"/>
                    /// </summary>
                    /// <typeparam name="T"><see cref="Attribute"/>'s type</typeparam>
                    /// <param name="member">Member maked with <typeparamref name="T"/></param>
                    /// <param name="attributes">Collection of <typeparamref name="T"/> instances</param>
                    public static bool TryGet<T>( this MemberInfo member, out IEnumerable<T> attributes) where T : Attribute {
                        var attrs = Gik.Attributes.Of(member, typeof (T));
                        attributes = attrs.Cast<T>();
                        return attrs.Length > 0;
                    }
*/



            /// <summary>
            /// Checks, is <paramref name="member"/> marked with <typeparamref name="T"/>.
            /// <typeparamref name="T"/> should be concrete attribute type. Inheritance is not allowed.
            /// </summary>
            /// <param name="member">Member marked with <typeparamref name="T"/></param>
            /// <seealso cref="IsDefinedAny{T}"/>
            /// <param name="includeInherited">
            /// In .net 4.0: if <see langword="true"/> - also search attribute in ancensor of <paramref name="member"/>. 
            /// In other target frameworks parameter does nothing.
            /// Default is <see langword="true"/>.
            /// </param>
            /// <seealso cref="IsDefinedAny{T}"/>
        public static bool IsDefined<T>(this MemberInfo member, bool includeInherited = true) where T : Attribute {
            return IsDefined(member, typeof (T), includeInherited);
        }


        /// <summary>
        /// Checks, is <paramref name="member"/> marked with attribute of <paramref name="type"/>.
        /// </summary>
        /// <param name="member">Member marked with attribute of <paramref name="type"/>.</param>
        /// <param name="type">Should be concrete attribute type. Inheritance is not allowed.</param>
        /// <param name="includeInherited">
        /// In .net 4.0: if <see langword="true"/> - also search attribute in ancensor of <paramref name="member"/>. 
        /// In other target frameworks parameter does nothing.
        /// Default is <see langword="true"/>.
        /// </param>
        /// <seealso cref="IsDefinedAny"/>
        public static bool IsDefined(this MemberInfo member, Type type, bool includeInherited = true) {
#if NET40
            return Attribute.IsDefined(member, type, includeInherited);
#else
            return member.CustomAttributes.Any(x => x.AttributeType == type); 
#endif
        }

        //todo: research, is MemberInfo.CustomAttributes also returns inherited attributes.
        //todo: research, is it faster to cache IsDefined results, or not.



        /// <summary>
        /// Checks, is <paramref name="member"/> marked with <typeparamref name="T"/>.
        /// <typeparamref name="T"/> can be concrete, or some base type of required attribute.
        /// </summary>
        /// <param name="member">Member marked with <typeparamref name="T"/></param>
        /// <param name="includeInherited">
        /// In .net 4.0: if <see langword="true"/> - also search attribute in ancensor of <paramref name="member"/>. 
        /// In other target frameworks parameter does nothing.
        /// Default is <see langword="true"/>.
        /// </param>
        /// <seealso cref="IsDefined{T}"/>
        public static bool IsDefinedAny<T>(this MemberInfo member, bool includeInherited = true) where T : Attribute {
            return IsDefinedAny(member, typeof (T), includeInherited);
        }



        /// <summary>
        /// Checks, is <paramref name="member"/> marked with attribute of <paramref name="type"/>.
        /// </summary>
        /// <param name="member">Member marked with attribute of <paramref name="type"/></param>
        /// <param name="type">Can be concrete, or some base type of required attribute.</param>
        /// <param name="includeInherited">
        /// In .net 4.0: if <see langword="true"/> - also search attribute in ancensor of <paramref name="member"/>. 
        /// In other target frameworks parameter does nothing.
        /// Default is <see langword="true"/>.
        /// </param>
        /// <seealso cref="IsDefined"/>
        public static bool IsDefinedAny(this MemberInfo member, Type type, bool includeInherited = true) {
#if NET40
            return Attribute.IsDefined(member, type, includeInherited); //todo: fix it
#else
            return member.CustomAttributes.Any(x => type.IsAssignableFrom(type));
#endif
        }
    }
}