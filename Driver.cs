using System;
using System.Collections;
using Chess.UI;

namespace Chess
{
	/// <summary>
	/// A launch point for the chess app.
	/// </summary>
	public class Driver
	{
		static void Main() 
		{

			bool speedTestIt = false;

			if( speedTestIt )
			{
				SpeedTest s = new SpeedTest();
			}
			else
			{
				Board board = new Board();
				ChessApp app = new ChessApp( board );
			}


// SHORT TERM
// Have the pieces remember what their "RealMoves" are .. if the state is the same (maybe use hash)
// Figure out a way that MovePiece() doesn't have to refigure everything out.
// Have AI pick a good piece to move, even when there are ties.
// Figure out how to have the hash take into account enPassant & Castles
// Create a profiler class to take time measurements of this program.
// In "RealMoves()" should we really be doing that kingDirection check there?  or in every piece
// Reformat code to use the "internal" keyword so that methods will be public to all classes inside this assembly
// Maybe have the Move Piece return somekind of informational struct that explains why the piece can't be moved... instead of throwing exception
// Make the MovePiece variables "piece1, piece2" more readable... and logical
// Check that ALL arguments are verified.. like "AddPiece( color, type, id )" for instance...
// Organize data files into "Engine" "DataStructures" "UI"
// Implement an Iterator for the History (and maybe start recording all techniques and where they can be found )

// LONG TERM
// AI should do quick search on what it feels the human is most likely to do, and then while the human is thinking, be thinking about its next move
		}
	}
}

