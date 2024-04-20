using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  Represents a Player that makes random moves.
	/// </summary>
	public class Player_AI_Random : Player
	{
		public Player_AI_Random()
		{
			ai = new AI_Random();
		}
		public override Move GetMove( Board board )
		{
			return ai.GetMove( board, side );
		}
	}
}
