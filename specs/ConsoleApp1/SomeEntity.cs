using System;
using System.Xml.Serialization;
using Dccelerator;


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


        public byte[] Serialize() {
            return this.ToBinnary();
        }


        public static SomeEntity Deserialize(byte[] bytes) {
            return bytes.FromBytes<SomeEntity>();
        }
    }




    [Serializable]
    public class SomeOtherEntity {
        public Guid Id { get; set; }

        public Guid SomeEntityId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }


        public byte[] Serialize() {
            return this.ToBinnary();
        }


        public static SomeOtherEntity Deserialize(byte[] bytes) {
            return bytes.FromBytes<SomeOtherEntity>();
        }
    }
}