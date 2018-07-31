using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dccelerator.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Reports;
using FastMember;
#if !NET40
using BenchmarkDotNet.Exporters.Csv;
#endif


namespace Dccelerator.Specifications.Shared.Core.Reflection {
    public class RUtils_Benchmarks {
        public class MyClass {
            public string Value { get; set; }
            public MyClass Nested { get; set; }

            public int IntValue { get; set; }

        }


        public static MyClass obj = new MyClass();
        public static dynamic dlr = obj;

        public static PropertyInfo prop = typeof(MyClass).GetProperty("Value");
        public static PropertyInfo intProp = typeof(MyClass).GetProperty("IntValue");

        public static PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj)["Value"];
        public static PropertyDescriptor intDescriptor = TypeDescriptor.GetProperties(obj)["IntValue"];

        public static TypeAccessor acessor = TypeAccessor.Create(typeof(MyClass));
        public static ObjectAccessor wrapped = ObjectAccessor.Create(obj);



        public class Config : ManualConfig { // benchmark preset from FastMember project
            public Config() {
#if NET40
                Add(Job.Default.WithLaunchCount(1));
#else
                Add(Job.ShortRun);
#endif

                Add(StatisticColumn.Median, StatisticColumn.StdDev);
                Add(CsvExporter.Default, MarkdownExporter.Default, MarkdownExporter.GitHub);
                Add(new ConsoleLogger());
            }
        }


        static string Info(BenchmarkReport report) {
#if NET40
            return report.Benchmark.Target.MethodTitle;
#else
            return report.BenchmarkCase.Descriptor.DisplayInfo.Substring(nameof(RUtils_Benchmarks).Length + 6).Trim(' ', '\'');
#endif


        }


        public static void Main(string[] args) {
            Expression<Func<object, object>> exp = x => ((MyClass) x).Value.Length;

            var m = new MyClass {
                Value = "ssdf",
                /*Nested = new MyClass {
                    Value = "asd"
                }*/
            };
            RUtils<MyClass>.TrySet(m, "Nested.Nested.Nested.Value", "fdg");
            RUtils<MyClass>.TryGet(m, "Nested.Nested.Nested.Value", out var value);


            RUtils<MyClass>.Set(obj, "Value", "asd");
            RUtils<MyClass>.Get(obj, "Value");

            var summary = BenchmarkRunner.Run<RUtils_Benchmarks>(new Config());
            Console.WriteLine();

            Console.WriteLine("\nString value set and then get");

            var runCount = summary.Reports.Length;// / 2;

            foreach (var report in summary.Reports.Take(runCount).OrderBy(x => x.ResultStatistics.Median)) {
                var ns = $"{report.ResultStatistics.Median:N1} ns";
                var spase = new string(' ', 10 - ns.Length);

                Console.WriteLine($"{ns}{spase}{Info(report)}");
            }

            /*
            Console.WriteLine("\nString value set and then get");
            foreach (var report in summary.Reports.Skip(runCount).Take(runCount).OrderBy(x => x.ResultStatistics.Median)) {
                var ns = $"{report.ResultStatistics.Median:N1} ns";
                var spase = new string(' ', 10 - ns.Length);

                Console.WriteLine($"{ns}{spase}{Info(report)}");
            }*/

            /*
            Console.WriteLine("\nString value set and then get");
            foreach (var report in summary.Reports.Skip(16).OrderBy(x => x.ResultStatistics.Median)) {
                var ns = $"{report.ResultStatistics.Median:N1} ns";
                var spase = new string(' ', 10 - ns.Length);

                Console.WriteLine($"{ns}{spase}{Info(report)}");
            }
            */

            Console.WriteLine();
            Console.ReadLine();

        }



        /*
        [Benchmark(Description = "01. Static C#", Baseline = true)]
        public string StaticCSharp() {
            obj.Value = "abc";
            return obj.Value;
        }

        [Benchmark(Description = "02. Dynamic C#")]
        public string DynamicCSharp() {
            dlr.Value = "abc";
            return dlr.Value;
        }*/

