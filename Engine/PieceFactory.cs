using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  Creates new instances of Pieces.
	/// </summary>
	public class PieceFactory
	{
		public static Piece CreatePiece( Board board, int type, int color, int id, bool wasPawn)
		{
			Piece piece;

			switch ( type )
			{
				case Piece.PAWN:
					piece = new Pawn();
					break;
				case Piece.ROOK:
					piece = new Rook();
					break;
				case Piece.KNIGHT:
					piece = new Knight();
					break;
				case Piece.BISHOP:
					piece = new Bishop();
					break;
				case Piece.QUEEN:
					piece = new Queen();
					break;
				case Piece.KING:
					piece = new King();
					break;
				default:
					throw new ApplicationException( "PieceFactory: CreatePiece: Invalid Piece Type" );
			}

			piece.Initialize( board, type, color, id, wasPawn);

			return piece;
		}
	}
}
