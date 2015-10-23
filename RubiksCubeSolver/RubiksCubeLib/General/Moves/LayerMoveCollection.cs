using System;
using System.Collections.Generic;
using System.Linq;

namespace RubiksCubeLib
{
    /// <summary>
    /// Represents a collection of moves of specific layers
    /// </summary>
    public class LayerMoveCollection : IList<LayerMove>, IMove
    {
        // *** PROPERTIES ***

        /// <summary>
        /// Returns a connected strings of all LayerMove names
        /// </summary>
        public string Name => string.Join(", ", this._moves.Select(m => m.Name).ToArray());

        /// <summary>
        /// Returns true if MultipleLayers are allowed
        /// </summary>
        public bool MultipleLayers => true;

        /// <summary>
        /// Gets the reverse move
        /// </summary>
        public IMove ReverseMove
        {
            get
            {
                var reverseMove = new LayerMoveCollection();
                reverseMove.AddRange(this.Select(m => new LayerMove(m.Layer, !m.Direction, m.Twice)));
                return reverseMove;
            }
        }

        /// <summary>
        /// Returns the count of the moves
        /// </summary>
        public int Count => this._moves.Count;

        /// <summary>
        /// Returns true if this Layer is readonly
        /// </summary>
        public bool IsReadOnly => false;

        // *** OPERATORS ***

        /// <summary>
        /// Adds a single LayerMove to the given collection
        /// </summary>
        /// <param name="collection">Defines the collection to be expanded</param>
        /// <param name="newMove">Defines the additional LayerMove</param>
        /// <returns></returns>
        public static LayerMoveCollection operator &(LayerMoveCollection collection, LayerMove newMove)
        {
            var lmc = new LayerMoveCollection();
            lmc.AddRange(collection);
            lmc.Add(newMove);
            return lmc;
        }

        /// <summary>
        /// Adds a collection of LayerMoves to the given collection
        /// </summary>
        /// <param name="collection1">Defines the collection to be expanded</param>
        /// <param name="collection2">Defines the collection to be added</param>
        /// <returns></returns>
        public static LayerMoveCollection operator &(LayerMoveCollection collection1, LayerMoveCollection collection2)
        {
            var lmc = new LayerMoveCollection();
            lmc.AddRange(collection1);
            lmc.AddRange(collection2);
            return lmc;
        }

        // *** PRIVATE FIELDS

        //The list of the single moves
        private readonly List<LayerMove> _moves = new List<LayerMove>();

        // *** METHODS ***

        /// <summary>
        /// Returns the index of a item in the collection
        /// </summary>
        /// <param name="item">Defines the item</param>
        /// <returns></returns>
        public int IndexOf(LayerMove item) => this._moves.IndexOf(item);

