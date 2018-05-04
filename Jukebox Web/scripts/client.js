console.log('Client-side code running');

const button = document.getElementById('getMusics');
button.addEventListener('click', function(e) {
	console.log('button was clicked');
	fetch('/unity', {method: 'POST'})
		.then(function(response) {
			if(response.ok) {
				console.log('Click was recorded');
				window.location.reload(true);
				return;
			}
			throw new Error('Request failed.');
		})
		.catch(function(error) {
			console.log(error);
		});
});

const buttonSend = document.getElementsByName('sendMusic');
buttonSend.forEach((button) => {
	button.addEventListener('click', function(e) {
		fetch('/music', {
			headers: {
				'Accept': 'application/json, text/plain, */*',
				'Content-Type': 'application/json'
			},
			method: 'POST',
			body: JSON.stringify({title: button.id})
		}).then(function(response) {
			console.log(response);
			return
		})
			.catch(function(error) {
				console.log(error);
			});
	});
})