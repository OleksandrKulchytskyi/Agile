package com.udelphi.agile;

import java.util.*;

import org.json.*;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.os.Parcelable;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.*;
import android.widget.AdapterView.OnItemClickListener;

import com.udelphi.agile.common.*;
import com.zsoft.signala.hubs.HubInvokeCallback;

public class RoomActivity extends BaseActivity implements IOnRoomStateListener,
		IOnHubErrorHandler {

	private View mWaitView;
	private View mRoomForm;
	private UserAdapter _usrAdapter;
	private List<User> _usrList;

	ListView listView;
	private VoteItemsArrayAdapter _votesAdapter;
	private List<VoteItem> _voteList;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_room);
		mWaitView = (View) findViewById(R.id.roomwaitstatusview);
		mRoomForm = (View) findViewById(R.id.RoomActivity_form);

		_usrList = new ArrayList<User>();
		GridView gridView = (GridView) findViewById(R.id.room_usergridview);
		_usrAdapter = new UserAdapter(getBaseContext(), R.layout.gridview_item,
				_usrList);
		gridView.setAdapter(_usrAdapter);

		_voteList = new ArrayList<VoteItem>();
		_votesAdapter = new VoteItemsArrayAdapter(getBaseContext(), _voteList);
		listView = (ListView) findViewById(R.id.voteItemsList);
		listView.setAdapter(_votesAdapter);
		listView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
		listView.setOnItemClickListener(new OnItemClickListener() {
			@Override
			public void onItemClick(AdapterView<?> arg0, View arg1, int arg2, long arg3) {
				VoteItem vi = (VoteItem) arg0.getAdapter().getItem(arg2) ;
				if (vi.Opened){
					Intent voteIntent = new Intent(RoomActivity.this, VoteItemActivity.class);
					voteIntent.putParcelableArrayListExtra("Users", (ArrayList<? extends Parcelable>)_usrList);
					voteIntent.putExtra("Room", ((Room)AgileApplication.container.get("SelectedRoom")).Name);
					voteIntent.putExtra("VoteId", vi.Id);
					voteIntent.putExtra("Question", vi.Content);
					startActivity(voteIntent);
				}
			}
		});
	}

	@Override
	protected void onStart() {
		super.onStart();

		HubInvokeCallback joinCallback = new HubInvokeCallback() {
			@Override
			public void OnResult(boolean succeeded, String response) {
				try {
					JSONObject js = new JSONObject(response);
					JoinRoomResponse resp = new JoinRoomResponse();
					resp.Active = js.optBoolean("Active", false);
					resp.Active = js.optBoolean("HostMaster", false);
					if (!resp.Active)
						showProgress(true);
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
		hubService.getHubProxy().Invoke("joinRoom", args, joinCallback);

		if ((Boolean) AgileApplication.container
				.get(MainActivity.LoggedAsMasterKey)) {
			HubInvokeCallback changeStateCallback = new HubInvokeCallback() {
				@Override
				public void OnResult(boolean succeeded, String response) {
					Log.d("ChangeRoomState success", String.valueOf(succeeded));
					Log.d("ChangeRoomState response", response);
					if (succeeded)
						Toast.makeText(RoomActivity.this,
								"ChnageRoomState: " + response,
								Toast.LENGTH_LONG).show();
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
					changeRoomStateArgs, changeStateCallback);
		}

		// new GerRoomStateTask().execute((String) AgileApplication.container
		// .get("ServerUrl")
		// + "/api/room/isRoomActive?roomId="
		// + String.valueOf(room.Id));
	}

	@Override
	protected void onResume() {
		super.onResume();
		super.hubService.SubscribeOnRoomStateChanged(this);
		super.hubService.SubscribeOnHubErrorCallback(this);
	}

	@Override
	protected void onPause() {
		super.onPause();
		super.hubService.UnsubscribeOnRoomStateChanged(this);
		super.hubService.UnsubscribeOnHubErrorCallback(this);
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

		if (roomState.ConnectedUsers != null) {
			for (User usr : roomState.ConnectedUsers) {
				if (_usrList.indexOf(usr) == -1)
					_usrList.add(usr);
			}
			for (User usr : _usrList) {
				if (roomState.ConnectedUsers.indexOf(usr) == -1)
					_usrList.remove(usr);
			}
			_usrAdapter.notifyDataSetChanged();
		}

		if (roomState.ItemsToVote != null) {
			for (VoteItem vote : roomState.ItemsToVote) {
				if (_voteList.indexOf(vote) == -1)
					_voteList.add(vote);
			}
			for (VoteItem vote : _voteList) {
				if (roomState.ItemsToVote.indexOf(vote) == -1)
					_usrList.remove(vote);
			}
			_usrAdapter.notifyDataSetChanged();

			for (VoteItem v : _voteList) {
				int indx = roomState.ItemsToVote.indexOf(v);
				if (indx != -1) {
					VoteItem server = roomState.ItemsToVote.get(indx);
					v.copyPartialState(server);
				}
			}
		}

		if (!roomState.Active)
			showProgress(true);
		else
			showProgress(false);
	}

	@Override
	public void onInitRoom(Room roomState) {
		if (roomState == null)
			return;

		if (roomState.ConnectedUsers != null) {
			for (User user : roomState.ConnectedUsers) {
				_usrList.add(user);
				_usrAdapter.notifyDataSetChanged();
			}
		}

		if (roomState.ItemsToVote != null) {
			for (VoteItem vote : roomState.ItemsToVote) {
				_voteList.add(vote);
				_votesAdapter.notifyDataSetChanged();
			}
		}
	}

	@Override
	public void onVoteItemOpened(VoteItem state) {
		if (state == null)
			return;

		int indx = _voteList.indexOf(state);
		VoteItem listItem = _voteList.get(indx);
		if (listItem != null) {
			listItem.Closed = state.Closed;
			listItem.Opened = state.Opened;
		}
		_votesAdapter.notifyDataSetChanged();
	}

	@Override
	public void onVoteItemClosed(VoteItem state) {
		if (state == null)
			return;

		int indx = _voteList.indexOf(state);
		VoteItem listItem = _voteList.get(indx);
		if (listItem != null) {
			listItem.Closed = state.Closed;
			listItem.Opened = state.Opened;
			listItem.OveralMark = state.OveralMark;
		}
		_votesAdapter.notifyDataSetChanged();
	}

	@Override
	public void onJoinedRoom(User user) {
		if (user == null)
			return;
		if (_usrList.indexOf(user) == -1) {
			_usrList.add(user);
			_usrAdapter.notifyDataSetChanged();
		}
	}

	@Override
	public void onLeftRoom(User user) {
		if (user == null)
			return;

		if (_usrList.indexOf(user) != -1) {
			_usrList.remove(user);
			_usrAdapter.notifyDataSetChanged();
		}
	}

	@SuppressWarnings("unused")
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

	@Override
	public void onHubErrorHandler(String data) {
		if (data != null)
			Toast.makeText(getBaseContext(), data, Toast.LENGTH_LONG).show();
	}

}
