(function (ko, datacontext) {

	datacontext.userViewModel = userViewModel;
	datacontext.userConnectionState = userConnectionState;

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
		self.isAdmin = ko.observable(data.IsAdmin);
		self.closed = ko.observable(data.Closed);
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

		self.userId = ko.observable(data.User.Id);
		self.voteId = ko.observable(data.VoteItem.Id);
		self.mark = ko.observable(data.Mark);

		self.toJson = function () {
			return ko.toJSON(self);
		};
	}

})(ko, agileApp.datacontext);