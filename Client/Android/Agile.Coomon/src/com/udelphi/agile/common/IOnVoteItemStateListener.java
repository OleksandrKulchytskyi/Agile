package com.udelphi.agile.common;

public interface IOnVoteItemStateListener {	
	void onUserVoted(UserVote userVote);
	void onVoteItemClosed(VoteItem voteItem);
}
