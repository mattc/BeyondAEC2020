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

            var material = new Material("office",Colors.Aqua);
            var solid = new Elements.Geometry.Solids.Extrude(rectangle, height, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room rm = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "Grocery", "100", rectangle.Area(), 
            1.0, 0, 0, height, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );

            var grid = new Grid2d(rectangle);
            grid.U.DivideByFixedLength(3, FixedDivisionMode.RemainderAtBothEnds);
            grid.V.DivideByPattern(new[] { 1.0, 5.0 });
            var rooms = grid.GetCells().Select(c => GetRoomFromCell(c));
            output.model.AddElements(rooms);
            
            output.model.AddElement(rm);
            return output;
        }

        private static Element GetRoomFromCell (Grid2d cell)
        {
            var polygon = (Polygon)cell.GetCellGeometry();
            var material = new Material("office",Colors.Red);
            var solid = new Elements.Geometry.Solids.Extrude(polygon, 11, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
            var room = new Room((Polygon)polygon, Vector3.ZAxis, "Section 1", "100", "Grocery", "100", polygon.Area(), 
            1.0, 0, 0, 11, polygon.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            return room;
        }
      }
}