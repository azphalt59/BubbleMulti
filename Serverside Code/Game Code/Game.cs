using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;


	public class Player : BasePlayer 
	{
		public string Name;
		public int PlayerID;
		public bool isReady = false;
	}

	

	[RoomType("bounce")]
	public class GameCode : Game<Player> 
	{
	public int playerConnected = 0;
		 

		// This method is called when an instance of your the game is created
		public override void GameStarted() 
		{
			// anything you write to the Console will show up in the 
			// output window of the development server
			Console.WriteLine("Game is started: " + RoomId);
		}

		private void resetgame() 
		{
			
		}


		// This method is called when the last player leaves the room, and it's closed down.
		public override void GameClosed() {
			Console.WriteLine("RoomId: " + RoomId);
		}

		// This method is called whenever a player joins the game
		public override void UserJoined(Player player) 
		{
			foreach(Player pl in Players) {
				if(pl.ConnectUserId != player.ConnectUserId) {
					pl.Send("PlayerJoined", player.Name, player.PlayerID, player.isReady);
					player.Send("PlayerJoined", pl.Name, pl.PlayerID, pl.isReady);

				}
			}
			//playerConnected++;
		}

		// This method is called when a player leaves the game
		public override void UserLeft(Player player) {
			Broadcast("PlayerLeft", player.ConnectUserId);
		}

		// This method is called when a player sends a message into the server code
		public override void GotMessage(Player player, Message message) 
		{
			switch(message.Type) {
				// called when a player clicks on the ground
				

				case "Chat":
					foreach(Player pl in Players) {
						if(pl.ConnectUserId != player.ConnectUserId) {
							pl.Send("Chat", message.GetString(0), message.GetString(1));
						}
					}
					break;
				case "ChangeName":
				playerConnected++;
				foreach (Player pl in Players)
					{
						if(pl.ConnectUserId != player.ConnectUserId)
						{
						
						pl.Send("ChangeName", message.GetInt(0), message.GetString(1));
						}
					}
				break;

			}
		}
	}
