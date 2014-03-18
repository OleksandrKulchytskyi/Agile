﻿window.agileApp.roomActivityViewModel = (function (ko, datacontext, notify) {

	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var userList = ko.observableArray(),
		voteList = ko.observableArray(),
		error = ko.observable(),
		isUserAdmin = ko.observable(),
		loggedUser = ko.observable(),
		mySession = ko.observable(),
		roomDtoState = ko.observable(),
		selectedVoteItem = ko.observable(),
		previouslySelectedVote = ko.observable(),
		selectedVoteItemChanged = function (entry, event) {
			if (previouslySelectedVote()) {
				$(previouslySelectedVote()).css("background-color", "white");

				if (selectedVoteItem().id == entry.id) {
					console.log(entry.id);
					selectedVoteItem(null);
					previouslySelectedVote(null);
					return;
				}
			}

			previouslySelectedVote(event.target);
			selectedVoteItem(entry);
			$(event.target).css("background-color", "lightblue");
		},
		setLoggedUser = function (user) {
			loggedUser(new datacontext.userViewModel(user));
		},
		setUserState = function (userState) {
			mySession(new datacontext.userConnectionState(userState));
		},
		setRoomDtoState = function (roomDto) {
			roomDtoState(new datacontext.roomDtoModel(roomDto));
		},
		importQuestions = function () {
			$("#importDialog").dialog("open");
		},
		openVoteItem = function () {
			var roomName = $("#roomName").val();
			if (selectedVoteItem())
				agileHub.server.openVoteItem(roomName, selectedVoteItem().id).fail(function () {
					agileApp.notifyService.error("Unable to open vote item.", {}, true);
				});
			else
				agileApp.notifyService.warning("Please select vote item row", {}, true);
		},
		closeVoteItem = function () {
			$("#closeVoteDialog").dialog('open');
		},
		changeRoomState = function () {
			$("#changeRoomStateDialog").dialog('open');
		},
		submitVoteForItem = function () {
			$("#voteForItemDialog").dialog('open');
		};

	function initDialogs() {

		jQuery.browser = {};
		(function () {
			jQuery.browser.msie = false;
			jQuery.browser.version = 0;
			if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
				jQuery.browser.msie = true;
				jQuery.browser.version = RegExp.$1;
			}
		})();

		$("#importForm").submit(function (e) {
			e.preventDefault(); //STOP default action
			var roomID = $("#roomId").val();
			console.log(roomID);
			var form = $("#importForm");
			var data = new FormData($(this)[0]);
			var formURL = $(this).attr("action");
			$.ajax(
			{
				url: formURL,
				type: "POST",
				data: data,
				cache: false,
				contentType: false,
				processData: false,
				beforeSend: function (xhr) { xhr.setRequestHeader('RoomId', roomID); },
				success: function (data, textStatus, jqXHR) {
					if (data.Status === "Ok") {
						agileApp.notifyService.success("File has been successfully submitted.", {}, true);
						$("#importDialog").dialog("close");
					}
					else
						agileApp.notifyService.error("Unable to upload file.", {}, true);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					agileApp.notifyService.error("Unable to upload file.", {}, true);
					console.log(errorThrown);
					console.log(jqXHR.responseText);
					console.log(jqXHR.status);
				}
			});
		});

		$("#importDialog").dialog({
			autoOpen: false,
			height: 280,
			width: 370,
			modal: true,
			buttons: {
				Cancel: function () {
					$(this).dialog("close");
				}
			},
			close: function () {
				var from = $("#importForm");
				from.find('input:file').val('');
			}
		});

		$("#closeVoteDialog").dialog({
			autoOpen: false,
			height: 280,
			width: 370,
			modal: true,
			buttons: {
				"Set": function () {
					if (!selectedVoteItem()) {
						agileApp.notifyService.warning("Please select vote item row", {}, true);
						return;
					}
					//var field = $("#closeVoteItemForm  #closeVoteItemForm");
					var mark = $('#closeVoteItemForm input[id="txtVoteItemMark"]').val();
					if (isNaN(mark)) {
						agileApp.notifyService.warning("Mark value must be a digit.", {}, true);
						return;
					}
					else {
						var roomName = $("#roomName").val();
						agileHub.server.closeVoteItem(roomName, selectedVoteItem().id, mark)
						.done(function () {
							agileApp.notifyService.success("Vote item is closed with mark:" + mark, {}, true);
							$('#closeVoteDialog').dialog('close');
						})
						.fail(function () {
							agileApp.notifyService.error("Unable to close vote item.", {}, true);
						});
					}
				},
				Cancel: function () {
					$(this).dialog("close");
				}
			},
			close: function () {

			}
		});

		$("#changeRoomStateDialog").dialog({
			autoOpen: false,
			height: 280,
			width: 370,
			modal: true,
			buttons: {
				"Change": function () {
					//var option = $("#changeRoomStateForm  #comboState");
					var text = $('#changeRoomStateForm #comboState :selected').text();
					var value = $('#changeRoomStateForm #comboState :selected').val();
					console.log(text);
					if (value != null) {
						var roomName = $("#roomName").val();
						agileHub.server.changeRoomState($("#roomName").val(), value)
						.done(function () {
							agileApp.notifyService.success("Room state changed:" + text, {}, true);
							$('#changeRoomStateDialog').dialog('close');
						})
						.fail(function () {
							agileApp.notifyService.error("Unable to change room state.", {}, true);
						});
					}
				},
				Cancel: function () {
					$(this).dialog("close");
				}
			},
			close: function () {

			}
		});

		$("#voteForItemDialog").dialog({
			autoOpen: false,
			height: 380,
			width: 370,
			modal: true,
			buttons: {
				"Vote": function () {
					//var option = $("#changeRoomStateForm  #comboState");
					if (!selectedVoteItem()) {
						agileApp.notifyService.warning("Please select vote item first.", {}, true);
						return;
					}
					var mark = $('#submitVoteForm input[id="txtVoteItemMark"]').val();
					if (isNaN(mark)) {
						agileApp.notifyService.warning("Mark value must be a digit.", {}, true);
						return;
					}
					agileHub.server.voteForItem($("#roomName").val(), selectedVoteItem().id, mark)
					.done(function () {
						agileApp.notifyService.success("Vote has been accepted.", {}, true);
						$('#voteForItemDialog').dialog('close');
					})
					.fail(function () {
						agileApp.notifyService.error("Unable to submit vote mark.", {}, true);
					});
				},
				Cancel: function () {
					$(this).dialog("close");
				}
			},
			open: function () {
				if (selectedVoteItem()) {
					var data = selectedVoteItem().content();
					console.log(data);
					$('#submitVoteForm #voteItemContent').text(data);
				}
			},
			close: function () {

			}
		});
	}

	initDialogs();

	//var viewModel = ko.mapping.fromJS(@Html.Raw(Model.ToJson())); 
	return {
		userList: userList,
		voteList: voteList,
		error: error,
		mySession: mySession,
		loggedUser: loggedUser,
		selectedVoteItem: selectedVoteItem,
		previouslySelectedVote: previouslySelectedVote,
		selectedVoteItemChanged: selectedVoteItemChanged,
		setLoggedUser: setLoggedUser,
		setUserState: setUserState,
		roomDtoState: roomDtoState,
		setRoomDtoState: setRoomDtoState,
		importQuestions: importQuestions,
		openVoteItem: openVoteItem,
		closeVoteItem: closeVoteItem,
		changeRoomState: changeRoomState,
		submitVoteForItem: submitVoteForItem
	};
})(ko, agileApp.datacontext, agileApp.notifyService);

