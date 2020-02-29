using System;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;
using GeometryEx;

namespace PreparedFoodLayout
{
      public static class PreparedFoodLayout
    {
        /// <summary>
        /// The PreparedFoodLayout function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A PreparedFoodLayoutOutputs instance containing computed results and the model with any new elements.</returns>
        public static PreparedFoodLayoutOutputs Execute(Dictionary<string, Model> inputModels, PreparedFoodLayoutInputs input)
        {
            var counterDepth = input.CounterDepth / 39.37; if (counterDepth == 0) counterDepth = 1;
            var counterHeight = input.CounterHeight / 39.37; if (counterHeight == 0) counterHeight = 1;
        
            var aisle = input.AisleFrontage / 39.37; if (aisle <= 0) aisle = 1.0;
          
         PreparedFoodLayoutOutputs output = new PreparedFoodLayoutOutputs();   

          
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
            //throw new ApplicationException("Need Departments as input!");

            

            // default:
            double inputLength = 55;
            double inputWidth = 25;
            double inputHeight = 5;
            
            var rectangle = Polygon.Rectangle(inputLength, inputWidth);
            var mass = new Mass(rectangle, inputHeight);
            output.model.AddElement(mass);
            var material = new Material("office",new Color(0,0,1, 0.5) );
            

            var solid = new Elements.Geometry.Solids.Extrude(rectangle, inputHeight, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room r = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "prepared", "100", rectangle.Area(), 
                          1.0, 0, 0, inputHeight, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            
            output.model.AddElement(r);

          
            rooms = new List<Room>();
            rooms.Add(r);
           }

// ok, now the real work begins... from the room

          // this function only deals with certain departments.

        var appropriateRooms = 
          rooms.Where( r => r.Department == "prepared");

        if (appropriateRooms.Count()==0) throw new ApplicationException("This function works only on rooms with 'prepared' department");


        foreach( var r in appropriateRooms)
        {

          var side1 = r.Perimeter.Segments()[0];
          var side2 = r.Perimeter.Segments()[1];

          Console.WriteLine("side1: " + side1.Direction());
          Console.WriteLine("side2: " + side2.Direction());
          
          var line1 = side1.DivideByLength(aisle).First();
          var line2 = side2.Reversed().DivideByLength(aisle).First();
          var roomCenter = r.Perimeter.GetCenter();

          //roomCenter.HighlightPoint(2, output.model);
         // var lineX = new Line( roomCenter, roomCenter + new Vector3(10,0,0));
          //var lineY = new Line( roomCenter, roomCenter + new Vector3(0,5,0));
          //lineX.HighlightThis(output.model);
          //lineY.HighlightThis(output.model);

          // ok - sweep is not working so we're going to have to build out the plan view and extrude up.
          // the "B" will be the inside curve.
  
    
         

          //line1.HighlightThis(output.model);
          //line2.HighlightThis(output.model);
         
          var smallerSide = (Math.Min(side1.Length(), side2.Length()));
          var radius = (input.CounterRadiusRatio / 100.0 ) * smallerSide;

          //var radius = (side1.Length() - line1.Length()) * 0.3;

          var p1 = line1.PointAt(1.0);
          var p2 = line2.PointAt(1.0);
          

         // p1.HighlightPoint(1,output.model);
          //p2.HighlightPoint(1,output.model);
         // p1b.HighlightPoint(0.5, output.model);
          //p2b.HighlightPoint(0.5, output.model);
          

          var dir1 = side2.Direction().WhichPointsTo( p1, roomCenter);
          var dir2 = side1.Direction().WhichPointsTo( p2, roomCenter);

          var line1Offset = new Line(p1, p1 + dir1 * 10);
          var line2Offset = new Line(p2, p2 + dir2 * 10);

          var line1bOffset = line1Offset.OffsetTowards(roomCenter, counterDepth);
          var line2bOffset = line2Offset.OffsetTowards(roomCenter, counterDepth);

          var p1b = line1bOffset.Start;
          var p2b = line2bOffset.Start;

          
         Console.WriteLine("L1Offset: " + line1Offset.Direction());
         Console.WriteLine("L2Offset: " + line2Offset.Direction());
         Console.WriteLine("L1bOffset: " + line1bOffset.Direction());
         Console.WriteLine("L2bOffset: " + line2bOffset.Direction());
          

          

          var arc = line1Offset.Fillet(line2Offset, radius);
          var arcB = line1bOffset.Fillet(line2bOffset, radius - counterDepth);

          // which end will be nearest?
          var closest1 = (p1.DistanceTo(arc.Start) < p1.DistanceTo(arc.End))? arc.Start : arc.End;
          var closest2 = (p2.DistanceTo(arc.Start) < p2.DistanceTo(arc.End))? arc.Start : arc.End;
          var closest1b = (p1b.DistanceTo(arcB.Start) < p1b.DistanceTo(arcB.End))? arcB.Start : arcB.End;
          var closest2b = (p2b.DistanceTo(arcB.Start) < p2b.DistanceTo(arcB.End))? arcB.Start : arcB.End;

  //line1Offset.HighlightThis(output.model);
         // line2Offset.HighlightThis(output.model);

          // redefine to the arc points
          line1Offset = new Line(p1, closest1);
          line2Offset = new Line(p2, closest2);

          //line1Offset.HighlightThis(output.model);
          //line2Offset.HighlightThis(output.model);
          //line1bOffset.HighlightThis(output.model);
          //line2bOffset.HighlightThis(output.model);

        // arc.HighlightThis(output.model);
        // arcB.HighlightThis(output.model);

          // ok - we want to draw the lines now

          // ordered list of vectors
          int numSegments = 10;
          var list = new List<Vector3>();
          list.Add(p1);          
          if (arc.Start.DistanceTo(closest1)<arc.Start.DistanceTo(closest2))
          {
            list.AddRange( arc.Divide(numSegments));
          }
          else
          {
            list.AddRange( arc.Reversed().Divide(numSegments));
          }
          list.Add(p2);
          list.Add(p2b);
          if (arcB.Start.DistanceTo(closest2b) < arcB.Start.DistanceTo(closest1b))
          {
            list.AddRange( arcB.Divide(numSegments));
          }
          else
          {
            list.AddRange( arcB.Divide(numSegments));
          }
          list.Add(p1b);
          Polygon newP = new Polygon(list);
          newP = newP.Reversed(); // it was the wrong way.


//line1.ThickenThis(model);

         var counter = new Material("Countertop", Colors.Granite);
         var prepFoodCounter = new Elements.Geometry.Solids.Extrude( new Profile(newP), counterHeight, Vector3.ZAxis, false);
         
         Representation geomRep = null; // new Representation(new Elements.Geometry.Solids.SolidOperation[] { prepFoodCounter});
         var mass = new Mass(new Profile(newP), counterHeight, counter,null, geomRep);
         
         output.model.AddElement(mass);
            
            // Console.WriteLine(output.model.ToJson(true));
            //System.IO.File.WriteAllText(@"C:\temp\model.json", output.model.ToJson(true));
          

          }                      
       

        return output; 
        }

