using System;
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
          ShelvingLayoutOutputs output = new ShelvingLayoutOutputs();

          // convert the inputs into meters
          var fixtureHeight = input.FixtureHeight / 39.37; if (fixtureHeight==0) fixtureHeight = 1.0;
          var fixtureWidth = input.FixtureWidth / 39.37;
          var minAisleWidth = input.MinAisleWidth / 39.37;
          var ShelvingDepth = input.ShelvingDepth / 39.37;


           Model model = null;
           IList<Room> rooms = null;
           if (inputModels.TryGetValue("Departments", out model))
           {
              rooms = model.AllElementsOfType<Room>().ToList();
           }
           else
           {
            throw new ApplicationException("Need Departments as input!");
            model = new Model();
            // default:
            double inputLength = 100;
            double inputWidth = 30;
            double inputHeight = 5;
            
            var rectangle = Polygon.Rectangle(inputLength, inputWidth);
            var mass = new Mass(rectangle, inputHeight);
            output.model.AddElement(mass);
            var material = new Material("office",new Color(0,0,1.0, 0.5) );
            

            var solid = new Elements.Geometry.Solids.Extrude(rectangle, inputHeight, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room r = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "general", "100", rectangle.Area(), 
                          1.0, 0, 0, inputHeight, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            
            model.AddElement(r);
            rooms = new List<Room>();
            rooms.Add(r);
           }

// ok, now the real work begins... from the room

          // this function only deals with certain departments.

        var appropriateRooms = 
          rooms.Where( r => r.Department == "general");

        if (appropriateRooms.Count()==0) throw new ApplicationException("This function works only on rooms with 'general' department");


        foreach( var r in appropriateRooms)
        {

          // make a 2D grid
          var grid = new Elements.Spatial.Grid2d( r.Perimeter, new Transform());
          
          // we want to subdivide:
          var side1 = r.Perimeter.Segments().First();
          var side2 = r.Perimeter.Segments()[1];

          var geom= grid.U.GetCellGeometry();

          var sideLength = grid.U.Domain.Length; //grid.U.GetCellGeometry().Length();

          double available = sideLength - (2.0 * ShelvingDepth);
          int count = (int)(available / (2 * ShelvingDepth + minAisleWidth));
          // debug:
          System.Console.WriteLine("sideLength: " + sideLength + " available: " + available + " count: " + count);

          grid.U.DivideByPattern( new double[]{ ShelvingDepth, ShelvingDepth, minAisleWidth}, PatternMode.Cycle, FixedDivisionMode.RemainderAtEnd);
          grid.V.DivideByFixedLength(fixtureWidth);

          // now we will try making the shelving
          for (int i=0; i<grid.U.Cells.Count;i++)
          {
            for (int j=0;j<grid.V.Cells.Count;j++)
            {
            var cell = grid.GetCellAtIndices(i,j);
             // make a poly from each gridcell
             // shelf, shelf, aisle
             int c = i+1;

             if ( ((c % 3) == 0))
             {
               // we skip for the aisle
             }
             else
             {
               
               Material mat = new Material(Colors.Beige,0,0,Guid.NewGuid(), "Gondola");
              var shelfMass = new Mass((Polygon)cell.GetCellGeometry(), fixtureHeight, mat);
              output.model.AddElement(shelfMass);
             }
            }

          }                      
        } 

            return output;
        }
      }
}