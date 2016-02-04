using System;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;


namespace Dccelerator.DataAccess.Lazy {
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Property, TargetMemberAttributes = MulticastAttributes.Public, AllowMultiple = false)]
    public sealed class LazyLazyDataAccessAttribute : LazyDataAccessAttributeBase {
        public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo) {
            base.CompileTimeInitialize(targetLocation, aspectInfo);
            AlreadyLoaded = !IsAccepted(targetLocation);
        }


        protected override SeverityType IsAcceptedMessageSeverityType() {
            return SeverityType.ImportantInfo;
        }
    }
}