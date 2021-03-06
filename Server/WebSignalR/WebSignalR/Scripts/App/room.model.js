﻿(function (ko, datacontext) {

	datacontext.userViewModel = userViewModel;
	datacontext.userConnectionState = userConnectionState;
	datacontext.roomDtoModel = roomDtoModel;
	datacontext.voteItemModel = voteItemViewModel;

	function userConnectionState(data) {
		var self = this;
		data = data || {};

		self.sessionId = data.SessionId;
		self.userId = data.UserId;

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

	function userViewModel(data) {
		var self = this;
		data = data || {};

		self.id = data.Id;
		self.name = ko.observable(data.Name);
		self.isAdmin = ko.observable(data.IsAdmin);

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

	function voteItemViewModel(data) {
		var self = this;
		data = data || {};
		self.id = data.Id;

		self.content = ko.observable(data.Content);
		self.closed = ko.observable(data.Closed);
		self.opened = ko.observable(data.Opened);

		self.hostRoomId = ko.observable(data.HostRoomId);

		self.overallMark = ko.observable(data.OverallMark);
		self.votedUsers = ko.observableArray(ko.utils.arrayMap(data.VotedUsers, function myfunction(userVote) {
			return new userVoteViewModel(userVote);
		}));

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

	function userVoteViewModel(data) {
		var self = this;
		data = data || {};
		self.id = data.Id;
		self.userId = ko.observable(data.UserId);
		self.voteId = ko.observable(data.VoteItemId);
		self.mark = ko.observable(data.Mark);

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

	function roomDtoModel(data) {
		var self = this;
		data = data || {};

		self.id = data.Id;
		self.active = ko.observable(data.Active);
		self.name = ko.observable(data.Name);
		self.description = ko.observable(data.Description);

		self.connectedUsers = ko.observableArray(ko.utils.arrayMap(data.ConnectedUsers, function myfunction(user) {
			return new userViewModel(user);
		}));

		self.itemsToVote = ko.observableArray(ko.utils.arrayMap(data.ItemsToVote, function myfunction(voteItem) {
			return new voteItemViewModel(voteItem);
		}));

		self.containsVote = function (vid) {
			return ko.utils.arrayFirst(itemsToVote(), function (item) {
				return vid === item.id;
			});
		}

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

})(ko, agileApp.datacontext);