package com.udelphi.agile;

import android.app.Activity;
import android.os.Bundle;

public class BaseActivity extends Activity {
	
	protected AgileApplication mApp;

	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		mApp = (AgileApplication) this.getApplicationContext();
	}

	protected void onResume() {
		super.onResume();
		mApp.setCurrentActivity(this);
	}

	protected void onPause() {
		clearReferences();
		super.onPause();
	}

	protected void onDestroy() {
		clearReferences();
		super.onDestroy();
	}

	private void clearReferences() {
		Activity currActivity = mApp.getCurrentActivity();
		if (currActivity != null && currActivity.equals(this))
			mApp.setCurrentActivity(null);
	}
}