        /// <summary>
        /// Inserts an item in the collection at a index
        /// </summary>
        /// <param name="index">Defines the index where the item is meant to be inserted</param>
        /// <param name="item">Defines te</param>
        public void Insert(int index, LayerMove item)
        {
            this._moves.Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the given index
        /// </summary>
        /// <param name="index">Defines the index where the item is meant to be removed</param>
        public void RemoveAt(int index)
        {
            this._moves.RemoveAt(index);
        }

        /// <summary>
        /// Returns the layermove at the given index
        /// </summary>
        /// <param name="index">Defines the index of the item</param>
        /// <returns></returns>
        public LayerMove this[int index]
        {
            get
            {
                return this._moves[index];
            }
            set
            {
                this._moves[index] = value;
            }
        }

        /// <summary>
        /// Adds an item at the end of the collection
        /// </summary>
        /// <param name="item">Defines the item which is meant to be added</param>
        /// <exception cref="System.Exception">Thrown if this movement would be impossible</exception>
        public void Add(LayerMove item)
        {
            var flag = this._moves.Aggregate(item.Layer, (current, m) => current | m.Layer);
            if (CubeFlagService.IsPossibleMove(flag))
            {
                this._moves.Add(item);
            }
            else
                throw new Exception("Impossible movement");
        }

        /// <summary>
        /// Adds multiple items at the end of the collection
        /// </summary>
        /// <param name="items">Defines the items which are meant to be added</param>
        public void AddRange(IEnumerable<LayerMove> items)
        {
            foreach (var m in items) this.Add(m);
        }

        /// <summary>
        /// Adds multiple items at the end of the collection
        /// </summary>
        /// <param name="items">Defines the items which are meant to be added</param>
        public void AddRange(LayerMoveCollection items)
        {
            foreach (var m in items) this.Add(m);
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            this._moves.Clear();
        }

        /// <summary>
        /// Returns true if this collection contains the given item
        /// </summary>
        /// <param name="item">Defines the item to be searched for</param>
        /// <returns></returns>
        public bool Contains(LayerMove item) => this._moves.Contains(item);

        /// <summary>
        /// Copies the entire collection into the given array starting at the given index
        /// </summary>
        /// <param name="array">The</param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(LayerMove[] array, int arrayIndex)
        {
            this._moves.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the given item out of the collection
        /// </summary>
        /// <param name="item">Defines the item to be removed</param>
        /// <returns></returns>
        public bool Remove(LayerMove item) => this._moves.Remove(item);

        /// <summary>
        /// Returns the enumerator of the collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<LayerMove> GetEnumerator() => this._moves.GetEnumerator();

        /// <summary>
        /// Returns the enumerator of the collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this._moves.GetEnumerator();

        /// <summary>
        /// Parses a notation string into a LayerMoveCollection
        /// </summary>
        /// <param name="notation">Defines to string to be parsed</param>
        public static LayerMoveCollection Parse(string notation)
        {
            var layer = notation[0].ToString();
            var moves = new LayerMoveCollection();
            var ccwChars = new[] { '\'', 'i' };
            var direction = !ccwChars.All(notation.Contains);
            var twice = notation.Contains("2");

            switch (layer)
            {
                case "r": moves = new LayerMove(CubeFlag.RightSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, direction, twice); break;
                case "l": moves = new LayerMove(CubeFlag.LeftSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, !direction, twice); break;
                case "u": moves = new LayerMove(CubeFlag.TopLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, direction, twice); break;
                case "d": moves = new LayerMove(CubeFlag.BottomLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, !direction, twice); break;
                case "f": moves = new LayerMove(CubeFlag.FrontSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, direction, twice); break;
                case "b": moves = new LayerMove(CubeFlag.BackSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, !direction, twice); break;

                case "x":
                    moves = new LayerMove(CubeFlag.RightSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, direction, twice) &
            new LayerMove(CubeFlag.LeftSlice, !direction, twice); break;
                case "y":
                    moves = new LayerMove(CubeFlag.TopLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, direction, twice) &
            new LayerMove(CubeFlag.BottomLayer, !direction, twice); break;
                case "z":
                    moves = new LayerMove(CubeFlag.FrontSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, direction, twice) &
            new LayerMove(CubeFlag.BackSlice, !direction, twice); break;
            }

            return moves;
        }

        /// <summary>
        /// Parses a notation string into a layer move collection
        /// </summary>
        /// <param name="notation">String to be parsed</param>
        /// <param name="moves">The resulting layer move</param>
        /// <returns>True, if the string was successfully parsed into a layermove</returns>
        public static bool TryParse(string notation, out LayerMoveCollection moves)
        {
            var layer = notation[0].ToString();
            moves = new LayerMoveCollection();
            var ccwChars = new[] { '\'', 'i' };
            var direction = !ccwChars.Any(notation.Contains);
            var twice = notation.Contains("2");

            switch (layer)
            {
                case "r": moves = new LayerMove(CubeFlag.RightSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, direction, twice); break;
                case "l": moves = new LayerMove(CubeFlag.LeftSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, !direction, twice); break;
                case "u": moves = new LayerMove(CubeFlag.TopLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, direction, twice); break;
                case "d": moves = new LayerMove(CubeFlag.BottomLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, !direction, twice); break;
                case "f": moves = new LayerMove(CubeFlag.FrontSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, direction, twice); break;
                case "b": moves = new LayerMove(CubeFlag.BackSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, !direction, twice); break;

                case "x":
                    moves = new LayerMove(CubeFlag.RightSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSliceSides, direction, twice) &
            new LayerMove(CubeFlag.LeftSlice, !direction, twice); break;
                case "y":
                    moves = new LayerMove(CubeFlag.TopLayer, direction, twice) & new LayerMove(CubeFlag.MiddleLayer, direction, twice) &
            new LayerMove(CubeFlag.BottomLayer, !direction, twice); break;
                case "z":
                    moves = new LayerMove(CubeFlag.FrontSlice, direction, twice) & new LayerMove(CubeFlag.MiddleSlice, direction, twice) &
            new LayerMove(CubeFlag.BackSlice, !direction, twice); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// True, if the item accomplishes the equality conditions
        /// </summary>
        /// <param name="obj">LayerMoveCollection to be compared</param>
        public override bool Equals(object obj)
        {
            if (!(obj is LayerMoveCollection))
            {
                return false;
            }
            var coll = (LayerMoveCollection)obj;
            return coll._moves.SequenceEqual(this._moves);
        }

        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Transforms the layer move collection
        /// </summary>
        /// <param name="rotationLayer">Transformation layer</param>
        /// <returns>The transformed layer move collection</returns>
        public IMove Transform(CubeFlag rotationLayer)
        {
            var newMoves = new LayerMoveCollection();
            return this._moves.Select(move => new LayerMove(move.Layer, move.Direction, move.Twice)).Aggregate(newMoves, (current, newMove) => current & (LayerMove)newMove.Transform(rotationLayer));
        }
    }
}