var mainVM = window.agileApp.roomActivityViewModel;

var agileHub = $.connection.agileHub;

//Multiple methods added to proxy in JavaScript using jQuery: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//$.extend(agileHub.server, {
//	method1: function (param1, param2) {},
//	method2: function (param3, param4) {}
//});

$.connection.hub.logging = true;
$.connection.hub.start({ transport: ['webSockets', 'longPolling'] })
				.done(function () {
					// Call the Initialize function on the server. Will respond with auctionInitialized message
					if (window.agileApp.notifyService !== undefined)
						window.agileApp.notifyService.success("Successfully connected to the hub service!", null, true);
					agileHub.server.testMethod("Hello");
				})
				.fail(function (ex) {
					if (window.agileApp.notifyService !== undefined)
						window.agileApp.notifyService.error("Could not connect to the hub service!" + ex.message, null, true);
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
		//$.connection.auctionHub.initialize();
		clearTimeout(timeout);
		timeout = null;
	}
});

agileHub.client.onErrorHandler = function (exMsg) {
	if (agileApp.notifyService !== undefined)
		agileApp.notifyService.error(exMsg, {}, true);

	//$("#error").fadeIn(1000, function () {
	//	$("#error").fadeOut(3000);
	//});
	//agileHub.connection.stop();
	//chat.connection.stop();
}

