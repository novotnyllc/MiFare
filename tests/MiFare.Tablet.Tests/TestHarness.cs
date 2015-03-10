using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MiFare.Tablet.Tests
{
    public class TestHarness
    {
        [Fact]
        public void ListReadersTest()
        {
            var readers = CardReader.GetReaderNames();

            Assert.Equal(1, readers.Count);
        }
    }
}
