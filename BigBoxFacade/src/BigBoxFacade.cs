using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBoxFacade
{
      public static class BigBoxFacade
    {
        /// <summary>
        /// The BigBoxFacade function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A BigBoxFacadeOutputs instance containing computed results and the model with any new elements.</returns>
        public static BigBoxFacadeOutputs Execute(Dictionary<string, Model> inputModels, BigBoxFacadeInputs input)
        {
            Envelope envelope = null;
            inputModels.TryGetValue("Envelope", out var envelopeModel);
            if (envelopeModel != null)
            {
                var envelopes = new List<Envelope>();
                envelopes.AddRange(envelopeModel.AllElementsOfType<Envelope>());
                var aboveGradeEnvelopes = envelopes.Where(e => e.Elevation >= 0.0).ToList();
                if (aboveGradeEnvelopes.Count() > 0)
                {
                    envelope = aboveGradeEnvelopes.First();
                }
            }
            if (envelope == null)
            {
                var envMatl = new Material("envelope", new Color(1.0, 1.0, 1.0, 0.2), 0.0f, 0.0f);
                var height = 15.0;
                var footprint = Polygon.Rectangle(60, 40);
                var extrude = new Elements.Geometry.Solids.Extrude(footprint, height, Vector3.ZAxis, false);
                var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
                envelope = new Envelope(footprint, 0.0, height, Vector3.ZAxis, 0.0,
                              new Transform(), envMatl, geomRep, false, Guid.NewGuid(), "");
            }

            var output = new BigBoxFacadeOutputs(envelope.Profile.Perimeter.Area());
            var boundarySegments = envelope.Profile.Perimeter.Segments();
            var panelMat = new Material("envelope", new Color(1.0, 1.0, 1.0, 1), 0.5f, 0.5f);
            
            var lowestSegment = boundarySegments.OrderBy((s) => s.Start.Average(s.End).Y).First();
            foreach(var s in boundarySegments)
            {
                var d = s.Direction();

                try {
                    var t = new Transform(s.Start + new Vector3(0,0,0), d, d.Cross(Vector3.ZAxis));
                    var l = s.Length();

                    if (lowestSegment == s)
                    {
                        var grid = new Grid1d(s);
                        var doorWidth = 4;
                        var firstDoor = l / 10 * 3;
                        var secondDoor = l / 10 * 7;
                        grid.SplitAtPositions(new []{firstDoor - doorWidth / 2, firstDoor + doorWidth / 2, secondDoor - doorWidth / 2, secondDoor + doorWidth / 2});
                        var lines = grid.GetCells().Select(c => c.GetCellGeometry()).OfType<Line>();
                        var wallIdx = 0;
                        foreach (var wallLine in lines)
                        {
                            var segmentLength = wallLine.Length();
                            var segmentTransform = new Transform(wallLine.Start + new Vector3(0,0,0), d, d.Cross(Vector3.ZAxis));
                            if (wallIdx % 2 == 1)
                            {
                                CreateBranding(envelope.Height, new Transform(wallLine.Start.Average(wallLine.End), Vector3.ZAxis,0), output.model);
                            }
                            else
                            {
                                CreateStandardPanel(segmentLength, envelope.Height, 0.1, segmentTransform, panelMat, out FacadePanel panel);
                                output.model.AddElement(panel);
                            }
                            wallIdx++;
                        }
                    }
                    else
                    {
                        CreateStandardPanel(l,
                                            envelope.Height,
                                            0.1,
                                            t,
                                            panelMat,
                                            out FacadePanel panel);
                        output.model.AddElement(panel);
                    }

                    var parapetLine = new Line(new Vector3(s.Start.X, s.Start.Y, envelope.Height), new Vector3(s.End.X, s.End.Y, envelope.Height));
                    var parapet = new StandardWall(parapetLine, 0.4, 0.9, panelMat);
                    output.model.AddElement(parapet);
                }
                catch(System.Exception ex)
                {
                    System.Console.WriteLine(ex);
                }
            }

            return output;
        }

        private static void CreateStandardPanel(double width,
                                                double height,
                                                double thickness,
                                                Transform lowerLeft,
                                                Material material,
                                                out FacadePanel facadePanel)
        {
            var a = new Vector3(0,0,0);
            var b = new Vector3(width,0,0);
            var c = new Vector3(width, height, 0);
            var d = new Vector3(0, height, 0);

            var profile = new Profile(new Polygon(new[]{a,b,c,d}.Shrink(0.01)));
            var solidOps = new List<SolidOperation>(){new Extrude(profile, thickness, Vector3.ZAxis, false)};
            var representation = new Representation(solidOps);
            facadePanel = new FacadePanel(thickness, lowerLeft, material, representation, false, Guid.NewGuid(), "");
        }

        private static void CreateBranding(double height, Transform center, Model model)
        {
            var awningHeight = height * 0.4;
            var awningMat = new Material("awning", new Color(0, 0.1, 0.5, 1), 0, 0);
            var depth = 2;
            CreateColumn(-2.1, depth * -1.0 + 0.4, 0.3, awningHeight, center, awningMat, model);
            CreateColumn(2.1, depth * -1.0 + 0.4, 0.3, awningHeight, center, awningMat, model);
            var a = new Vector3(-4, 0, awningHeight);
            var b = new Vector3(-2.5, depth * -1.0, awningHeight);
            var c = new Vector3(2.5, depth * -1.0, awningHeight);
            var d = new Vector3(4, 0, awningHeight);
            var topHeight = height * 1.1;

            var profile = new Profile(new Polygon(new[]{a,b,c,d}));
            var solidOps = new List<SolidOperation>(){new Extrude(profile, topHeight - awningHeight, Vector3.ZAxis, false)};
            var representation = new Representation(solidOps);
            model.AddElement(new Envelope(profile, awningHeight, topHeight, Vector3.ZAxis, 0, center, awningMat, representation, false, Guid.NewGuid(), ""));
        }

        private static void CreateColumn(double centerX, double centerY, double radius, double height, Transform transform, Material material, Model model)
        {
            var a = new Vector3(centerX - radius, centerY - radius, 0);
            var b = new Vector3(centerX - radius, centerY + radius, 0);
            var c = new Vector3(centerX + radius, centerY + radius, 0);
            var d = new Vector3(centerX + radius, centerY - radius, 0);

            var profile = new Profile(new Polygon(new[]{a,b,c,d}));
            var solidOps = new List<SolidOperation>(){new Extrude(profile, height, Vector3.ZAxis, false)};
            var representation = new Representation(solidOps);
            model.AddElement(new Envelope(profile, 0.0, height, Vector3.ZAxis, 0, transform, material, representation, false, Guid.NewGuid(), ""));
        }
    }
}