// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ShelvingLayout
{
    public class ShelvingLayoutInputs: S3Args
    {
		/// <summary>
		/// The depth of one side of a shelving unit (in)
		/// </summary>
		[JsonProperty("Shelving Depth")]
		public double ShelvingDepth {get;}

		/// <summary>
		/// The minimum width of an aisle (inch)
		/// </summary>
		[JsonProperty("Min Aisle Width")]
		public double MinAisleWidth {get;}

		/// <summary>
		/// The width of each fixture (in)
		/// </summary>
		[JsonProperty("Fixture Width")]
		public double FixtureWidth {get;}

		/// <summary>
		/// The height of each gondola fixture (in)
		/// </summary>
		[JsonProperty("Fixture Height")]
		public double FixtureHeight {get;}


        
        /// <summary>
        /// Construct a ShelvingLayoutInputs with default inputs.
        /// This should be used for testing only.
        /// </summary>
        public ShelvingLayoutInputs() : base()
        {
			this.ShelvingDepth = 48;
			this.MinAisleWidth = 144;
			this.FixtureWidth = 48;
			this.FixtureHeight = 120;

        }


        /// <summary>
        /// Construct a ShelvingLayoutInputs specifying all inputs.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public ShelvingLayoutInputs(double shelvingdepth, double minaislewidth, double fixturewidth, double fixtureheight, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey): base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
			this.ShelvingDepth = shelvingdepth;
			this.MinAisleWidth = minaislewidth;
			this.FixtureWidth = fixturewidth;
			this.FixtureHeight = fixtureheight;

		}

		public override string ToString()
		{
			var json = JsonConvert.SerializeObject(this);
			return json;
		}
	}
}