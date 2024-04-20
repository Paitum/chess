using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// This class keeps track of which Pieces are "visible" from 
	/// certain directions.
	/// </summary>
	public class SightGrid
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// An array of pieces used to store the pieces located in different directions.
		/// </summary>
		private Piece[] grid;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The piece that is associated with this sightgrid.
		/// </summary>
		private Piece owner;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes a sight grid.
		/// </summary>
		/// <param name="inOwner">the piece that contains this sight grid.</param>
		public SightGrid( Piece inOwner )
		{
			grid = new Piece[8];
			owner = inOwner;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Clears the sightgrid of any pieces.
		/// </summary>
		public void Clear()
		{
			for ( int x = 0; x < 8; x++ )
			{
				grid[x] = null;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Looks to see if this sightgrid can "see" a given piece.  If it finds it
		/// it returns the direction where it found it.
		/// </summary>
		/// <param name="ap">the piece to look for</param>
		/// <returns>the direction of the piece.  Directions.Unknown is returns if not found.</returns>
		public int Find( Piece ap )
		{
			for ( int x = 0; x < 8; x++ )
			{
				if ( grid[x].Equals(ap) )
					return x;
			}
			return Directions.Unknown;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Validates this sightGrid to determine if the data contains corrupt data.
		/// Used for debugging purposes.
		/// </summary>
		public void Validate()
		{
			int odir;

			for( int dir = 0; dir < 8; dir++)
			{
				odir = (dir + 4) % 8;
				if ( this[dir] == null )
					throw new DataIntegrityException( "null in SightGrid" );
				if ( this[dir].SightGrid == null )
					throw new DataIntegrityException( "null SightGrid at end point" );
				if ( this[dir].SightGrid[odir] == null )
					throw new DataIntegrityException( "null SightGrid at reflexive end point" );

				if ( this[dir].SightGrid[odir].SightGrid != this )
					throw new DataIntegrityException( dir + " did not reflect back" );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Links itself to the line-of-sight pieces
		/// </summary>
		public void Link()
		{
			int odir;

			for( int dir = 0; dir < 8; dir++)
			{
				odir = Directions.Inverse[dir];

				if ( this[dir] == null )
					throw new DataIntegrityException( "SighGrid: SightGridLinker: this sightGrid has a null element" );
				if ( this[dir].SightGrid == null )
					throw new DataIntegrityException( "SighGrid: SightGridLinker: null SightGrid towards: " + dir );
				if ( this[dir].SightGrid[odir] == null )
					throw new DataIntegrityException( "SighGrid: SightGridLinker: null SightGrid Element towards: " + (dir + 4));

				this[dir].SightGrid[odir] = owner;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Unlinks this sightGrid with the line-of-sight pieces
		/// </summary>
		public void Unlink()
		{
			for( int dir = 0; dir < 4; dir++ )
			{
				this[dir].SightGrid[dir+4] = this[dir+4];
				this[dir+4].SightGrid[dir] = this[dir];
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Converts this sightgrid into a multiline string representation.
		/// </summary>
		/// <returns>the string representation</returns>
		public override String ToString()
		{
			String output = "";

			output += ( grid[(int) Directions.UpLeft] == null ) ? ".." : grid[(int) Directions.UpLeft].ToSmallString();
			output += ( grid[(int) Directions.Up] == null ) ? ".." : grid[(int) Directions.Up].ToSmallString();
			output += ( grid[(int) Directions.UpRight] == null ) ? ".." : grid[(int) Directions.UpRight].ToSmallString();
			output += Environment.NewLine;
			output += ( grid[(int) Directions.Left] == null ) ? ".." : grid[(int) Directions.Left].ToSmallString();
			output += "  ";
			output += ( grid[(int) Directions.Right] == null ) ? ".." : grid[(int) Directions.Right].ToSmallString();
			output += Environment.NewLine;
			output += ( grid[(int) Directions.DownLeft] == null ) ? ".." : grid[(int) Directions.DownLeft].ToSmallString();
			output += ( grid[(int) Directions.Down] == null ) ? ".." : grid[(int) Directions.Down].ToSmallString();
			output += ( grid[(int) Directions.DownRight] == null ) ? ".." : grid[(int) Directions.DownRight].ToSmallString();

			return output;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Indexer.  Provides access to this sightgrid through array notation by using a
		/// direction.
		/// </summary>
		public Piece this[ int dir ]
		{
			get
			{
				return grid[dir];
			}
			set
			{
				grid[dir] = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets or Sets the Owner of this sightgrid.
		/// </summary>
		public Piece Owner
		{
			get
			{
				return owner;
			}
			set
			{
				owner = value;
			}
		}

	}
}
