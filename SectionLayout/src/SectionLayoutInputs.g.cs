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

namespace SectionLayout
{
    public class SectionLayoutInputs: S3Args
    {
		/// <summary>
		/// Percentage of space allocated to produce products
		/// </summary>
		[JsonProperty("Percent Produce")]
		public double PercentProduce {get;}

		/// <summary>
		/// Percentage of space allocated to prepared products
		/// </summary>
		[JsonProperty("Percent Prepared")]
		public double PercentPrepared {get;}

		/// <summary>
		/// Percentage of space allocated to general products
		/// </summary>
		[JsonProperty("Percent General")]
		public double PercentGeneral {get;}

		/// <summary>
		/// Percentage of space allocated to refrigerated products
		/// </summary>
		[JsonProperty("Percent Refrigerated")]
		public double PercentRefrigerated {get;}


        
        /// <summary>
        /// Construct a SectionLayoutInputs with default inputs.
        /// This should be used for testing only.
        /// </summary>
        public SectionLayoutInputs() : base()
        {
			this.PercentProduce = 100;
			this.PercentPrepared = 100;
			this.PercentGeneral = 100;
			this.PercentRefrigerated = 100;

        }


        /// <summary>
        /// Construct a SectionLayoutInputs specifying all inputs.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public SectionLayoutInputs(double percentproduce, double percentprepared, double percentgeneral, double percentrefrigerated, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey): base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
			this.PercentProduce = percentproduce;
			this.PercentPrepared = percentprepared;
			this.PercentGeneral = percentgeneral;
			this.PercentRefrigerated = percentrefrigerated;

		}

		public override string ToString()
		{
			var json = JsonConvert.SerializeObject(this);
			return json;
		}
	}
}