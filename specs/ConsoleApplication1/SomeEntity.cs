using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Dccelerator.DataAccess.Entities;


namespace ConsoleApplication1 {

    [Serializable]
    public class SomeEntity : IIdentifiedEntity {

        /// <summary>
        /// Id of <see cref="SomeEntity"/>
        /// </summary>
        public Guid Id { get; set; }


        /// <summary>
        /// Name of <see cref="SomeEntity"/>
        /// </summary>
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



    [Serializable]
    public class SomeOtherEntity : IIdentifiedEntity {

        public Guid Id { get; set; }

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