using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace ShelvingLayout
{
      public static class ShelvingLayout
    {
        /// <summary>
        /// The ShelvingLayout function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A ShelvingLayoutOutputs instance containing computed results and the model with any new elements.</returns>
        public static ShelvingLayoutOutputs Execute(Dictionary<string, Model> inputModels, ShelvingLayoutInputs input)
        {
            /// Your code here.
           
           
            var volume = input.Length * input.Width * input.Height;
            var output = new ShelvingLayoutOutputs(volume);
            var rectangle = Polygon.Rectangle(input.Length, input.Width);
            var mass = new Mass(rectangle, input.Height);
            output.model.AddElement(mass);
            var material = new Material("office",new Color(0,0,1.0, 0.5) );
            

            var solid = new Elements.Geometry.Solids.Extrude(rectangle, input.Height, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room r = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "Grocery", "100", rectangle.Area(), 
                          1.0, 0, 0, input.Height, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            output.model.AddElement(r);

// ok, now the real work begins... from the room



          // make a 2D grid
          var grid = new Elements.Spatial.Grid2d( r.Perimeter, new Transform());
          
          // we want to subdivide:
          var side1 = r.Perimeter.Segments().First();
          var side2 = r.Perimeter.Segments()[1];

          var geom= grid.U.GetCellGeometry();

          var sideLength = grid.U.Domain.Length; //grid.U.GetCellGeometry().Length();

          double available = sideLength - (2.0 * input.ShelvingDepth);
          int count = (int)(available / (2 * input.ShelvingDepth + input.MinAisleWidth));

          grid.U.DivideByPattern( new double[]{ input.ShelvingDepth, input.ShelvingDepth, input.MinAisleWidth}, PatternMode.Cycle, FixedDivisionMode.RemainderAtEnd);

          // now we will try making the shelving
          for (int i=0; i<grid.U.Cells.Count;i++)
          {
            var cell = grid.GetCellAtIndices(i,0);
             // make a poly from each gridcell
             // shelf, shelf, aisle
             int c = i+1;

             if ( ((c % 3) == 0))
             {
               // we skip for the aisle
             }
             else
             {
               
              var shelfMass = new Mass((Polygon)cell.GetCellGeometry(), input.Height *0.75);
              output.model.AddElement(shelfMass);
             }

          }                      
          

            return output;
        }
      }
}