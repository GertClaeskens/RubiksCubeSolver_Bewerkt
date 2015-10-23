namespace TwoPhaseAlgorithmSolver
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public partial class TwoPhaseAlgorithm
    {
        private void InitMoveTables()
        {

            InitMoves();
            InitTwistMoveTable();
            InitFlipMoveTable();
            InitParityMove();
            InitFRtoBR_MoveTable();
            InitURFtoDLF_MoveTable();
            InitURtoUL_MoveTable();
            InitUBtoDF_MoveTable();
            InitURtoDF_MoveTable();
            InitMergeURtoULandUBtoDF();


            //var initMovesTask = Task.Run(() => { this.InitMoves(); });
            //var twistMoveTableTask = Task.Run(() => { this.InitTwistMoveTable(); });
            //var flipMoveTableTask = Task.Run(() => { this.InitFlipMoveTable(); });
            //var parityMoveTask = Task.Run(() => { this.InitParityMove(); });
            //var moveTableTask = Task.Run(() => { this.InitFRtoBR_MoveTable(); });
            //var table1Task = Task.Run(() => { this.InitURFtoDLF_MoveTable(); });
            //var table2Task = Task.Run(() => { this.InitURtoUL_MoveTable(); });
            //var table3Task = Task.Run(() => { this.InitUBtoDF_MoveTable(); });
            //var table4Task = Task.Run(() => { this.InitURtoDF_MoveTable(); });
            //var table5Task = Task.Run(() => { this.InitMergeURtoULandUBtoDF(); });
            //await Task.WhenAll(initMovesTask , twistMoveTableTask, flipMoveTableTask, parityMoveTask,moveTableTask,table1Task, table2Task, table3Task, table4Task, table5Task);


            //await Task.Run(() => { this.InitMoves(); });
            //await Task.Run(() => { this.InitTwistMoveTable(); });
            //await Task.Run(() => { this.InitFlipMoveTable(); });
            //await Task.Run(() => { this.InitParityMove(); });
            //await Task.Run(() => { this.InitFRtoBR_MoveTable(); });
            //await Task.Run(() => { this.InitURFtoDLF_MoveTable(); });
            //await Task.Run(() => { this.InitURtoUL_MoveTable(); });
            //await Task.Run(() => { this.InitUBtoDF_MoveTable(); });
            //await Task.Run(() => { this.InitURtoDF_MoveTable(); });
            //await Task.Run(() => { this.InitMergeURtoULandUBtoDF(); });



        }

        private readonly CoordCube[] moves = new CoordCube[N_MOVE];

        private short[,] twistMove = new short[N_TWIST, N_MOVE];
        private short[,] flipMove = new short[N_FLIP, N_MOVE];
        private short[,] parityMove;
        private short[,] FRtoBR_Move = new short[N_FRtoBR, N_MOVE];
        private short[,] URFtoDLF_Move = new short[N_URFtoDLF, N_MOVE];
        private short[,] URtoUL_Move = new short[N_URtoUL, N_MOVE];
        private short[,] UBtoDF_Move = new short[N_UBtoDF, N_MOVE];
        private short[,] URtoDF_Move = new short[N_URtoDF, N_MOVE];
        private short[,] mergeURtoULandUBtoDF = new short[336, 336];

        private readonly CoordCube cc1 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 0, 0, 0 }),
            CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 }),
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        private readonly CoordCube cc2 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 1, 1, 3, 0, 1, 1, 4 }),
            CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 2, 2, 2, 5, 1, 1, 11 }),
            new byte[] { 2, 0, 0, 1, 1, 0, 0, 2 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        private readonly CoordCube cc3 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 1, 1, 4, 1, 0, 0 }),
            CoordCube.FromInversions(new byte[] { 0, 0, 1, 1, 1, 1, 2, 2, 7, 4, 0, 0 }),
            new byte[] { 1, 2, 0, 0, 2, 1, 0, 0 },
            new byte[] { 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0 });
        private readonly CoordCube cc4 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 0, 0, 0, 3 }),
            CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0 }),
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        private readonly CoordCube cc5 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 1, 1, 4, 1, 0 }),
            CoordCube.FromInversions(new byte[] { 0, 0, 0, 1, 1, 1, 1, 2, 2, 7, 4, 0 }),
            new byte[] { 0, 1, 2, 0, 0, 2, 1, 0 },
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        private readonly CoordCube cc6 = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 1, 1, 4, 1 }),
            CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 7, 4 }),
            new byte[] { 0, 0, 1, 2, 0, 0, 2, 1 },
            new byte[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 });


        private void InitMoves()
        {
            //this.moves[0] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 0, 0, 0 }),
            //CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 }),
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            //this.moves[3] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 1, 1, 3, 0, 1, 1, 4 }),
            //CoordCube.FromInversions(new byte[] { 0, 1, 1, 1, 0, 2, 2, 2, 5, 1, 1, 11 }),
            //new byte[] { 2, 0, 0, 1, 1, 0, 0, 2 },
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); ;

            //this.moves[6] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 1, 1, 4, 1, 0, 0 }),
            //CoordCube.FromInversions(new byte[] { 0, 0, 1, 1, 1, 1, 2, 2, 7, 4, 0, 0 }),
            //new byte[] { 1, 2, 0, 0, 2, 1, 0, 0 },
            //new byte[] { 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0 }); ;

            //this.moves[9] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 0, 0, 0, 3 }),
            //CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0 }),
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            //this.moves[12] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 1, 1, 4, 1, 0 }),
            //CoordCube.FromInversions(new byte[] { 0, 0, 0, 1, 1, 1, 1, 2, 2, 7, 4, 0 }),
            //new byte[] { 0, 1, 2, 0, 0, 2, 1, 0 },
            //new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); ;

            //this.moves[15] = new CoordCube(CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 1, 1, 4, 1 }),
            //CoordCube.FromInversions(new byte[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 7, 4 }),
            //new byte[] { 0, 0, 1, 2, 0, 0, 2, 1 },
            //new byte[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 });


            this.moves[0] = cc1;

            this.moves[3] = cc2;

            this.moves[6] = cc3;

            this.moves[9] = cc4;

            this.moves[12] = cc5;

            this.moves[15] = cc6;

            for (var i = 0; i < N_MOVE; i += 3)
            {
                var move = this.moves[i].DeepClone();
                for (var j = 1; j < 3; j++)
                {
                    move.Multiply(this.moves[i]);
                    this.moves[i + j] = move.DeepClone();
                }
            }
        }

        private void InitTwistMoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "twist_move.file"),
                N_MOVE,
                N_TWIST,
                out this.twistMove))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_TWIST; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.Twist = i;
                    a.Multiply(this.moves[j]);
                    this.twistMove[i, j] = a.Twist;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "twist_move.file"), this.twistMove);
        }

        private void InitFlipMoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "flip_move.file"),
                N_MOVE,
                N_FLIP,
                out this.flipMove))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_FLIP; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.Flip = i;
                    a.Multiply(this.moves[j]);
                    this.flipMove[i, j] = a.Flip;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "flip_move.file"), this.twistMove);
        }


        private void InitParityMove()
        {
            this.parityMove = new short[,] { { 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 } };
        }

        private void InitFRtoBR_MoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "fr_to_br_move.file"),
                N_MOVE,
                N_FRtoBR,
                out this.FRtoBR_Move))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_FRtoBR; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.FRtoBR = i;
                    a.Multiply(this.moves[j]);
                    this.FRtoBR_Move[i, j] = a.FRtoBR;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "fr_to_br_move.file"), this.FRtoBR_Move);
        }

        private void InitURFtoDLF_MoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "urf_to_dlf_move.file"),
                N_MOVE,
                N_URFtoDLF,
                out this.URFtoDLF_Move))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_URFtoDLF; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.URFtoDLF = i;
                    a.Multiply(this.moves[j]);
                    this.URFtoDLF_Move[i, j] = a.URFtoDLF;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "urf_to_dlf_move.file"), this.URFtoDLF_Move);
        }

        private void InitURtoUL_MoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "ur_to_ul_move.file"),
                N_MOVE,
                N_URtoUL,
                out this.URtoUL_Move))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_URtoUL; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.URtoUL = i;
                    a.Multiply(this.moves[j]);
                    this.URtoUL_Move[i, j] = a.URtoUL;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "ur_to_ul_move.file"), this.URtoUL_Move);
        }

        private void InitUBtoDF_MoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "ub_to_df_move.file"),
                N_MOVE,
                N_UBtoDF,
                out this.UBtoDF_Move))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_UBtoDF; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.UBtoDF = i;
                    a.Multiply(this.moves[j]);
                    this.UBtoDF_Move[i, j] = a.UBtoDF;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "ub_to_df_move.file"), this.UBtoDF_Move);
        }

        private void InitURtoDF_MoveTable()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "ur_to_df_move.file"),
                N_MOVE,
                N_URtoDF,
                out this.URtoDF_Move))
            {
                return;
            }
            var a = new CoordCube();
            for (short i = 0; i < N_URtoDF; i++)
            {
                for (var j = 0; j < N_MOVE; j++)
                {
                    a.URtoDF = i;
                    a.Multiply(this.moves[j]);
                    this.URtoDF_Move[i, j] = (short)a.URtoDF;
                }
            }
            SaveMoveTable(Path.Combine(this.TablePath, "ur_to_df_move.file"), this.URtoDF_Move);
        }

        private void InitMergeURtoULandUBtoDF()
        {
            if (LoadMoveTableSuccessful(
                Path.Combine(this.TablePath, "merge_move.file"),
                336,
                336,
                out this.mergeURtoULandUBtoDF))
            {
                return;
            }
            for (short uRtoUL = 0; uRtoUL < 336; uRtoUL++)
                for (short uBtoDF = 0; uBtoDF < 336; uBtoDF++) this.mergeURtoULandUBtoDF[uRtoUL, uBtoDF] = (short)CoordCube.GetURtoDF(uRtoUL, uBtoDF);
            SaveMoveTable(Path.Combine(this.TablePath, "merge_move.file"), this.mergeURtoULandUBtoDF);
        }

        #region Save and load move tables from files
        private static short[,] LoadMoveTable(string filename, int lengthX, int lengthY)
        {
            if (!File.Exists(filename)) throw new Exception("File does not exist!");
            var newTable = new short[lengthY, lengthX];
            using (var sr = new StreamReader(filename))
            {
                var rowIndex = 0;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line != null)
                    {
                        var entries = line.Split(';');
                        if (entries.Length != lengthX)
                            throw new Exception("Invalid input file!");
                        for (var columnIndex = 0; columnIndex < lengthX; columnIndex++)
                        {
                            short entry;
                            if (short.TryParse(entries[columnIndex], out entry))
                                newTable[rowIndex, columnIndex] = short.Parse(entries[columnIndex]);
                            else
                                throw new Exception("Invalid input file!");
                        }
                    }
                    rowIndex++;
                }
                if (rowIndex != lengthY)
                    throw new Exception("Invalid input file!");
                sr.Close();
            }

            return newTable;
        }

        private static bool LoadMoveTableSuccessful(string filename, int lengthX, int lengthY, out short[,] newTable)
        {
            newTable = new short[lengthY, lengthX];
            try
            {
                newTable = LoadMoveTable(filename, lengthX, lengthY);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SaveMoveTable(string filename, short[,] table)
        {
            using (var sw = new StreamWriter(filename))
            {
                for (var rowIndex = 0; rowIndex < table.GetLength(0); rowIndex++)
                {
                    var line = table[rowIndex, 0].ToString();
                    for (var columnIndex = 1; columnIndex < table.GetLength(1); columnIndex++)
                        line += $";{table[rowIndex, columnIndex]}";
                    sw.WriteLine(line);
                }
            }
        }
        #endregion
    }
}
