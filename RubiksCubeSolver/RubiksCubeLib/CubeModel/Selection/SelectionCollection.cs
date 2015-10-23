using System.Collections.Generic;
using System.Linq;

namespace RubiksCubeLib.CubeModel
{
    /// <summary>
    /// Represents a collection of faces at a specific cube position and the related selections
    /// </summary>
    public class SelectionCollection
    {
        // *** CONSTRUCTOR ***

        /// <summary>
        /// Initializes a new instance of the SelectionCollection class
        /// </summary>
        public SelectionCollection()
        {
            this._selections = new Dictionary<PositionSpec, Selection>();
        }

        // *** PRIVATE FIELDS ***

        private readonly Dictionary<PositionSpec, Selection> _selections;

        // *** METHODS ***

        /// <summary>
        /// Adds a new entry to the collection
        /// </summary>
        /// <param name="key">Cube position and face position</param>
        /// <param name="value">Selection</param>
        public void Add(PositionSpec key, Selection value)
        {
            this._selections.Add(key, value);
        }

        /// <summary>
        /// Resets the selection of all entries
        /// </summary>
        public void Reset()
        {
            foreach (var key in this._selections.Keys.ToList())
            {
                this._selections[key] = Selection.None;
            }
        }

        /// <summary>
        /// Gets or sets the selection of the given cube and face position
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Selection this[PositionSpec key]
        {
            get
            {
                return this._selections.ContainsKey(key) ? this._selections[key] : Selection.None;
            }
            set
            {
                if (this._selections.ContainsKey(key)) this._selections[key] = value;
            }
        }
    }
}