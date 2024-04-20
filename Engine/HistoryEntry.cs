using System;
using System.Collections;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Enumeration.  Defines the states that a HistoryEntry can represent
	/// </summary>
	public enum HistoryEvent { Move, Capture, EnPassant, Castle, PawnUpgrade };
	
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// struct.  A collection of value-types that store a single move, along
	/// with other data for more efficient undoing and redoing.
	/// </summary>
	public struct HistoryEntry
	{
		public Location from, to;
		public SimplePiece movedPiece;
		public SimplePiece capturedPiece;
		public HistoryEvent historyEvent;
		public Board.State state;	// For efficiency
		public int numberOfTurnsSincePawnOrCapture;	// For efficiency
		public ulong checkRestrictionBoard;	// For efficiency
		public ulong enPassantBoard;
		public int castleBits;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Sets the internal data to the arguments.
		/// </summary>
		/// <param name="F">location where the piece moved FROM</param>
		/// <param name="T">location where the piece moved TO</param>
		/// <param name="MP">moving piece</param>
		/// <param name="CP">captured piece</param>
		/// <param name="HE">what kind of move</param>
		/// <param name="S">state of the board before the move</param>
		/// <param name="NOTSPOC">numberOfTurnsSincePawnOrCapture of the board before the move</param>
		/// <param name="CRB">checkRestrictionBoard of the board before the move</param>
		/// <param name="EPB">enPassantBoard of the board before the move</param>
		/// <param name="CB">castleBits of the board before the move</param>
		public HistoryEntry( Location F, Location T, SimplePiece MP, SimplePiece CP, HistoryEvent HE, Board.State S, int NOTSPOC, ulong CRB, ulong EPB, int CB)
		{
			from = F;
			to = T;
			movedPiece = MP;
			capturedPiece = CP;
			historyEvent = HE;
			state = S;
			numberOfTurnsSincePawnOrCapture = NOTSPOC;
			checkRestrictionBoard = CRB;
			enPassantBoard = EPB;
			castleBits = CB;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs a member-wise equality test.
		/// </summary>
		/// <param name="he">a HistoryEntry object to test</param>
		/// <returns>true if all the members have the same value</returns>
		public bool Equals( HistoryEntry he )
		{
			return ( from.Equals( he.from )
				&& to.Equals( he.to )
				&& movedPiece.Equals( he.movedPiece )
				&& capturedPiece.Equals( he.capturedPiece )
				&& historyEvent == he.historyEvent
				&& state == he.state
				&& numberOfTurnsSincePawnOrCapture == he.numberOfTurnsSincePawnOrCapture
				&& checkRestrictionBoard == he.checkRestrictionBoard
				&& enPassantBoard == he.enPassantBoard
				&& castleBits == he.castleBits );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Generates a descriptive string based on the kind of move.
		/// </summary>
		/// <returns>an description of the move</returns>
		public override string ToString()
		{
			string output;
			string theMove = "from (" + from.x + "," + from.y + ") to (" + to.x + "," + to.y + ")";

			switch ( historyEvent )
			{
				case HistoryEvent.Move:
					output = movedPiece.ToNormalString() + " moved " + theMove;
					break;
				case HistoryEvent.EnPassant:
				case HistoryEvent.Capture:
					output = movedPiece.ToNormalString() + " captured a " + capturedPiece.ToNormalString() + " by moving " + theMove;
					break;
				case HistoryEvent.Castle:
					output = movedPiece.ToNormalString() + " castled " + theMove;
					break;
				case HistoryEvent.PawnUpgrade:
					output = Player.PlayerToString[movedPiece.Color] + " " + Piece.TypeToString[Piece.PAWN] + " became a ";
					output += Piece.TypeToString[ movedPiece.Type ];
					if ( ! capturedPiece.IsNull )
						output += " and captured a " + capturedPiece.ToNormalString();
					output += " by moving " + theMove;
					break;
				default:
					throw new ApplicationException( "HistoryEntry: Description: Invalid HistoryEvent" );
			}

			return output;
		}
	}
}