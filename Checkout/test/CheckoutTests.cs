using Xunit;
using Elements;
using Elements.Serialization.glTF;
using System.Collections.Generic;

namespace Checkout.Tests
{
    public class CheckoutTests
    {
        [Fact]
        public void CheckoutTest()
        {
            var inputs = new CheckoutInputs(2.0, "", "", new Dictionary<string, string>(), "", "", "");
            var model = Model.FromJson(System.IO.File.ReadAllText("../../../../departments.json"));
            var outputs = Checkout.Execute(new Dictionary<string, Model> { { "departments", model } }, inputs);
            System.IO.File.WriteAllText("../../../../Checkout.json", outputs.model.ToJson());
            outputs.model.ToGlTF("../../../../Checkout.glb");
        }
    }
}