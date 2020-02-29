using System;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;

namespace ProduceLayout
{
      public static class ProduceLayout
    {
        /// <summary>
        /// The ProduceLayout function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A ProduceLayoutOutputs instance containing computed results and the model with any new elements.</returns>
        public static ProduceLayoutOutputs Execute(Dictionary<string, Model> inputModels, ProduceLayoutInputs input)
        {
             // convert the inputs to meters
     
        var minAisle = input.MinAisleWidth / 39.37;
        if (minAisle ==0) minAisle = 1;


         ProduceLayoutOutputs output = new ProduceLayoutOutputs();   

          
          Model model = null;
           IList<Room> rooms = null;
           //test inputModels.Clear();
           // we want Departments as inputs!
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
             Room r = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "produce", "100", rectangle.Area(), 
                          1.0, 0, 0, inputHeight, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            
            model.AddElement(r);
            rooms = new List<Room>();
            rooms.Add(r);
           }

// ok, now the real work begins... from the room

          // this function only deals with certain departments.

        var appropriateRooms = 
          rooms.Where( r => r.Department == "produce");

        if (appropriateRooms.Count()==0) throw new ApplicationException("This function works only on rooms with 'produce' department");


        foreach( var r in appropriateRooms)
        {

          // make a 2D grid
          var grid = new Elements.Spatial.Grid2d( r.Perimeter, new Transform());
          
          // we want to subdivide:
         

          var geom= grid.V.GetCellGeometry();

          var sideLength = grid.V.Domain.Length; //grid.U.GetCellGeometry().Length();

          var standardDepth = 0.889; // 35.5"
          var standardWidth = 1.22; // 48"

          double available = sideLength - (2.0 * standardDepth);
          int count = (int)(available / (2 * standardDepth + minAisle));
          // debug:
          System.Console.WriteLine("side Length: " + sideLength + " available: " + available + " count: " + count);
          System.Console.WriteLine("Min Aisle: " + minAisle);

          grid.V.DivideByPattern( new double[]{ standardDepth, standardDepth, minAisle}, PatternMode.Cycle, FixedDivisionMode.RemainderAtEnd);
          grid.U.DivideByFixedLength(standardWidth);

          var produce = new Material( "Produce", Colors.Gray, 0f,0f, null,Guid.NewGuid());
         

          // now we will try making the shelving
          for (int i=0; i<grid.V.Cells.Count;i++)
          {
            for (int j=0;j<grid.U.Cells.Count;j++)
            {
            var cell = grid.GetCellAtIndices(j,i);
             // make a poly from each gridcell
             // shelf, shelf, aisle
             int c = i+1;

             if ( ((c % 3) == 0))
             {
               // we skip for the aisle
             }
             else
             {
               var height = 0.9;
               
               var material = new Material( Colors.Brown, 0, 0, Guid.NewGuid(),"ProduceBase");
              var shelfMass = new Mass((Polygon)cell.GetCellGeometry(), height, material);

              // for the angled base, start by copying it up.
              var p = (Polygon)cell.GetCellGeometry();
              p.Transform(new Transform(0,0,height));
              
              Vector3 tmpOrigin = p.Vertices.First();
              double rotAngle = 30;
              if ((c % 3) == 2) 
              {
                tmpOrigin = p.Vertices[2];
                rotAngle = -30;
              }
              // this is dumb. But it's a hackathon
              p.Transform(new Transform(-1 * tmpOrigin.X, -1 * tmpOrigin.Y, -1 * tmpOrigin.Z));
              var rotate = new Transform();
              rotate.Rotate(Vector3.XAxis,rotAngle);
              p.Transform(rotate);
              p.Transform(new Transform(tmpOrigin.X, tmpOrigin.Y, tmpOrigin.Z));
              
              
              
              
              var angle = new Elements.Geometry.Solids.Lamina(p, false);
              var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { angle });
              var envelope = new Envelope( p, 0.0, height, Vector3.ZAxis, 0.0,
                              new Transform(), produce, geomRep, false, System.Guid.NewGuid(), "");

              output.model.AddElement(shelfMass);
              output.model.AddElement(envelope);
             }
            }

          }                      
        }

        return output; 
        }
      }
}