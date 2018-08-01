using System.Collections.Generic;
using System.Linq;


namespace Dccelerator.UnEnumeratedHelpers {
    public class ByteArrayComparer : IEqualityComparer<byte[]> {
        public bool Equals(byte[] left, byte[] right) {
            if (left == null || right == null) {
                return left == right;
            }

            return left.SequenceEqual(right);
        }


        public int GetHashCode(byte[] key) {
            if (key == null)
                return 0;

            var h = 13;
            foreach (var b in key) {
                h *= 7 + b;
            }

            return h;
        }
    }
}