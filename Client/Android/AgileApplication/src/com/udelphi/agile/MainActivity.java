package com.udelphi.agile;

import java.util.ArrayList;
import java.util.List;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.content.Intent;
import android.content.OperationApplicationException;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.CheckBox;
import android.widget.Spinner;
import android.widget.Toast;

import com.udelphi.agile.common.*;
import com.zsoft.signala.hubs.HubInvokeCallback;

public class MainActivity extends BaseActivity implements
		OnConnectionRequestedListener, IOnUserStateLoggedListener {

	private RoomAdapter roomAdapter;
	private Spinner roomSpinner;
	private CheckBox chckMaster;
	private List<Room> rooms;
	private View mWaitView;
	private View mChooseRoomForm;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		mWaitView = (View) findViewById(R.id.wait_status_view);
		mChooseRoomForm = (View) findViewById(R.id.chooseRoom_form);

		chckMaster = (CheckBox) findViewById(R.id.chckLogAsMaster);

		rooms = new ArrayList<Room>();
		roomAdapter = new RoomAdapter(getBaseContext(), R.layout.room_item,
				rooms);
		roomSpinner = (Spinner) findViewById(R.id.roomSpinner);
		roomSpinner.setAdapter(roomAdapter);
		roomSpinner.setOnItemSelectedListener(new OnItemSelectedListener() {
			@Override
			public void onItemSelected(AdapterView<?> adapterView, View view,
					int position, long id) {
				Room room = roomAdapter.getItem(position);
				if (room != null) {
					AgileApplication.container.put("SelectedRoom", room);
				}
			}

			@Override
			public void onNothingSelected(AdapterView<?> adapter) {
			}
		});

		hubService.SetAddress((String) AgileApplication.container
				.get("ServerUrl"));
		ConnectionRequested(Uri.parse((String) AgileApplication.container
				.get("ServerUrl")));
	}

	@Override
	protected void onStart() {
		super.onStart();
		new RetreiveRoomTask().execute((String) AgileApplication.container
				.get("ServerUrl") + "/api/room/getrooms/");

		if (!hubService.isStarted())
			hubService.Start();
	}

	@Override
	public void ConnectionRequested(Uri address) {

		try {
			String aspxauth = (String) AgileApplication.container
					.get(".ASPXAUTH");
			hubService.CreateProxy("agileHub", aspxauth);
		} catch (OperationApplicationException e) {
			Log.e("OperationApplicationException", e.getMessage());
			e.printStackTrace();
		}
	}

	@Override
	protected void onResume() {
		super.onResume();
		hubService.SubcribeOnUserState(this);
	}

	@Override
	protected void onStop() {
		super.onStop();
		hubService.UnsubcribeOnUserState(this);
	}

	public class RetreiveRoomTask extends AsyncTask<String, Void, JSONArray> {

		private Exception exception;

		protected JSONArray doInBackground(String... urls) {
			JSONArray data = null;

			try {
				String url = urls[0];
				data = JsonHelper.GetFromRequest(url);
			} catch (Exception e) {
				this.exception = e;
				exception.printStackTrace();
			}

			return data;
		}

		protected void onPostExecute(JSONArray data) {
			if (exception == null && data != null) {
				int len = data.length();
				for (int i = 0; i < len; i++) {
					try {
						JSONObject jsObj = (JSONObject) data.get(i);
						Room room = new Room();
						room.Id = jsObj.getInt("Id");
						room.Name = jsObj.getString("Name");
						rooms.add(room);
					} catch (JSONException e) {
						e.printStackTrace();
					}
				}
				roomAdapter.notifyDataSetChanged();
			}
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	public void onStartSession(View v) {
		HubInvokeCallback callback = new HubInvokeCallback() {
			@Override
			public void OnResult(boolean succeeded, String response) {
				try {
					JoinRoomResponse resp = new JoinRoomResponse();
					JSONObject js = new JSONObject(response);
					resp.Active = js.optBoolean("Active", false);
					resp.Active = js.optBoolean("HostMaster", false);
					if (!chckMaster.isChecked()
							&& chckMaster.getVisibility() != View.VISIBLE) {
						Intent navIntent = new Intent(getBaseContext(),
								RoomActivity.class);
						startActivity(navIntent);
					}
				} catch (JSONException e) {
					e.printStackTrace();
				}
			}

			@Override
			public void OnError(Exception ex) {
				Log.d("joinRoom", ex.getMessage());
				ex.printStackTrace();
			}
		};

		Room room = (Room) AgileApplication.container.get("SelectedRoom");
		SessionState state = (SessionState) AgileApplication.container
				.get(SessionState.class.getName());

		if (room == null || state == null) {
			Log.d("onStartSession",
					"Is room null:" + String.valueOf(room == null)
							+ ",is state null:" + String.valueOf(state == null));
			Toast.makeText(getBaseContext(), "Please select room.",
					Toast.LENGTH_SHORT).show();
			return;
		}

		List<String> args = new ArrayList<String>();
		args.add(room.Name);
		args.add(state.SessionId);
		hubService.getHubProxy().Invoke("joinRoom", args, callback);

		if (chckMaster.getVisibility() == View.VISIBLE
				&& chckMaster.isChecked()) {
			HubInvokeCallback changeRoomStateCallback = new HubInvokeCallback() {
				@Override
				public void OnResult(boolean succeeded, String response) {
					Log.d("ChangeRoomState success", String.valueOf(succeeded));
					Log.d("ChangeRoomState response", response);
					if (succeeded)
						Toast.makeText(MainActivity.this,
								"JoinRoom: " + response, Toast.LENGTH_LONG)
								.show();
				}

				@Override
				public void OnError(Exception ex) {
					Log.d("ChangeRoomState", ex.getMessage());
					ex.printStackTrace();
				}
			};

			List<String> changeRoomStateArgs = new ArrayList<String>();
			changeRoomStateArgs.add(room.Name);
			changeRoomStateArgs.add(String.valueOf(true));
			hubService.getHubProxy().Invoke("changeRoomState",
					changeRoomStateArgs, changeRoomStateCallback);
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

			mChooseRoomForm.setVisibility(View.VISIBLE);
			mChooseRoomForm.animate().setDuration(shortAnimTime)
					.alpha(show ? 0 : 1)
					.setListener(new AnimatorListenerAdapter() {
						@Override
						public void onAnimationEnd(Animator animation) {
							mChooseRoomForm.setVisibility(show ? View.GONE
									: View.VISIBLE);
						}
					});
		} else {
			mWaitView.setVisibility(show ? View.VISIBLE : View.GONE);
			mChooseRoomForm.setVisibility(show ? View.GONE : View.VISIBLE);
		}
	}

	@Override
	public void onStateCallback(SessionState state) {
		Log.d("onStateCallback",
				"Is state null:" + String.valueOf(state == null));
		AgileApplication.container.put(SessionState.class.getName(), state);
	}

	@Override
	public void onUserLoggedCallback(User loggedUser) {
		Log.d("onUserLoggedCallback",
				"Is user null:" + String.valueOf(loggedUser == null));
		if (loggedUser != null) {
			AgileApplication.container.put("LoggedUser", loggedUser);
			for (Privilege p : loggedUser.Privileges) {
				if (p.Name.equalsIgnoreCase("ScrumMaster")) {
					chckMaster.setVisibility(View.VISIBLE);
					break;
				}
			}
		}
	}

}
