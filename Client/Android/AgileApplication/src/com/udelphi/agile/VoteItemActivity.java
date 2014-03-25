package com.udelphi.agile;

import com.udelphi.agile.common.IOnVoteItemStateListener;
import com.udelphi.agile.common.UserVote;
import com.udelphi.agile.common.VoteItem;

import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.support.v4.app.NavUtils;
import android.annotation.TargetApi;
import android.os.Build;

public class VoteItemActivity extends BaseActivity implements IOnVoteItemStateListener {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_vote_item);
		// Show the Up button in the action bar.
		setupActionBar();
	}

	/**
	 * Set up the {@link android.app.ActionBar}, if the API is available.
	 */
	@TargetApi(Build.VERSION_CODES.HONEYCOMB)
	private void setupActionBar() {
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
			getActionBar().setDisplayHomeAsUpEnabled(true);
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.vote_item, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case android.R.id.home:
			// This ID represents the Home or Up button. In the case of this
			// activity, the Up button is shown. Use NavUtils to allow users
			// to navigate up one level in the application structure. For
			// more details, see the Navigation pattern on Android Design:
			//
			// http://developer.android.com/design/patterns/navigation.html#up-vs-back
			//
			NavUtils.navigateUpFromSameTask(this);
			return true;
		}
		return super.onOptionsItemSelected(item);
	}

	@Override
	public void onUserVoted(UserVote userVote) {
		// TODO Auto-generated method stub
		Log.i("VoteItemActivity", "User voted");
	}

	@Override
	public void onVoteItemClosed(VoteItem voteItem) {
		// TODO Auto-generated method stub
		Log.i("VoteItemActivity", "Vote item was closed");
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

}
