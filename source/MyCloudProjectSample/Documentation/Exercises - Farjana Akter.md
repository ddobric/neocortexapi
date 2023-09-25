# My Exercises

#### Exercise 1 

1. Here is the url for: [Scripts](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2001/FarjanaAkterExercise1Scripts.bat)

2. Here is the url for preview: [Exercise 1](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2001)

#### Exercise 2 - Docker in Azure

1. [URL to the docker file](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2002/ConsoleAppForDockerPractice/ConsoleAppForDockerPractice/Dockerfile)
2. [URL to the publish image in the Docker Hub](https://hub.docker.com/layers/farjanaakter1/farjanaakter_consoleappfor_dockerpractice/1st/images/sha256-46dcb0076016703158ab7e26946582c4db6190c54bba1942e48efbeae56d0b8c?context=repo)
3. [URL to the image in the Azure Registry](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2002/ConsoleAppForDockerPractice/Azure_Container_Registry.png)

#### Exercise 3 - Host a web application with Azure App service

1. [URL of the webapplication](https://exercise03.azurewebsites.net/)
2. [URL to the source code of the hosted application](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2003/CloudProject/Program.cs)
3. [AZ scripts URL](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%20%2003/CloudProject/AZ%20Script.bat)

#### Exercise 4 - Deploy and run the containerized app in AppService

1. Created a resource group named ``learn-deploy-container-acr-rg``
2. created a registry ``ex4container1`` under the resource group name ``learn-deploy-container-acr-rg``
3. created a sperated directory in ``https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/Farjana_Akter/MyWork/Cloud%20Exercise%20-%2004``
4. logged in to azure account in ``powershell`` in  and cloned a simple webapp from ``https://github.com/MicrosoftDocs/mslearn-deploy-run-container-app-service.git`` in the directory.
5. then go to the directory by running command in powershell ``cd mslearn-deploy-run-container-app-service/dotnet`` 
6. then run the command ``az acr build --registry ex4container1 --image webimage .``
7. The Docker file contains the step-by-step instructions for building a Docker image from the source code for the web app. Container Registry runs these steps to build the image, and as each step completes, a message is generated. The build process should finish after a couple of minutes without any errors or warnings.

#### Exercise 5 - Blob Storage

- [URL of the Input
Container](https://blbstrg.blob.core.windows.net/inputcontainer?sp=r&st=2023-06-25T22:49:00Z&se=2023-10-26T06:49:00Z&sv=2022-11-02&sr=c&sig=OgXrBsxNR7ifcOWe5DpNmwWZwQk%2BKzl%2BAa8%2BNzDj5mM%3D)

- [URL to the Image of Input Container](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%2005/Blob_StorageInputContainer.png)

- [URL of the Output
Container](https://blbstrg.blob.core.windows.net/outputcontainer?sp=r&st=2023-06-25T22:49:46Z&se=2023-10-26T06:49:46Z&sv=2022-11-02&sr=c&sig=2Cc3AHRXoSEOsiucAlcrHsv5SKWKFIAaNdWIjDz%2FvMk%3D)

- [URL to the Image of Output Container](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-%2005/OutputContainer.png)

- [ (URL for file 1)](https://azstblbst.blob.core.windows.net/inputcontainer/Exercise%201%20-%20delete%20resources%20from%20azure%20cli.png?sp=r&st=2023-06-15T10:06:20Z&se=2024-06-15T18:06:20Z&spr=https&sv=2022-11-02&sr=b&sig=Qo3JWYkWDpvyiVadFeUKtMgRyUvq6yvzQGyqv4Bl%2FtM%3D)   [ (URL for file 2)](https://azstblbst.blob.core.windows.net/inputcontainer/Azure%20Storage%20(1).pptx?sp=r&st=2023-06-15T10:08:50Z&se=2024-06-15T18:08:50Z&spr=https&sv=2022-11-02&sr=b&sig=r%2BQd9N0oNmFiBFdYfoMlAZ2FH5WpIL86CZtL2laSoUc%3D)  [(URL for file 3)](https://azstblbst.blob.core.windows.net/inputcontainer/Exercise%201%20-%20login_from_azure_cli.png?sp=r&st=2023-06-15T10:09:40Z&se=2024-06-15T18:09:40Z&spr=https&sv=2022-11-02&sr=b&sig=5nPKsOYCWs09kjIe%2FWJ66yvuVmqvwD3uQDtIl7vhUWg%3D)

#### Exercise 6 - Table Storage

- [Table service SAS URL](https://blbstrg.table.core.windows.net/tablestorage?sp=r&st=2023-06-25T22:46:40Z&se=2023-10-26T22:46:00Z&sv=2022-11-02&sig=q%2B4u3zmVQpeN5w8f233Wv7UFz0GBqHKuUozm3EOv7aw%3D&tn=tablestorage)
- [URL to  the image of table storage in Azure](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-06/table%20storage.png)




#### Exercise 7 - Queue Storage

- [Queue service SAS URL](https://blbstrg.queue.core.windows.net/queuestorage?sp=r&st=2023-06-25T22:48:03Z&se=2023-10-26T22:48:00Z&sv=2022-11-02&sig=Owq81y1PhM4mvgAM926lv0dcD8f5%2BdxoAFKBv%2F44H%2BY%3D)
- [URL to  the image of queue storage in Azure](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Farjana_Akter/MyWork/Cloud%20Exercise%20-07/Queue%20Storage.png)
