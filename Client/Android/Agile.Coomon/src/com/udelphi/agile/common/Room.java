package com.udelphi.agile.common;

import java.util.ArrayList;
import java.util.List;

public class Room {

	public Room() {
		ConnectedUsers = new ArrayList<User>();
		ItemsToVote = new ArrayList<VoteItem>();
	}

	public int Id;
	public String Name;
	public String Description;

	public List<User> ConnectedUsers;
	public List<VoteItem> ItemsToVote;

	public void AddUser(User usr) {
		ConnectedUsers.add(usr);
	}

	public void RemoveUser(User usr) {
		ConnectedUsers.remove(usr);
	}
}
