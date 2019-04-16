# Steps to see the full app in Microsoft Teams

1. Go to the Azure portal:"https://portal.azure.com" and Create a cosmos document db named "celebrationbotdb" with basic configuration and add a collection named "Events" in Db, that was created in previous step. This collection will store all the celebration events created by users.

2. Begin your tunnelling service to get an https endpoint. For this example ngrok is used. Start an ngrok tunnel with the following command (you'll need the https endpoint for the bot registration):<br>
   
   ```
    ngrok http 3978 --host-header=localhost
   ```
	
3. Register a new bot (or update an existing one) with Bot Framework by using the https endpoint started by ngrok and the extension "/api/messages" as the full endpoint for the bot's "Messaging endpoint". e.g. "https://####abcd.ngrok.io/api/messages" - Bot registration is here (open in a new browser tab): https://dev.botframework.com/bots

   > **NOTE**: When you create your bot you will create an App ID and App password - make sure you keep these for later.

4. Open the solution file of project, the microsoft-teams-celebrations-app, with visual studio and navigate to web.config file and replace placeholders with values.
   
    * domain - set to your ngrok's https endpoint domain
    * MicrosoftAppId - set to your registered bot's app ID
    * MicrosoftAppPassword - set to your registered bot's app password    
    * DocumentDbUrl - get it from cosmos db settings - keys
    * DocumentDbKey - get it from cosmos db settings - keys
	
	   ```
		"{{domain}}": "#####abc.ngrok.io"
        "{{MicrosoftAppId}}": "88888888-8888-8888-8888-888888888888"
        "{{MicrosoftAppPassword}}": "aaaa22229999dddd0000999"
		"{{DocumentDbUrl}}": "https://#####.documents.azure.com:443/"
		"{{DocumentDbKey}}": "fd0CmiFn4uxIza89tOUtzO6ocDfza9nOWXmSlY2bbxY3kPiv"
		```
5. Once the app is running, a manifest file is needed. 
   * Open the manifest.json file in any editor and replace the placeholders with values
    
       ```
        "{{domain}}": "#####abc.ngrok.io"
        "{{MicrosoftAppId}}": "88888888-8888-8888-8888-888888888888"
		```
   * Save the file and zip this file and bot icons (located next to it) together to create a manifest.zip file
		 
6. Once complete, sideload your zipped manifest to a team as described here (open in a new browser tab): https://msdn.microsoft.com/en-us/microsoft-teams/sideload

7. Congratulations!!! You have just created and sideloaded your celebration bot in Microsoft Teams app! Try adding new events from Events tab to celebrate them with teams.

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
