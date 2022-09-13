using NUnit.Framework;
using runtime.compiler;
using runtime.core;

namespace test
{
    public class Tests
    {
        [SetUp]
        public void Setup() {}

        [Test]
        public void SimpleStructEval() {
            Julia.Eval("struct s end");
        }
        
        
    }
}