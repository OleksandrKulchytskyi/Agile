package com.udelphi.agile;

import java.util.List;

import android.content.Context;
import android.graphics.Color;
import android.util.Log;
import android.view.*;
import android.widget.*;

import com.udelphi.agile.common.VoteItem;

public class VoteItemsArrayAdapter extends ArrayAdapter<VoteItem> {

	private final Context context;
	private final List<VoteItem> values;
	private LayoutInflater inflater;

	public VoteItemsArrayAdapter(Context context, List<VoteItem> values) {
		super(context, R.layout.voteitemview, values);
		this.context = context;
		this.values = values;
		inflater = LayoutInflater.from(context);
	}

	public int getCount() {
		return values.size();
	}

	public VoteItem getItem(int position) {
		return values.get(position);
	}

	public long getItemId(int position) {
		return position;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup viewGroup) {
		View view = null;
		TextView contentView;
		TextView markView;

		if (view == null) {
			view = inflater.inflate(R.layout.voteitemview, viewGroup, false);
			view.setTag(R.id.voteContent, view.findViewById(R.id.voteContent));
			view.setTag(R.id.voteMark, view.findViewById(R.id.voteMark));
		}
		contentView = (TextView) view.getTag(R.id.voteContent);
		markView = (TextView) view.getTag(R.id.voteMark);

		VoteItem vote = values.get(position);
		if (vote == null)
			Log.d("VoteItems adapter", "Vote is null");

		if (vote.Opened && !vote.Closed)
			view.setBackgroundResource(R.drawable.item_opened_bg);
		
		if (vote.Closed && !vote.Opened)
			view.setBackgroundResource(R.drawable.item_closed_bg);
			
		contentView.setText(vote.Content);
		markView.setText(String.valueOf(vote.OveralMark));

		return view;
	}
}
