package com.udelphi.agile.common;

public interface IOnRoomStateListener {
	void onRoomStateChanged(Room roomState);
	void onJoinedRoom(User state);
	void onLeftRoom(User state);
}
