package com.udelphi.agile;

import java.util.HashMap;
import java.util.Map;

import android.app.Activity;
import android.app.Application;

public class AgileApplication extends Application {

	public static final Map<String, Object> container = new HashMap<String, Object>();

	public AgileApplication() {
	}

	@Override
	public void onCreate() {
		super.onCreate();
		container.put("Context", this.getBaseContext());
	}

	private Activity mCurrentActivity = null;

	public Activity getCurrentActivity() {
		return mCurrentActivity;
	}

	public void setCurrentActivity(Activity mCurrentActivity) {
		this.mCurrentActivity = mCurrentActivity;
	}
}
