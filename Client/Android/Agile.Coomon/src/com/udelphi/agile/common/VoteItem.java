package com.udelphi.agile.common;

import java.util.List;
import java.util.ArrayList;

public class VoteItem {

	public VoteItem() {
		VotedUsers = new ArrayList<Integer>();
	}

	public int Id;

	public boolean Opened;
	public boolean Closed;
	public boolean Finished;

	public String Content;
	public int OveralMark;

	public Room HostRoom;
	public int HostRoomId;

	public List<Integer> VotedUsers;

	@Override
	public boolean equals(Object o) {
		if (!(o instanceof VoteItem)) {
			return false;
		}
		VoteItem other = (VoteItem) o;
		return (Id == other.Id);
	}
	
	@Override
	public int hashCode() {
		return Id;
	}
	
	public void copyPartialState(VoteItem item){
		if(item==null)
			return;
		
		this.Opened=item.Opened;
		this.Closed=item.Closed;
		this.OveralMark=item.OveralMark;
	}
}
