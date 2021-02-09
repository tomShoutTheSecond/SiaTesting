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
            var fileReader = new FileReader();
            fileReader.filename
            fileReader.readAsBinaryString(file);
            fileReader.onload = event => 
            {
                console.log(fileReader.result);

                var filename = "bernie.jpg";

                fetch("https://siasky.net/skynet/skyfile", 
                {
                    "headers": 
                    {
                        "accept": "*/*",
                        "accept-language": "en-GB,en-US;q=0.9,en;q=0.8",
                        "content-type": "multipart/form-data; boundary=dart-http-boundary-jOWJUI3DuhK1eGPtwW_5n+Vu3bYHYC75gNHBiQRuBfux.+s3kEf",
                        "sec-fetch-dest": "empty",
                        "sec-fetch-mode": "cors",
                        "sec-fetch-site": "same-site"
                    },
                    "referrer": "https://skysend.hns.siasky.net/",
                    "referrerPolicy": "no-referrer-when-downgrade",
                    "body": `--dart-http-boundary-jOWJUI3DuhK1eGPtwW_5n+Vu3bYHYC75gNHBiQRuBfux.+s3kEf\r\ncontent-type: application/octet-stream\r\ncontent-disposition: form-data; name=\"file\"; filename=\"${filename}\"\r\n\r\n${fileReader.result}\r\n--dart-http-boundary-jOWJUI3DuhK1eGPtwW_5n+Vu3bYHYC75gNHBiQRuBfux.+s3kEf\r\n`,
                    "method": "POST",
                    "mode": "cors"
                    }).then(response => 
                    {
                        resolve(response.text());
                    });
                /*
                var requestParams = new FormData();
                requestParams.append("name", "file");
                requestParams.append("filename", "blob");
                requestParams.append("file", fileReader.result);

                console.log(new URLSearchParams(requestParams).toString());

                var url = `https://${SkynetConfig.portal}/skynet/skyfile`;

                var req = fetch(url, {
                    method: 'post',
                    body: requestParams // or aFile[0]
                }); // returns a promise
                
                req.then(response =>
                {
                    // returns status + response headers
                    // but not yet the body, 
                    // for that call `response[text() || json() || arrayBuffer()]` <-- also promise
                
                    resolve(response.text());

                    if (response.ok) 
                    {
                    // status code was 200-299
                    } else {
                    // status was something else
                    }
                }, error => 
                {
                    reject("Request error");
                })
                */
            }

            fileReader.onerror = e => 
            {
                reject("FileReader error");
            }

            /*
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
            */
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