# Title of your SE Project - Azure Cloud Implementation

Use this file to describe your experiment.
This file is the whole documentation you need.
It should include images, best with relative path in Documentation. For Example "/pic/image.png"  
Do not paste code-snippets here as image. Use rather markdoown (MD) code documentation.
For example:

~~~csharp
public voiud MyFunction()
{
    Debug.WriteLine("this is a code sample");
}
~~~


## What is your experiment about

Describe here what your experiment is doing. Provide a reference to your SE project documentation (PDF)*)

1. What is the **input**?

2. What is the **output**?

3. What your algorithmas does? How ?

## How to run experiment

Describe Your Cloud Experiment based on the Input/Output you gave in the Previous Section.

**_Describe the Queue Json Message you used to trigger the experiment:_**  

~~~json
{
     ExperimentId = "123",
     InputFile = "https://beststudents2.blob.core.windows.net/documents2/daenet.mp4",
     .. // see project sample for more information 
};
~~~

- ExperimentId : Id of the experiment which is run  
- InputFile: The video file used for trainign process  

**_Describe your blob container registry:**  

what are the blob containers you used e.g.:  
- 'training_container' : for saving training dataset  
  - the file provided for training:  
  - zip, images, configs, ...  
- 'result_container' : saving output written file  
  - The file inside are result from the experiment, for example:  
  - **file Example** screenshot, file, code  


**_Describe the Result Table_**

 What is expected ?
 
 How many tables are there ? 
 
 How are they arranged ?
 
 What do the columns of the table mean ?
 
 Include a screenshot of your table from the portal or ASX (Azure Storage Explorer) in case the entity is too long, cut it in half or use another format
 
 - Column1 : explaination
 - Column2 : ...
Some columns are obligatory to the ITableEntities and don't need Explaination e.g. ETag, ...
 
