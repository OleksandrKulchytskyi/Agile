package com.udelphi.agile;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.app.Activity;
import android.content.Intent;
import android.content.OperationApplicationException;
import android.graphics.Paint.Join;
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
import android.widget.TextView;
import android.widget.Toast;

import com.udelphi.agile.common.*;
import com.zsoft.signala.hubs.HubConnection;
import com.zsoft.signala.hubs.HubInvokeCallback;
import com.zsoft.signala.hubs.HubOnDataCallback;
import com.zsoft.signala.hubs.IHubProxy;
import com.zsoft.signala.transport.StateBase;
import com.zsoft.signala.transport.longpolling.LongPollingTransport;

public class MainActivity extends Activity implements
		OnConnectionRequestedListener, OnDisconnectionRequestedListener {

	protected HubConnection hubCon = null;
	protected IHubProxy hubProxy = null;

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

		ConnectionRequested(Uri.parse("http://10.0.2.2:6404/"));
	}

	@Override
	protected void onStart() {
		super.onStart();
		new RetreiveRoomTask().execute((String) AgileApplication.container
				.get("ServerUrl") + "/api/room/getrooms/");

		// initialize hub proxy for listening
		if (hubCon != null)
			hubCon.Start();
	}

	@Override
	protected void onStop() {
		super.onStop();
		DisconnectionRequested();
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

	@Override
	public void ConnectionRequested(Uri address) {

		LongPollingTransport transport = new LongPollingTransport();
		hubCon = new HubConnection(address.toString(), this, transport) {

			@Override
			public void OnStateChanged(StateBase oldState, StateBase newState) {
				Log.d("State",
						oldState.getState() + " -> " + newState.getState());

				switch (newState.getState()) {
				case Connected:
					Toast.makeText(MainActivity.this, "Connected",
							Toast.LENGTH_SHORT).show();
					break;
				case Disconnected:
					Toast.makeText(MainActivity.this, "Dissconnected",
							Toast.LENGTH_SHORT).show();
					break;
				default:
					break;
				}
				super.OnStateChanged(oldState, newState);
			}

			@Override
			public void OnError(Exception exception) {
				Log.e("OnError", exception.getMessage());
				Toast.makeText(MainActivity.this,
						"On error: " + exception.getMessage(),
						Toast.LENGTH_LONG).show();

				super.OnError(exception);
			}

			@Override
			public void SetNewState(StateBase state) {
				super.SetNewState(state);
			}
		};

		try {
			String aspxauth = (String) AgileApplication.container
					.get(".ASPXAUTH");
			hubCon.addHeader("Cookie", ".ASPXAUTH=" + aspxauth);
			hubProxy = hubCon.CreateHubProxy("agileHub");
		} catch (OperationApplicationException e) {
			Log.e("OperationApplicationException", e.getMessage());
			e.printStackTrace();
		}

		hubProxy.On("onUserLogged", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				User loggedUser = parseLoggedUser(args);
				if (loggedUser != null)
					AgileApplication.container.put("LoggedUser", loggedUser);
				for (Privilege p : loggedUser.Privileges) {
					if (p.Name.equalsIgnoreCase("ScrumMaster")) {
						chckMaster.setVisibility(View.VISIBLE);
						break;
					}
				}
			}

			private User parseLoggedUser(JSONArray args) {
				User usr = null;
				try {
					if (args.length() == 1) {
						JSONObject jsObj = ((JSONObject) args.get(0));
						usr = new User();
						usr.Privileges = new ArrayList<Privilege>();
						usr.Id = jsObj.getInt("Id");
						usr.Name = jsObj.getString("Name");
						// usr.Password = jsObj.getString("Password");
						// usr.IsAdmin = jsObj.getBoolean("IsAdmin");
						JSONArray priviligies = jsObj
								.getJSONArray("Privileges");
						int len = priviligies.length();
						for (int i = 0; i < len; i++) {
							JSONObject obj2 = (JSONObject) priviligies.get(i);
							Privilege p = new Privilege();
							p.Id = obj2.getInt("Id");
							p.Name = obj2.getString("Name");
							p.Description = obj2.optString("Description", "");
							usr.Privileges.add(p);
						}
					}
				} catch (Exception ex) {
					ex.printStackTrace();
					Log.e("Parsing error", ex.getMessage());
				}

				return usr;
			}
		});

		hubProxy.On("onState", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("OnState callback", args.toString());

				try {
					JSONObject jsObj = new JSONObject((String) args.get(0));
					SessionState state = new SessionState();
					state.UserId = jsObj.getInt("UserId");
					state.SessionId = jsObj.getString("SessionId");

					AgileApplication.container.put(
							SessionState.class.getName(), state);

				} catch (Exception ex) {
					ex.printStackTrace();
				}
			}
		});

		hubProxy.On("onRoomStateChanged", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("onRoomStateChanged callback", args.toString());
				showProgress(false);
				Intent nav=new Intent(getBaseContext(), RoomActivity.class);
				startActivity(nav);
			}
		});
	}

	@Override
	public void DisconnectionRequested() {
		// TODO Auto-generated method stub
		if (hubCon != null) {
			hubCon.Stop();
		}
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
					if (!resp.Active) {
						showProgress(true);
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
			Toast.makeText(getBaseContext(), "Please select room.",
					Toast.LENGTH_SHORT).show();
			return;
		}

		List<String> args = new ArrayList<String>();
		args.add(room.Name);
		args.add(state.SessionId);
		hubProxy.Invoke("joinRoom", args, callback);

		if (chckMaster.getVisibility() == View.VISIBLE
				&& chckMaster.isChecked()) {
			HubInvokeCallback callback2 = new HubInvokeCallback() {
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

			List<String> args2 = new ArrayList<String>();
			args.add(room.Name);
			args.add(String.valueOf(true));
			hubProxy.Invoke("changeRoomState", args2, callback2);
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

		if (AgileApplication.container.containsKey("IsLogged")) {
			boolean isLogged = ((Boolean) AgileApplication.container
					.get("IsLogged"));
			Log.d("Logged", isLogged == true ? "Logged" : "Not logged");
			if (!show && isLogged) {
				Intent nav = new Intent(getBaseContext(), MainActivity.class);
				startActivity(nav);
			}
		}
	}

}
