using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class UltraMapperUsageStaticVsRuntimeType
    {
        public class A
        {
            public int A1 { get; set; }
        }

        public class B : A
        {
            public int B1 { get; set; }
        }

        public class C
        {
            public int C1 { get; set; }
        }

        [TestMethod]
        public void SourceStatic()
        {
            var source = new A();

            var mapper = new Mapper();
            var result1 = mapper.Map<A>( source );
            var result2 = mapper.Map<B>( source );

            var result3 = mapper.Map<C>( source );
        }
    }
}
