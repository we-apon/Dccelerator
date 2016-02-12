using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Dccelerator.DataAccess;
using Dccelerator.DataAccess.Attributes;
using Dccelerator.DataAccess.Entities;
using Dccelerator.DataAccess.Lazy;


namespace ConsoleApplication1 {

    [LazyLazyDataAccess]
    [Serializable]
    public class SomeEntity : LazyEntity, IIdentifiedEntity {

        /// <summary>
        /// Id of <see cref="SomeEntity"/>
        /// </summary>
        public Guid Id { get; set; }


        /// <summary>
        /// Name of <see cref="SomeEntity"/>
        /// </summary>
        [DataMember(Name = "asdasd", IsRequired = true)]
        [XmlElement("asdas", typeof(LazyEntity))]
        public string Name { get; set; }


        /// <summary>
        /// Value of <see cref="SomeEntity"/>
        /// </summary>
        public string Value { get; set; }


        [NonSerialized]
        List<SomeOtherEntity> _someEntities;
        public List<SomeOtherEntity> SomeEntities {
            get { return _someEntities; }
            set { _someEntities = value; }
        }
    }



    [LazyLazyDataAccess]
    [Serializable]
    public class SomeOtherEntity : LazyEntity, IIdentifiedEntity {

        public Guid Id { get; set; }

        [ForeignKey(Relationship = Relationship.ManyToOne, 
            DuplicatesPolicy = DuplicatesPolicy.UNSORTED, 
            ForeignEntityName = "SomeEntity", 
            ForeignEntityNavigationPath = "SomeEntity")]
        public Guid SomeEntityId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }


        [NonSerialized]
        SomeEntity _someEntity;

        public SomeEntity SomeEntity {
            get { return _someEntity; }
            set { _someEntity = value; }
        }
    }
}