agileHub.client.onTestMethod = function (data) {
	if (window.agileApp.notifyService !== undefined)
		window.agileApp.notifyService.info(data, null, true);
}

agileHub.client.onState = function (state) {
	mainVM.setUserState(state);
	agileHub.server.joinRoom($("#roomName").val(), mainVM.mySession().sessionId).fail(logHubInvokeExc)
					.done(function (joinRoomResult) {
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

	var headerRow = $('#activityTable thead tr')[0];
	var voteItems = $('#activityTable tbody tr')
	var users = $(headerRow).find('th');

	if (users.length == 1) return;
	if (voteItems.length == 0) return;

	var userId;
	var voteId;
	var voteItemRow;

	//foreach vote item
	for (var v = 0; v < voteItems.length; v++) {
		var voteItemRow = $(voteItems[v]);
		voteId = voteItemRow.attr('id');
		//foreach user for vote item
		for (var u = 1; u < users.length; u++) {
			var user = $(users[u]);
			userId = user.attr('id');
			addCellToRow(voteItemRow, userId, voteId);
		}
	}
}

function addCellToRow(row, uId, vId) {
	var cell = $('<td>No vote</td>');
	cell.attr('id', vId + "_" + uId);
	cell.appendTo(row);
	//addRowContents(newRow);
}

function RemoveCellFromRow(row, uId, vId) {
	var cell = $(row).find('#' + vId + '_' + uId);
	if (cell != null)
		cell.remove();
	else
		console.log("cell not found:" + vId + "_" + uId);
	//addRowContents(newRow);
}

function addRow(grid) {
	var newRow = $('<tr></tr>').appendTo(grid);
	addRowContents(newRow);
}

function addRowAfter(target) {
	var newRow = $('<tr></tr>');
	addRowContents(newRow);
	target.after(newRow);
}

function addRowContents(row) {
	$('<td><textarea/></td>').appendTo(row);
	$('<td id = "regla"></td>').droppable(drpOptions).appendTo(row);
	var buttonCell = $('<td></td>').appendTo(row);
	$('<button></button>').addClass('addRow').text('+').click(function () {
		addRowAfter($(this).closest('tr'));
		$(this).hide();
	}).appendTo(buttonCell);
}

function logHubInvokeExc(e) {
	if (e.source === 'HubException' && agileApp.notifyService !== undefined) {
		var data = e.message + ' : ' + e.data.user;
		console.log(data);
		agileApp.notifyService.error("Could not inoke hub server method! " + data, null, true);
	}
};

agileHub.client.onJoinedRoom = function (userDto) {

	var match = ko.utils.arrayFirst(mainVM.roomDtoState().connectedUsers(), function (item) {
		return item.id === userDto.Id;
	});
	if (!match) {
		var newUser = new agileApp.datacontext.userViewModel(userDto);
		mainVM.roomDtoState().connectedUsers.push(newUser);

		var voteItems = $('#activityTable tbody tr')
		if (voteItems.length == 0) return;

		var userId = userDto.Id;
		var voteId;
		var voteItemRow;

		//foreach vote item
		for (var v = 0; v < voteItems.length; v++) {
			var voteItemRow = $(voteItems[v]);
			voteId = voteItemRow.attr('id');
			addCellToRow(voteItemRow, userId, voteId);
		}
	}
}

agileHub.client.onLeftRoom = function (userDto) {
	console.log("onLeftRoom ");
	mainVM.roomDtoState().connectedUsers.remove(function (item) { return item.id == userDto.Id })
	var voteItems = $('#activityTable tbody tr')
	if (voteItems.length == 0) return;

	var userId = userDto.Id;
	var voteId;
	var voteItemRow;

	//foreach vote item
	for (var v = 0; v < voteItems.length; v++) {
		var voteItemRow = $(voteItems[v]);
		voteId = voteItemRow.attr('id');
		RemoveCellFromRow(voteItemRow, userId, voteId);
	}
}

agileHub.client.onRoomStateChanged = function (roomDto) {
	console.log("onRoomStateChanged ");
	console.log(roomDto);
	var roomDtoObservable = new agileApp.datacontext.roomDtoModel(roomDto);
	handleUsers(roomDtoObservable.connectedUsers);
	handleVotes(roomDtoObservable);
}

function handleUsers(usersObservale) {
	console.log(usersObservale);
	var rows = $('#activityTable thead tr');

	var columns;
	for (var i = 0; i < rows.length; i++) {
		columns = $(rows[i]).find('th');
		for (var j = 0; j < columns.length; j++) {
			if (j == 0)
				continue;
			console.log($(columns[j]).html());
		}
	}
}

function handleVotes(roomObs) {

	var headerRow = $('#activityTable thead tr')[0];
	var voteItems = $('#activityTable tbody tr')
	var users = $(headerRow).find('th');

	if (roomObs.itemsToVote().length === voteItems.length &&
		users.length - 1 == roomObs.connectedUsers().length) {
		console.log("all equals");
		return
	}

	if (users.length - 1 === voteItems.length - 1)

		if (users.length === 1) return;
	if (voteItems.length === 0) return;

	var userId;
	var voteId;
	var voteItemRow;

	//foreach vote item
	for (var v = 0; v < voteItems.length; v++) {
		var voteItemRow = $(voteItems[v]);
		voteId = voteItemRow.attr('id');
		//foreach user for vote item
		for (var u = 1; u < users.length; u++) {
			var user = $(users[u]);
			userId = user.attr('id');
			addCellToRow(voteItemRow, userId, voteId);
		}
	}

	//var usersRow = $('#activityTable thead tr')[0];
	//var voteItems = $('#activityTable tbody tr')
	//var users = $(usersRow).find('th');

	//if (users.length == 1) return;
	//if (voteItems.length == 0) return;

	//var userId;
	//var voteId;
	//var voteItemRow;

	//var forPurge = [];
	////remove deleted votes
	//$(voteItems).each(function (indx, row) {
	//	var id = $(row).attr('id');
	//	if (!room.containsVote(id))
	//		forPurge.push(id);
	//});

	//for (var i = 0; i < forPurge.length; i++) {
	//	$("#activityTable > tbody > tr:" + i).remove();
	//}

	////foreach vote item
	//for (var v = 0; v < voteItems.length; v++) {
	//	var voteItemRow = $(voteItems[v]);
	//	voteId = voteItemRow.attr('id');
	//	//foreach user for vote item
	//	for (var u = 1; u < users.length; u++) {
	//		var user = $(users[u]);
	//		userId = user.attr('id');
	//		addCellToRow(voteItemRow, userId, voteId);
	//	}
	//}
}

agileHub.client.onUserVoted = function (userVoteDto) {
	console.log(userVoteDto);
	var userId = userVoteDto.UserId;
	var voteId = userVoteDto.VoteItemId;

	var voteRow = $('#activityTable tr[id="' + voteId + '"]');
	var cellId = voteId + "_" + userId;
	console.log("cell id " + cellId);
	var cell = getCelInRowById(voteRow, cellId);
	if (cell != null) {
		$(cell).css('background-color', 'lightblue')
		var mark = userVoteDto.Mark;
		$(cell).val(mark);
		$(cell).text(mark);
	}
}

function getCelInRowById(row, cellId) {
	return $(row).find('td[id="' + cellId + '"]');
}

function getCell(columnId, rowId) {
	var column = $('#' + column).index();
	var row = $('#' + row)
	return row.find('td').eq(column);
}

agileHub.client.onVoteItemClosed = function (voteItemDto) {
	var id = voteItemDto.Id;
	var mark = voteItemDto.OverallMark;
	var voteRow = $('#activityTable tr[id="' + id + '"]');
	var cells = $(voteRow).children('td');
	$(cells).css('background-color', '#000');
	for (var i = 0; i < cells.length; i++) {
		if (i == 0)
			continue;
		$(cells[i]).val(mark);
		$(cells[i]).text(mark);
	}
}

agileHub.client.onVoteItemOpened = function (voteItemDto) {
	var id = voteItemDto.Id;
	var voteRow = $('#activityTable tr[id="' + id + '"]');
	var cells = $(voteRow).children('td');
	$(cells).css('background-color', 'aliceblue');

	for (var i = 0; i < cells.length; i++) {
		if (i == 0)
			continue;
		$(cells[i]).val("");
		$(cells[i]).text("");
	}
}

agileHub.client.onRoomDeleted = function (roomDto) {
	agileApp.notifyService.warning("Sorry room is no longer valid!!!!", {}, true);
}