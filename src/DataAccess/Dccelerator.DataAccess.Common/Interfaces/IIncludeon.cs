namespace Dccelerator.DataAccess {
    public interface IIncludeon {

        IEntityInfo Info { get; }

        bool IsCollection { get; }

        string TargetPath { get; }
    }
}