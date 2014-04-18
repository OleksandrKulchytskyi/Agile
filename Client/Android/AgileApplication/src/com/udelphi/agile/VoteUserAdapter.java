package com.udelphi.agile;

import java.util.ArrayList;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class VoteUserAdapter extends ArrayAdapter<VoteUserItem> {
	
	private ArrayList<VoteUserItem> values;
	private LayoutInflater inflater;

	public VoteUserAdapter(Context context, int resource, ArrayList<VoteUserItem> objects) {
		super(context, resource, objects);
		inflater = LayoutInflater.from(context);
		values = objects;
	}
	
	public int getCount() {
		return values.size();
	}

	public VoteUserItem getItem(int position) {
		return values.get(position);
	}

	public long getItemId(int position) {
		return position;
	}
	
	@Override
	public View getView(int position, View view, ViewGroup viewGroup) {
		View v = view;
		TextView userView;
		TextView voteView;
		
		if (v == null) {
			v = inflater.inflate(R.layout.voteuser_item, viewGroup, false);
			v.setTag(R.id.vote_user, v.findViewById(R.id.vote_user));
			v.setTag(R.id.vote_userVote, v.findViewById(R.id.vote_userVote));
		}
		
		userView = (TextView)v.getTag(R.id.vote_user);
		voteView = (TextView)v.getTag(R.id.vote_userVote);
		
		VoteUserItem user = values.get(position);
		if (user == null)
			Log.d("VoteUserAdapter", "User is null");
		
		if (user.getVote() == 0) {
			v.setBackgroundResource(R.drawable.item_opened_bg);
		} else {
			v.setBackgroundResource(R.drawable.item_closed_bg);
		}
		
		userView.setText(user.getUser());
		if (user.isFinished())
			voteView.setText(user.getVote());
		
		return v;
	}

}
