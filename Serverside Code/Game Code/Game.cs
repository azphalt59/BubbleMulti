using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;


	public class Player : BasePlayer 
	{
		public string name
	}

	

	[RoomType("bounce")]
	public class GameCode : Game<Player> 
	{
		
		 

		// This method is called when an instance of your the game is created
		public override void GameStarted() 
		{
			// anything you write to the Console will show up in the 
			// output window of the development server
			Console.WriteLine("Game is started: " + RoomId);
		}

		private void resetgame() 
		{
			
			// broadcast who won the round
			if(winner.toadspicked > 0) {
				Broadcast("Chat", "Server", winner.ConnectUserId + " picked " + winner.toadspicked + " Toadstools and won this round.");
			} else {
				Broadcast("Chat", "Server", "No one won this round.");
			}
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
					pl.Send("PlayerJoined", player.ConnectUserId, player.ConnectUserID);
					player.Send("PlayerJoined", pl.ConnectUserId, pl.ConnectUserId);
				}
			}
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
				case "Move":
					player.posx = message.GetFloat(0);
					player.posz = message.GetFloat(1);
					Broadcast("Move", player.ConnectUserId, player.posx, player.posz);
					break;
				case "Chat":
					foreach(Player pl in Players) {
						if(pl.ConnectUserId != player.ConnectUserId) {
							pl.Send("Chat", r.GetString(0), message.GetString(0));
						}
					}
					break;
			}
		}
	}
