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

namespace BigBoxFacade
{
    public class BigBoxFacadeInputs: S3Args
    {
		/// <summary>
		/// Whether to put the primary entrance on the right side of the building.
		/// </summary>
		[JsonProperty("Entrance on right")]
		public bool EntranceOnRight {get;}


        
        /// <summary>
        /// Construct a BigBoxFacadeInputs with default inputs.
        /// This should be used for testing only.
        /// </summary>
        public BigBoxFacadeInputs() : base()
        {
			this.EntranceOnRight = false;

        }


        /// <summary>
        /// Construct a BigBoxFacadeInputs specifying all inputs.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public BigBoxFacadeInputs(bool entranceonright, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey): base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
			this.EntranceOnRight = entranceonright;

		}

		public override string ToString()
		{
			var json = JsonConvert.SerializeObject(this);
			return json;
		}
	}
}