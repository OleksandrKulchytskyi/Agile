package com.udelphi.agile.common;

import java.util.List;

import android.os.Parcel;
import android.os.Parcelable;

public class User implements Parcelable {

	public int Id;
	public String Name;
	public String Password;
	public boolean IsAdmin;

	public List<Privilege> Privileges;

	@Override
	public boolean equals(Object o) {
		if (!(o instanceof User))
			return false;

		return (Id == ((User) o).Id);
	}

	@Override
	public int hashCode() {
		return Id;
	}

	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel dest, int flags) {
		dest.writeInt(Id);
		dest.writeString(Name);
		dest.writeString(Password);
		dest.writeInt(IsAdmin ? 1 : 0);
	}
	
	public User(Parcel parcel) {
		Id = parcel.readInt();
		Name = parcel.readString();
		Password = parcel.readString();
		IsAdmin = parcel.readInt() > 0;
	}
	
	public User() { }
	
	public static final Parcelable.Creator<User> CREATOR = new Parcelable.Creator<User>() {
		public User createFromParcel(Parcel in) {
		    return new User(in);
		}
		
		public User[] newArray(int size) {
		    return new User[size];
		}
	};
}
