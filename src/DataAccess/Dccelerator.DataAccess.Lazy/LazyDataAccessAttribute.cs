using System;
using PostSharp;
using PostSharp.Extensibility;
using PostSharp.Reflection;


namespace Dccelerator.DataAccess.Lazy {
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Property, TargetMemberAttributes = MulticastAttributes.Public, AllowMultiple = false)]
    public sealed class LazyDataAccessAttribute : LazyDataAccessAttributeBase {
        public override bool CompileTimeValidate(LocationInfo locationInfo) {
            base.CompileTimeValidate(locationInfo);

            if (locationInfo.LocationKind != LocationKind.Property || !IsAccepted(locationInfo))
                return false;

            Message.Write(MessageLocation.Of(locationInfo), SeverityType.ImportantInfo, "Info", $"'LazyDataAccessAttribute' applied on {locationInfo.DeclaringType.FullName}.{locationInfo.PropertyInfo.Name}");
            return true;
        }


        protected override SeverityType IsAcceptedMessageSeverityType() {
            return SeverityType.Error;
        }
    }
}