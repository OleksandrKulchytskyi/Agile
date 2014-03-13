package com.udelphi.agile;

import java.util.List;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.*;

import com.udelphi.agile.common.User;

public class UserAdapter extends ArrayAdapter<User> {

	// Your sent context
	//private Context context;
	// Your custom values for the spinner (User)
	private List<User> values;
	private LayoutInflater inflater;

	public UserAdapter(Context context, int resource, List<User> objects) {
		super(context, resource, objects);
		inflater = LayoutInflater.from(context);
		values = objects;
		//this.context = context;
	}

	public int getCount() {
		return values.size();
	}

	public User getItem(int position) {
		return values.get(position);
	}

	public long getItemId(int position) {
		return position;
	}

	// And the "magic" goes here
	// This is for the "passive" state of the spinner
	@Override
	public View getView(int position, View view, ViewGroup viewGroup) {

		View v = view;
		//ImageView picture;
		TextView name;

		if (v == null) {
			v = inflater.inflate(R.layout.gridview_item, viewGroup, false);
			// v.setTag(R.id.picture, v.findViewById(R.id.picture));
			v.setTag(R.id.text, v.findViewById(R.id.text));
		}
		// picture = (ImageView)v.getTag(R.id.picture);
		name = (TextView) v.getTag(R.id.text);

		User item = getItem(position);
		name.setText(item.Name);
		return v;
	}

	// // Normally is the same view, but you can customize it if you want
	// @Override
	// public View getDropDownView(int position, View convertView, ViewGroup
	// parent) {
	// TextView label = new TextView(context);
	// label.setTextColor(Color.BLACK);
	// label.setText(values.get(position).Name);
	//
	// return label;
	// }

}