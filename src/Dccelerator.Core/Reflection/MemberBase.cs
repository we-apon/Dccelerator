namespace Dccelerator.Reflection
{
    public abstract class MemberBase
    {
        protected MemberBase(string name, MemberKind kind) {
            Name = name;
            Kind = kind;
        }

        public string Name { get; }
        public MemberKind Kind { get; }
    }



    static class CastExtensions
    {
        public static TOut CastTo<TOut>(this object value) {
            return (TOut) value;
        }
    }
}