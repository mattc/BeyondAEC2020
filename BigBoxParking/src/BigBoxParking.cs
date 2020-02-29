using Elements;
using Elements.Geometry;
using Elements.Spatial;
using System.Collections.Generic;

namespace BigBoxParking
{
    public static class BigBoxParking
    {
        /// <summary>
        /// The BigBoxParking function.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A BigBoxParkingOutputs instance containing computed results and the model with any new elements.</returns>
        public static BigBoxParkingOutputs Execute(Dictionary<string, Model> inputModels, BigBoxParkingInputs input)
        {
            var output = new BigBoxParkingOutputs(253);
            var lot = new Floor(input.Boundary, 0.7, new Transform(0, 0, 0.8),
                            new Material(Colors.Granite, 0.0, 0.0, System.Guid.NewGuid(), ""), null, false, System.Guid.NewGuid(), null);
            output.model.AddElement(lot);

            var grid = new Grid2d(input.Boundary);
            grid.U.SplitAtOffset(8);
            grid.U.SplitAtOffset(8, true);
            var parkingCorridor = grid.GetCellAtIndices(1, 0);
            parkingCorridor.V.DivideByPattern(new[]{ 4, 5.6, 5.6, 4});
            var rowCount = ((int)((double)parkingCorridor.V.Domain.Length / 19.2));

            var separators = parkingCorridor.GetCellSeparators(GridDirection.U);
            for (var i = 2; i < separators.Count - 3; i+= 4)
            {
                AddLine((Line)separators[i], output.model);
            }

            for (var i = 0; i < rowCount * 4; i++)
            {
                if (i % 4 == 1 || i % 4 == 2)
                {
                    AddRow(parkingCorridor.GetCellAtIndices(0, i), output.model);
                }
            }

            return output;
        }

        public static void AddRow(Grid2d row, Model model)
        {
            row.U.DivideByApproximateLength(2.7, EvenDivisionMode.RoundUp);
            var spaceSeparators = row.GetCellSeparators(GridDirection.V);
            foreach (var line in spaceSeparators)
            {
                AddLine((Line)line, model);
            }
        }

        public static void AddLine(Line line, Model model)
        {
            var material = new Material(Colors.White, 0.0, 0.0, System.Guid.NewGuid(), "divider");
            var verticallyAdjustedLine = new Line(new Vector3(line.Start.X, line.Start.Y, -0.1), new Vector3(line.End.X, line.End.Y, -0.1));
            var wall = new StandardWall(verticallyAdjustedLine, 0.4, 1.2, material);
            model.AddElement(wall);
        }
    }
}