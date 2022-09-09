#pokemon-cards

The project was created using Unity 2020.3.35f1 and is structured as follows:

Assets/	
	Scenes/
		MainScene.unity
	Prefabs/
		PokemonTemplate.prefab
	Scripts/
		Network/
			Endpoints.cs
			PokemonDto.cs
			WebRequestClient.cs
		View/
			PokemonContentList.cs
			PokemonTemplate.cs
			ProgressContainer.cs
			Texts.cs
		GameManager.cs
		
The entry point for the project is the MainScene.unity and pressing play will start the fetching process.

The GameManager class is in charge of starting the fetch requests, retrying if there are any kind of errors and notifying the views to display the proper information as follows:
	- the list of url requests is generated based on the list of pokemon names stored in the _ownedPokemonNames array. When generating it I'm assuming the names should be lower cased and remove 
		any duplicates. The API has no specific documentation regarding the name format so I'm relying on sending the requests using the names as they are and if the request will fail I'll send back 
		a null object.
	- based on the url list created above I will start the requests to the server. If there will be any fails I will retry for a limited number of times that can be setup using NO_OF_RETRIES. After each retry 
		I will add a delay time which will be increased on every retry so I won't spam the server. The minimum delay time can be setup using MINIMUM_WAITING_TIMES
	- the succesful responses will be added to a list and sorted on spot. 
		- after each succesful response an event (OnRequestSuccesful) will be triggered to notify the views about the current progress,
		- after the whole fetching process is finished an event (OnPokemonInfoFetched) will be triggered to notify the views that we have all data. 
		- any view can subscribe to the above events through the Unity inspector.

The WebRequestClient class will handle starting the server requests as follows:
	- the exposed public methods GetAnyAsync/GetAllAsync can be used to start multiple batched requests. Each batch will run the requests in parallel using Task.WhenAny or Task.WhenAll. 
	The maximum size of a batch can be setup using BATCH_SIZE. Starting parallel requests in batches is the optimization I chose for sending a large number of requests to the server. 
	- I decided on using the GetAnyAsync method which will rely on Task.WhenAny because it will give me a better way to visually show to the user the progress for a large number of requests. 
	- any failed request will be added to a list and will return default for the generic type, which will be null for our objects. The GetAnyAsync/GetAllAsync will prune the null object and only return
	valid results.
	- the data fetched from the server is encapsulated in the PokemonDto class.

The PokemonContentList class will handle populating the pokemon list with all the data received from the server:
	- I created a standard UI for a scrolling list that will be able to hold any large number of objects. 
	- the class will subscribe to the event defined in the GameManager and populate the list accordingly 
	
The ProgressContainer class is handling any notifications for the user:
	- I created different states used for triggering the notifications
	- I also added a progress bar that will show the current progress to the user. It will subscribe to the event defined in the GameManager to update it correctly.

Helper classes:
	- Endpoints: will hold the list of endpoints used by the project
	- Texts: will hold all the texts used by the project

In terms of UI I tried to make it rescalable for any portrait resolutions by using anchors and layout groups whenever necessary. 
The assumption was the project will be available only in portrait mode.
	
	
