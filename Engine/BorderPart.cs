using System;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents a "dummy" piece that will help make a Border.  The BorderPart pieces in the
	/// border create a "dummy" wall around the board to assist in code simplification and for
	/// piece-search efficiency.
	/// </summary>
	public class BorderPart : Piece
	{

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes a new BorderPart.
		/// </summary>
		/// <param name="inIndex">the index of this BorderPart in the Border</param>
		public BorderPart( int inIndex )
		{
			sightGrid = new SightGrid( this );
			isReal = false;
			id = inIndex;
			
			type = Piece.BORDER;
			color = Player.UNKNOWN;
			location.SetLocation( -1, -1 );
		}

		#region Overriden Piece Methods That Do Not Apply To BorderPart
		public override void FixMoves( int dir )
		{
			throw new ApplicationException( "BorderPart: FixMoves: A Border piece should never be asked to fix its moves" );
		}
		public override void GenerateAttacks()
		{
			throw new ApplicationException( "BorderPart: GenerateAttacks: A Border piece should never be asked to generate attacks" );
		}
		public override void GenerateMoves()
		{
			throw new ApplicationException( "BorderPart: GenerateMoves: A Border piece should never be asked to generate moves" );
		}
		public override void SetLocationFromIndex()
		{
			throw new ApplicationException( "BorderPart: SetLocationFromIndex: A Border piece should never be asked to set location" );
		}
		public override void PutDownAt( int x, int y ) 
		{
			throw new ApplicationException( "BorderPart: PutDownAt: A Border piece should never be 'put down at'" );
		}
		#endregion

	}
}
