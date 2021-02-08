class App
{
    fileInputController;
    fileUploader;
    log;

    //entry point
    constructor()
    {
        this.fileInputController = new FileInputController();
        this.fileUploader = new FileUploader();
        this.log = new Log();
    }
}

class SkynetConfig 
{
    static get portal()
    {
        return 'siasky.net';
    }
}

class FileUploader
{
    static instance;

    constructor()
    {
        FileUploader.instance = this;
    }

    async uploadFileToSkynet(file) 
    {
        return new Promise((resolve, reject) => {
            var requestParams = new FormData();
            requestParams.append("filename", "myFile");
            requestParams.append("file", file);

            var url = `https://${SkynetConfig.portal}/skynet/skyfile`;
        
            var request = new XMLHttpRequest();
            request.open("POST", url, true);
            request.onload = e => 
            {
                //uploaded
                resolve(request.responseText);
            };

            request.onerror = e =>
            {
                //error
                reject("error");
            }

            console.log(new URLSearchParams(requestParams).toString());
            request.send(requestParams);
        });
    }
}

class FileInputController
{
    static instance;

    fileInput;
    file;

    constructor()
    {
        FileInputController.instance = this;
        this.addButtonListener();
    }

    addButtonListener()
    {
        this.fileInput = document.getElementById("fileInput");
        this.fileInput.onchange = () => this.uploadFile();
    }

    async uploadFile()
    {
        Log.instance.log("Uploading file...");

        this.file = this.fileInput.files[0];
        Log.instance.log(this.file);

        let response = await FileUploader.instance.uploadFileToSkynet(this.file);
        Log.instance.log(response);
    }
}

class Log
{
    static instance;

    constructor()
    {
        Log.instance = this;
    }

    log(message)
    {
        let logArea = document.getElementById("log");
        logArea.innerText += message + "\n";
    }
}

let app = new App();