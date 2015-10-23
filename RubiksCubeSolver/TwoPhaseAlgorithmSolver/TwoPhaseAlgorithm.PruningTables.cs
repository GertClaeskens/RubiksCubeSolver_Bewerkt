namespace TwoPhaseAlgorithmSolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class TwoPhaseAlgorithm
    {
       // private bool _loadPruningOK;
        private async void InitPruningTables()
        {
            //TASKS TOEGEVOEGD
            var flipTurningTask = Task.Run(() => { this.InitSliceFlipPruningTable(); });
            var twistPruningTask = Task.Run(() => { this.InitSliceTwistPruningTable(); });
            var slice1PruningTask = Task.Run(() => { this.InitSliceURFtoDLF_PruningTable(); });
            var slice2PruningTask = Task.Run(() => { this.InitSliceURtoDF_PruningTable(); });
            await Task.WhenAll(flipTurningTask, twistPruningTask, slice2PruningTask, slice1PruningTask);

        }

        // All pruning tables
        private byte[] sliceFlipPrun = new byte[N_SLICE1 * N_FLIP / 2];
        private byte[] sliceTwistPrun = new byte[N_SLICE1 * N_TWIST / 2 + 1];
        private byte[] sliceURFtoDLF_Prun = new byte[N_SLICE2 * N_URFtoDLF * N_PARITY / 2];
        private byte[] sliceURtoDF_Prun = new byte[N_SLICE2 * N_URtoDF * N_PARITY / 2];

        private void InitSliceTwistPruningTable()
        {
            if (LoadPruningTableSuccessful(
                Path.Combine(this.TablePath, "slice_twist_prun.file"),
                N_SLICE1 * N_TWIST / 2 + 1,
                out this.sliceTwistPrun))
            {
                return;
            }
            for (var i = 0; i < N_SLICE1 * N_TWIST / 2 + 1; i++) this.sliceTwistPrun[i] = 0xFF; // = -1 for signed byte
            var depth = 0;
            SetPruning(this.sliceTwistPrun, 0, 0);
            var done = 1;
            while (done != N_SLICE1 * N_TWIST)
            {
                for (var i = 0; i < N_SLICE1 * N_TWIST; i++)
                {
                    var _twist = i / N_SLICE1;
                    var _slice = i % N_SLICE1;
                    if (GetPruning(this.sliceTwistPrun, i) != depth)
                    {
                        continue;
                    }
                    for (var j = 0; j < N_MOVE; j++)
                    {
                        var newSlice = this.FRtoBR_Move[_slice * 24, j] / 24;
                        int newTwist = this.twistMove[_twist, j];
                        if (GetPruning(this.sliceTwistPrun, N_SLICE1 * newTwist + newSlice) != 0x0F)
                        {
                            continue;
                        }
                        SetPruning(this.sliceTwistPrun, N_SLICE1 * newTwist + newSlice, (byte)(depth + 1));
                        done++;
                    }
                }
                depth++;
            }
            SavePruningTable(Path.Combine(this.TablePath, "slice_twist_prun.file"), this.sliceTwistPrun);
        }

        private void InitSliceFlipPruningTable()
        {
            if (LoadPruningTableSuccessful(
                Path.Combine(this.TablePath, "slice_flip_prun.file"),
                N_SLICE1 * N_FLIP / 2,
                out this.sliceFlipPrun))
            {
                return;
            }
            for (var i = 0; i < N_SLICE1 * N_FLIP / 2; i++) this.sliceFlipPrun[i] = 0xFF; // = -1 for signed byte
            var depth = 0;
            SetPruning(this.sliceFlipPrun, 0, 0);
            var done = 1;
            while (done != N_SLICE1 * N_FLIP)
            {
                for (var i = 0; i < N_SLICE1 * N_FLIP; i++)
                {
                    var _flip = i / N_SLICE1;
                    var _slice = i % N_SLICE1;
                    if (GetPruning(this.sliceFlipPrun, i) != depth)
                    {
                        continue;
                    }
                    for (var j = 0; j < N_MOVE; j++)
                    {
                        var newSlice = this.FRtoBR_Move[_slice * 24, j] / 24;
                        int newFlip = this.flipMove[_flip, j];
                        if (GetPruning(this.sliceFlipPrun, N_SLICE1 * newFlip + newSlice) != 0x0F)
                        {
                            continue;
                        }
                        SetPruning(this.sliceFlipPrun, N_SLICE1 * newFlip + newSlice, (byte)(depth + 1));
                        done++;
                    }
                }
                depth++;
            }
            SavePruningTable(Path.Combine(this.TablePath, "slice_flip_prun.file"), this.sliceFlipPrun);
        }

        private void InitSliceURFtoDLF_PruningTable()
        {
            if (LoadPruningTableSuccessful(
                Path.Combine(this.TablePath, "slice_urf_to_dlf_prun.file"),
                N_SLICE2 * N_URFtoDLF * N_PARITY / 2,
                out this.sliceURFtoDLF_Prun))
            {
                return;
            }
            for (var i = 0; i < N_SLICE2 * N_URFtoDLF * N_PARITY / 2; i++) this.sliceURFtoDLF_Prun[i] = 0xFF; // -1
            var depth = 0;
            SetPruning(this.sliceURFtoDLF_Prun, 0, 0);
            var done = 1;
            var forbidden = new[] { 3, 5, 6, 8, 12, 14, 15, 17 };
            while (done < N_SLICE2 * N_URFtoDLF * N_PARITY)
            {
                for (var i = 0; i < N_SLICE2 * N_URFtoDLF * N_PARITY; i++)
                {
                    var parity = i % 2;
                    var URFtoDLF = (i / 2) / N_SLICE2;
                    var slice = (i / 2) % N_SLICE2;
                    var prun = GetPruning(this.sliceURFtoDLF_Prun, i);
                    if (prun != depth)
                    {
                        continue;
                    }
                    for (var j = 0; j < 18; j++)
                    {
                        if (forbidden.Contains(j))
                        {
                            continue;
                        }
                        var newSlice = this.FRtoBR_Move[slice, j] % 24;
                        int newURFtoDLF = this.URFtoDLF_Move[URFtoDLF, j];
                        int newParity = this.parityMove[parity, j];
                        if (GetPruning(this.sliceURFtoDLF_Prun, (N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity)
                            != 0x0F)
                        {
                            continue;
                        }
                        SetPruning(
                            this.sliceURFtoDLF_Prun,
                            (N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity,
                            (byte)(depth + 1));
                        done++;
                    }
                }
                depth++;
            }
            SavePruningTable(Path.Combine(this.TablePath, "slice_urf_to_dlf_prun.file"), this.sliceURFtoDLF_Prun);
        }

        private void InitSliceURtoDF_PruningTable()
        {
            if (LoadPruningTableSuccessful(
                Path.Combine(this.TablePath, "slice_ur_to_df_prun.file"),
                N_SLICE2 * N_URtoDF * N_PARITY / 2,
                out this.sliceURtoDF_Prun))
            {
                return;
            }
            for (var i = 0; i < N_SLICE2 * N_URtoDF * N_PARITY / 2; i++) this.sliceURtoDF_Prun[i] = 0xFF; // = -1 for signed byte
            var depth = 0;
            SetPruning(this.sliceURtoDF_Prun, 0, 0);
            var done = 1;
            while (done != N_SLICE2 * N_URtoDF * N_PARITY)
            {
                var forbidden = new[] { 3, 5, 6, 8, 12, 14, 15, 17 };
                for (var i = 0; i < N_SLICE2 * N_URtoDF * N_PARITY; i++)
                {
                    var _parity = i % 2;
                    var URtoDF = (i / 2) / N_SLICE2;
                    var slice = (i / 2) % N_SLICE2;
                    if (GetPruning(this.sliceURtoDF_Prun, i) != depth)
                    {
                        continue;
                    }
                    for (var j = 0; j < 18; j++)
                    {
                        if (forbidden.Contains(j))
                        {
                            continue;
                        }
                        var newSlice = this.FRtoBR_Move[slice, j] % 24;
                        int newURtoDF = this.URtoDF_Move[URtoDF, j];
                        int newParity = this.parityMove[_parity, j];
                        if (GetPruning(this.sliceURtoDF_Prun, (N_SLICE2 * newURtoDF + newSlice) * 2 + newParity)
                            != 0x0F)
                        {
                            continue;
                        }
                        SetPruning(this.sliceURtoDF_Prun, (N_SLICE2 * newURtoDF + newSlice) * 2 + newParity, (byte)(depth + 1));
                        done++;
                    }
                }
                depth++;
            }
            SavePruningTable(Path.Combine(this.TablePath, "slice_ur_to_df_prun.file"), this.sliceURtoDF_Prun);
        }

        private static void SetPruning(IList<byte> table, int index, byte value)
        {
            if ((index & 1) == 0)
                table[index / 2] &= (byte)(0xF0 | value);
            else
                table[index / 2] &= (byte)(0x0F | (value << 4));
        }

        private static byte GetPruning(IList<byte> table, int index)
        {
            if ((index & 1) == 0)
                return (byte)(table[index / 2] & 0x0F);
            return (byte)((table[index / 2] & 0xF0) >> 4);
        }

        private static byte[] LoadPruningTable(string filename, int length = 0)
        {
            var newBytes = File.ReadAllBytes(filename);
            if (newBytes.Length != length)
                throw new Exception("Invalid input file");
            return newBytes;
        }

        private static void SavePruningTable(string filename, byte[] table)
        {
            File.WriteAllBytes(filename, table);
        }

        private static bool LoadPruningTableSuccessful(string filename, int length, out byte[] newTable)
        {
            newTable = new byte[length];
            try
            {
                newTable = LoadPruningTable(filename, length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
