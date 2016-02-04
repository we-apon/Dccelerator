#if NET40 || NET45

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Dccelerator {


    public static class BinaryFormatterUtils {
        static readonly Type _nullableGuid= typeof (Guid?);


        public static byte[] ToBinnary<T>(this T entity) {
            if (entity == null)
                return null;
            
            var type = entity.GetType();
            if (_nullableGuid.IsAssignableFrom(type))
                return entity.SafeCastTo<Guid?>().Value.ToByteArray();

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream()) {
                formatter.Serialize(stream, entity);
                return stream.ToArray();
            }
        }


        public static T FromBytes<T>(this byte[] array) {
            return (T) FromBytes(array);
        }

        public static object FromBytes(this byte[] bytes) {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream()) {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream);
            }
        }

    }


}

#endif