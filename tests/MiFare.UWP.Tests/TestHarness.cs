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
        public async void ListReadersTest()
        {

            var reader = await CardReader.FindAsync();

            Assert.NotNull(reader);
        }
    }
}
