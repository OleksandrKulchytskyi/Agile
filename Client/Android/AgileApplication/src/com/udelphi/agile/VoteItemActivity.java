package com.udelphi.agile;

import java.util.ArrayList;
import java.util.List;

import com.udelphi.agile.common.IOnVoteItemStateListener;
import com.udelphi.agile.common.User;
import com.udelphi.agile.common.UserVote;
import com.udelphi.agile.common.VoteItem;
import com.zsoft.signala.hubs.HubInvokeCallback;

import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.GridView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.support.v4.app.NavUtils;

public class VoteItemActivity extends BaseActivity implements IOnVoteItemStateListener, OnClickListener, OnItemSelectedListener {
	
	//Integer[] values = {1, 2, 3, 5, 8, 13, 21};
	
	/*private View mWaitView;
	private View mVoteForm;*/
	private Spinner mValueSpin;
	private Button mVoteBtn;
	
	private ArrayList<User> users;
	private VoteUserAdapter _usrAdapter;
	private ArrayList<VoteUserItem> _voteUserList;

	private String roomName;
	private int voteId;
	private int vote;
	private boolean backEnabled;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_vote_item);
		
		/*mWaitView = (View)findViewById(R.id.vote_waitStatusView);
		mVoteForm = (View)findViewById(R.id.vote_activityForm);*/
		mValueSpin = (Spinner)findViewById(R.id.vote_voteSpinner);
		mVoteBtn = (Button)findViewById(R.id.vote_voteButton);
		GridView gridView = (GridView)findViewById(R.id.vote_userGridView);
		
		users = new ArrayList<User>();
		_voteUserList = new ArrayList<VoteUserItem>();
		users = getIntent().getExtras().getParcelableArrayList("Users"); 
		for (User user : users) {
			_voteUserList.add(new VoteUserItem(user.Id, user.Name, 0, false));
		}
		_usrAdapter = new VoteUserAdapter(getBaseContext(), R.layout.voteuser_item, _voteUserList);
		gridView.setAdapter(_usrAdapter);
		
		roomName = getIntent().getExtras().getString("Room");
		voteId = getIntent().getExtras().getInt("VoteId");
		((TextView)findViewById(R.id.vote_questionTxt)).setText(getIntent().getExtras().getString("Question"));
		
		ArrayAdapter<Integer> adapter = new ArrayAdapter<Integer>(this, android.R.layout.simple_spinner_item, getVoteValues(21));
		adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
		mValueSpin.setAdapter(adapter);
		mValueSpin.setOnItemSelectedListener(this);
		
		mVoteBtn.setOnClickListener(VoteItemActivity.this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.vote_item, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case android.R.id.home:
			NavUtils.navigateUpFromSameTask(this);
			return true;
		}
		return super.onOptionsItemSelected(item);
	}

	@Override
	public void onUserVoted(UserVote userVote) {
		if (userVote.VoteItemId == voteId) {
			for (VoteUserItem item : _voteUserList) {
				if (item.getUserId() == userVote.UserId && item.getVote() == 0) {
					int index = _voteUserList.indexOf(item);
					_voteUserList.set(index, new VoteUserItem(item.getUserId(), item.getUser(), vote, false));
					break;
				}
			}
			_usrAdapter.notifyDataSetChanged();
			backEnabled = false;
		}
		Log.i("VoteItemActivity", "User voted");
	}

	@Override
	public void onVoteItemClosed(VoteItem voteItem) {
		backEnabled = true;
		Log.i("VoteItemActivity", "Vote item was closed");
	}
	
	@Override
	public void onVoteFinished(VoteItem voteItem) {
		if (voteItem.Id == voteId) {
			for (VoteUserItem item : _voteUserList) {
				int index = _voteUserList.indexOf(item);
				_voteUserList.set(index, new VoteUserItem(item.getUserId(), item.getUser(), item.getVote(), voteItem.Finished));
			}
			_usrAdapter.notifyDataSetChanged();
		}
		Log.i("VoteItemActivity", "Vote item was finished");
	}

	@Override
	protected void onResume() {
		super.onResume();
		super.hubService.SubscribeOnVoteItemCallback(this);
	}

	@Override
	protected void onPause() {
		super.onPause();
		super.hubService.SubscribeOnVoteItemCallback(this);
	}

	@Override
	public void onClick(View v) {
		if (v.getId() == R.id.vote_voteButton) {
			HubInvokeCallback voteCallback = new HubInvokeCallback() {
				@Override
				public void OnResult(boolean succeeded, String response) { }
				
				@Override
				public void OnError(Exception ex) { }
			};
			
			List<String> args = new ArrayList<String>();
			args.add(roomName); // room
			args.add(String.valueOf(voteId)); // vote item id
			args.add(String.valueOf(vote)); // mark
			hubService.getHubProxy().Invoke("VoteForItem", args, voteCallback);
			
			mVoteBtn.setEnabled(false);
			mValueSpin.setEnabled(false);
		}
	}
	
	private ArrayList<Integer> getVoteValues(int max) {
		ArrayList<Integer> val = new ArrayList<Integer>();
		val.add(1);
		val.add(2);
		do {
			val.add(val.get(val.size() - 1) + val.get(val.size() - 2));
		} while (max - val.get(val.size() - 1) >= val.get(val.size() - 2));
		return val;
	}

	@Override
	public void onItemSelected(AdapterView<?> adView, View view, int position, long id) {
		vote = (Integer)adView.getItemAtPosition(position);
	}

	@Override
	public void onNothingSelected(AdapterView<?> arg0) { }

	@Override
	public void onBackPressed() {
		if (backEnabled)
			super.onBackPressed();
	}

}
