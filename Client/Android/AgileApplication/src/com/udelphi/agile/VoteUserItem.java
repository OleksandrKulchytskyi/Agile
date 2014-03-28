package com.udelphi.agile;

public class VoteUserItem {
	
	private int userId;
	private String user;
	private int vote;
	private boolean finished;
	
	public VoteUserItem(int userId, String user, int vote, boolean finished) {
		this.setUserId(userId);
		this.setUser(user);
		this.setVote(vote);
		this.setFinished(finished);
	}
	
	public int getUserId() {
		return userId;
	}

	public void setUserId(int userId) {
		this.userId = userId;
	}

	public String getUser() {
		return user;
	}

	public void setUser(String user) {
		this.user = user;
	}

	public int getVote() {
		return vote;
	}

	public void setVote(int vote) {
		this.vote = vote;
	}

	public boolean isFinished() {
		return finished;
	}

	public void setFinished(boolean finished) {
		this.finished = finished;
	}

}
