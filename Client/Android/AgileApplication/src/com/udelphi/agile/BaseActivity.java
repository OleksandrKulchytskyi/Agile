package com.udelphi.agile;

import android.app.Activity;
import android.os.Bundle;
import android.widget.Toast;

import com.udelphi.agile.common.IOnErrorListener;
import com.udelphi.agile.common.IOnStateChangedListener;
import com.zsoft.signala.transport.StateBase;

public class BaseActivity extends Activity implements IOnErrorListener,
		IOnStateChangedListener {

	protected AgileApplication mApp;
	protected HubService hubService;

	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		mApp = (AgileApplication) this.getApplicationContext();
		hubService = mApp.getHubService();
	}

	protected void onResume() {
		super.onResume();
		mApp.setCurrentActivity(this);
		hubService.SubscribeOnError(this);
		hubService.SubscribeOnState(this);
	}

	protected void onPause() {
		clearReferences();
		super.onPause();
		hubService.SubscribeOnError(this);
	}

	protected void onDestroy() {
		clearReferences();
		super.onDestroy();
		hubService.UnsubscribeOnError(this);
		hubService.UnsubscribeOnState(this);
	}

	private void clearReferences() {
		Activity currActivity = mApp.getCurrentActivity();
		if (currActivity != null && currActivity.equals(this))
			mApp.setCurrentActivity(null);
	}

	@Override
	public void onError(Exception ex) {
		if (ex == null)
			return;
		String exMsg = ex.getMessage();
		Toast.makeText(getBaseContext(), exMsg, Toast.LENGTH_LONG).show();
	}

	@Override
	public void onStateChanged(Object newState, Object oldState) {
		StateBase state = (StateBase) newState;
		StateBase prevState = (StateBase) oldState;
		if (state != null && prevState != null) {
			Toast.makeText(getBaseContext(),
					prevState.getState() + " -> " + state.getState(),
					Toast.LENGTH_SHORT).show();
		}
	}
}
