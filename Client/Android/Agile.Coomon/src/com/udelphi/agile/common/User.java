package com.udelphi.agile.common;

import java.util.List;

public class User {

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
}
