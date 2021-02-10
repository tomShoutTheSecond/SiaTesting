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
        return new Promise((resolve, reject) => 
        {
            let requestParams = new FormData();
            requestParams.append("file", file);

            console.log(new URLSearchParams(requestParams).toString());

            let url = `https://${SkynetConfig.portal}/skynet/skyfile`;
            let req = fetch(url, 
            {
                method: 'post',
                body: requestParams
            });
            
            req.then(response =>
            {
                if (response.ok) 
                {
                    //status code was 200-299
                    resolve(response.text());
                } 
                else 
                {
                    //status was something else
                    resolve(response.text());
                }
            }, error => 
            {
                reject("Request error");
            })
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

    async uploadFile()
    {
        Log.instance.log("Uploading file...");

        this.file = this.fileInput.files[0];
        
        let response = await FileUploader.instance.uploadFileToSkynet(this.file);
        Log.instance.log(response);
        Log.instance.logHtml(this.getSkylinkAnchor(response));
    }

    addButtonListener()
    {
        this.fileInput = document.getElementById("fileInput");
        this.fileInput.onchange = () => this.uploadFile();
    }

    getSkylinkAnchor(response)
    {
        let linkUrl = this.getSkylinkUrl(response);

        let a = document.createElement('a');
        let linkText = document.createTextNode(linkUrl);
        a.appendChild(linkText);
        a.title = linkUrl;
        a.href = linkUrl;

        return a;
    }

    getSkylinkUrl(response)
    {
        let responseJson = JSON.parse(response);
        return `https://${SkynetConfig.portal}/${responseJson.skylink}`
    }
}

class Log
{
    static instance;
    
    logArea;

    constructor()
    {
        Log.instance = this;
        this.logArea = document.getElementById("log");
    }

    log(message)
    {
        let p = document.createElement('p');
        p.innerText = message;
        this.logArea.appendChild(p);
    }

    logHtml(element)
    {
        this.logArea.appendChild(element);
    }
}

let app = new App();