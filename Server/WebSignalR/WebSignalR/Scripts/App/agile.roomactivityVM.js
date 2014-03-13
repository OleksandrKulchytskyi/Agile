window.agileApp.roomActivityViewModel = (function (ko, datacontext, notify) {
	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var userList = ko.observableArray(),
		voteList = ko.observableArray(),
        error = ko.observable(),
		isUserAdmin = ko.observable(),
		loggedUser = ko.observable(),
		mySession = ko.observable(),

		setLoggedUser = function (user) {
			loggedUser(new datacontext.userViewModel(user));
		},
		setUserState = function (userState) {
			mySession(new datacontext.userConnectionState(userState));
		};

	//datacontext.getRoomList(roomList, error); // load roomList
	//notify.info("Loading", null, true);

	return {
		userList: userList,
		voteList: voteList,
		error: error,
		mySession: mySession,
		loggedUser: loggedUser,
		setLoggedUser: setLoggedUser,
		setUserState: setUserState
	};

})(ko, agileApp.datacontext, agileApp.notifyService);

var mainVM = window.agileApp.roomActivityViewModel;
// Initiate the Knockout bindings
ko.applyBindings(mainVM);

var agileHub = $.connection.agileHub;

$.connection.hub.logging = true;
$.connection.hub.start()
	   .done(function () {
	   	// Call the Initialize function on the server. Will respond with auctionInitialized message
	   	if (window.agileApp.notifyService !== undefined) {
	   		window.agileApp.notifyService.success("Successfully connected to the hub service!", null, true);
	   	}
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
	if (window.agileApp.notifyService !== undefined)
		window.agileApp.notifyService.info(ko.toJSON(state), null, true);

	agileHub.server.joinRoom($("#roomName").val(), mainVM.mySession().sessionId).done(function (joinRoomResult) {
		agileApp.notifyService.info(ko.toJSON(joinRoomResult), null, true);
	});
}

agileHub.client.onUserLogged = function (user) {
	mainVM.setLoggedUser(user);
	if ($("#userId").val() === "0")
		$("#userId").val(mainVM.loggedUser().id);

	if (window.agileApp.notifyService !== undefined)
		window.agileApp.notifyService.info(ko.toJSON(user), null, true);
}

agileHub.client.onJoinedRoom = function (userDto) {

}

agileHub.client.onLeftRoom = function (userDto) {

}

agileHub.client.onRoomStateChanged = function (roomDto) {

}

agileHub.client.onUserVoted = function (userVoteDto) {

}

agileHub.client.onVoteItemClosed = function (voteItemDto) {

}