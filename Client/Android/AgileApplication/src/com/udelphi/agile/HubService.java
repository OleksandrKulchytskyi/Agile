package com.udelphi.agile;

import java.util.*;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.content.OperationApplicationException;
import android.util.Log;

import com.udelphi.agile.common.*;

import com.zsoft.signala.hubs.HubConnection;
import com.zsoft.signala.hubs.HubOnDataCallback;
import com.zsoft.signala.hubs.IHubProxy;
import com.zsoft.signala.transport.StateBase;
import com.zsoft.signala.transport.longpolling.LongPollingTransport;

public class HubService {

	private String serverURL = "";
	private Context applicationCtx;
	private HubConnection hubCon = null;
	private IHubProxy hubProxy = null;
	private StateBase currentState;
	private static HubService _instance;
	private List<IOnErrorListener> _onError;
	private List<IOnStateChangedListener> _onConnectionState;
	private List<IOnUserStateLoggedListener> _onUserState;
	private List<IOnRoomStateListener> _onRoomState;
	private boolean isStarted;

	public static HubService getInstance() {
		if (_instance == null) {
			_instance = new HubService();
		}
		return _instance;
	}

	private HubService() {
		_onError = new ArrayList<IOnErrorListener>();
		_onConnectionState = new ArrayList<IOnStateChangedListener>();
		_onUserState = new ArrayList<IOnUserStateLoggedListener>();
		_onRoomState = new ArrayList<IOnRoomStateListener>();

		applicationCtx = (Context) AgileApplication.container.get("Context");
		Log.d("HubService",
				"Is context null:" + String.valueOf(applicationCtx == null));
	}

	public void SetAddress(String url) {
		if (url != null)
			serverURL = url;
		else
			serverURL = (String) AgileApplication.container.get("ServerUrl");

		// if (!serverURL.endsWith("/"))
		// serverURL = serverURL + "/";
		Log.d("HUBService setup address", serverURL);
	}

	public StateBase getCurrentState() {
		return currentState;
	}

	private void InitializeHubConnection() {
		Log.d("HUBService", "Initializing hub connection");
		hubCon = new HubConnection(serverURL, applicationCtx,
				new LongPollingTransport()) {
			@Override
			public void OnStateChanged(StateBase oldState, StateBase newState) {
				Log.d("HubService State", oldState.getState() + " -> "
						+ newState.getState());
				currentState = newState;
				for (IOnStateChangedListener element : _onConnectionState) {
					element.onStateChanged(newState, oldState);
				}

				switch (newState.getState()) {
				case Connected:
					break;
				case Disconnected:
					break;
				default:
					break;
				}
				super.OnStateChanged(oldState, newState);
			}

			@Override
			public void OnError(Exception exception) {
				Log.e("HubService OnError", exception.getMessage());
				for (IOnErrorListener element : _onError) {
					element.onError(exception);
				}
			}

			@Override
			public void SetNewState(StateBase state) {
				super.SetNewState(state);
			}
		};
	}

	public void CreateProxy(String hubName, String aspxAuth)
			throws OperationApplicationException {

		try {
			setIsStarted(false);
			InitializeHubConnection();
			Log.d("HUBService", "Creating proxy");
			hubCon.addHeader("Cookie", ".ASPXAUTH=" + aspxAuth);
			hubProxy = hubCon.CreateHubProxy(hubName);
			InitializeCallbackListening();
		} catch (OperationApplicationException ex) {
			Log.e("HubProxy OperationApplicationException", ex.getMessage());
			throw ex;
		}
	}

