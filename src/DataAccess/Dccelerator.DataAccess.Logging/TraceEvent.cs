using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Logging {

    [Serializable]
    public class TraceEvent : IIdentified<byte[]> {


        [NotNull]
        public byte[] Id { get; set; }

        [NotNull, SecondaryKey]
        public byte[] EventId { get; set; }
        
        public DateTime TimeCreated { get; set; }

        [NotNull, SecondaryKey]
        public string Source { get; set; }

        public string Message { get; set; }

        [SecondaryKey]
        public TraceEventType Type { get; set; }



        [CanBeNull]
        public List<TracedObject> RelatedObjects {
            get { return _relatedObjects; }
            set { _relatedObjects = value; }
        }

        [NonSerialized]
        List<TracedObject> _relatedObjects;
    }
}