function createDatePicker(elementId)
{
	var element = document.getElementById(elementId);
	var r = rome(element,
		{
			time: false,
			inputFormat: 'MMMM D, YYYY',
		});

	r.on('data', function (value) {
		element.value = value;
		var event = new Event('change');
		element.dispatchEvent(event);
	});

	r.show();
}