#### Exercise I 

- To confirm that azure cli is being installed properly I wrote the command ``az --version``, than I used ``az login`` command to login to my account, 
- To show the account details I need this command- ``az account show``,
- It shows all the availabe subscriptions one account have - ``az account list`` 
- To find about a command I need to type this command -  ``az find vm`` or ``az find "az vm"``

[Here, Scripts URL](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise%20-%20%2001/Ex1scripts.bat)


#### Exercise 2 - Docker in Azure

1. [Here is the URL to the docker file](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise%20-%2002/ConsoleAppDocker/ConsoleAppDocker/Dockerfile) 
2. [Here is  the URL to the publish image in the Docker Hub](https://hub.docker.com/layers/mostainahmed/cloudcomputing/1st/images/sha256-afb0e7328313f2cb768325f56ce0f31d87eaa74394bde951fad08252e74e42ff?context=repo)
3. [URL to the image in the Azure Registry](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise%20-%2002/ConsoleAppDocker/AzureContainerRegistry.png)

#### Exercise 3 - Host a web application with Azure App service

1.[The public URL of the webapplication](https://exercie03.azurewebsites.net/)

2.[The URL to the source code of the hosted application](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise-%2003/CloudProject/Program.cs)

3.[AZ scripts](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise-%2003/MostainAhmedScripts.bat)

#### Exercise 4 - Deploy and run the containerized app in AppService

1. Created a resource group named ``learn-deploy-container-acr-rg``
2. created a registry ``ex4container`` under the resource group name ``learn-deploy-container-acr-rg``
3. created a sperated directory in ``https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/Syed_Mostain_Ahmed/MyWork/Cloud%20Exercise%20-%20%2004``
4. logged in to azure account in ``powershell`` in  and cloned a simple webapp from ``https://github.com/MicrosoftDocs/mslearn-deploy-run-container-app-service.git`` in the directory.
5. then go to the directory by running command in powershell ``cd mslearn-deploy-run-container-app-service/dotnet`` 
6. then run the command ``az acr build --registry ex4container --image webimage .``
7. The Docker file contains the step-by-step instructions for building a Docker image from the source code for the web app. Container Registry runs these steps to build the image, and as each step completes, a message is generated. The build process should finish after a couple of minutes without any errors or warnings.

#### Exercise 5 - Blob Storage

- [Input
Container link](https://blbstrg5.blob.core.windows.net/inputcontainer?sp=r&st=2023-06-20T12:08:05Z&se=2023-10-20T20:08:05Z&spr=https&sv=2022-11-02&sr=c&sig=HXbAPPmjOaBimOzp%2BjY4XVAf3Kbxim6%2FbMz%2FfhOtPZk%3D)

- [Output
Container link](https://blbstrg5.blob.core.windows.net/outputcontainer?sp=r&st=2023-06-20T12:06:54Z&se=2023-10-20T20:06:54Z&spr=https&sv=2022-11-02&sr=c&sig=C4fMuwFtnY7BwLVv1r349JE2q3AgVngWvgIXll8FHo4%3D)

- [File 1 link](https://blbstrg5.blob.core.windows.net/outputcontainer/App%20Service.pptx?sp=r&st=2023-06-20T12:09:09Z&se=2024-06-20T20:09:09Z&spr=https&sv=2022-11-02&sr=b&sig=BGBp3YDbYwIyaxC3TI6K7%2F%2FXqwvfKnmW8kFAOgoZDZk%3D)  [File 2 link](https://blbstrg5.blob.core.windows.net/outputcontainer/Azure.pptx?sp=r&st=2023-06-20T12:10:28Z&se=2024-06-20T20:10:28Z&spr=https&sv=2022-11-02&sr=b&sig=KNt%2FJ1z10PxWTxIM%2BAN%2Fw%2BqYTxb%2B3NwPOERpY6Jm2So%3D)  [File 3 link](https://blbstrg5.blob.core.windows.net/outputcontainer/Cloud%20Project%20Architecture.pptx?sp=r&st=2023-06-20T12:11:05Z&se=2024-06-20T20:11:05Z&spr=https&sv=2022-11-02&sr=b&sig=IxmMbFumliwKNG4hJYA9AaKEW3ZQ4Zr80TswuAtlfVg%3D) with 1 year time.

#### Exercise 6 - Table Storage

[URL for table storage](https://blbstrg5.table.core.windows.net/?sv=2022-11-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2023-10-20T20:11:56Z&st=2023-06-20T12:11:56Z&spr=https&sig=NWns%2FsdyNDnqM9EMU8Stlaj95xnJ2zPRoZGiBdOa8U8%3D)

#### Exercise 7 - Queue Storage

[Url for queue storage](https://blbstrg5.queue.core.windows.net/?sv=2022-11-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2023-10-20T20:11:56Z&st=2023-06-20T12:11:56Z&spr=https&sig=NWns%2FsdyNDnqM9EMU8Stlaj95xnJ2zPRoZGiBdOa8U8%3D)