namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbMapping {
        public string Name { get; set; } 

        public Relationship Relationship { get; set; }
        public string ForeignKeyPath { get; set; }
    }
}