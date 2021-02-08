class FileUploader
{
    async uploadFileToSkynet(file) 
    {
        return new Promise((resolve, reject) => {
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

            request.send(file);
        });
    }
}

