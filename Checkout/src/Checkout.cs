using System;
using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System.Linq;
using GeometryEx;

namespace Checkout
{
    public static class Checkout
    {
        /// <summary>
        /// The Checkout function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A CheckoutOutputs instance containing computed results and the model with any new elements.</returns>
        public static CheckoutOutputs Execute(Dictionary<string, Model> inputModels, CheckoutInputs input)
        {
            var rooms = new List<Room>();
            inputModels.TryGetValue("Departments", out var model);
            if (model == null)
            {
                throw new ArgumentException("No Departments found.");
            }
            rooms.AddRange(model.AllElementsOfType<Room>());
            var checkout = rooms.Find(r => r.Department == "checkout");
            var lines = checkout.Perimeter.Segments().OrderBy(l => l.Length()).ToList();
            var ang = Math.Atan2(lines.First().End.Y - lines.First().Start.Y, lines.First().End.X - lines.First().Start.X) * (180 / Math.PI);
            var sequence = new Line(lines[0].Midpoint(), lines[1].Midpoint());
            var insert = sequence.Start;
            var counter = MakeCounter(false);
            var output = new CheckoutOutputs(0.0);
            foreach (var register in counter)
            {
                output.model.AddElement(new Mass(register.polygon.Rotate(Vector3.Origin, ang).MoveFromTo(Vector3.Origin, insert),
                                          register.height,
                                          register.material));
            }
            while (insert.DistanceTo(sequence.End) >= 1.2 + input.AisleWidth)
            {
                insert = sequence.PositionAt(1.2 + input.AisleWidth);
                sequence = new Line(insert, sequence.End);
                counter = MakeCounter();
                foreach (var register in counter)
                {
                    output.model.AddElement(new Mass(register.polygon.Rotate(Vector3.Origin, ang).MoveFromTo(Vector3.Origin, insert),
                                              register.height,
                                              register.material));
                }
            }
            return output;
        }

        private struct Register
        {
            public Polygon polygon;
            public double height;
            public Material material;
        }

        private static List<Register> MakeCounter(bool full = true)
        {
            var cash = new Material("cash", Colors.Yellow, 0.0f, 0.0f);
            var conveyer = new Material("conveyor", Colors.Orange, 0.0f, 0.0f);
            var counter = new List<Register>
            {
                new Register
                {
                    polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(0.6, 0.6)),
                    height = 0.7,
                    material = cash
                },
                new Register
                {
                    polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(2.5, 0.6))
                            .MoveFromTo(Vector3.Origin, new Vector3(-0.3, 0.6)),
                    height = 0.9,
                    material = conveyer
                },
            };
            if (!full)
            {
                return counter;
            }
            counter.Add(
                new Register
                {
                    polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(0.6, 0.6))
                            .MoveFromTo(Vector3.Origin, new Vector3(1.5, 0.0)),
                    height = 0.7,
                    material = cash
                });
            counter.Add(
                new Register
                {
                    polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(2.5, 0.6))
                            .MoveFromTo(Vector3.Origin, new Vector3(1.1, -0.6)),
                    height = 0.9,
                    material = conveyer
                });
            return counter;
        }
            
      }
}