	private void InitializeCallbackListening() {
		Log.d("HUBService", "Initialize Callbacks Listening");

		hubProxy.On("onState", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("OnState callback", args.toString());

				SessionState state = null;
				try {
					JSONObject jsObj = (JSONObject) args.get(0);
					state = new SessionState();
					state.UserId = jsObj.getInt("UserId");
					state.SessionId = jsObj.getString("SessionId");

				} catch (Exception ex) {
					Log.e("onState", ex.getMessage());
					ex.printStackTrace();
				}
				if (state != null)
					for (IOnUserStateLoggedListener element : _onUserState) {
						element.onStateCallback(state);
					}
			}
		});

		hubProxy.On("onUserLogged", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				User loggedUser = null;
				try {
					loggedUser = parseLoggedUser((JSONObject) args.get(0));
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				if (loggedUser != null) {
					for (IOnUserStateLoggedListener element : _onUserState) {
						element.onUserLoggedCallback(loggedUser);
					}
				}
			}
		});

		hubProxy.On("onJoinedRoom", new HubOnDataCallback() {

			@Override
			public void OnReceived(JSONArray args) {
				Log.d("onJoinedRoom", args.toString());
				User user = null;
				try {
					JSONObject jsObj = (JSONObject) args.get(0);
					user = parseLoggedUser(jsObj);
				} catch (JSONException e) {
					e.printStackTrace();
				}
				if (user == null)
					return;
				for (IOnRoomStateListener listener : _onRoomState) {
					listener.onJoinedRoom(user);
				}
			}
		});

		hubProxy.On("onLeftRoom", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("onLeftRoom", args.toString());
				User user = null;
				try {
					JSONObject jsObj = (JSONObject) args.get(0);
					user = parseLoggedUser(jsObj);
				} catch (JSONException e) {
					e.printStackTrace();
				}
				if (user != null)
					for (IOnRoomStateListener listener : _onRoomState) {
						listener.onLeftRoom(user);
					}
			}
		});
		
		hubProxy.On("onRoomStateChanged", new HubOnDataCallback() {
			@Override
			public void OnReceived(JSONArray args) {
				Log.d("onRoomStateChanged", args.toString());
				Room room = null;
				try {
					JSONObject jsObj = (JSONObject) args.get(0);
					room = parseRoomState(jsObj);
				} catch (JSONException e) {
					e.printStackTrace();
				}
				if (room != null)
					for (IOnRoomStateListener listener : _onRoomState) {
						listener.onRoomStateChanged(room);
					}
			}
		});
		
	}

	private Room parseRoomState(JSONObject jsObj) throws JSONException {
		Room room = new Room();
		room.Id = jsObj.optInt("Id", -1);
		room.Active = jsObj.getBoolean("Active");
		room.Name = jsObj.getString("Name");
		room.Description = jsObj.optString("Description", "");
		JSONArray userArray = jsObj.getJSONArray("ConnectedUsers");
		for (int i = 0; i < userArray.length(); i++) {
			User usr = parseLoggedUser((JSONObject) userArray.get(i));
			if (usr != null)
				room.AddUser(usr);
		}
		return room;
	}

	private User parseLoggedUser(JSONObject args) {
		User usr = null;
		try {
			JSONObject jsObj = ((JSONObject) args);
			usr = new User();
			usr.Privileges = new ArrayList<Privilege>();
			usr.Id = jsObj.getInt("Id");
			usr.Name = jsObj.getString("Name");
			// usr.Password = jsObj.getString("Password");
			// usr.IsAdmin = jsObj.getBoolean("IsAdmin");
			JSONArray priviligies = jsObj.getJSONArray("Privileges");
			int len = priviligies.length();
			for (int i = 0; i < len; i++) {
				JSONObject obj2 = (JSONObject) priviligies.get(i);
				Privilege p = new Privilege();
				p.Id = obj2.getInt("Id");
				p.Name = obj2.getString("Name");
				p.Description = obj2.optString("Description", "");
				usr.Privileges.add(p);
			}
		} catch (Exception ex) {
			ex.printStackTrace();
			Log.e("Parsing error", ex.getMessage());
		}
		return usr;
	}

	public IHubProxy getHubProxy() {
		return hubProxy;
	}

	public void Start() {
		if (hubCon != null && !isStarted) {
			Log.d("HubService", "Starting connection");
			try {
				hubCon.Start();
			} finally {
				setIsStarted(true);
			}
		}
	}

	public void Stop() {
		if (hubCon != null && isStarted) {
			Log.d("HubService", "Stopping connection");
			try {
				hubCon.Stop();
			} finally {
				setIsStarted(false);
			}
		}
	}

	private synchronized void setIsStarted(boolean started) {
		isStarted = started;
	}

	public synchronized boolean isStarted() {
		return isStarted;
	}

	public void SubscribeOnError(IOnErrorListener listener) {
		_onError.add(listener);
	}

	public void UnsubscribeOnError(IOnErrorListener listener) {
		_onError.remove(listener);
	}

	public void SubscribeOnState(IOnStateChangedListener listener) {
		_onConnectionState.add(listener);
	}

	public void UnsubscribeOnState(IOnStateChangedListener listener) {
		_onConnectionState.remove(listener);
	}

	public void SubcribeOnUserState(IOnUserStateLoggedListener listener) {
		_onUserState.add(listener);
	}

	public void UnsubcribeOnUserState(IOnUserStateLoggedListener listener) {
		_onUserState.remove(listener);
	}

	public void SubscribeOnRoomStateChanged(IOnRoomStateListener listener) {
		_onRoomState.add(listener);
	}

	public void UnsubscribeOnRoomStateChanged(IOnRoomStateListener listener) {
		_onRoomState.remove(listener);
	}
}
