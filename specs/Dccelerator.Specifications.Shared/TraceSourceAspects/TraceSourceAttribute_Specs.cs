#if DEBUG

using System;
using Dccelerator.TraceSourceAttributes;
using Machine.Specifications;


namespace Dccelerator.Specifications.Shared.TraceSourceAspects
{
    [Subject(typeof(TraceSourceAttribute))]
    [TraceSource(LogCollectionItems = true)]
    public class When_using_it
    {

        public static string SomeProperty { get; set; }


        public string PublicProperty { get; set; }

        public static string SomeMethod(int asd, string[] lines) {
            return $"{asd}, {string.Join("; ", lines)}";
        }

        public static string SomeJsonSerizlizerParameter(object value) {
            return value.ToString();
        }


        public static void MainMethod(int singleParam) {
            SomeMethod(singleParam, new[] { "one", "two", "three" });
            SomeProperty = "asd";
            SomeJsonSerizlizerParameter(new When_using_it {PublicProperty = "asdjjaksdkk asdj skdf"});
            Console.WriteLine(SomeProperty);
        }
        
        /// <summary>
        /// This is integrational test. See resulted logs in file 'TestsLog.svclog' (configured in App.Config) 
        /// </summary>
        It should_do_activity_tracing_on_methods_and_properties = () => {
            MainMethod(42);
            MainMethod(87);
        };
    }

}

#endif