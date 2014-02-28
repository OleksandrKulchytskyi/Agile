package com.udelphi.agile.common;

public interface IOnUserStateLoggedListener {
	void onStateCallback(SessionState state);

	void onUserLoggedCallback(User state);
}
