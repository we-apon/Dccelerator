using System;
using System.Collections.Generic;
using System.Text;
using Machine.Specifications;
using Machine.Specifications.Model;

namespace Dccelerator.Specifications.Shared
{
    [Subject("Mamku ibal")]
    class SomeSpec {
        It should_test_some = () => 5.ShouldEqual(7);
    }
}
