using System;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;

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
        
            var aisle = 1;


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

            model = new Model();
            // default:
            double inputLength = 55;
            double inputWidth = 25;
            double inputHeight = 5;
            
            var rectangle = Polygon.Rectangle(inputLength, inputWidth);
            var mass = new Mass(rectangle, inputHeight);
            output.model.AddElement(mass);
            var material = new Material("office",new Color(0,1,0, 0.5) );
            

            var solid = new Elements.Geometry.Solids.Extrude(rectangle, inputHeight, Vector3.ZAxis, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>(){ solid});
             Room r = new Room(rectangle, Vector3.ZAxis, "Section 1", "100", "prepared", "100", rectangle.Area(), 
                          1.0, 0, 0, inputHeight, rectangle.Area(), new Transform(), material, geomRep, false, System.Guid.NewGuid(), "Section 1" );
            
            model.AddElement(r);

            Line test = new Line(Vector3.Origin, new Vector3(200,200,0));
            var poly = test.Thicken(2.0);
            var mass2 = new Mass( new Profile(poly), 2.0);
            model.AddElement(mass2);


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

          var radius = (side1.Length() - line1.Length()) * 0.67;

          var p1 = line1.PointAt(1.0);
          var p2 = line2.PointAt(1.0);

          var line1Offset = new Line(p1, p1 + side2.Direction() * 10);
          var line2Offset = new Line(p2, p2 + side1.Direction() * 10);
         
         Console.WriteLine("L1Offset: " + line1Offset.Direction());
         Console.WriteLine("L2Offset: " + line2Offset.Direction());
         
          var arc = line1Offset.Fillet(line2Offset, radius);

          // which end will be nearest?
          var closest1 = (p1.DistanceTo(arc.Start) < p1.DistanceTo(arc.End))? arc.Start : arc.End;
          var closest2 = (p2.DistanceTo(arc.Start) < p2.DistanceTo(arc.End))? arc.Start : arc.End;

          // redefine to the arc points
          line1Offset = new Line(p1, closest1);
          line2Offset = new Line(p2, closest2);


line1.ThickenThis(model);

          // make the p1 profile box
          var pts1 = new Vector3[] { p1, p1 + line1.Direction().Unitized() * counterDepth, 
                                                                      p1 + line1.Direction().Unitized() * counterDepth + Vector3.ZAxis * counterHeight, 
                                                                      p1 + Vector3.ZAxis * counterHeight};
          for (int i=0; i< pts1.Length; i++) Console.WriteLine("pts1-" + i + ": " + pts1[i]);

          Profile profile1 = new Profile( new Polygon(pts1 ).Reversed());

          var pts2 =  new Vector3[] { closest1, closest1 + line1.Direction().Unitized() * counterDepth,
                                                                      closest1 + line1.Direction().Unitized() * counterDepth + Vector3.ZAxis * counterHeight,
                                                                      closest1 + Vector3.ZAxis * counterHeight};
          for (int i=0; i< pts2.Length; i++) Console.WriteLine("pts2-" + i + ": " + pts2[i]);
          Profile profileArc = new Profile( new Polygon(pts2).Reversed());

          Profile profile2 = new Profile( new Polygon( new Vector3[] { p2, p2 + line2.Direction().Unitized() * counterDepth,
                                                                      p2 + line2.Direction().Unitized() * counterDepth + Vector3.ZAxis * counterHeight,
                                                                      p2 + Vector3.ZAxis * counterHeight}).Reversed());
           
           var counter = new Material(Colors.Red, 0.25, 0.25, Guid.NewGuid(), "Counter");

           // make the sweeps.
           var sweep1 = new Elements.Geometry.Solids.Sweep(profile1, line1Offset, 0, 0, false);
           var sweepArc = new Elements.Geometry.Solids.Sweep(profileArc, arc, 0,0,false);
           var sweep2 = new Elements.Geometry.Solids.Sweep(profile2, line2Offset, 0,0,false);
           

             var geomRep1 = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { sweep1 });
              var envelope1 = new Envelope( profile1, 0.0, 100.0, Vector3.XAxis, 0.0,
                              new Transform(), counter, geomRep1, false, System.Guid.NewGuid(), "");
             var geomRepArc = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { sweepArc });
              var envelopeArc = new Envelope( profileArc, 0.0, 00.0, Vector3.XAxis, 0.0,
                              new Transform(), counter, geomRepArc, false, System.Guid.NewGuid(), "");
             var geomRep2 = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { sweep2 });
              var envelope2 = new Envelope( profile2, 0.0, 0.0, Vector3.XAxis, 0.0,
                              new Transform(), counter, geomRep2, false, System.Guid.NewGuid(), "");
            model.AddElement(envelope1);

            //model.AddElements(new Element[]{ envelope1, envelopeArc, envelope2});                              
            
            // profile1.HighlightThis(model);
            // profile2.HighlightThis(model);
            // profileArc.HighlightThis(model);

            // line1Offset.HighlightThis(model);
            // line2Offset.HighlightThis(model);
            // arc.HighlightThis(model);
           
            // profile1.HighlightBox(model);
            // profile2.HighlightBox(model);
            
            // Console.WriteLine(model.ToJson(true));
            System.IO.File.WriteAllText(@"C:\temp\model.json", model.ToJson(true));
          

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

        
      }
}