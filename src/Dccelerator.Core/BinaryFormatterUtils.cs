#if NET40 || NET45

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Dccelerator {


    public static class BinaryFormatterUtils {
        static readonly Type _nullableGuid = typeof (Guid?);
        static readonly Type _byteArrayType = typeof (byte[]);


        public static byte[] ToBinnary<T>(this T entity) {
            if (entity == null)
                return null;
            
            var type = entity.GetType();
            if (_byteArrayType == type)
                return entity as byte[];

            if (_nullableGuid.IsAssignableFrom(type))
                return entity.SafeCastTo<Guid?>()?.ToByteArray();

            using (var stream = new MemoryStream()) {
                try {
                    new BinaryFormatter().Serialize(stream, entity);
                    return stream.ToArray();
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Error, $"Can't serialize entity {entity} to {nameof(Byte)}[].\n\n{e}");
                    return null;
                }
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