package com.udelphi.agile;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import com.udelphi.agile.common.*;
import android.app.Activity;
import android.content.OperationApplicationException;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.Toast;

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

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		ConnectionRequested(Uri.parse("http://10.0.2.2:6404/"));
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

		hubProxy.On("hello", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("On hello callback", args.toString());
				for (Map.Entry<String, String> e : hubCon.getHeaders()
						.entrySet()) {
					Log.d("Header entry:", e.toString());
				}

				if (!mShowAll)
					return;
				for (int i = 0; i < args.length(); i++) {
					Toast.makeText(MainActivity.this, args.opt(i).toString(),
							Toast.LENGTH_SHORT).show();
				}
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
						String json = (String)args.get(i);
						JSONObject jsObj=new JSONObject(json);
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

	public void onPush(View v) {
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
