using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a King piece.
	/// </summary>
	public class King : Chess.Piece
	{

		#region Static Members
		///////////////////////////////////////////////////////////////////////
		// Static.  Several static "precalculation" and constants to improve
		// readability and efficiency.
		public static ulong[,] attackTemplates;
		public static ulong[] LEFT_HELPER_BOARD = { 112UL, 8070450532247928832UL };
		public static ulong[] RIGHT_HELPER_BOARD = { 28UL, 2017612633061982208UL };
		public static int[] CASTLE_MASK = { 3, 12 };
		public static int[] CASTLE_MASK_LEFT = { 2, 8 };
		public static int[] CASTLE_MASK_RIGHT = { 1, 4 };

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static Constructor.  Initializes some of the static members.
		/// </summary>
		static King()
		{
			attackTemplates = new ulong[8,8];
			for( int x = 0; x < 8; x++ )
				for( int y = 0; y < 8; y++ )
					attackTemplates[x,y] = BitBoard.SetSurroundingBits( BitBoard.EMPTY, x, y, true );

		}
		#endregion

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location back to its starting position.
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( color == Player.WHITE )
				location.SetLocation( 3, 0 );
			else
				location.SetLocation( 3, 7 );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Updates this piece with respect to the given direction.
		/// </summary>
		/// <param name="dir">The direction from which to update from</param>
		public override void FixMoves( int dir )
		{
			// Kings do not have to FixMoves because they are ALWAYS refreshed every turn.
			// See Board.PutDownPiece() and Board.RefreshPieces()
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override. Populates the attackBoard bitBoard with true values everywhere this
		/// piece has an attacking influence on.
		/// </summary>
		public override void GenerateAttacks()
		{
			attackBoard = attackTemplates[location.x, location.y];
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public override void GenerateMoves()
		{
			ulong enemyAttackBoard = board.AttackBitBoard( (color == Player.WHITE) ? Player.BLACK : Player.WHITE );
			moveBoard = (attackBoard & ~enemyAttackBoard) & ~board.PiecesBitBoard[color];
			int castleBits = board.CastleBits;
			int homeRow = Player.FIRST_ROW[ color ];

			// Castling
			if ( ( castleBits & CASTLE_MASK[color] ) != 0 && location.x == 3 && location.y == homeRow )
			{
				// Left
				if ( ( castleBits & CASTLE_MASK_LEFT[color] ) != 0 
					&& sightGrid[Directions.Left] == board[0,homeRow]
					&& board[0,homeRow] != null
					&& board[0,homeRow].Type == Piece.ROOK
					&& board[0,homeRow].Color == color
					&& ( LEFT_HELPER_BOARD[color] & enemyAttackBoard) == 0)
				{
					moveBoard |= BitBoard.bit[1, Player.FIRST_ROW[color] ];
				}
				// Right
				if ( ( castleBits & CASTLE_MASK_RIGHT[color] ) != 0 
					&& sightGrid[Directions.Right] == board[7,homeRow]
					&& board[7,homeRow] != null
					&& board[7,homeRow].Type == Piece.ROOK
					&& board[7,homeRow].Color == color
					&& (RIGHT_HELPER_BOARD[color] & enemyAttackBoard) == 0)
				{
					moveBoard |= BitBoard.bit[5, Player.FIRST_ROW[color] ];
				}
			}
		}
	}
}
