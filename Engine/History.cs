using System;
using System.Collections;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  Stores the move history of a game.
	/// </summary>
	public class History
	{

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// An ArrayList to store the HistoryElements
		/// </summary>
		protected ArrayList history;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A pointer to the current index of the ArrayList to undo and redo from.
		/// </summary>
		protected int current = 0;


		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes internal data.
		/// </summary>
		public History()
		{
			history = new ArrayList();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Clears the history of any previous information.
		/// </summary>
		public void Reset()
		{
			history.Clear();
			current = 0;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Stores the HistoryElement argument into the history.
		/// </summary>
		/// <param name="he">a HistoryElement to store</param>
		public void Add( HistoryEntry he )
		{
			// If the current history pointer is not at the end of the history
			if ( current <  history.Count)
			{
				// If this move is EXACTLY the same move that is in the record, then simply move the current pointer;
				if ( ( (HistoryEntry) history[current]).Equals( he ) )
				{
					current++;
					return;
				}
				else	// otherwise remove all entries after this one, because they no longer apply
					history.RemoveRange( current, history.Count - current );
			}

			if ( history.Count < current )
				throw new DataIntegrityException( "History: Error:Count < current" );

			history.Add( he );
			current = history.Count;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the previous move and moves the internal "pointer" back one move.
		/// If the history cannot undo further then an exception is thrown.
		/// </summary>
		/// <returns>the previous move</returns>
		public HistoryEntry Undo()
		{
			if ( current == 0 )
				throw new ApplicationException( "History:  Cannot Undo Any Further" );
			return (HistoryEntry) history[--current];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the next move and DOES NOT move the internal "pointer".  
		/// If the history cannot redo further then an exception is thrown.
		/// </summary>
		/// <returns>the next move</returns>
		public HistoryEntry Redo()
		{
			if ( current >= history.Count )
				throw new ApplicationException( "History:  Cannot Redo Any Further" );
			return (HistoryEntry) history[current];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Retrieves a move
		/// </summary>
		/// <param name="i">the index of the move to retrieve</param>
		/// <returns>the move at the specified index</returns>
		public HistoryEntry Peek( int i )
		{
			if ( i < 0 || i > history.Count )
				throw new ApplicationException( "History: Peek: Index out of bounds" );
			return (HistoryEntry) history[i];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the current index.
		/// </summary>
		public int Current
		{
			get
			{
				return current;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the number of entries in this History
		/// </summary>
		public int Count
		{
			get
			{
				return history.Count;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a boolean that represents whether this history can undo further.
		/// </summary>
		public bool CanUndo
		{
			get
			{
				return ( current > 0 );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a boolean that represents whether this history can redo further.
		/// </summary>
		public bool CanRedo
		{
			get
			{
				return ( current < history.Count );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Iterates through the history and creates a multiline string from all
		/// of the history entries.
		/// </summary>
		/// <returns>a dump of information across multiple lines</returns>
		public string DumpToString()
		{
			string output = "";
			int length = history.Count;;

			if ( length == 0)
				return " << Empty >>";

			for( int x = 0; x < length; x++ )
			{
				if ( x == current )
					output += " << CURRENT >>" + Environment.NewLine;
				output += String.Format("{0:00}", x) + ": " + (HistoryEntry) history[x];
				if ( x < length - 1 )
					output += Environment.NewLine;
			}
			if ( current == length )
				output += Environment.NewLine + " << CURRENT >>";
			return output;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Creates a single line summary of this history.
		/// </summary>
		/// <returns>the single line summary of this history</returns>
		public override string ToString()
		{
			return "History [" + current + "/" + history.Count + "]";
		}
	}
}