        public static void HighlightThis(this Profile p, Model m)
        {
          Material highlight = new Material("Highlight", Colors.Red );
           foreach( var c in p.Perimeter.Segments())
           {
             m.AddElement( new ModelCurve( c, highlight));
           }

        }

        public static void HighlightPoint(this Vector3 pt, double howBig, Model model)
        {
          Material highlight = new Material("Highlight", Colors.Red);
          ModelCurve m1 = new ModelCurve( new Line( pt + new Vector3(-1 * howBig, -1*howBig,0), pt + new Vector3(howBig, howBig,0)), highlight);
          ModelCurve m2 = new ModelCurve( new Line( pt + new Vector3(-1 * howBig,  1*howBig,0), pt + new Vector3(howBig, -1*howBig,0)), highlight);

         model.AddElement( m1);
         model.AddElement(m2);
        }
        public static void HighlightThis(this Curve curve, Model model)
        {
          Material highlight = new Material("Highlight", Colors.Red);
         model.AddElement( new ModelCurve(curve, highlight));

        }

         public static void ThickenThis(this Curve curve, Model model)
        {
          Material highlight = new Material("Highlight", Colors.Red);
          var line = (Line)curve;
          var poly = line.Thicken(2.0);

         model.AddElement( new Mass(poly, 2.0, highlight));

        }

        public static void HighlightBox(this Profile profile, Model model)
        {
          Material highlight = new Material("Highlight", Colors.Red);
          // make a box around this
         
          Envelope e = new Envelope(profile, 0,0, Vector3.ZAxis,0,new Transform(), highlight, 
                  new Representation( new Elements.Geometry.Solids.SolidOperation[]{ new Elements.Geometry.Solids.Lamina(profile.Perimeter, false)} ), false, Guid.NewGuid(), "");
          model.AddElement(e);

        }

        public static Vector3 GetCenter(this Polygon polygon)
        {
          double x=0;double y=0; double z=0;
          int count = 0;
          foreach( var ln in polygon.Segments())
          {
            count++;
            x += ln.Start.X; y += ln.Start.Y; z += ln.Start.Z;
          }

          return new Vector3( x/(double)count, y/(double)count, z/(double)count);

        }

        /// Does this vector point towards the point? or its opposite?
        public static Vector3 WhichPointsTo(this Vector3 v, Vector3 start,  Vector3 target)
        {
           Vector3 t1 = start + v;
           Vector3 t2 = start + v.Negate();

           return ((t1.DistanceTo(target) < t2.DistanceTo(target) ? v : v.Negate()));
        }

        public static Line OffsetTowards(this Line line, Vector3 point, double dist)
        {
            var off1 = line.Offset(dist, false);
            var off2 = line.Offset(-1 * dist, false);            

            double dist1 = off1.ProjectionDist(point);
            double dist2 = off2.ProjectionDist(point);

            // return the one that is closer.
            return ( (dist1< dist2) ? off1 : off2); 
        }

        public static Vector3 Project( this Line line, Vector3 point)
        {
      
            Vector3 vec = point - line.Start;
            Vector3 dir = line.Direction();
            double proj = vec.Dot(dir) / dir.Dot(dir);

            return line.Start + (dir * proj);

        }

        public static double ProjectionDist( this Line line, Vector3 point)
        {
          var projected = line.Project(point);
          return projected.DistanceTo(point);
        }
    

        
      }
}