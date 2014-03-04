package com.udelphi.agile.common;

public interface IOnRoomStateListener {
	void onRoomStateChanged(Room roomState);
	void onJoinedRoom(Room state);
	void onLeftRoom(Room state);
}
