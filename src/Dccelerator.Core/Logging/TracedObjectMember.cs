using System;
using Dccelerator.DataAccess;
using JetBrains.Annotations;


namespace Dccelerator.Logging {
    [Serializable]
    public class TracedObjectMember : IIdentified<byte[]> {
        
        [NotNull]
        public byte[] Id { get; set; }

        
        [NotNull, ForeignKey(nameof(TracedObject), nameof(Logging.TracedObject))]
        public byte[] TracedObjectId { get; set; }

        [NotNull, SecondaryKey]
        public string Name { get; set; }


        [NotNull, SecondaryKey]
        public object Value { get; set; }


        [NotNull]
        public TracedObject TracedObject { get; set; }
    }
}