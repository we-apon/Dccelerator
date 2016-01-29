﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ServiceStack;


namespace ConsoleApplication1 {

    [Serializable]
    public class SomeEntity {
        /// <summary>
        /// Id of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public Guid Id { get; set; }


        /// <summary>
        /// Name of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public string Name { get; set; }


        /// <summary>
        /// Value of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public string Value { get; set; }

    }



    [Serializable]
    public class SomeOtherEntity {
        public Guid Id { get; set; }

        public Guid SomeEntityId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        
    }
}