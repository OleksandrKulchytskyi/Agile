window.app = window.agileApp || {};

window.app.service = (function () {
	var baseUri = '/api/';
	var serviceUrls = {
		rooms: function () { return baseUri + 'room/getRooms/'; },
		roombyName: function (roomname) { return baseUri+'room/getroom/' + '?name=' + roomname; }
	}

	function ajaxRequest(type, url, data) {
		var options = {
			url: url,
			headers: {
				Accept: "application/json"
			},
			contentType: "application/json",
			cache: false,
			type: type,
			data: data ? ko.toJSON(data) : null
		};
		return $.ajax(options);
	}

	return {
		allRooms: function () {
			return ajaxRequest('get', serviceUrls.rooms());
		},
		roomByName: function (roomname) {
			return ajaxRequest('get', serviceUrls.roombyName(roomname));
		}
	};
})();
