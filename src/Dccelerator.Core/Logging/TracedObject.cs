using System;
using System.Collections.Generic;
using Dccelerator.DataAccess;
using JetBrains.Annotations;


namespace Dccelerator.Logging {
    [Serializable]
    public class TracedObject : IIdentified<byte[]> {

        [NotNull]
        public byte[] Id { get; set; }

        [NotNull, ForeignKey(nameof(TraceEvent), nameof(Logging.TraceEvent))]
        public byte[] TraceEventId { get; set; }

        /// <summary>
        /// Full type name (<see cref="System.Type.FullName"/>).
        /// </summary>
        [NotNull, SecondaryKey]
        public string Type { get; set; }

        /// <summary>
        /// Assembly name (<see cref="System.Type.Assembly"/>
        /// </summary>
        [NotNull, SecondaryKey]
        public string Assembly { get; set; }


        [CanBeNull, ForeignKey(nameof(Parent), nameof(TracedObject))]
        public byte[] ParentId { get; set; }


        [CanBeNull]
        public string InParentName { get; set; }



        [NotNull]
        public List<TracedObjectMember> Members {
            get { return _members; }
            set { _members = value; }
        }

        [CanBeNull]
        public TracedObject Parent {
            get { return _parent; }
            set { _parent = value; }
        }


        [CanBeNull]
        public List<TracedObject> IncludedObjects {
            get { return _includedObjects; }
            set { _includedObjects = value; }
        }


        [NotNull]
        public TraceEvent TraceEvent {
            get { return _traceEvent; }
            set { _traceEvent = value; }
        }


        [NonSerialized]
        List<TracedObjectMember> _members = new List<TracedObjectMember>();

        [NonSerialized]
        TracedObject _parent;

        [NonSerialized]
        List<TracedObject> _includedObjects;

        [NonSerialized]
        TraceEvent _traceEvent;
    }
}