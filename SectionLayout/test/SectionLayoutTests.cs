using Xunit;
using Elements;
using Elements.Serialization.glTF;
using System.Collections.Generic;

namespace SectionLayout.Tests
{
    public class SectionLayoutTests
    {
        [Fact]
        public void SectionLayoutTest()
        {
            var inputs = new SectionLayoutInputs(20.0, 20.0, 20.0, 20.0, 2.0, 0.5, "", "", new Dictionary<string, string>(), "", "", "");
            var model = Model.FromJson(System.IO.File.ReadAllText("../../../../Envelope.json"));
            var outputs = SectionLayout.Execute(new Dictionary<string, Model> { { "Envelope", model } }, inputs);
            System.IO.File.WriteAllText("../../../../SectionLayout.json", outputs.model.ToJson());
            outputs.model.ToGlTF("../../../../SectionLayout.glb");
        }
    }
}