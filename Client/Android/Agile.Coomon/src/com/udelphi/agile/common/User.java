package com.udelphi.agile.common;

import java.util.List;

public class User {
	public int Id;
	public String Name;
	public String Password;
	public boolean IsAdmin;
	
	public List<Privilege> Privileges;
}
