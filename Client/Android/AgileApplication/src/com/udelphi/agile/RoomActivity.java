package com.udelphi.agile;

import org.json.*;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import com.udelphi.agile.common.*;

public class RoomActivity extends BaseActivity implements IOnRoomStateListener {

	private View mWaitView;
	private View mRoomForm;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_room);
		mWaitView = (View) findViewById(R.id.roomwaitstatusview);
		mRoomForm = (View) findViewById(R.id.RoomActivity_form);
	}

	@Override
	protected void onStart() {
		super.onStart();

		Room r = (Room) AgileApplication.container.get("SelectedRoom");
		new GerRoomStateTask().execute((String) AgileApplication.container
				.get("ServerUrl")
				+ "/api/room/isRoomActive?roomId="
				+ String.valueOf(r.Id));
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
		if (roomState == null)
			return;

		if (!roomState.Active)
			showProgress(true);
		else
			showProgress(false);
	}

	private class GerRoomStateTask extends AsyncTask<String, Void, String> {

		private Exception exception;

		protected String doInBackground(String... urls) {
			String data = null;
			Log.d("GerRoomStateTask", "begin invoke doInBackground");
			try {
				String url = urls[0];
				data = JsonHelper.GetStringFromRequest(url);
			} catch (Exception e) {
				this.exception = e;
				exception.printStackTrace();
			}

			return data;
		}

		protected void onPostExecute(String data) {
			if (exception == null && data != null) {
				Log.d("OnPostExecute", data.toString());
				try {
					Boolean isActive = Boolean.valueOf(data);
					if (isActive == false) {
						showProgress(true);
					}
				} catch (Exception e) {
					Log.e("OnPostExecute", e.getMessage());
					e.printStackTrace();
				}
			}
		}
	}

	private void showProgress(final boolean show) {

		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR2) {
			int shortAnimTime = getResources().getInteger(
					android.R.integer.config_shortAnimTime);

			mWaitView.setVisibility(View.VISIBLE);
			mWaitView.animate().setDuration(shortAnimTime).alpha(show ? 1 : 0)
					.setListener(new AnimatorListenerAdapter() {
						@Override
						public void onAnimationEnd(Animator animation) {
							mWaitView.setVisibility(show ? View.VISIBLE
									: View.GONE);
						}
					});

			mRoomForm.setVisibility(View.VISIBLE);
			mRoomForm.animate().setDuration(shortAnimTime).alpha(show ? 0 : 1)
					.setListener(new AnimatorListenerAdapter() {
						@Override
						public void onAnimationEnd(Animator animation) {
							mRoomForm.setVisibility(show ? View.GONE
									: View.VISIBLE);
						}
					});
		} else {
			mWaitView.setVisibility(show ? View.VISIBLE : View.GONE);
			mRoomForm.setVisibility(show ? View.GONE : View.VISIBLE);
		}
	}
}