        /*
        [Benchmark(Description = "02. IProperty.Try*Value")]
        public string IProperty() {
            _valueProperty.Setter.Delegate(obj, "asd");
            return _valueProperty.Getter.Delegate(obj);
        }

        [Benchmark(Description = "03. TypeAccessor.Create")]
        public string TypeAccesor() {
            acessor[obj, "Value"] = "abc";
            return (string) acessor[obj, "Value"];
        }*/

        

        /*
        [Benchmark(Description = "04. ObjectAccessor.Create")]
        public string ObjectAccesor() {
            wrapped["Value"] = "abc";
            return (string) wrapped["Value"];
        }*/

        /*
        [Benchmark(Description = "05. object.TryGetValue")]
        public string TryGetValue() {
            ((object)obj).TrySetValueOnPath("Value", "asd");
            ((object)obj).TryGetValueOnPath("Value", out string value);
            return value;
        }*/

            
            

        [Benchmark(Description = "06. RUrils.TryGetValue")]
        public string REtilsTrySetValue() {
            RUtils<MyClass>.TrySet(obj, "Value", "asd");
            RUtils<MyClass>.TryGet(obj, "Value", out object value);
            return (string)value;
        }
            
        

        [Benchmark(Description = "07. RUrils.SetValue")]
        public string REtilsSetValue() {
            RUtils<MyClass>.Set(obj, "Value", "asd");
            return (string) RUtils<MyClass>.Get(obj, "Value");
        }
        


        
        [Benchmark(Description = "08. MyClass.TrySet (Generic)")]
        public string Generic_TrySet_Extension() {
            obj.TrySet("Value", "asd");
            obj.TryGet("Value", out object value);
            return (string) value;
        }
        
        
        [Benchmark(Description = "09. object.TrySet")]
        public string Object_TrySet_Extension() {
            ((object)obj).TrySet("Value", "asd");
            ((object)obj).TryGet("Value", out object value);
            return (string) value;
        }


        [Benchmark(Description = "05. TypeAccessor.Create")]
        public string TypeAccessor_Create() {
            var accessor = TypeAccessor.Create(typeof(MyClass));
            accessor[obj, "Value"] = "asd";
            return (string) accessor[obj, "Value"];
        }
        
        /*
        [Benchmark(Description = "05. RUrils.Set")]
        public string REtilsSet() {
            RUtils<MyClass>.Set(obj, "Value", "asd");
            return (string) RUtils<MyClass>.Get(obj, "Value");
        }*/






        /*
        [Benchmark(Description = "11. Static C# int")]
        public int StaticCSharpInt() {
            obj.IntValue = 1488;
            return obj.IntValue;
        }

        [Benchmark(Description = "12. Dynamic C# int")]
        public int DynamicCSharpInt() {
            dlr.IntValue = 1488;
            return dlr.IntValue;
        }


        [Benchmark(Description = "13. TypeAccessor.Create  get int")]
        public int TypeAccessorInt() {
            acessor[obj, "IntValue"] = 1488;
            return (int) acessor[obj, "IntValue"];
        }

        
        [Benchmark(Description = "14. ObjectAccessor.Create get int")]
        public int ObjectAccessorInt() {
            wrapped["IntValue"] = 1488;
            return (int) wrapped["IntValue"];
        }*/

       /* 
    [Benchmark(Description = "15. Dccelerator.TryGetValue Int")]
    public int TryGetIntValue() {
        obj.TrySetValueOnPath("IntValue", 1488);
        obj.TryGetValueOnPath("IntValue", out object value);
        return (int) value;
    }*/

        /*
    [Benchmark(Description = "16. RUrils.TryGetValue int")]
    public int REtilsTrySetValueInt() {
        RUtils<MyClass>.TrySetValueOnPath(obj, "IntValue", 1488);
        RUtils<MyClass>.TryGetValueOnPath(obj, "IntValue", out int value);
        return value;
    }*/


            /*
        [Benchmark(Description = "20. c# new()")]
        public MyClass CSharpNew() {
            return new MyClass();
        }


        [Benchmark(Description = "21. Activator.CreateInstance")]
        public object ActivatorCreateInstance() {
            return Activator.CreateInstance(typeof(MyClass));
        }


        [Benchmark(Description = "22. TypeAccessor.CreateNew")]
        public object TypeAccessorCreateNew() {
            return acessor.CreateNew();
        }*/




    }
}
