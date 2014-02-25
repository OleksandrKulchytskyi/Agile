package com.udelphi.agile;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Activity;
import android.content.OperationApplicationException;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.Spinner;
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

	private Boolean mShowAll = true;
	protected HubConnection hubCon = null;
	protected IHubProxy hubProxy = null;

	private RoomAdapter _adapter;
	private Spinner roomSpinner;
	private List<Room> rooms;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		rooms = new ArrayList<Room>();
		_adapter = new RoomAdapter(getBaseContext(), R.layout.room_item, rooms);
		roomSpinner = (Spinner) findViewById(R.id.roomSpinner);
		roomSpinner.setAdapter(_adapter);

		roomSpinner.setOnItemSelectedListener(new OnItemSelectedListener() {

			@Override
			public void onItemSelected(AdapterView<?> adapterView, View view,
					int position, long id) {
				Room room = _adapter.getItem(position);
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
				_adapter.notifyDataSetChanged();
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

		hubCon = new HubConnection(address.toString(), this,
				new LongPollingTransport()) {

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
			}

			@Override
			public void OnError(Exception exception) {
				Log.e("OnError", exception.getMessage());
				Toast.makeText(MainActivity.this,
						"On error: " + exception.getMessage(),
						Toast.LENGTH_LONG).show();
			}

			@Override
			public void SetNewState(StateBase state) {
				// TODO Auto-generated method stub
				super.SetNewState(state);
			}
		};

		try {
			String aspxauth = (String) AgileApplication.container
					.get(".ASPXAUTH");
			hubCon.addHeader("Cookie", ".ASPXAUTH=" + aspxauth);
			hubProxy = hubCon.CreateHubProxy("testhub");
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
			}

			private User parseLoggedUser(JSONArray args) {
				User usr = null;
				try {
					if (args.length() == 1) {
						JSONObject jsObj = ((JSONObject) args.get(0));
						usr = new User();
						usr.Privileges=new ArrayList<Privilege>();
						usr.Id = jsObj.getInt("Id");
						usr.Name = jsObj.getString("Name");
						// usr.Password = jsObj.getString("Password");
						// usr.IsAdmin = jsObj.getBoolean("IsAdmin");
						JSONArray priviligies = jsObj
								.getJSONArray("Privileges");
						for (int i = 0; i < priviligies.length(); i++) {
							JSONObject obj2 = (JSONObject) args.get(i);
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

				for (Map.Entry<String, String> e : hubCon.getHeaders()
						.entrySet()) {
					Log.d("Header entry:", e.toString());
				}

				try {
					SessionState state = null;
					for (int i = 0; i < args.length(); i++) {
						String json = (String) args.get(i);
						JSONObject jsObj = new JSONObject(json);
						state = new SessionState();
						state.UserId = jsObj.getInt("UserId");
						state.SessionId = jsObj.getString("SessionId");
					}

					AgileApplication.container.put("SessionStae", state);

				} catch (Exception ex) {
					ex.printStackTrace();
				}
			}
		});

		hubCon.Start();// initialize hub proxy for listening
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
				Toast.makeText(MainActivity.this, response, Toast.LENGTH_SHORT)
						.show();
			}

			@Override
			public void OnError(Exception ex) {
				Toast.makeText(MainActivity.this, "Error: " + ex.getMessage(),
						Toast.LENGTH_SHORT).show();
			}
		};

		List<String> args = new ArrayList<String>(1);
		hubProxy.Invoke("hello", args, callback);
	}
}
