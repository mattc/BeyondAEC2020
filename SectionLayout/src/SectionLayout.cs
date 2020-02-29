using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;
using System.Linq;
using System;


namespace SectionLayout
{
      public static class SectionLayout
    {
        /// <summary>
        /// The SectionLayout function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A SectionLayoutOutputs instance containing computed results and the model with any new elements.</returns>
        public static SectionLayoutOutputs Execute(Dictionary<string, Model> inputModels, SectionLayoutInputs input)
        {
            Envelope envelope = null;
            inputModels.TryGetValue("Envelope", out var model);
            if (model != null)
            {
                var envelopes = new List<Envelope>();
                envelopes.AddRange(model.AllElementsOfType<Envelope>());
                var aboveGradeEnvelopes = envelopes.Where(e => e.Elevation >= 0.0).ToList();
                if (aboveGradeEnvelopes.Count() > 0)
                {
                    envelope = aboveGradeEnvelopes.First();
                }
            }
            if (envelope == null)
            {
                var envMatl = new Material("envelope", new Color(1.0, 1.0, 1.0, 0.2), 0.0f, 0.0f);
                var height = 10.0;
                var footprint = Polygon.Rectangle(60, 40);
                var extrude = new Elements.Geometry.Solids.Extrude(footprint, height, Vector3.ZAxis, false);
                var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
                envelope = new Envelope(footprint, 0.0, height, Vector3.ZAxis, 0.0,
                              new Transform(), envMatl, geomRep, false, System.Guid.NewGuid(), "");
            }

            var output = new SectionLayoutOutputs(10);

            var grid = new Grid2d(envelope.Profile.Perimeter);
            var length = grid.V.Domain.Length;
            var width = grid.U.Domain.Length;

            //Define Dimensions of non-prdoduct spaces
            var entryDepth = 5;
            var checkoutDepth = 10;
            var percentService = .2;

            //var buildingArea = envelope.Profile.Perimeter.Area();

            var circulationWidth = input.CirculationWidth;
            
            //Variables driving the division of the main shelf space
            var _percentProduce = input.PercentProduce;
            var _percentPrepared = input.PercentPrepared;
            var _percentGeneral = input.PercentGeneral;
            var _percentRefrigerated = input.PercentRefrigerated;

            var _percentLeft = _percentProduce + _percentPrepared;
            var _leftSplit = _percentProduce / (_percentProduce + _percentPrepared);

            var totalShelf = _percentLeft + _percentGeneral + _percentRefrigerated;

            var percentLeft = _percentLeft / totalShelf;
            var percentGeneral = _percentGeneral / totalShelf + percentLeft;
            //var percentRefrigerated = _percentRefrigerated / totalShelf;
            

            //Split into rooms front to back
            grid.V.SplitAtParameters(new[] {entryDepth/length, checkoutDepth/length, (1 - percentService)});
            
            var entryArea = grid.GetCellAtIndices(0,0);
            entryArea.U.SplitAtParameters(new[] {.2, .4, .6, .8});
                var front1 = entryArea.GetCellAtIndices(0,0);
                var entry1 = entryArea.GetCellAtIndices(1,0);
                var front2 = entryArea.GetCellAtIndices(2,0);
                var entry2 = entryArea.GetCellAtIndices(3,0);
                var front3 = entryArea.GetCellAtIndices(4,0);
            var checkoutArea = grid.GetCellAtIndices(0,1);
            var shelfArea = grid.GetCellAtIndices(0,2);
            var serviceArea = grid.GetCellAtIndices(0,3);
            
            //Split Shelf Area into sub-rooms
            shelfArea.U.SplitAtParameters(new[] {percentLeft, percentGeneral});
                var left = shelfArea.GetCellAtIndices(0,0);
                left.V.SplitAtParameter(_leftSplit);
                    var produce = left.GetCellAtIndices(0,0);
                    var prepared = left.GetCellAtIndices(0,1);
                var general = shelfArea.GetCellAtIndices(1,0);
                var refrig = shelfArea.GetCellAtIndices(2,0);
            //var other = shelfArea.GetCellAtIndices(3,0);

            //var rooms = grid.GetCells().Select(c => GetRoomFromCell(c));
            //output.model.AddElements(rooms);
            
            var entryMaterial = new Material("entry material",new Color(0,0,1,.9));
            var frontMaterial = new Material("front material",new Color(.9,.7,.7,.9));
            var checkoutMaterial = new Material("checkout material",new Color(0,.5,.5,.9));
            var serviceMaterial = new Material("service material",new Color(.25,.25,.25,.9));

            var produceMaterial = new Material("produce material",new Color(0,1,0,.9));
            var preparedMaterial = new Material("prepared material",new Color(1,.25,.25,.9));
            var generalMaterial = new Material("general material",new Color(1,0,0,.9));
            var refrigMaterial = new Material("refrigerated material",new Color(.75,.75,1,.9));
            var otherMaterial = new Material("other material",new Color(0,0,0,.9));


            //Label and return rooms --> shelf area excluded due to inclusion of sub-rooms
            //AddRoomFromCell(entryArea, "entry", entryMaterial, output.model, circulationWidth);
            AddRoomFromCell(front1, "front1", frontMaterial, output.model, circulationWidth);
            AddRoomFromCell(entry1, "entrance1", entryMaterial, output.model, circulationWidth);
            AddRoomFromCell(front2, "front2", frontMaterial, output.model, circulationWidth);
            AddRoomFromCell(entry2, "entrance2", entryMaterial, output.model, circulationWidth);
            AddRoomFromCell(front3, "front3", frontMaterial, output.model, circulationWidth);
            
            AddRoomFromCell(checkoutArea, "checkout", checkoutMaterial, output.model,circulationWidth);
            AddRoomFromCell(serviceArea, "service", serviceMaterial, output.model, circulationWidth);

            AddRoomFromCell(produce, "produce", produceMaterial, output.model, circulationWidth);
            AddRoomFromCell(prepared, "prepared", preparedMaterial, output.model, circulationWidth);
            AddRoomFromCell(general, "general", generalMaterial, output.model, circulationWidth);
            AddRoomFromCell(refrig, "refrig", refrigMaterial, output.model, circulationWidth);
            


            ////output.model.AddElement(rm);
            return output;
        }

        private static void AddRoomFromCell (Grid2d cell, string department, Material material, Model model, double circulationWidth)
        {
            var polygons = cell.GetTrimmedCellGeometry();
            if (polygons.Count() == 0) {
                return;
            }
            var polygon = (Polygon) polygons.First();

            var newPoints = polygon.Vertices.ToList().ToArray().Shrink(circulationWidth);
            var newPolygon = new Polygon(newPoints);

            var solid = new Elements.Geometry.Solids.Extrude(newPolygon, 11, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
            var room = new Room((Polygon)newPolygon, Vector3.ZAxis, "Section 1", "100", department, "100", newPolygon.Area(), 
            1.0, 0, 0, 11, newPolygon.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            model.AddElement(room);
        }


      }
}