using System;
using System.Threading.Tasks;
using APSToolkit;
using APSToolkit.Auth;
using NUnit.Framework;

namespace ForgeToolkitUnit
{
    [TestFixture]
    public class DerivativeTest
    {
        [SetUp]
        public void SetUp()
        {
            Settings.Token2Leg = Authentication.Get2LeggedToken().Result;
        }

        /// <summary>
        /// Test Download Full SVF Data Model
        /// </summary>
        /// <param name="urn">URN Model</param>
        /// <param name="folder">folder to dowload</param>
        [Test]
        [TestCase("dXJuOmFkc2sud2lwcHJvZDpmcy5maWxlOnZmLjEzLVdVN2NBU2kyQThVdUNqQVFmUkE_dmVyc2lvbj0x", "result")]
        public async Task TestDownloadSVF(string urn, string folder)
        {
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            Console.WriteLine("Start check data process export svf");
            await Derivatives.SaveFileSvfAsync(folder, urn, Settings.Token2Leg.access_token);
            Console.WriteLine("Done process save data svf");
            // check size fodler > 0
            Assert.IsTrue(System.IO.Directory.GetFiles(folder).Length > 0);
        }
    }
}