window.agileApp.roomActivityViewModel = (function (ko, datacontext, notify) {

	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var userList = ko.observableArray(),
		voteList = ko.observableArray(),
		error = ko.observable(),
		isUserAdmin = ko.observable(),
		loggedUser = ko.observable(),
		mySession = ko.observable(),
		roomDtoState = ko.observable(),

		setLoggedUser = function (user) {
			loggedUser(new datacontext.userViewModel(user));
		},
		setUserState = function (userState) {
			mySession(new datacontext.userConnectionState(userState));
		},
		setRoomDtoState = function (roomDto) {
			roomDtoState(new datacontext.roomDtoModel(roomDto));
		};

	//var viewModel = ko.mapping.fromJS(@Html.Raw(Model.ToJson()));
	return {
		userList: userList,
		voteList: voteList,
		error: error,
		mySession: mySession,
		loggedUser: loggedUser,
		setLoggedUser: setLoggedUser,
		setUserState: setUserState,
		roomDtoState: roomDtoState,
		setRoomDtoState: setRoomDtoState
	};
})(ko, agileApp.datacontext, agileApp.notifyService);

var mainVM = window.agileApp.roomActivityViewModel;

var agileHub = $.connection.agileHub;

$.connection.hub.logging = true;
$.connection.hub.start()
				.done(function () {
					// Call the Initialize function on the server. Will respond with auctionInitialized message
					if (window.agileApp.notifyService !== undefined)
						window.agileApp.notifyService.success("Successfully connected to the hub service!", null, true);
					agileHub.server.testMethod("Hello");
				})
				.fail(function () {
					if (window.agileApp.notifyService !== undefined)
						window.agileApp.notifyService.warning("Could not connect to the hub service!", null, true);
				});

// Handle connection loss and reconnect in a robust way
var timeout = null;
var interval = 10000;

$.connection.hub.stateChanged(function (change) {
	if (change.newState === $.signalR.connectionState.reconnecting) {
		timeout = setTimeout(function () {
			window.agileApp.notifyService.warning('Server is unreachable, trying to reconnect...', {}, true);
		}, interval);
	}
	else if (timeout && change.newState === $.signalR.connectionState.connected) {
		mainVM.mySession().sessionId = $.connection.hub.id;
		console.log('Server reconnected, reinitialize');
		$.connection.auctionHub.initialize();
		clearTimeout(timeout);
		timeout = null;
	}
});

agileHub.client.onTestMethod = function (data) {
	if (window.agileApp.notifyService !== undefined)
		window.agileApp.notifyService.info(data, null, true);
}

agileHub.client.onState = function (state) {
	mainVM.setUserState(state);
	agileHub.server.joinRoom($("#roomName").val(), mainVM.mySession().sessionId).done(function (joinRoomResult) {
		agileApp.notifyService.info(ko.toJSON(joinRoomResult), null, true);
	});
}

agileHub.client.onUserLogged = function (user) {
	mainVM.setLoggedUser(user);
	if ($("#userId").val() === "0")
		$("#userId").val(mainVM.loggedUser().id);
	//window.agileApp.notifyService.info("User e", null, true);
}

agileHub.client.onInitRoom = function (roomDto) {
	agileApp.notifyService.info("Initializing view datacontext.", {}, true);
	mainVM.setRoomDtoState(roomDto);
	ko.applyBindings(mainVM);// Initiate the Knockout bindings

	//var userSection = document.getElementById('users');
	//if (userSection !== null) 
	//ko.applyBindings(mainVM.roomDtoState(), userSection);
}

agileHub.client.onJoinedRoom = function (userDto) {

	var match = ko.utils.arrayFirst(mainVM.roomDtoState().connectedUsers(), function (item) {
		return item.id === userDto.Id;
	});
	if (!match) {
		var newUser = new agileApp.datacontext.userViewModel(userDto);
		mainVM.roomDtoState().connectedUsers.push(newUser);
	}
}

agileHub.client.onLeftRoom = function (userDto) {
	console.log("onLeftRoom ");
	mainVM.roomDtoState().connectedUsers.remove(function (item) { return item.id == userDto.Id })
	//var user = new agileApp.datacontext.userViewModel(userDto);
	//mainVM.roomDtoState().connectedUsers.remove(user);
}

agileHub.client.onRoomStateChanged = function (roomDto) {
	var user = new agileApp.datacontext.userViewModel(userDto);
	mainVM.roomDtoState().connectedUsers.remove(user);
}

agileHub.client.onUserVoted = function (userVoteDto) {

}

agileHub.client.onVoteItemClosed = function (voteItemDto) {

}

agileHub.client.onRoomDeleted = function (roomDto) {
	agileApp.notifyService.warning("Sorry room is no longer valid!!!!", {}, true);
}