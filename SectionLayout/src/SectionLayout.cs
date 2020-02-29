using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;
using System.Linq;


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
            /// Your code here.
            var height = 10.0;
            var volume = input.Length * input.Width * height;
            var output = new SectionLayoutOutputs(volume);
            var rectangle = Polygon.Rectangle(input.Length, input.Width);

            //Define Dimensions of non-product spaces
            var entryDepth = 5;
            var checkoutDepth = 10;
            var serviceDepth = 10;

            //Variables driving the division of the main shelf space
            var _percentProduce = input.PercentProduce;
            var _percentGeneral = input.PercentGeneral;
            var _percentRefrigerated = input.PercentRefrigerated;

            var totalShelf = _percentProduce + _percentGeneral + _percentRefrigerated;

            var percentProduce = _percentProduce / totalShelf;
            var percentGeneral = _percentGeneral / totalShelf;
            var percentRefrigerated = _percentRefrigerated / totalShelf;
            
            //Create room representing entire store
            var material = new Material("office",Colors.Aqua);
            var solid = new Elements.Geometry.Solids.Extrude(rectangle, height, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room rm = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "Grocery", "100", rectangle.Area(), 
            1.0, 0, 0, height, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );

            //Split into rooms front to back
            var grid = new Grid2d(rectangle);
            grid.V.SplitAtParameters(new[] {entryDepth/input.Length, checkoutDepth/input.Length, (1 - serviceDepth/input.Length)});
            
            var entryArea = grid.GetCellAtIndices(0,0);
            var checkoutArea = grid.GetCellAtIndices(0,1);
            var shelfArea = grid.GetCellAtIndices(0,2);
            var serviceArea = grid.GetCellAtIndices(0,3);
            
            //Split Shelf Area into sub-rooms
            shelfArea.U.SplitAtParameters(new[] {percentProduce, percentGeneral});
            var produce = shelfArea.GetCellAtIndices(0,0);
            var general = shelfArea.GetCellAtIndices(1,0);
            var refrig = shelfArea.GetCellAtIndices(2,0);

            //var rooms = grid.GetCells().Select(c => GetRoomFromCell(c));
            //output.model.AddElements(rooms);
            
            //Label and return rooms --> shelf area excluded due to inclusion of sub-rooms
            output.model.AddElement(GetRoomFromCell(entryArea, "entry"));
            output.model.AddElement(GetRoomFromCell(checkoutArea, "checkout"));
            ////output.model.AddElement(GetRoomFromCell(shelfArea, "shelf"));
            output.model.AddElement(GetRoomFromCell(serviceArea, "service"));

            output.model.AddElement(GetRoomFromCell(produce, "produce"));
            output.model.AddElement(GetRoomFromCell(general, "general"));
            output.model.AddElement(GetRoomFromCell(refrig, "refrig"));
            
            ////output.model.AddElement(rm);
            return output;
        }

        private static Element GetRoomFromCell (Grid2d cell, string department)
        {
            var polygon = (Polygon)cell.GetCellGeometry();
            var material = new Material("office",new Color(1,0,0,.9));
            var solid = new Elements.Geometry.Solids.Extrude(polygon, 11, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
            var room = new Room((Polygon)polygon, Vector3.ZAxis, "Section 1", "100", department, "100", polygon.Area(), 
            1.0, 0, 0, 11, polygon.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            return room;
        }
      }
}