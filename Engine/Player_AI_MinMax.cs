using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  Represents a Player that uses the AI_MinMax algorithm.
	/// </summary>
	public class Player_AI_MinMax : Player
	{
		public Player_AI_MinMax()
		{
			ai = new AI_MinMax( 3 );
			Initialize();
		}

		public Player_AI_MinMax( int depth )
		{
			ai = new AI_MinMax( depth );
			Initialize();
		}

		public void Initialize()
		{
			ai.ProgressAnnouncement += new ProgressEventHandler( ProgressAnnouncementResponder );
		}

		public override Move GetMove( Board board )
		{
			return ai.GetMove( board, side );
		}

		public void ProgressAnnouncementResponder( object o, ProgressEventArgs e )
		{
			game.UpdateProgress( e.percent );
		}
	}
}
