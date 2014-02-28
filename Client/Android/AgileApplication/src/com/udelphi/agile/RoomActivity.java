package com.udelphi.agile;

import android.os.Bundle;
import android.view.Menu;
import com.udelphi.agile.common.*;

public class RoomActivity extends BaseActivity implements IOnRoomStateListener {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_room);
	}
	
	@Override
	protected void onResume() {
		super.onResume();
		super.hubService.SubscribeOnRoomStateChanged(this);
	}
	
	@Override
	protected void onPause() {
		super.onPause();
		super.hubService.UnsubscribeOnRoomStateChanged(this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.room, menu);
		return true;
	}

	@Override
	public void onRoomStateChanged(Room roomState) {
		if(roomState==null) return;	
		
	}

}
