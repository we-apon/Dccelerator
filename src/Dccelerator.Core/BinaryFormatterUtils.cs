#if !DOTNET

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Dccelerator {


    public static class BinaryFormatterUtils {

        public static byte[] ToBinnary<T>(this T objct) {
            if (objct == null)
                return null;
            
            var type = objct.GetType();
            if (typeof (Guid?).IsAssignableFrom(type))
                return objct.SafeCastTo<Guid?>().Value.ToByteArray();

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream()) {
                formatter.Serialize(stream, objct);
                return stream.ToArray();
            }
        }


        public static T FromBytes<T>(this byte[] array) {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream()) {
                stream.Write(array, 0, array.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(stream);
            }
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