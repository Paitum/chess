using System;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// A "wall" of "dummy" pieces lining the outside of the board.  This
	/// allows for simpler code and for more efficient piece-searches.
	/// </summary>
	public class Border
	{

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the array of BorderParts that make up the border.
		/// </summary>
		private BorderPart[] borderParts;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Creates the Border.
		/// </summary>
		public Border()
		{
			// Instantiate the BorderParts
			borderParts = new BorderPart[36];
			for ( int x = 0; x < 36; x++ )
				borderParts[x] = new BorderPart(x);

			Reset();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Resets the Border data structure to its default state.  The default state 
		/// is when there are no pieces on the board and the borderParts are pointing 
		/// to each other.
		/// </summary>
		public void Reset()
		{
			// Clear all of the borderParts' SightGrids
			for ( int x = 0; x < 36; x++ )
				borderParts[x].SightGrid.Clear();

			// Link the borderParts to their default 'links'
			for ( int x = 1; x < 9; x++ )
			{
				borderParts[x].SightGrid[Directions.UpRight] = borderParts[18 - x];
				borderParts[x].SightGrid[Directions.Right] = borderParts[27 - x];
				borderParts[x].SightGrid[Directions.DownRight] = borderParts[36 - x];
			}
			for ( int x = 10; x < 18; x++ )
			{
				borderParts[x].SightGrid[Directions.DownRight] = borderParts[36 - x];
				borderParts[x].SightGrid[Directions.Down] = borderParts[45 - x];
				borderParts[x].SightGrid[Directions.DownLeft] = borderParts[18 - x];
			}
			for ( int x = 19; x < 27; x++ )
			{
				borderParts[x].SightGrid[Directions.DownLeft] = borderParts[54 - x];
				borderParts[x].SightGrid[Directions.Left] = borderParts[27 - x];
				borderParts[x].SightGrid[Directions.UpLeft] = borderParts[36 - x];
			}
			for ( int x = 28; x < 36; x++ )
			{
				borderParts[x].SightGrid[Directions.UpLeft] = borderParts[36 - x];
				borderParts[x].SightGrid[Directions.Up] = borderParts[45 - x];
				borderParts[x].SightGrid[Directions.UpRight] = borderParts[54 - x];
			}
			borderParts[0].SightGrid[Directions.UpRight] = borderParts[18];
			borderParts[9].SightGrid[Directions.DownRight] = borderParts[27];
			borderParts[18].SightGrid[Directions.DownLeft] = borderParts[0];
			borderParts[27].SightGrid[Directions.UpLeft] = borderParts[9];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Calculates what BorderPart in this Border is in the direction from
		/// the coordinate.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="dir">direction of the desired BorderPart</param>
		/// <returns>the desired BorderPart</returns>
		public BorderPart GetBorderPart( int x, int y, int dir )
		{
			int borderPartIndex;

			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				throw new ApplicationException( "Border: GetBorderPart: x or y inputs were out of range [0-7]" );

			switch ( dir )
			{
				case Directions.UpRight:
					borderPartIndex = 18 + x - y;
					break;
				case Directions.Right:
					borderPartIndex = 26 - y;
					break;
				case Directions.DownRight:
					borderPartIndex = 34 -x - y;
					break;
				case Directions.Down:
					borderPartIndex = 35 - x;
					break;
				case Directions.DownLeft:
					borderPartIndex = ( 36 + y - x ) % 36;
					break;
				case Directions.Left:
					borderPartIndex = 1 + y;
					break;
				case Directions.UpLeft:
					borderPartIndex = 9 - ( 7 - x ) + y;
					break;
				case Directions.Up:
					borderPartIndex = 10 + x;
					break;
				case Directions.Unknown:
					throw new ApplicationException("Border: GetBorderPart: Unknown direction is not allowed");
				default:
					throw new ApplicationException("Border: GetBorderPart: Invalid Direction");
			}

			return borderParts[ borderPartIndex ];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Indexer.  Gets the BorderPart in this Border at the specified index 
		/// </summary>
		public BorderPart this[int inIndex]
		{
			get
			{
				return borderParts[inIndex];
			}
		}
	}
}
