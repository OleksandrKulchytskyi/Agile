package com.udelphi.agile.common;

public interface IOnRoomStateListener {
	void onInitRoom(Room roomState);
	
	void onRoomStateChanged(Room roomState);
	void onJoinedRoom(User state);
	void onLeftRoom(User state);
	
	void onVoteItemOpened(VoteItem state);
	void onVoteItemClosed(VoteItem state);
}
